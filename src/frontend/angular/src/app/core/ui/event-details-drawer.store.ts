import { Injectable, Injector, computed, inject, signal } from '@angular/core';
import { Subscription, finalize } from 'rxjs';
import { EventDetailsDrawerDataService } from './event-details-drawer-data.service';
import {
  EventDetailsDrawerContext,
  EventDetailsDrawerDay,
  EventDetailsDrawerRegistrationType,
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
  private deleteReviewSubscription: Subscription | null = null;
  private saveRegistrationSubscription: Subscription | null = null;
  private deleteRegistrationSubscription: Subscription | null = null;

  readonly currentUsername = signal<string>('Username');

  readonly isOpen = signal<boolean>(false);
  readonly isLoading = signal<boolean>(false);
  readonly isSavingRegistration = signal<boolean>(false);
  readonly deletingFeedbackId = signal<string | null>(null);
  readonly errorMessage = signal<string>('');
  readonly context = signal<EventDetailsDrawerContext | null>(null);
  readonly selectedDay = signal<EventDetailsDrawerDay | null>(null);

  readonly reviewMaxLen = 250;
  readonly reviewText = signal<string>('');
  readonly reviewRating = signal<number>(0);
  readonly registrationType = signal<EventDetailsDrawerRegistrationType>(0);
  readonly registrationDayIds = signal<string[]>([]);

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
  readonly currentUserId = computed(() => this.context()?.data.currentUserId ?? null);
  readonly currentUserRegistrationId = computed(() => this.context()?.data.currentUserRegistrationId ?? null);
  readonly currentUserRegistrationPayment = computed(() => this.context()?.data.currentUserRegistrationPayment ?? null);
  readonly currentUserRegistrationType = computed(() => this.context()?.data.currentUserRegistrationType ?? null);
  readonly currentUserRegistrationDayIds = computed(() => this.context()?.data.currentUserRegistrationDayIds ?? []);
  readonly isCurrentUserRegistered = computed(() => this.currentUserRegistrationId() !== null);
  readonly canManageRegistration = computed(() => this.currentUserId() !== null);
  readonly canSaveRegistration = computed(() => {
    if (!this.canManageRegistration()) {
      return false;
    }

    const selectedDayIds = this.registrationDayIds();
    if (selectedDayIds.length === 0) {
      return false;
    }

    if (!this.isCurrentUserRegistered()) {
      return true;
    }

    return this.hasRegistrationChanges();
  });

  readonly canSaveReview = computed(() => {
    return this.reviewText().trim().length > 0 && this.reviewRating() > 0;
  });
  readonly registrationTypeOptions = [
    { value: 0 as const, label: 'Простой' },
    { value: 1 as const, label: 'VIP' },
    { value: 2 as const, label: 'Организатор' },
  ];
  readonly registrationMessage = computed(() => {
    if (!this.canManageRegistration()) {
      return 'Выполните вход, чтобы записаться на мероприятие.';
    }

    if (!this.isCurrentUserRegistered()) {
      return 'Вы еще не зарегистрированы на это мероприятие.';
    }

    const payment = this.currentUserRegistrationPayment();
    return payment ? 'Статус оплаты: оплачено.' : 'Статус оплаты: не оплачено.';
  });

  open(context: EventDetailsDrawerContext): void {
    this.cancelOngoingRequests();
    this.context.set(context);
    this.selectedDay.set(null);
    this.reviewText.set('');
    this.reviewRating.set(0);
    this.deletingFeedbackId.set(null);
    this.syncRegistrationFormFromContext(context);
    this.isLoading.set(false);
    this.isSavingRegistration.set(false);
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
        currentUserId: null,
        currentUsername: null,
        currentUserRegistrationId: null,
        currentUserRegistrationType: null,
        currentUserRegistrationPayment: null,
        currentUserRegistrationDayIds: [],
      },
    });
    this.selectedDay.set(null);
    this.reviewText.set('');
    this.reviewRating.set(0);
    this.registrationType.set(0);
    this.registrationDayIds.set([]);
    this.deletingFeedbackId.set(null);
    this.errorMessage.set('');
    this.isLoading.set(true);
    this.isSavingRegistration.set(false);
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
          this.syncRegistrationFormFromContext(context);
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
    this.isSavingRegistration.set(false);
    this.errorMessage.set('');
    this.context.set(null);
    this.selectedDay.set(null);
    this.reviewText.set('');
    this.reviewRating.set(0);
    this.registrationType.set(0);
    this.registrationDayIds.set([]);
    this.deletingFeedbackId.set(null);
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

  setRegistrationType(value: EventDetailsDrawerRegistrationType): void {
    this.registrationType.set(value);
  }

  toggleRegistrationDay(dayId: string, checked: boolean): void {
    if (checked) {
      this.registrationDayIds.update((ids) => (ids.includes(dayId) ? ids : [...ids, dayId]));
      return;
    }

    this.registrationDayIds.update((ids) => ids.filter((id) => id !== dayId));
  }

  saveRegistration(): void {
    if (!this.canSaveRegistration()) {
      return;
    }

    const currentContext = this.context();
    const currentUserId = this.currentUserId();
    if (!currentContext || !currentUserId) {
      this.errorMessage.set('Выполните вход, чтобы записаться на мероприятие.');
      return;
    }

    const selectedDayIds = this.registrationDayIds();
    if (selectedDayIds.length === 0) {
      this.errorMessage.set('Выберите хотя бы один день участия.');
      return;
    }

    this.errorMessage.set('');
    this.isSavingRegistration.set(true);
    this.saveRegistrationSubscription?.unsubscribe();

    const registrationId = this.currentUserRegistrationId();
    const request$ = registrationId
      ? this.dataService.updateRegistration(registrationId, {
          type: this.registrationType(),
          payment: this.currentUserRegistrationPayment(),
          dayIds: selectedDayIds,
        })
      : this.dataService.createRegistration({
          eventId: currentContext.eventId,
          userId: currentUserId,
          type: this.registrationType(),
          payment: false,
          dayIds: selectedDayIds,
        });

    this.saveRegistrationSubscription = request$
      .pipe(finalize(() => this.isSavingRegistration.set(false)))
      .subscribe({
        next: () => {
          this.reloadCurrentEvent();
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapRegistrationErrorMessage(error));
        },
      });
  }

  deleteRegistration(): void {
    const registrationId = this.currentUserRegistrationId();
    if (!registrationId) {
      return;
    }

    this.errorMessage.set('');
    this.isSavingRegistration.set(true);
    this.deleteRegistrationSubscription?.unsubscribe();
    this.deleteRegistrationSubscription = this.dataService.deleteRegistration(registrationId)
      .pipe(finalize(() => this.isSavingRegistration.set(false)))
      .subscribe({
        next: () => {
          this.reloadCurrentEvent();
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapRegistrationErrorMessage(error));
        },
      });
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

  deleteReview(feedbackId: string): void {
    const currentContext = this.context();
    const review = currentContext?.data.reviewRows.find((row) => row.feedbackId === feedbackId) ?? null;

    if (!currentContext || !review?.isOwnedByCurrentUser || this.deletingFeedbackId()) {
      return;
    }

    if (typeof window !== 'undefined' && !window.confirm('Удалить ваш отзыв?')) {
      return;
    }

    this.errorMessage.set('');
    this.deleteReviewSubscription?.unsubscribe();
    this.deletingFeedbackId.set(feedbackId);
    this.deleteReviewSubscription = this.dataService.deleteReview(feedbackId)
      .pipe(finalize(() => this.deletingFeedbackId.set(null)))
      .subscribe({
        next: () => {
          this.context.update((ctx) => {
            if (!ctx) {
              return null;
            }

            return {
              ...ctx,
              data: {
                ...ctx.data,
                reviewRows: ctx.data.reviewRows.filter((row) => row.feedbackId !== feedbackId),
              },
            };
          });
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapDeleteReviewErrorMessage(error));
        },
      });
  }

  private get dataService(): EventDetailsDrawerDataService {
    return this.injector.get(EventDetailsDrawerDataService);
  }

  private cancelOngoingRequests(): void {
    this.loadSubscription?.unsubscribe();
    this.saveReviewSubscription?.unsubscribe();
    this.deleteReviewSubscription?.unsubscribe();
    this.saveRegistrationSubscription?.unsubscribe();
    this.deleteRegistrationSubscription?.unsubscribe();
    this.loadSubscription = null;
    this.saveReviewSubscription = null;
    this.deleteReviewSubscription = null;
    this.saveRegistrationSubscription = null;
    this.deleteRegistrationSubscription = null;
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

  private mapDeleteReviewErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 401) {
      return 'Выполните вход, чтобы удалить отзыв.';
    }

    if (status === 404) {
      return 'Отзыв не найден.';
    }

    return 'Не удалось удалить отзыв.';
  }

  private hasRegistrationChanges(): boolean {
    const savedType = this.currentUserRegistrationType();
    const savedDayIds = this.currentUserRegistrationDayIds();
    const draftDayIds = this.registrationDayIds();

    if (savedType !== this.registrationType()) {
      return true;
    }

    if (savedDayIds.length !== draftDayIds.length) {
      return true;
    }

    const savedDaySet = new Set(savedDayIds);
    return draftDayIds.some((id) => !savedDaySet.has(id));
  }

  private syncRegistrationFormFromContext(context: EventDetailsDrawerContext): void {
    const registeredDayIds = context.data.currentUserRegistrationDayIds;
    this.registrationType.set(context.data.currentUserRegistrationType ?? 0);
    this.registrationDayIds.set(
      registeredDayIds.length > 0 ? [...registeredDayIds] : context.data.dayRows.map((day) => day.id)
    );
  }

  private reloadCurrentEvent(): void {
    const currentContext = this.context();
    if (!currentContext) {
      return;
    }

    this.openForEvent({
      eventId: currentContext.eventId,
      source: currentContext.source,
      viewerRole: currentContext.viewerRole,
      fallbackTitle: currentContext.data.title,
    });
  }

  private mapRegistrationErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 400) {
      return 'Проверьте выбранные дни и тип регистрации.';
    }

    if (status === 401) {
      return 'Выполните вход, чтобы управлять регистрацией.';
    }

    if (status === 404) {
      return 'Регистрация или мероприятие не найдены.';
    }

    if (status === 409) {
      return 'Регистрация уже существует.';
    }

    return 'Не удалось сохранить изменения регистрации.';
  }

  private extractStatusCode(error: unknown): number | null {
    if (typeof error !== 'object' || error === null || !('status' in error)) {
      return null;
    }

    const status = (error as { status?: unknown }).status;
    return typeof status === 'number' ? status : null;
  }
}
