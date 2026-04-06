import { Component, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DrawerCard } from '../../../drawer-card/drawer-card';
import { EventDetailsDrawerDay } from '../../event-details-drawer.models';

@Component({
  selector: 'app-event-details-info-tab',
  standalone: true,
  imports: [ButtonModule, TableModule, DrawerCard],
  templateUrl: './event-details-info-tab.html',
  styleUrl: './event-details-info-tab.scss',
})
export class EventDetailsInfoTab {
  readonly daysCount = input.required<number>();
  readonly participantsCount = input.required<number>();
  readonly dayRows = input.required<EventDetailsDrawerDay[]>();

  readonly dayParticipantsOpen = output<EventDetailsDrawerDay>();

  openDayParticipants(day: EventDetailsDrawerDay): void {
    this.dayParticipantsOpen.emit(day);
  }
}
