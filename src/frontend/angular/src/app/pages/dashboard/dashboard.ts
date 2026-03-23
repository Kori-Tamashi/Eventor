import { Component, computed, signal } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import {Button, ButtonModule} from 'primeng/button';
import { TableModule } from 'primeng/table';

type DashboardRow = {
  name: string;
  description: string;
  date: string;
  rating: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [InputTextModule, ButtonModule, TableModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {

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


  readonly totalPages = computed(() => {
    const total = this.allRows().length;
    const size = this.pageSize();
    return Math.max(1, Math.ceil(total / size));
  });

  readonly pageLabel = computed(() => `${this.pageIndex() + 1} из ${this.totalPages()}`);

  readonly pagedRows = computed(() => {
    const size = this.pageSize();
    const start = this.pageIndex() * size;
    return this.allRows().slice(start, start + size);
  });

  readonly canPrev = computed(() => this.pageIndex() > 0);
  readonly canNext = computed(() => this.pageIndex() < this.totalPages() - 1);

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

}
