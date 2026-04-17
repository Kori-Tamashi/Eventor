import { Injectable, computed, signal } from '@angular/core';

export type EventManagementDrawerMode = 'create' | 'superuser-manage';
export type EventManagementDrawerSource = 'organization' | 'superuser-page';
export type EventManagementDrawerViewerRole = 'user' | 'superuser';
export type EventManagementDrawerTab = 'settings' | 'days' | 'analytics' | 'reviews';
export type EventManagementDrawerDayDetailsTab = 'settings' | 'participants' | 'menu' | 'analytics';

export type EventManagementDrawerContext = {
  mode: EventManagementDrawerMode;
  source: EventManagementDrawerSource;
  viewerRole: EventManagementDrawerViewerRole;
  title: string;
};

export type EventManagementDrawerDayRow = {
  number: number;
  title: string;
  price: string;
  participantsCount: string;
};

export type EventManagementDrawerReview = {
  person: string;
  comment: string;
  rating: number;
};

@Injectable({
  providedIn: 'root',
})
export class EventManagementDrawerStore {
  readonly currentUsername = signal<string>('Username');

  readonly isOpen = signal<boolean>(false);
  readonly context = signal<EventManagementDrawerContext | null>(null);
  readonly activeTab = signal<EventManagementDrawerTab>('settings');

  readonly settingsDaysCount = signal<string>('5');

  readonly selectedDay = signal<EventManagementDrawerDayRow | null>(null);
  readonly dayDetailsActiveTab = signal<EventManagementDrawerDayDetailsTab>('settings');

  readonly reviewMaxLen = 250;
  readonly reviewText = signal<string>('');
  readonly reviewRating = signal<number>(0);
  readonly reviewRows = signal<EventManagementDrawerReview[]>([]);

  readonly mode = computed(() => this.context()?.mode ?? 'create');
  readonly source = computed(() => this.context()?.source ?? 'organization');
  readonly viewerRole = computed(() => this.context()?.viewerRole ?? 'user');

  readonly title = computed(() => {
    const context = this.context();

    if (!context) {
      return 'Создать мероприятие';
    }

    return context.title;
  });

  readonly dayDetailsOpen = computed(() => this.selectedDay() !== null);

  readonly dayDetailsTitle = computed(() => {
    return this.selectedDay()?.title ?? 'Название дня';
  });

  readonly reviewCountLabel = computed(() => `${this.reviewText().length}/${this.reviewMaxLen}`);

  readonly canSaveReview = computed(() => {
    return this.reviewText().trim().length > 0 && this.reviewRating() > 0;
  });

  readonly normalizedDaysCount = computed(() => {
    const raw = this.settingsDaysCount().trim();
    const parsed = Number(raw);

    if (!Number.isInteger(parsed) || parsed <= 0) {
      return 0;
    }

    return parsed;
  });

  readonly dayRows = computed<EventManagementDrawerDayRow[]>(() => {
    return Array.from({ length: this.normalizedDaysCount() }, (_, index) => ({
      number: index + 1,
      title: 'Name',
      price: 'N',
      participantsCount: 'N',
    }));
  });

  open(context: EventManagementDrawerContext): void {
    this.context.set(context);
    this.activeTab.set('settings');
    this.settingsDaysCount.set('5');
    this.selectedDay.set(null);
    this.dayDetailsActiveTab.set('settings');
    this.reviewText.set('');
    this.reviewRating.set(0);
    this.reviewRows.set(this.buildInitialReviewRows());
    this.isOpen.set(true);
  }

  close(): void {
    this.isOpen.set(false);
    this.context.set(null);
    this.activeTab.set('settings');
    this.settingsDaysCount.set('5');
    this.selectedDay.set(null);
    this.dayDetailsActiveTab.set('settings');
    this.reviewText.set('');
    this.reviewRating.set(0);
    this.reviewRows.set([]);
  }

  onDrawerVisibleChange(visible: boolean): void {
    if (!visible) {
      this.close();
      return;
    }

    this.isOpen.set(true);
  }

  setActiveTab(tab: EventManagementDrawerTab): void {
    this.activeTab.set(tab);
  }

  setSettingsDaysCount(value: string): void {
    const normalized = value.replace(/\D/g, '').slice(0, 2);
    this.settingsDaysCount.set(normalized);
  }

  openDayDetails(day: EventManagementDrawerDayRow): void {
    this.selectedDay.set(day);
    this.dayDetailsActiveTab.set('settings');
  }

  closeDayDetails(): void {
    this.selectedDay.set(null);
    this.dayDetailsActiveTab.set('settings');
  }

  setDayDetailsActiveTab(tab: EventManagementDrawerDayDetailsTab): void {
    this.dayDetailsActiveTab.set(tab);
  }

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

    const newReview: EventManagementDrawerReview = {
      person: this.currentUsername(),
      comment: this.reviewText().trim(),
      rating: this.reviewRating(),
    };

    this.reviewRows.update((rows) => [newReview, ...rows]);
    this.cancelReview();
  }

  private buildInitialReviewRows(): EventManagementDrawerReview[] {
    return [
      {
        person: 'Александр',
        comment: 'Хорошая организация, всё прошло по плану.',
        rating: 5,
      },
      {
        person: 'Мария',
        comment: 'Понравилась программа и общее сопровождение мероприятия.',
        rating: 4,
      },
      {
        person: 'Иван',
        comment: 'Удобная логистика и понятное расписание.',
        rating: 5,
      },
    ];
  }
}
