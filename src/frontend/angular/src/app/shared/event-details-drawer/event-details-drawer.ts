import { Component, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { TabsModule } from 'primeng/tabs';
import { EventDetailsDrawerStore } from '../../core/ui/event-details-drawer.store';
import { EventDetailsDrawerDay } from './event-details-drawer.models';
import { EventDetailsInfoTab } from './sections/event-details-info-tab/event-details-info-tab';
import { EventDetailsReviewsTab } from './sections/event-details-reviews-tab/event-details-reviews-tab';
import { EventDetailsDayParticipantsView } from './sections/event-details-day-participants-view/event-details-day-participants-view';

@Component({
  selector: 'app-event-details-drawer',
  standalone: true,
  imports: [
    ButtonModule,
    DrawerModule,
    TabsModule,
    EventDetailsInfoTab,
    EventDetailsReviewsTab,
    EventDetailsDayParticipantsView,
  ],
  templateUrl: './event-details-drawer.html',
  styleUrl: './event-details-drawer.scss',
})
export class EventDetailsDrawer {
  readonly store = inject(EventDetailsDrawerStore);

  readonly drawerOpen = this.store.isOpen;
  readonly isLoading = this.store.isLoading;
  readonly errorMessage = this.store.errorMessage;
  readonly dayParticipantsOpen = this.store.dayParticipantsOpen;
  readonly drawerTitle = this.store.drawerTitle;
  readonly parentTitle = this.store.parentTitle;
  readonly daysCount = this.store.daysCount;
  readonly participantsCount = this.store.participantsCount;
  readonly dayRows = this.store.dayRows;
  readonly currentUserId = this.store.currentUserId;
  readonly currentUserRegistrationId = this.store.currentUserRegistrationId;
  readonly currentUserRegistrationPayment = this.store.currentUserRegistrationPayment;
  readonly registrationType = this.store.registrationType;
  readonly registrationDayIds = this.store.registrationDayIds;
  readonly canSaveRegistration = this.store.canSaveRegistration;
  readonly isSavingRegistration = this.store.isSavingRegistration;
  readonly registrationMessage = this.store.registrationMessage;
  readonly selectedDayParticipants = this.store.selectedDayParticipants;
  readonly reviewText = this.store.reviewText;
  readonly reviewRating = this.store.reviewRating;
  readonly reviewRows = this.store.reviewRows;
  readonly reviewCountLabel = this.store.reviewCountLabel;
  readonly canSaveReview = this.store.canSaveReview;

  onDrawerVisibleChange(visible: boolean): void {
    this.store.onDrawerVisibleChange(visible);
  }

  close(): void {
    this.store.close();
  }

  openDayParticipants(day: EventDetailsDrawerDay): void {
    this.store.openDayParticipants(day);
  }

  onRegistrationTypeChange(value: 0 | 1 | 2): void {
    this.store.setRegistrationType(value);
  }

  onRegistrationDayToggle(event: { dayId: string; checked: boolean }): void {
    this.store.toggleRegistrationDay(event.dayId, event.checked);
  }

  saveRegistration(): void {
    this.store.saveRegistration();
  }

  deleteRegistration(): void {
    this.store.deleteRegistration();
  }

  closeDayParticipants(): void {
    this.store.closeDayParticipants();
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
}
