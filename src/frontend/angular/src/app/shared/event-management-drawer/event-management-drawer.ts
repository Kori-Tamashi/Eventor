import { Component, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { TabsModule } from 'primeng/tabs';
import {
  EventManagementDrawerStore,
  EventManagementDrawerTab,
} from '../../core/ui/event-management-drawer.store';

@Component({
  selector: 'app-event-management-drawer',
  standalone: true,
  imports: [
    ButtonModule,
    DrawerModule,
    TabsModule,
  ],
  templateUrl: './event-management-drawer.html',
  styleUrl: './event-management-drawer.scss',
})
export class EventManagementDrawer {
  readonly store = inject(EventManagementDrawerStore);

  readonly drawerOpen = this.store.isOpen;
  readonly title = this.store.title;
  readonly activeTab = this.store.activeTab;

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
}
