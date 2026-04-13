import { Injectable, Injector, computed, inject, signal } from '@angular/core';
import { Subscription, finalize } from 'rxjs';
import {
  EventManagementDrawerDataService,
  EventManagementDayData,
  EventManagementFormData,
  EventManagementLocationOption,
} from './event-management-drawer-data.service';

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
  eventId?: string | null;
};

export type EventManagementDrawerDayRow = EventManagementDayData;

export type EventManagementDrawerReview = {
  person: string;
  comment: string;
  rating: number;
};

@Injectable({
  providedIn: 'root',
})
export class EventManagementDrawerStore {
  private readonly injector = inject(Injector);

  private loadSubscription: Subscription | null = null;
  private saveEventSubscription: Subscription | null = null;
  private saveDaySubscription: Subscription | null = null;

  readonly currentUsername = signal<string>('Username');
  readonly currentUserId = signal<string>('');

  readonly isOpen = signal<boolean>(false);
  readonly isLoading = signal<boolean>(false);
  readonly isSavingSettings = signal<boolean>(false);
  readonly isSavingDay = signal<boolean>(false);
  readonly errorMessage = signal<string>('');
  readonly successMessage = signal<string>('');
  readonly context = signal<EventManagementDrawerContext | null>(null);
  readonly activeTab = signal<EventManagementDrawerTab>('settings');

  readonly eventId = signal<string | null>(null);
  readonly eventTitle = signal<string>('');
  readonly eventDescription = signal<string>('');
  readonly eventLocationId = signal<string>('');
  readonly eventStartDate = signal<string>('');
  readonly settingsDaysCount = signal<string>('1');
  readonly eventMarkup = signal<string>('0');
  readonly locationOptions = signal<EventManagementLocationOption[]>([]);
  readonly dayRowsState = signal<EventManagementDrawerDayRow[]>([]);
  readonly initialFormData = signal<EventManagementFormData | null>(null);

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
    return this.eventTitle().trim() || this.context()?.title || 'Создать мероприятие';
  });

  readonly dayDetailsOpen = computed(() => this.selectedDay() !== null);

  readonly dayDetailsTitle = computed(() => {
    return this.selectedDay()?.title ?? 'Название дня';
  });

  readonly reviewCountLabel = computed(() => `${this.reviewText().length}/${this.reviewMaxLen}`);

  readonly canSaveReview = computed(() => {
    return this.reviewText().trim().length > 0 && this.reviewRating() > 0;
  });
  readonly dayRows = computed(() => this.dayRowsState());

  open(context: EventManagementDrawerContext): void {
    this.cancelOngoingRequests();
    this.resetState();
    this.context.set(context);
    this.activeTab.set('settings');
    this.reviewText.set('');
    this.reviewRating.set(0);
    this.reviewRows.set(this.buildInitialReviewRows());
    this.isLoading.set(true);
    this.isOpen.set(true);

    const load$ = context.eventId
      ? this.dataService.loadEventData(context.eventId)
      : this.dataService.loadCreateData();

    this.loadSubscription = load$
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (data) => {
          this.applyLoadedData(data);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapLoadErrorMessage(error));
        },
      });
  }

  close(): void {
    this.cancelOngoingRequests();
    this.resetState();
    this.isOpen.set(false);
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
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  setSettingsDaysCount(value: string): void {
    const normalized = value.replace(/\D/g, '').slice(0, 2);
    this.settingsDaysCount.set(normalized);
  }

  setEventTitle(value: string): void {
    this.eventTitle.set(value);
  }

  setEventDescription(value: string): void {
    this.eventDescription.set(value);
  }

  setEventLocationId(value: string): void {
    this.eventLocationId.set(value);
  }

  setEventStartDate(value: string): void {
    this.eventStartDate.set(value);
  }

  setEventMarkup(value: string): void {
    this.eventMarkup.set(value);
  }

  cancelSettings(): void {
    const initial = this.initialFormData();

    if (!initial) {
      return;
    }

    this.applyLoadedData(initial);
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  saveSettings(): void {
    const validationError = this.validateEventForm();

    if (validationError) {
      this.errorMessage.set(validationError);
      this.successMessage.set('');
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.isSavingSettings.set(true);
    this.saveEventSubscription?.unsubscribe();
    this.saveEventSubscription = this.dataService.saveEvent({
      eventId: this.eventId(),
      currentUserId: this.currentUserId(),
      title: this.eventTitle().trim(),
      description: this.eventDescription().trim(),
      locationId: this.eventLocationId(),
      startDate: this.eventStartDate(),
      daysCount: Number(this.settingsDaysCount()),
      markup: Number(this.eventMarkup().replace(',', '.')),
    })
      .pipe(finalize(() => this.isSavingSettings.set(false)))
      .subscribe({
        next: (data) => {
          this.applyLoadedData(data);
          this.context.update((context) => {
            if (!context) {
              return null;
            }

            return {
              ...context,
              title: data.title,
              eventId: data.eventId,
            };
          });
          this.successMessage.set('Настройки мероприятия сохранены.');
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapSaveEventErrorMessage(error));
        },
      });
  }

  openDayDetails(day: EventManagementDrawerDayRow): void {
    this.selectedDay.set(day);
    this.dayDetailsActiveTab.set('settings');
    this.errorMessage.set('');
    this.successMessage.set('');
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

  saveSelectedDay(payload: { title: string; description: string }): void {
    const selectedDay = this.selectedDay();
    const eventId = this.eventId();

    if (!selectedDay || !eventId) {
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.isSavingDay.set(true);
    this.saveDaySubscription?.unsubscribe();
    this.saveDaySubscription = this.dataService.saveDay({
      eventId,
      dayId: selectedDay.id,
      title: payload.title,
      description: payload.description,
      menuId: selectedDay.menuId,
      sequenceNumber: selectedDay.number,
    })
      .pipe(finalize(() => this.isSavingDay.set(false)))
      .subscribe({
        next: (data) => {
          const selectedDayId = selectedDay.id;
          this.applyLoadedData(data);
          const refreshedDay = this.dayRowsState().find((day) => day.id === selectedDayId) ?? null;
          this.selectedDay.set(refreshedDay);
          this.successMessage.set('Настройки дня сохранены.');
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapSaveDayErrorMessage(error));
        },
      });
  }

  cancelSelectedDay(): void {
    const selectedDay = this.selectedDay();

    if (!selectedDay) {
      return;
    }

    const currentDay = this.dayRowsState().find((day) => day.id === selectedDay.id) ?? null;
    this.selectedDay.set(currentDay);
    this.errorMessage.set('');
    this.successMessage.set('');
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

  private get dataService(): EventManagementDrawerDataService {
    return this.injector.get(EventManagementDrawerDataService);
  }

  private applyLoadedData(data: EventManagementFormData): void {
    this.currentUserId.set(data.currentUserId);
    this.currentUsername.set(data.currentUsername);
    this.eventId.set(data.eventId);
    this.eventTitle.set(data.title);
    this.eventDescription.set(data.description);
    this.eventLocationId.set(data.locationId);
    this.eventStartDate.set(data.startDate);
    this.settingsDaysCount.set(data.daysCount);
    this.eventMarkup.set(data.markup);
    this.locationOptions.set(data.locations);
    this.dayRowsState.set(data.dayRows);
    this.initialFormData.set(data);
  }

  private resetState(): void {
    this.context.set(null);
    this.activeTab.set('settings');
    this.currentUserId.set('');
    this.currentUsername.set('Username');
    this.eventId.set(null);
    this.eventTitle.set('');
    this.eventDescription.set('');
    this.eventLocationId.set('');
    this.eventStartDate.set('');
    this.settingsDaysCount.set('1');
    this.eventMarkup.set('0');
    this.locationOptions.set([]);
    this.dayRowsState.set([]);
    this.initialFormData.set(null);
    this.selectedDay.set(null);
    this.dayDetailsActiveTab.set('settings');
    this.reviewText.set('');
    this.reviewRating.set(0);
    this.reviewRows.set([]);
    this.isLoading.set(false);
    this.isSavingSettings.set(false);
    this.isSavingDay.set(false);
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  private cancelOngoingRequests(): void {
    this.loadSubscription?.unsubscribe();
    this.saveEventSubscription?.unsubscribe();
    this.saveDaySubscription?.unsubscribe();
    this.loadSubscription = null;
    this.saveEventSubscription = null;
    this.saveDaySubscription = null;
  }

  private validateEventForm(): string | null {
    if (!this.eventTitle().trim()) {
      return 'Укажите название мероприятия.';
    }

    if (!this.eventLocationId()) {
      return 'Выберите локацию.';
    }

    if (!this.eventStartDate()) {
      return 'Укажите дату мероприятия.';
    }

    const daysCount = Number(this.settingsDaysCount());

    if (!Number.isInteger(daysCount) || daysCount <= 0) {
      return 'Количество дней должно быть больше нуля.';
    }

    const markup = Number(this.eventMarkup().replace(',', '.'));

    if (!Number.isFinite(markup) || markup < 0) {
      return 'Наценка должна быть числом не меньше нуля.';
    }

    return null;
  }

  private mapLoadErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 401) {
      return 'Выполните вход, чтобы управлять мероприятием.';
    }

    return 'Не удалось загрузить данные мероприятия.';
  }

  private mapSaveEventErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 401) {
      return 'Выполните вход, чтобы сохранить мероприятие.';
    }

    if (status === 404) {
      return 'Связанные данные мероприятия не найдены.';
    }

    return 'Не удалось сохранить настройки мероприятия.';
  }

  private mapSaveDayErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 401) {
      return 'Выполните вход, чтобы сохранить день.';
    }

    if (status === 404) {
      return 'День мероприятия не найден.';
    }

    return 'Не удалось сохранить настройки дня.';
  }

  private extractStatusCode(error: unknown): number | null {
    if (typeof error !== 'object' || error === null || !('status' in error)) {
      return null;
    }

    const status = (error as { status?: unknown }).status;
    return typeof status === 'number' ? status : null;
  }
}
