import { Component, computed, inject, signal } from '@angular/core';
import { finalize, forkJoin, of, switchMap } from 'rxjs';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DayApiModel } from '../../../core/api/models/day.models';
import { DaysApiService } from '../../../core/api/services/days-api.service';
import { EventsApiService } from '../../../core/api/services/events-api.service';
import { MenusApiService } from '../../../core/api/services/menus-api.service';
import { EventDetailsDrawerStore } from '../../../core/ui/event-details-drawer.store';
import { EventManagementDrawerStore } from '../../../core/ui/event-management-drawer.store';

type AdminDayRow = {
  id: string;
  eventId: string;
  eventTitle: string;
  menuTitle: string;
  title: string;
  description: string;
  sequenceNumber: number;
};

@Component({
  selector: 'app-admin-days-section',
  standalone: true,
  imports: [InputTextModule, ButtonModule, TableModule],
  templateUrl: './admin-days-section.html',
  styleUrl: './admin-days-section.scss',
})
export class AdminDaysSection {
  private readonly daysApiService = inject(DaysApiService);
  private readonly eventsApiService = inject(EventsApiService);
  private readonly menusApiService = inject(MenusApiService);
  private readonly eventDetailsDrawerStore = inject(EventDetailsDrawerStore);
  private readonly eventManagementDrawerStore = inject(EventManagementDrawerStore);

  readonly pageSize = signal<number>(10);
  readonly pageIndex = signal<number>(0);
  readonly searchTerm = signal<string>('');
  readonly isLoading = signal<boolean>(true);
  readonly errorMessage = signal<string>('');
  readonly allRows = signal<AdminDayRow[]>([]);

  constructor() {
    this.loadDays();
  }

  private readonly paginationState = computed(() => {
    const query = this.searchTerm().trim().toLowerCase();
    const all = this.allRows().filter((row) => {
      if (!query) {
        return true;
      }

      return (
        row.eventTitle.toLowerCase().includes(query) ||
        row.title.toLowerCase().includes(query) ||
        row.menuTitle.toLowerCase().includes(query) ||
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

  openDetails(row: AdminDayRow): void {
    this.eventDetailsDrawerStore.openForEvent({
      eventId: row.eventId,
      source: 'organization',
      viewerRole: 'superuser',
      fallbackTitle: row.eventTitle,
    });
  }

  openManageDrawer(row: AdminDayRow): void {
    this.eventManagementDrawerStore.open({
      mode: 'superuser-manage',
      source: 'superuser-page',
      viewerRole: 'superuser',
      title: row.eventTitle,
      eventId: row.eventId,
    });
  }

  private loadDays(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.daysApiService.listDays()
      .pipe(
        switchMap((days) => {
          const eventIds = days.map((day) => day.eventId);
          const menuIds = days.map((day) => day.menuId);

          return forkJoin({
            days: of(days),
            eventMap: this.eventsApiService.getEventTitleMap(eventIds),
            menuMap: this.menusApiService.getMenuTitleMap(menuIds),
          });
        }),
        finalize(() => this.isLoading.set(false)),
      )
      .subscribe({
        next: ({ days, eventMap, menuMap }) => {
          this.allRows.set(
            days
              .slice()
              .sort((a, b) => a.sequenceNumber - b.sequenceNumber)
              .map((day) => this.mapRow(day, eventMap, menuMap))
          );
          this.clampPageIndex();
        },
        error: (error: unknown) => {
          this.allRows.set([]);
          this.errorMessage.set(this.mapLoadErrorMessage(error));
        },
      });
  }

  private mapRow(
    day: DayApiModel,
    eventMap: Record<string, string>,
    menuMap: Record<string, string>,
  ): AdminDayRow {
    return {
      id: day.id,
      eventId: day.eventId,
      eventTitle: eventMap[day.eventId] ?? day.eventId,
      menuTitle: menuMap[day.menuId] ?? day.menuId,
      title: day.title,
      description: day.description ?? 'Без описания',
      sequenceNumber: day.sequenceNumber,
    };
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
      return 'Выполните вход, чтобы просматривать дни мероприятий.';
    }

    return 'Не удалось загрузить список дней.';
  }

  private extractStatusCode(error: unknown): number | null {
    if (typeof error !== 'object' || error === null || !('status' in error)) {
      return null;
    }

    const { status } = error as { status?: unknown };
    return typeof status === 'number' ? status : null;
  }
}
