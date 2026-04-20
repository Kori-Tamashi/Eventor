import { Component, computed, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import {
  EventManagementDrawerStore,
  EventManagementDrawerDayDetailsTab,
  EventManagementDrawerDayRow,
  EventManagementDrawerTab,
} from '../../core/ui/event-management-drawer.store';
import { EventManagementParticipantRow, MenuItemApiModel } from '../../core/ui/event-management-drawer-data.service';
import { EventDetailsReviewsTab } from '../event-details-drawer/sections/event-details-reviews-tab/event-details-reviews-tab';
import { EventManagementAnalyticsTab } from './sections/event-management-analytics-tab/event-management-analytics-tab';
import { EventManagementSettingsTab } from './sections/event-management-settings-tab/event-management-settings-tab';
import { EventManagementDaysTab } from './sections/event-management-days-tab/event-management-days-tab';
import { EventManagementDayDetailsView } from './sections/event-management-day-details-view/event-management-day-details-view';

@Component({
  selector: 'app-event-management-drawer',
  standalone: true,
  imports: [
    ButtonModule,
    DrawerModule,
    EventManagementSettingsTab,
    EventManagementDaysTab,
    EventManagementAnalyticsTab,
    EventManagementDayDetailsView,
    EventDetailsReviewsTab,
  ],
  templateUrl: './event-management-drawer.html',
  styleUrl: './event-management-drawer.scss',
})
export class EventManagementDrawer {
  readonly store = inject(EventManagementDrawerStore);

  readonly drawerOpen = this.store.isOpen;
  readonly isLoading = this.store.isLoading;
  readonly errorMessage = this.store.errorMessage;
  readonly successMessage = this.store.successMessage;
  readonly title = this.store.title;
  readonly activeTab = this.store.activeTab;
  readonly eventTitle = this.store.eventTitle;
  readonly eventDescription = this.store.eventDescription;
  readonly eventLocationId = this.store.eventLocationId;
  readonly eventStartDate = this.store.eventStartDate;
  readonly settingsDaysCount = this.store.settingsDaysCount;
  readonly eventMarkup = this.store.eventMarkup;
  readonly locationOptions = this.store.locationOptions;
  readonly isSavingSettings = this.store.isSavingSettings;
  readonly dayRows = this.store.dayRows;
  readonly mode = this.store.mode;

  readonly dayDetailsOpen = this.store.dayDetailsOpen;
  readonly dayDetailsTitle = this.store.dayDetailsTitle;
  readonly selectedDay = this.store.selectedDay;
  readonly dayDetailsActiveTab = this.store.dayDetailsActiveTab;
  readonly isSavingDay = this.store.isSavingDay;

  readonly reviewText = this.store.reviewText;
  readonly reviewRating = this.store.reviewRating;
  readonly reviewRows = this.store.reviewRows;
  readonly deletingFeedbackId = this.store.deletingFeedbackId;
  readonly reviewCountLabel = this.store.reviewCountLabel;
  readonly canSaveReview = this.store.canSaveReview;

  readonly eventAnalytics = this.store.eventAnalytics;
  readonly eventRating = this.store.eventRating;
  readonly eventPersonCount = this.store.eventPersonCount;
  readonly locationCapacity = this.store.locationCapacity;
  readonly eventDaysCount = computed(() => Number(this.store.settingsDaysCount()) || 0);

  readonly dayAnalytics = this.store.dayAnalytics;
  readonly participantRows = computed(() => this.store.selectedDay()?.participantRows ?? []);
  readonly participantsCount = computed(() => Number(this.store.selectedDay()?.participantsCount ?? '0'));
  readonly menuItems = this.store.menuItems;
  readonly availableItems = this.store.availableItems;
  readonly isLoadingMenuItems = this.store.isLoadingMenuItems;
  readonly isSavingMenuItems = this.store.isSavingMenuItems;
  readonly isSavingParticipants = this.store.isSavingParticipants;

  onDrawerVisibleChange(visible: boolean): void {
    this.store.onDrawerVisibleChange(visible);
  }

  close(): void {
    this.store.close();
  }

  setActiveTab(tab: EventManagementDrawerTab): void {
    this.store.setActiveTab(tab);
  }

  onTabValueChange(value: string | number | undefined): void {
    if (
      value === 'settings' ||
      value === 'days' ||
      value === 'analytics' ||
      value === 'reviews'
    ) {
      this.setActiveTab(value);
    }
  }

  onSettingsDaysCountChange(value: string): void {
    this.store.setSettingsDaysCount(value);
  }

  onEventTitleChange(value: string): void {
    this.store.setEventTitle(value);
  }

  onEventDescriptionChange(value: string): void {
    this.store.setEventDescription(value);
  }

  onEventLocationIdChange(value: string): void {
    this.store.setEventLocationId(value);
  }

  onEventStartDateChange(value: string): void {
    this.store.setEventStartDate(value);
  }

  onEventMarkupChange(value: string): void {
    this.store.setEventMarkup(value);
  }

  cancelSettings(): void {
    this.store.cancelSettings();
  }

  saveSettings(): void {
    this.store.saveSettings();
  }

  openDayDetails(day: EventManagementDrawerDayRow): void {
    this.store.openDayDetails(day);
  }

  closeDayDetails(): void {
    this.store.closeDayDetails();
  }

  onDayDetailsTabChange(tab: EventManagementDrawerDayDetailsTab): void {
    this.store.setDayDetailsActiveTab(tab);
  }

  saveSelectedDay(payload: { title: string; description: string }): void {
    this.store.saveSelectedDay(payload);
  }

  cancelSelectedDay(): void {
    this.store.cancelSelectedDay();
  }

  onReviewInput(value: string): void {
    this.store.onReviewInput(value);
  }

  onReviewRatingChange(value: number): void {
    this.store.onReviewRatingChange(value);
  }

  cancelReview(): void {
    this.store.cancelReview();
  }

  saveReview(): void {
    this.store.saveReview();
  }

  deleteReview(feedbackId: string): void {
    this.store.deleteReview(feedbackId);
  }

  onSaveParticipants(rows: EventManagementParticipantRow[]): void {
    this.store.saveParticipants(rows);
  }

  onCancelParticipants(): void {
    this.store.cancelParticipants();
  }

  onSaveMenuItems(items: MenuItemApiModel[]): void {
    this.store.saveMenuItems(items);
  }

  onCancelMenuItems(): void {
    this.store.cancelMenuItems();
  }
}
