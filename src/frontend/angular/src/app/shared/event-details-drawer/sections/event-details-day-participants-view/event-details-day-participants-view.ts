import { Component, input } from '@angular/core';
import { TableModule } from 'primeng/table';
import { DrawerCard } from '../../../drawer-card/drawer-card';
import {
  EventDetailsDrawerParticipant,
  EventDetailsDrawerParticipantPayment,
} from '../../event-details-drawer.models';

@Component({
  selector: 'app-event-details-day-participants-view',
  standalone: true,
  imports: [TableModule, DrawerCard],
  templateUrl: './event-details-day-participants-view.html',
  styleUrl: './event-details-day-participants-view.scss',
})
export class EventDetailsDayParticipantsView {
  readonly participants = input.required<EventDetailsDrawerParticipant[]>();

  paymentBadgeClass(payment: EventDetailsDrawerParticipantPayment): string {
    return payment === 'Оплачено'
      ? 'ui-status-badge ui-status-badge--paid'
      : 'ui-status-badge ui-status-badge--unpaid';
  }
}
