import { Component, computed, input } from '@angular/core';
import { DrawerCard } from '../../../drawer-card/drawer-card';

type EventManagementDayAnalyticsMetric = {
  label: string;
  value: string;
};

@Component({
  selector: 'app-event-management-day-analytics-tab',
  standalone: true,
  imports: [DrawerCard],
  templateUrl: './event-management-day-analytics-tab.html',
  styleUrl: './event-management-day-analytics-tab.scss',
})
export class EventManagementDayAnalyticsTab {
  readonly menuCost = input<number | null>(null);
  readonly dayPrice = input<number | null>(null);
  readonly dayPriceWithPrivileges = input<number | null>(null);
  readonly participantsCount = input.required<number>();

  readonly metrics = computed<EventManagementDayAnalyticsMetric[]>(() => {
    const menuCost = this.menuCost();
    const dayPrice = this.dayPrice();
    const dayPriceWithPrivileges = this.dayPriceWithPrivileges();
    const participantsCount = this.participantsCount();

    const coefficient =
      dayPrice !== null && dayPriceWithPrivileges !== null && dayPrice !== 0
        ? dayPriceWithPrivileges / dayPrice
        : null;

    return [
      { label: 'Стоимость', value: this.formatNullable(menuCost) },
      { label: 'Цена', value: this.formatNullable(dayPrice) },
      { label: 'Цена (с привилегиями)', value: this.formatNullable(dayPriceWithPrivileges) },
      { label: 'Количество участников', value: String(participantsCount) },
      { label: 'Коэффициент', value: coefficient !== null ? coefficient.toFixed(2) : 'N/A' },
    ];
  });

  private formatNullable(value: number | null): string {
    if (value === null) {
      return 'N/A';
    }

    return this.formatNumber(value);
  }

  private formatNumber(value: number): string {
    return Number.isInteger(value) ? String(value) : value.toFixed(2);
  }
}
