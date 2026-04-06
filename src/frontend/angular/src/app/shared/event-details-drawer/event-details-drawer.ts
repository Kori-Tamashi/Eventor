import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { RatingModule } from 'primeng/rating';
import { TableModule } from 'primeng/table';
import { TabsModule } from 'primeng/tabs';
import { TextareaModule } from 'primeng/textarea';
import { DrawerCard } from '../drawer-card/drawer-card';
import { EventDetailsDrawerStore } from '../../core/ui/event-details-drawer.store';
import {
  EventDetailsDrawerDay,
  EventDetailsDrawerParticipantPayment
} from './event-details-drawer.models';

@Component({
  selector: 'app-event-details-drawer',
  standalone: true,
  imports: [
    FormsModule,
    ButtonModule,
    DrawerModule,
    RatingModule,
    TableModule,
    TabsModule,
    TextareaModule,
    DrawerCard,
  ],
  templateUrl: './event-details-drawer.html',
  styleUrl: './event-details-drawer.scss',
})
export class EventDetailsDrawer {
  readonly store = inject(EventDetailsDrawerStore);

  readonly drawerOpen = this.store.isOpen;
  readonly dayParticipantsOpen = this.store.dayParticipantsOpen;
  readonly drawerTitle = this.store.drawerTitle;
  readonly parentTitle = this.store.parentTitle;
  readonly daysCount = this.store.daysCount;
  readonly participantsCount = this.store.participantsCount;
  readonly dayRows = this.store.dayRows;
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

  paymentBadgeClass(payment: EventDetailsDrawerParticipantPayment): string {
    return payment === 'Оплачено'
      ? 'ui-status-badge ui-status-badge--paid'
      : 'ui-status-badge ui-status-badge--unpaid';
  }
}
