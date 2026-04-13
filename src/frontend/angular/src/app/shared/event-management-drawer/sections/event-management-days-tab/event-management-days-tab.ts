import { Component, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DrawerCard } from '../../../drawer-card/drawer-card';
import { EventManagementDrawerDayRow } from '../../../../core/ui/event-management-drawer.store';

@Component({
  selector: 'app-event-management-days-tab',
  standalone: true,
  imports: [ButtonModule, TableModule, DrawerCard],
  templateUrl: './event-management-days-tab.html',
  styleUrl: './event-management-days-tab.scss',
})
export class EventManagementDaysTab {
  readonly dayRows = input.required<EventManagementDrawerDayRow[]>();

  readonly dayDetailsOpen = output<EventManagementDrawerDayRow>();

  openDayDetails(day: EventManagementDrawerDayRow): void {
    this.dayDetailsOpen.emit(day);
  }
}
