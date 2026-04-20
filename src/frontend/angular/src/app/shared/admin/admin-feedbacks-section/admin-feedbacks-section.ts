import { Component, computed, inject, signal } from '@angular/core';
import { finalize, forkJoin, of, switchMap } from 'rxjs';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { AdminUsersApiService } from '../../../core/api/services/admin-users-api.service';
import { EventsApiService } from '../../../core/api/services/events-api.service';
import { AdminFeedbacksApiService } from '../../../core/api/services/admin-feedbacks-api.service';
import { FeedbacksApiService } from '../../../core/api/services/feedbacks-api.service';
import { RegistrationsApiService } from '../../../core/api/services/registrations-api.service';
import { FeedbackApiModel } from '../../../core/api/models/feedback.models';
import { EventDetailsDrawerStore } from '../../../core/ui/event-details-drawer.store';
import { AdminFeedbackDrawer } from '../admin-feedback-drawer/admin-feedback-drawer';
import { AdminFeedbackDraftValue, AdminFeedbackSavePayload } from '../admin-feedback-drawer/admin-feedback.models';

type AdminFeedbackRow = {
  id: string;
  registrationId: string;
  eventId: string;
  eventTitle: string;
  author: string;
  comment: string;
  rate: number;
};

@Component({
  selector: 'app-admin-feedbacks-section',
  standalone: true,
  imports: [InputTextModule, ButtonModule, TableModule, AdminFeedbackDrawer],
  templateUrl: './admin-feedbacks-section.html',
  styleUrl: './admin-feedbacks-section.scss',
})
export class AdminFeedbacksSection {
  private readonly feedbacksApiService = inject(FeedbacksApiService);
  private readonly adminFeedbacksApiService = inject(AdminFeedbacksApiService);
  private readonly registrationsApiService = inject(RegistrationsApiService);
  private readonly adminUsersApiService = inject(AdminUsersApiService);
  private readonly eventsApiService = inject(EventsApiService);
  private readonly eventDetailsDrawerStore = inject(EventDetailsDrawerStore);

  readonly pageSize = signal<number>(10);
  readonly pageIndex = signal<number>(0);
  readonly searchTerm = signal<string>('');
  readonly isLoading = signal<boolean>(true);
  readonly errorMessage = signal<string>('');
  readonly successMessage = signal<string>('');
  readonly rows = signal<AdminFeedbackRow[]>([]);

  readonly drawerOpen = signal<boolean>(false);
  readonly drawerInitialValue = signal<AdminFeedbackDraftValue | null>(null);
  readonly drawerErrorMessage = signal<string>('');
  readonly isDrawerLoading = signal<boolean>(false);
  readonly isSavingDrawer = signal<boolean>(false);
  readonly editingFeedbackId = signal<string | null>(null);
  readonly deletingFeedbackId = signal<string | null>(null);

  constructor() {
    this.loadFeedbacks();
  }

  private readonly paginationState = computed(() => {
    const query = this.searchTerm().trim().toLowerCase();
    const all = this.rows().filter((row) => {
      if (!query) {
        return true;
      }

      return (
        row.eventTitle.toLowerCase().includes(query) ||
        row.author.toLowerCase().includes(query) ||
        row.comment.toLowerCase().includes(query)
      );
    });
    const size = this.pageSize();
    const index = this.pageIndex();
    return { all, size, index };
  });

  readonly totalPages = computed(() => {
    const { all, size } = this.paginationState();
    return Math.max(1, Math.ceil(all.length / size));
  });
  readonly pageLabel = computed(() => `${this.pageIndex() + 1} из ${this.totalPages()}`);
  readonly pagedRows = computed(() => {
    const { all, size, index } = this.paginationState();
    const start = index * size;
    return all.slice(start, start + size);
  });
  readonly canPrev = computed(() => this.pageIndex() > 0);
  readonly canNext = computed(() => this.pageIndex() < this.totalPages() - 1);

  prevPage(): void {
    if (this.canPrev()) {
      this.pageIndex.update((value) => value - 1);
    }
  }

  nextPage(): void {
    if (this.canNext()) {
      this.pageIndex.update((value) => value + 1);
    }
  }

  onSearchInput(value: string): void {
    this.searchTerm.set(value);
    this.pageIndex.set(0);
  }

  openRelatedEvent(row: AdminFeedbackRow): void {
    this.eventDetailsDrawerStore.openForEvent({
      eventId: row.eventId,
      source: 'organization',
      viewerRole: 'superuser',
      fallbackTitle: row.eventTitle,
    });
  }

  openEditDrawer(row: AdminFeedbackRow): void {
    this.drawerInitialValue.set(null);
    this.drawerErrorMessage.set('');
    this.editingFeedbackId.set(row.id);
    this.isDrawerLoading.set(true);
    this.drawerOpen.set(true);

    this.feedbacksApiService.getFeedback(row.id)
      .pipe(finalize(() => this.isDrawerLoading.set(false)))
      .subscribe({
        next: (feedback) => {
          this.drawerInitialValue.set({
            comment: feedback.comment,
            rate: feedback.rate,
          });
        },
        error: (error: unknown) => {
          this.drawerErrorMessage.set(this.mapDrawerLoadErrorMessage(error));
        },
      });
  }

  closeDrawer(): void {
    this.drawerOpen.set(false);
    this.drawerInitialValue.set(null);
    this.drawerErrorMessage.set('');
    this.isDrawerLoading.set(false);
    this.isSavingDrawer.set(false);
    this.editingFeedbackId.set(null);
  }

  saveFeedback(payload: AdminFeedbackSavePayload): void {
    const feedbackId = this.editingFeedbackId();
    if (!feedbackId) {
      return;
    }

    this.drawerErrorMessage.set('');
    this.isSavingDrawer.set(true);

    this.feedbacksApiService.updateFeedback(feedbackId, payload)
      .pipe(finalize(() => this.isSavingDrawer.set(false)))
      .subscribe({
        next: (feedback) => {
          this.rows.update((rows) =>
            rows.map((row) =>
              row.id === feedbackId
                ? { ...row, comment: feedback.comment, rate: feedback.rate }
                : row
            )
          );
          this.closeDrawer();
          this.successMessage.set('Отзыв обновлен.');
        },
        error: (error: unknown) => {
          this.drawerErrorMessage.set(this.mapDrawerSaveErrorMessage(error));
        },
      });
  }

  deleteFeedback(row: AdminFeedbackRow): void {
    if (this.deletingFeedbackId()) {
      return;
    }

    if (typeof window !== 'undefined' && !window.confirm('Удалить этот отзыв?')) {
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.deletingFeedbackId.set(row.id);

    this.adminFeedbacksApiService.deleteFeedback(row.id)
      .pipe(finalize(() => this.deletingFeedbackId.set(null)))
      .subscribe({
        next: () => {
          this.rows.update((rows) => rows.filter((feedback) => feedback.id !== row.id));
          this.clampPageIndex();
          this.successMessage.set('Отзыв удален.');
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapDeleteErrorMessage(error));
        },
      });
  }

  private loadFeedbacks(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.feedbacksApiService.listFeedbacks()
      .pipe(
        switchMap((feedbacks) => {
          const registrationIds = Array.from(new Set(feedbacks.map((feedback) => feedback.registrationId)));

          if (registrationIds.length === 0) {
            return of({ feedbacks, registrationMap: {}, userMap: {}, eventMap: {} });
          }

          return forkJoin({
            feedbacks: of(feedbacks),
            registrations: forkJoin(
              registrationIds.map((registrationId) => this.registrationsApiService.getRegistration(registrationId))
            ),
          }).pipe(
            switchMap(({ feedbacks: loadedFeedbacks, registrations }) => {
              const registrationMap = registrations.reduce<Record<string, { eventId: string; userId: string }>>((acc, registration) => {
                acc[registration.id] = {
                  eventId: registration.eventId,
                  userId: registration.userId,
                };
                return acc;
              }, {});

              const userIds = registrations.map((registration) => registration.userId);
              const eventIds = registrations.map((registration) => registration.eventId);

              return forkJoin({
                feedbacks: of(loadedFeedbacks),
                registrationMap: of(registrationMap),
                userMap: this.adminUsersApiService.getUserNameMap(userIds),
                eventMap: this.eventsApiService.getEventTitleMap(eventIds),
              });
            })
          );
        }),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: ({ feedbacks, registrationMap, userMap, eventMap }) => {
          this.rows.set(feedbacks.map((feedback) => this.mapRow(feedback, registrationMap, userMap, eventMap)));
          this.clampPageIndex();
        },
        error: (error: unknown) => {
          this.rows.set([]);
          this.errorMessage.set(this.mapLoadErrorMessage(error));
        },
      });
  }

  private mapRow(
    feedback: FeedbackApiModel,
    registrationMap: Record<string, { eventId: string; userId: string }>,
    userMap: Record<string, string>,
    eventMap: Record<string, string>,
  ): AdminFeedbackRow {
    const registration = registrationMap[feedback.registrationId];
    const eventId = registration?.eventId ?? '';
    const userId = registration?.userId ?? '';

    return {
      id: feedback.id,
      registrationId: feedback.registrationId,
      eventId,
      eventTitle: (eventMap[eventId] ?? eventId) || 'Неизвестное мероприятие',
      author: (userMap[userId] ?? userId) || 'Неизвестный пользователь',
      comment: feedback.comment,
      rate: feedback.rate,
    };
  }

  private clampPageIndex(): void {
    const maxPageIndex = Math.max(0, Math.ceil(this.rows().length / this.pageSize()) - 1);
    if (this.pageIndex() > maxPageIndex) {
      this.pageIndex.set(maxPageIndex);
    }
  }

  private mapLoadErrorMessage(error: unknown): string {
    return this.extractStatusCode(error) === 401
      ? 'Выполните вход, чтобы просматривать отзывы.'
      : 'Не удалось загрузить список отзывов.';
  }

  private mapDrawerLoadErrorMessage(error: unknown): string {
    return this.extractStatusCode(error) === 404
      ? 'Отзыв не найден.'
      : 'Не удалось загрузить отзыв.';
  }

  private mapDrawerSaveErrorMessage(error: unknown): string {
    return this.extractStatusCode(error) === 404
      ? 'Отзыв не найден.'
      : 'Не удалось обновить отзыв.';
  }

  private mapDeleteErrorMessage(error: unknown): string {
    return this.extractStatusCode(error) === 404
      ? 'Отзыв не найден.'
      : 'Не удалось удалить отзыв.';
  }

  private extractStatusCode(error: unknown): number | null {
    if (typeof error !== 'object' || error === null || !('status' in error)) {
      return null;
    }

    const { status } = error as { status?: unknown };
    return typeof status === 'number' ? status : null;
  }
}
