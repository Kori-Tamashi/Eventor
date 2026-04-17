import { Component, computed, signal } from '@angular/core';
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
  readonly cost = signal<number>(62050);
  readonly price = signal<number>(13321.55);
  readonly privilegedPrice = signal<number>(16911.66);
  readonly participantsCount = signal<number>(5);
  readonly coefficient = signal<number>(1.01);

  readonly metrics = computed<EventManagementDayAnalyticsMetric[]>(() => [
    { label: 'Стоимость', value: this.formatNumber(this.cost()) },
    { label: 'Цена', value: this.formatNumber(this.price()) },
    {
      label: 'Цена (с привилегиями)',
      value: this.formatNumber(this.privilegedPrice()),
    },
    {
      label: 'Количество участников',
      value: this.formatNumber(this.participantsCount()),
    },
    { label: 'Коэффициент', value: this.coefficient().toFixed(2) },
  ]);

  private formatNumber(value: number): string {
    return Number.isInteger(value) ? String(value) : value.toFixed(2);
  }
}
