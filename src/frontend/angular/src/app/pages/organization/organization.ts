import {Component, computed, signal} from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DrawerModule } from 'primeng/drawer';
import { TabsModule } from 'primeng/tabs';
import { DrawerCard } from '../../shared/drawer-card/drawer-card';

type OrganizationEventRow = {
  name: string;
  location: string;
  description: string;
  date: string;
  peopleCount: number;
  daysCount: number;
  markup: number;
  rating: number;
};

@Component({
  selector: 'app-organization',
  standalone: true,
  imports: [InputTextModule, ButtonModule, TableModule, DrawerModule, TabsModule, DrawerCard],
  templateUrl: './organization.html',
  styleUrl: './organization.scss',
})
export class Organization {
  readonly pageSize = signal<number>(10);
  readonly pageIndex = signal<number>(0);

  readonly allRows = signal<OrganizationEventRow[]>([
    { name: 'Название', location: 'Локация', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, markup: 0, rating: 0 },
    { name: 'Название', location: 'Локация', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, markup: 0, rating: 0 },
    { name: 'Название', location: 'Локация', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, markup: 0, rating: 0 },
    { name: 'Название', location: 'Локация', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, markup: 0, rating: 0 },
    { name: 'Название', location: 'Локация', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, markup: 0, rating: 0 },
    { name: 'Название', location: 'Локация', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, markup: 0, rating: 0 },
    { name: 'Название', location: 'Локация', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, markup: 0, rating: 0 },
    { name: 'Название', location: 'Локация', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, markup: 0, rating: 0 },
    { name: 'Название', location: 'Локация', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, markup: 0, rating: 0 },
    { name: 'Название', location: 'Локация', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, markup: 0, rating: 0 },
    { name: 'Название', location: 'Локация', description: 'Описание', date: 'dd.mm.yyyy', peopleCount: 0, daysCount: 0, markup: 0, rating: 0 },
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
    if (!this.canPrev())
      return;
    this.pageIndex.update((v) => v - 1);
  }

  nextPage(): void {
    if (!this.canNext())
      return;
    this.pageIndex.update((v) => v + 1);
  }

  readonly drawerOpen = signal<boolean>(false);
  readonly selectedRow = signal<OrganizationEventRow | null>(null);

  openDetails(row: OrganizationEventRow): void {
    this.selectedRow.set(row);
    this.drawerOpen.set(true);
  }

  closeDetails(): void {
    this.drawerOpen.set(false);
  }
}
