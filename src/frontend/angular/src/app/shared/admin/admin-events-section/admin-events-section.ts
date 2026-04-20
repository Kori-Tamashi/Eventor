import { Component, computed, inject, signal } from '@angular/core';
import { finalize, forkJoin, of, switchMap } from 'rxjs';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { EventApiModel } from '../../../core/api/models/event.models';
import { AdminEventsApiService } from '../../../core/api/services/admin-events-api.service';
import { AdminUsersApiService } from '../../../core/api/services/admin-users-api.service';
import { LocationsApiService } from '../../../core/api/services/locations-api.service';
import { EventDetailsDrawerStore } from '../../../core/ui/event-details-drawer.store';
import { EventManagementDrawerStore } from '../../../core/ui/event-management-drawer.store';
import { UiNotificationsService } from '../../../core/ui/ui-notifications.service';

type AdminEventRow = {
  id: string;
  title: string;
  organizer: string;
  location: string;
  description: string;
  date: string;
  peopleCount: number;
  daysCount: number;
  markup: number;
  rating: number;
};

@Component({
  selector: 'app-admin-events-section',
  standalone: true,
  imports: [InputTextModule, ButtonModule, TableModule],
  templateUrl: './admin-events-section.html',
  styleUrl: './admin-events-section.scss',
})
export class AdminEventsSection {
  private static readonly EMPTY_GUID = '00000000-0000-0000-0000-000000000000';
  private readonly adminEventsApiService = inject(AdminEventsApiService);
  private readonly locationsApiService = inject(LocationsApiService);
  private readonly adminUsersApiService = inject(AdminUsersApiService);
  private readonly eventDetailsDrawerStore = inject(EventDetailsDrawerStore);
  private readonly eventManagementDrawerStore = inject(EventManagementDrawerStore);
  private readonly uiNotificationsService = inject(UiNotificationsService);

  readonly pageSize = signal<number>(10);
  readonly pageIndex = signal<number>(0);
  readonly searchTerm = signal<string>('');
  readonly isLoading = signal<boolean>(true);
  readonly deletingEventId = signal<string | null>(null);
  readonly errorMessage = signal<string>('');
  readonly successMessage = signal<string>('');
  readonly allRows = signal<AdminEventRow[]>([]);

  constructor() {
    this.loadEvents();
  }

  private readonly paginationState = computed(() => {
    const query = this.searchTerm().trim().toLowerCase();
    const all = this.allRows().filter((row) => {
      if (!query) {
        return true;
      }

      return (
        row.title.toLowerCase().includes(query) ||
        row.organizer.toLowerCase().includes(query) ||
        row.location.toLowerCase().includes(query) ||
        row.description.toLowerCase().includes(query)
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

  openDetails(row: AdminEventRow): void {
    this.eventDetailsDrawerStore.openForEvent({
      eventId: row.id,
      source: 'organization',
      viewerRole: 'superuser',
      fallbackTitle: row.title,
    });
  }

  openManageDrawer(row: AdminEventRow): void {
    this.openDrawer(row, 'settings');
  }

  openDaysDrawer(row: AdminEventRow): void {
    this.openDrawer(row, 'days');
  }

  openReviewsDrawer(row: AdminEventRow): void {
    this.openDrawer(row, 'reviews');
  }

  private openDrawer(row: AdminEventRow, initialTab: 'settings' | 'days' | 'reviews'): void {
    this.successMessage.set('');
    this.eventManagementDrawerStore.open({
      mode: 'superuser-manage',
      source: 'superuser-page',
      viewerRole: 'superuser',
      title: row.title,
      eventId: row.id,
      initialTab,
    });
  }

  deleteEvent(row: AdminEventRow): void {
    if (this.deletingEventId()) {
      return;
    }

    if (typeof window !== 'undefined' && !window.confirm(`Удалить мероприятие "${row.title}"?`)) {
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.deletingEventId.set(row.id);

    this.adminEventsApiService.deleteEvent(row.id)
      .pipe(finalize(() => this.deletingEventId.set(null)))
      .subscribe({
        next: () => {
          this.allRows.update((rows) => rows.filter((item) => item.id !== row.id));
          this.clampPageIndex();
          this.uiNotificationsService.success('Мероприятие удалено.');
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapDeleteErrorMessage(error));
        },
      });
  }

  private loadEvents(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.adminEventsApiService.listEvents()
      .pipe(
        switchMap((events) => {
          const locationIds = events.map((event) => event.locationId);
          const organizerIds = events.map((event) => event.createdByUserId);

          return forkJoin({
            events: of(events),
            locationMap: this.locationsApiService.getLocationTitleMap(locationIds),
            userMap: this.adminUsersApiService.getUserNameMap(organizerIds),
          });
        }),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: ({ events, locationMap, userMap }) => {
          this.allRows.set(events.map((event) => this.mapRow(event, {
            location: locationMap[event.locationId] ?? event.locationId,
            organizer: userMap[event.createdByUserId] ?? this.resolveOrganizerLabel(event.createdByUserId),
          })));
          this.clampPageIndex();
        },
        error: (error: unknown) => {
          this.allRows.set([]);
          this.errorMessage.set(this.mapLoadErrorMessage(error));
        },
      });
  }

  private mapRow(event: EventApiModel, resolved: { location: string; organizer: string }): AdminEventRow {
    return {
      id: event.id,
      title: event.title,
      organizer: resolved.organizer,
      location: resolved.location,
      description: event.description ?? 'Без описания',
      date: this.formatDate(event.startDate),
      peopleCount: event.personCount,
      daysCount: event.daysCount,
      markup: event.percent,
      rating: event.rating,
    };
  }

  private formatDate(date: string): string {
    const [year, month, day] = date.split('-');

    if (!year || !month || !day) {
      return date;
    }

    return `${day}.${month}.${year}`;
  }

  private resolveOrganizerLabel(userId: string): string {
    if (!userId || userId === AdminEventsSection.EMPTY_GUID) {
      return 'Не указан';
    }

    return userId;
  }

  private clampPageIndex(): void {
    const maxPageIndex = Math.max(0, Math.ceil(this.allRows().length / this.pageSize()) - 1);
    if (this.pageIndex() > maxPageIndex) {
      this.pageIndex.set(maxPageIndex);
    }
  }

  private mapLoadErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 401) {
      return 'Выполните вход, чтобы просматривать мероприятия.';
    }

    return 'Не удалось загрузить список мероприятий.';
  }

  private mapDeleteErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 401) {
      return 'Выполните вход, чтобы удалять мероприятия.';
    }

    if (status === 404) {
      return 'Мероприятие не найдено.';
    }

    return 'Не удалось удалить мероприятие.';
  }

  private extractStatusCode(error: unknown): number | null {
    if (typeof error !== 'object' || error === null || !('status' in error)) {
      return null;
    }

    const { status } = error as { status?: unknown };
    return typeof status === 'number' ? status : null;
  }
}
