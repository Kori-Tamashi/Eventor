import { Component, computed, inject, signal } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { EventDetailsDrawerStore } from '../../core/ui/event-details-drawer.store';
import { buildMockEventDetailsDrawerContext } from '../../shared/event-details-drawer/event-details-drawer.mock';

type EventsRow = {
  name: string;
  description: string;
  date: string;
  peopleCount: number;
  daysCount: number;
  rating: number;
};

@Component({
  selector: 'app-events',
  standalone: true,
  imports: [InputTextModule, ButtonModule, TableModule],
  templateUrl: './events.html',
  styleUrl: './events.scss',
})
export class Events {
  private readonly eventDetailsDrawerStore = inject(EventDetailsDrawerStore);

  readonly pageSize = signal<number>(10);
  readonly pageIndex = signal<number>(0);

  readonly allRows = signal<EventsRow[]>([
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, rating: 0 },
  ]);

  private readonly paginationState = computed(() => {
    const all = this.allRows();
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

  openDetails(row: EventsRow): void {
    this.eventDetailsDrawerStore.open(
      buildMockEventDetailsDrawerContext(row.name, 'events', 'user')
    );
  }
}
