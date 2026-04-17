import { Component, input, output } from '@angular/core';
import { TabsModule } from 'primeng/tabs';
import { EventManagementDrawerDayDetailsTab } from '../../../../core/ui/event-management-drawer.store';
import { EventManagementDayAnalyticsTab } from '../event-management-day-analytics-tab/event-management-day-analytics-tab';
import { EventManagementDayMenuTab } from '../event-management-day-menu-tab/event-management-day-menu-tab';
import { EventManagementDayParticipantsTab } from '../event-management-day-participants-tab/event-management-day-participants-tab';
import { EventManagementDaySettingsTab } from '../event-management-day-settings-tab/event-management-day-settings-tab';

@Component({
  selector: 'app-event-management-day-details-view',
  standalone: true,
  imports: [
    TabsModule,
    EventManagementDaySettingsTab,
    EventManagementDayParticipantsTab,
    EventManagementDayMenuTab,
    EventManagementDayAnalyticsTab,
  ],
  templateUrl: './event-management-day-details-view.html',
  styleUrl: './event-management-day-details-view.scss',
})
export class EventManagementDayDetailsView {
  readonly activeTab = input.required<EventManagementDrawerDayDetailsTab>();
  readonly dayTitle = input.required<string>();

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
