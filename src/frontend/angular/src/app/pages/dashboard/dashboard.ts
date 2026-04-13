import { Component, computed, inject, signal } from '@angular/core';
import { finalize, switchMap } from 'rxjs';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { EventsApiService } from '../../core/api/services/events-api.service';
import { UsersApiService } from '../../core/api/services/users-api.service';
import { EventApiModel } from '../../core/api/models/event.models';
import { EventDetailsDrawerStore } from '../../core/ui/event-details-drawer.store';

type DashboardRow = {
  id: string;
  name: string;
  description: string;
  date: string;
  rating: number;
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
  private readonly eventDetailsDrawerStore = inject(EventDetailsDrawerStore);

  readonly pageSize = signal<number>(9);
  readonly pageIndex = signal<number>(0);
  readonly searchTerm = signal<string>('');
  readonly isLoading = signal<boolean>(true);
  readonly errorMessage = signal<string>('');
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
        row.name.toLowerCase().includes(query) ||
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
      eventId: row.id,
      source: 'dashboard',
      viewerRole: 'user',
      fallbackTitle: row.name,
    });
  }

  private loadDashboardEvents(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.usersApiService.getMe()
      .pipe(
        switchMap((user) => {
          this.currentUsername.set(user.name);
          return this.eventsApiService.listUserEvents(user.id);
        }),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (events) => {
          this.allRows.set(events.map((event) => this.mapEventToRow(event)));
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapErrorMessage(error));
          this.allRows.set([]);
        },
      });
  }

  private mapEventToRow(event: EventApiModel): DashboardRow {
    return {
      id: event.id,
      name: event.title,
      description: event.description ?? 'Без описания',
      date: this.formatDate(event.startDate),
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

  private mapErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 401) {
      return 'Выполните вход, чтобы просматривать ваши мероприятия.';
    }

    return 'Не удалось загрузить последние мероприятия.';
  }

  private extractStatusCode(error: unknown): number | null {
    if (typeof error !== 'object' || error === null || !('status' in error)) {
      return null;
    }

    const status = (error as { status?: unknown }).status;
    return typeof status === 'number' ? status : null;
  }
}
