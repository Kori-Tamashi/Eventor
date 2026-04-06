import { Component, input, output } from '@angular/core';
import { TabsModule } from 'primeng/tabs';
import { EventManagementDrawerDayDetailsTab } from '../../../../core/ui/event-management-drawer.store';

@Component({
  selector: 'app-event-management-day-details-view',
  standalone: true,
  imports: [TabsModule],
  templateUrl: './event-management-day-details-view.html',
  styleUrl: './event-management-day-details-view.scss',
})
export class EventManagementDayDetailsView {
  readonly activeTab = input.required<EventManagementDrawerDayDetailsTab>();

  readonly tabChange = output<EventManagementDrawerDayDetailsTab>();

  onTabValueChange(value: string | number | undefined): void {
    if (
      value === 'settings' ||
      value === 'participants' ||
      value === 'menu' ||
      value === 'analytics'
    ) {
      this.tabChange.emit(value);
    }
  }
}
