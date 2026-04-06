import { Component, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { TabsModule } from 'primeng/tabs';
import {
  EventManagementDrawerStore,
  EventManagementDrawerDayDetailsTab,
  EventManagementDrawerDayRow,
  EventManagementDrawerTab,
} from '../../core/ui/event-management-drawer.store';
import { EventManagementSettingsTab } from './sections/event-management-settings-tab/event-management-settings-tab';
import { EventManagementDaysTab } from './sections/event-management-days-tab/event-management-days-tab';
import { EventManagementDayDetailsView } from './sections/event-management-day-details-view/event-management-day-details-view';

@Component({
  selector: 'app-event-management-drawer',
  standalone: true,
  imports: [
    ButtonModule,
    DrawerModule,
    TabsModule,
    EventManagementSettingsTab,
    EventManagementDaysTab,
    EventManagementDayDetailsView,
  ],
  templateUrl: './event-management-drawer.html',
  styleUrl: './event-management-drawer.scss',
})
export class EventManagementDrawer {
  readonly store = inject(EventManagementDrawerStore);

  readonly drawerOpen = this.store.isOpen;
  readonly title = this.store.title;
  readonly activeTab = this.store.activeTab;
  readonly settingsDaysCount = this.store.settingsDaysCount;
  readonly dayRows = this.store.dayRows;
  readonly mode = this.store.mode;

  readonly dayDetailsOpen = this.store.dayDetailsOpen;
  readonly dayDetailsTitle = this.store.dayDetailsTitle;
  readonly dayDetailsActiveTab = this.store.dayDetailsActiveTab;

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

  openDayDetails(day: EventManagementDrawerDayRow): void {
    this.store.openDayDetails(day);
  }

  closeDayDetails(): void {
    this.store.closeDayDetails();
  }

  onDayDetailsTabChange(tab: EventManagementDrawerDayDetailsTab): void {
    this.store.setDayDetailsActiveTab(tab);
  }
}
