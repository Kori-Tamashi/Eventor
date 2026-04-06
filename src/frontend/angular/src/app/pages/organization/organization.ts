import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DrawerModule } from 'primeng/drawer';
import { TabsModule } from 'primeng/tabs';
import { TextareaModule } from 'primeng/textarea';
import { RatingModule } from 'primeng/rating';
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

type DayRow = {
  name: string;
  price: string;
  participants: number;
};

type ReviewRow = {
  person: string;
  comment: string;
  rating: number;
};

@Component({
  selector: 'app-organization',
  standalone: true,
  imports: [
    FormsModule,
    InputTextModule,
    ButtonModule,
    TableModule,
    DrawerModule,
    TabsModule,
    TextareaModule,
    RatingModule,
    DrawerCard
  ],
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

  readonly dayRows = signal<DayRow[]>([
    { name: 'День 1 - Название', price: 'N', participants: 4 },
    { name: 'День 2 - Название', price: 'N', participants: 4 },
    { name: 'День 3 - Название', price: 'N', participants: 4 },
  ]);

  readonly reviewMaxLen = 250;

  readonly currentUsername = signal<string>('Username');

  readonly reviewText = signal<string>('');
  readonly reviewRating = signal<number>(0);

  readonly reviewCountLabel = computed(() => `${this.reviewText().length}/${this.reviewMaxLen}`);

  readonly canSaveReview = computed(() => {
    return this.reviewText().trim().length > 0 && this.reviewRating() > 0;
  });

  readonly reviewRows = signal<ReviewRow[]>([
    { person: 'Александр', comment: 'Хорошая организация, всё прошло по плану.', rating: 5 },
    { person: 'Мария', comment: 'Понравилась программа и общее сопровождение мероприятия.', rating: 4 },
    { person: 'Иван', comment: 'Удобная логистика и понятное расписание.', rating: 5 },
  ]);

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

    const newReview: ReviewRow = {
      person: this.currentUsername(),
      comment: this.reviewText().trim(),
      rating: this.reviewRating(),
    };

    this.reviewRows.update((rows) => [newReview, ...rows]);
    this.cancelReview();
  }

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
