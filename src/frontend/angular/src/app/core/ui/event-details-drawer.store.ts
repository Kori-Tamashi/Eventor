import { Injectable, Injector, computed, inject, signal } from '@angular/core';
import { Subscription, finalize } from 'rxjs';
import { EventDetailsDrawerDataService } from './event-details-drawer-data.service';
import {
  EventDetailsDrawerContext,
  EventDetailsDrawerDay,
  EventDetailsDrawerSource,
  EventDetailsDrawerViewerRole,
} from '../../shared/event-details-drawer/event-details-drawer.models';

@Injectable({
  providedIn: 'root',
})
export class EventDetailsDrawerStore {
  private readonly injector = inject(Injector);

  private loadSubscription: Subscription | null = null;
  private saveReviewSubscription: Subscription | null = null;

  readonly currentUsername = signal<string>('Username');

  readonly isOpen = signal<boolean>(false);
  readonly isLoading = signal<boolean>(false);
  readonly errorMessage = signal<string>('');
  readonly context = signal<EventDetailsDrawerContext | null>(null);
  readonly selectedDay = signal<EventDetailsDrawerDay | null>(null);

  readonly reviewMaxLen = 250;
  readonly reviewText = signal<string>('');
  readonly reviewRating = signal<number>(0);

  readonly dayParticipantsOpen = computed(() => this.selectedDay() !== null);

  readonly drawerTitle = computed(() => {
    if (this.dayParticipantsOpen()) {
      return this.selectedDay()?.name ?? 'Название дня';
    }

    return this.context()?.data.title ?? 'Название мероприятия';
  });

  readonly parentTitle = computed(() => this.context()?.data.title ?? 'Название мероприятия');

  readonly daysCount = computed(() => this.context()?.data.daysCount ?? 0);
  readonly participantsCount = computed(() => this.context()?.data.participantsCount ?? 0);

  readonly dayRows = computed(() => this.context()?.data.dayRows ?? []);
  readonly selectedDayParticipants = computed(() => this.selectedDay()?.participantRows ?? []);
  readonly reviewRows = computed(() => this.context()?.data.reviewRows ?? []);

  readonly reviewCountLabel = computed(() => `${this.reviewText().length}/${this.reviewMaxLen}`);

  readonly canSaveReview = computed(() => {
    return this.reviewText().trim().length > 0 && this.reviewRating() > 0;
  });

  open(context: EventDetailsDrawerContext): void {
    this.cancelOngoingRequests();
    this.context.set(context);
    this.selectedDay.set(null);
    this.reviewText.set('');
    this.reviewRating.set(0);
    this.isLoading.set(false);
    this.errorMessage.set('');
    this.isOpen.set(true);
  }

  openForEvent(params: {
    eventId: string;
    source: EventDetailsDrawerSource;
    viewerRole: EventDetailsDrawerViewerRole;
    fallbackTitle?: string;
  }): void {
    this.cancelOngoingRequests();
    this.context.set({
      eventId: params.eventId,
      source: params.source,
      viewerRole: params.viewerRole,
      data: {
        title: params.fallbackTitle?.trim() || 'Загрузка мероприятия',
        daysCount: 0,
        participantsCount: 0,
        dayRows: [],
        reviewRows: [],
        currentUserRegistrationId: null,
      },
    });
    this.selectedDay.set(null);
    this.reviewText.set('');
    this.reviewRating.set(0);
    this.errorMessage.set('');
    this.isLoading.set(true);
    this.isOpen.set(true);

    this.loadSubscription = this.dataService.loadEventDetails({
      eventId: params.eventId,
      source: params.source,
      viewerRole: params.viewerRole,
    })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: ({ context, currentUsername }) => {
          this.currentUsername.set(currentUsername);
          this.context.set(context);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapLoadErrorMessage(error));
        },
      });
  }

  close(): void {
    this.cancelOngoingRequests();
    this.isOpen.set(false);
    this.isLoading.set(false);
    this.errorMessage.set('');
    this.context.set(null);
    this.selectedDay.set(null);
    this.reviewText.set('');
    this.reviewRating.set(0);
  }

  onDrawerVisibleChange(visible: boolean): void {
    if (!visible) {
      this.close();
      return;
    }

    this.isOpen.set(true);
  }

  openDayParticipants(day: EventDetailsDrawerDay): void {
    this.selectedDay.set(day);
  }

  closeDayParticipants(): void {
    this.selectedDay.set(null);
  }

  onReviewInput(value: string): void {
    this.reviewText.set(value.slice(0, this.reviewMaxLen));
  }

  onReviewRatingChange(value: number): void {
    this.reviewRating.set(value ?? 0);
  }

  cancelReview(): void {
    this.reviewText.set('');
    this.reviewRating.set(0);
  }

  saveReview(): void {
    if (!this.canSaveReview()) {
      return;
    }

    const currentContext = this.context();
    const registrationId = currentContext?.data.currentUserRegistrationId;

    if (!currentContext || !registrationId) {
      this.errorMessage.set('Нельзя оставить отзыв без регистрации на мероприятие.');
      return;
    }

    this.errorMessage.set('');
    this.saveReviewSubscription?.unsubscribe();
    this.saveReviewSubscription = this.dataService.createReview({
      registrationId,
      comment: this.reviewText().trim(),
      rate: this.reviewRating(),
    }, this.currentUsername())
      .subscribe({
        next: (newReview) => {
          this.context.update((ctx) => {
            if (!ctx) {
              return null;
            }

            return {
              ...ctx,
              data: {
                ...ctx.data,
                reviewRows: [newReview, ...ctx.data.reviewRows],
              },
            };
          });

          this.cancelReview();
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapSaveReviewErrorMessage(error));
        },
      });
  }

  private get dataService(): EventDetailsDrawerDataService {
    return this.injector.get(EventDetailsDrawerDataService);
  }

  private cancelOngoingRequests(): void {
    this.loadSubscription?.unsubscribe();
    this.saveReviewSubscription?.unsubscribe();
    this.loadSubscription = null;
    this.saveReviewSubscription = null;
  }

  private mapLoadErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 401) {
      return 'Выполните вход, чтобы просматривать детали мероприятия.';
    }

    if (status === 404) {
      return 'Мероприятие не найдено.';
    }

    return 'Не удалось загрузить детали мероприятия.';
  }

  private mapSaveReviewErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 401) {
      return 'Выполните вход, чтобы оставить отзыв.';
    }

    if (status === 404) {
      return 'Регистрация для отзыва не найдена.';
    }

    if (status === 409) {
      return 'Отзыв для этой регистрации уже существует.';
    }

    return 'Не удалось сохранить отзыв.';
  }

  private extractStatusCode(error: unknown): number | null {
    if (typeof error !== 'object' || error === null || !('status' in error)) {
      return null;
    }

    const status = (error as { status?: unknown }).status;
    return typeof status === 'number' ? status : null;
  }
}
