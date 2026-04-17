import { Component, input, output } from '@angular/core';
import { TabsModule } from 'primeng/tabs';
import { EventManagementDrawerDayDetailsTab, EventManagementDayAnalytics } from '../../../../core/ui/event-management-drawer.store';
import { EventManagementParticipantRow, ItemApiModel, MenuItemApiModel } from '../../../../core/ui/event-management-drawer-data.service';
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
  readonly dayDescription = input<string>('');
  readonly isSaving = input<boolean>(false);
  readonly errorMessage = input<string>('');
  readonly successMessage = input<string>('');

  readonly participantRows = input.required<EventManagementParticipantRow[]>();
  readonly isSavingParticipants = input<boolean>(false);
  readonly participantsCount = input<number>(0);

  readonly menuItems = input.required<MenuItemApiModel[]>();
  readonly availableItems = input.required<ItemApiModel[]>();
  readonly isLoadingMenuItems = input<boolean>(false);
  readonly isSavingMenuItems = input<boolean>(false);

  readonly dayAnalytics = input<EventManagementDayAnalytics | null>(null);

  readonly tabChange = output<EventManagementDrawerDayDetailsTab>();
  readonly saveDay = output<{ title: string; description: string }>();
  readonly cancelDay = output<void>();
  readonly saveParticipants = output<EventManagementParticipantRow[]>();
  readonly cancelParticipants = output<void>();
  readonly saveMenuItems = output<MenuItemApiModel[]>();
  readonly cancelMenuItems = output<void>();

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

  onSaveDay(payload: { title: string; description: string }): void {
    this.saveDay.emit(payload);
  }

  onCancelDay(): void {
    this.cancelDay.emit();
  }

  onSaveParticipants(rows: EventManagementParticipantRow[]): void {
    this.saveParticipants.emit(rows);
  }

  onCancelParticipants(): void {
    this.cancelParticipants.emit();
  }

  onSaveMenuItems(items: MenuItemApiModel[]): void {
    this.saveMenuItems.emit(items);
  }

  onCancelMenuItems(): void {
    this.cancelMenuItems.emit();
  }
}
