import { Injectable, computed, signal } from '@angular/core';
import {
  EventDetailsDrawerContext,
  EventDetailsDrawerDay,
} from '../../shared/event-details-drawer/event-details-drawer.models';

@Injectable({
  providedIn: 'root',
})
export class EventDetailsDrawerStore {
  readonly currentUsername = signal<string>('Username');

  readonly isOpen = signal<boolean>(false);
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
    this.context.set(context);
    this.selectedDay.set(null);
    this.reviewText.set('');
    this.reviewRating.set(0);
    this.isOpen.set(true);
  }

  close(): void {
    this.isOpen.set(false);
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

    if (!currentContext) {
      return;
    }

    const newReview = {
      person: this.currentUsername(),
      comment: this.reviewText().trim(),
      rating: this.reviewRating(),
    };

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
  }
}
