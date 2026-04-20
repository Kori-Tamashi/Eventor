import { Component, computed, inject, signal } from '@angular/core';
import { finalize, forkJoin, of, switchMap } from 'rxjs';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { EventsApiService } from '../../core/api/services/events-api.service';
import { RegistrationsApiService } from '../../core/api/services/registrations-api.service';
import { UsersApiService } from '../../core/api/services/users-api.service';
import { EventApiModel } from '../../core/api/models/event.models';
import { RegistrationApiModel } from '../../core/api/models/registration.models';
import { EventDetailsDrawerStore } from '../../core/ui/event-details-drawer.store';
import { UiNotificationsService } from '../../core/ui/ui-notifications.service';

type DashboardRow = {
  registrationId: string;
  eventId: string;
  eventTitle: string;
  description: string;
  date: string;
  type: 0 | 1 | 2;
  typeLabel: string;
  paymentLabel: string;
  daysLabel: string;
  rating: number;
  canCancel: boolean;
};

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [InputTextModule, ButtonModule, TableModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  private readonly usersApiService = inject(UsersApiService);
  private readonly eventsApiService = inject(EventsApiService);
  private readonly registrationsApiService = inject(RegistrationsApiService);
  private readonly eventDetailsDrawerStore = inject(EventDetailsDrawerStore);
  private readonly uiNotificationsService = inject(UiNotificationsService);

  readonly pageSize = signal<number>(9);
  readonly pageIndex = signal<number>(0);
  readonly searchTerm = signal<string>('');
  readonly isLoading = signal<boolean>(true);
  readonly cancelingRegistrationId = signal<string | null>(null);
  readonly errorMessage = signal<string>('');
  readonly successMessage = signal<string>('');
  readonly currentUsername = signal<string>('Username');

  readonly allRows = signal<DashboardRow[]>([]);

  constructor() {
    this.loadDashboardEvents();
  }

  private readonly paginationState = computed(() => {
    const query = this.searchTerm().trim().toLowerCase();
    const all = this.allRows().filter((row) => {
      if (!query) {
        return true;
      }

      return (
        row.eventTitle.toLowerCase().includes(query) ||
        row.description.toLowerCase().includes(query) ||
        row.typeLabel.toLowerCase().includes(query) ||
        row.paymentLabel.toLowerCase().includes(query) ||
        row.daysLabel.toLowerCase().includes(query)
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

  readonly pageLabel = computed(() => {
    const { index } = this.paginationState();
    return `${index + 1} из ${this.totalPages()}`;
  });

  readonly pagedRows = computed(() => {
    const { all, size, index } = this.paginationState();
    const start = index * size;
    return all.slice(start, start + size);
  });

  readonly canPrev = computed(() => {
    const { index } = this.paginationState();
    return index > 0;
  });

  readonly canNext = computed(() => {
    const { index } = this.paginationState();
    return index < this.totalPages() - 1;
  });

  prevPage(): void {
    if (!this.canPrev()) {
      return;
    }

    this.pageIndex.update((v) => v - 1);
  }

  nextPage(): void {
    if (!this.canNext()) {
      return;
    }

    this.pageIndex.update((v) => v + 1);
  }

  onSearchInput(value: string): void {
    this.searchTerm.set(value);
    this.pageIndex.set(0);
  }

  openDetails(row: DashboardRow): void {
    this.eventDetailsDrawerStore.openForEvent({
      eventId: row.eventId,
      source: 'dashboard',
      viewerRole: 'user',
      fallbackTitle: row.eventTitle,
    });
  }

  cancelRegistration(row: DashboardRow): void {
    if (this.cancelingRegistrationId() || !row.canCancel) {
      return;
    }

    if (typeof window !== 'undefined' && !window.confirm(`Отменить регистрацию на "${row.eventTitle}"?`)) {
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.cancelingRegistrationId.set(row.registrationId);

    this.registrationsApiService.deleteRegistration(row.registrationId)
      .pipe(finalize(() => this.cancelingRegistrationId.set(null)))
      .subscribe({
        next: () => {
          this.allRows.update((rows) => rows.filter((item) => item.registrationId !== row.registrationId));
          this.clampPageIndexAfterDeletion();
          this.uiNotificationsService.success('Регистрация отменена.');
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapCancelErrorMessage(error));
        },
      });
  }

  private loadDashboardEvents(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.usersApiService.getMe()
      .pipe(
        switchMap((user) => {
          this.currentUsername.set(user.name);
          return this.registrationsApiService.listByUser(user.id);
        }),
        switchMap((registrations) => {
          const eventIds = Array.from(new Set(registrations.map((registration) => registration.eventId)));

          if (eventIds.length === 0) {
            return of({
              registrations,
              eventMap: {} as Record<string, EventApiModel>,
            });
          }

          return forkJoin({
            registrations: of(registrations),
            events: forkJoin(
              eventIds.map((eventId) => this.eventsApiService.getEvent(eventId))
            ),
          }).pipe(
            switchMap(({ registrations: loadedRegistrations, events }) =>
              of({
                registrations: loadedRegistrations,
                eventMap: events.reduce<Record<string, EventApiModel>>((acc, event) => {
                  acc[event.id] = event;
                  return acc;
                }, {}),
              })
            )
          );
        }),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: ({ registrations, eventMap }) => {
          this.allRows.set(
            registrations
              .filter((registration) => registration.type !== 2)
              .map((registration) => this.mapRegistrationToRow(registration, eventMap[registration.eventId]))
              .filter((row): row is DashboardRow => row !== null)
          );
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapErrorMessage(error));
          this.allRows.set([]);
        },
      });
  }

  private mapRegistrationToRow(
    registration: RegistrationApiModel,
    event: EventApiModel | undefined,
  ): DashboardRow | null {
    if (!event) {
      return null;
    }

    return {
      registrationId: registration.id,
      eventId: event.id,
      eventTitle: event.title,
      description: event.description ?? 'Без описания',
      date: this.formatDate(event.startDate),
      type: registration.type,
      typeLabel: this.typeLabel(registration.type),
      paymentLabel: registration.payment ? 'Оплачено' : 'Не оплачено',
      daysLabel: this.formatDays(registration),
      rating: event.rating,
      canCancel: registration.type !== 2,
    };
  }

  private typeLabel(type: 0 | 1 | 2): string {
    switch (type) {
      case 0: return 'Простой';
      case 1: return 'VIP';
      case 2: return 'Организатор';
      default: return 'Не указано';
    }
  }

  private formatDays(registration: RegistrationApiModel): string {
    if (registration.days.length === 0) {
      return 'Дни не выбраны';
    }

    return registration.days
      .slice()
      .sort((a, b) => a.sequenceNumber - b.sequenceNumber)
      .map((day) => day.title?.trim() || `День ${day.sequenceNumber}`)
      .join(', ');
  }

  private formatDate(date: string): string {
    const [year, month, day] = date.split('-');

    if (!year || !month || !day) {
      return date;
    }

    return `${day}.${month}.${year}`;
  }

  private mapErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 401) {
      return 'Выполните вход, чтобы просматривать ваши регистрации.';
    }

    return 'Не удалось загрузить ваши регистрации.';
  }

  private mapCancelErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 401) {
      return 'Выполните вход, чтобы отменять регистрацию.';
    }

    if (status === 404) {
      return 'Регистрация не найдена.';
    }

    return 'Не удалось отменить регистрацию.';
  }

  private clampPageIndexAfterDeletion(): void {
    const size = this.pageSize();
    const totalRows = this.allRows().length;
    const maxPageIndex = Math.max(0, Math.ceil(totalRows / size) - 1);

    if (this.pageIndex() > maxPageIndex) {
      this.pageIndex.set(maxPageIndex);
    }
  }

  private extractStatusCode(error: unknown): number | null {
    if (typeof error !== 'object' || error === null || !('status' in error)) {
      return null;
    }

    const status = (error as { status?: unknown }).status;
    return typeof status === 'number' ? status : null;
  }
}
