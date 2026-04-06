import { Component, computed, inject, signal } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { EventDetailsDrawerStore } from '../../core/ui/event-details-drawer.store';
import { buildMockEventDetailsDrawerContext } from '../../shared/event-details-drawer/event-details-drawer.mock';

type DashboardRow = {
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
  private readonly eventDetailsDrawerStore = inject(EventDetailsDrawerStore);

  readonly pageSize = signal<number>(9);
  readonly pageIndex = signal<number>(0);

  readonly allRows = signal<DashboardRow[]>([
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', rating: 0 },
    { name: 'Название', description: 'Описание', date: 'dd.mm.yyyy', rating: 0 },
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

  openDetails(row: DashboardRow): void {
    this.eventDetailsDrawerStore.open(
      buildMockEventDetailsDrawerContext(row.name, 'dashboard', 'user')
    );
  }
}
