import { Component, computed, input } from '@angular/core';
import { DrawerCard } from '../../../drawer-card/drawer-card';
import { EventManagementEventAnalytics } from '../../../../core/ui/event-management-drawer-data.service';

type EventManagementAnalyticsMetric = {
  label: string;
  value: string;
};

@Component({
  selector: 'app-event-management-analytics-tab',
  standalone: true,
  imports: [DrawerCard],
  templateUrl: './event-management-analytics-tab.html',
  styleUrl: './event-management-analytics-tab.scss',
})
export class EventManagementAnalyticsTab {
  readonly eventAnalytics = input.required<EventManagementEventAnalytics>();
  readonly eventRating = input.required<number>();
  readonly eventPersonCount = input.required<number>();
  readonly eventDaysCount = input.required<number>();
  readonly locationCapacity = input.required<number>();

  readonly generalMetrics = computed<EventManagementAnalyticsMetric[]>(() => {
    const analytics = this.eventAnalytics();
    const personCount = this.eventPersonCount();
    const capacity = this.locationCapacity();
    const daysCount = this.eventDaysCount();
    const rating = this.eventRating();
    const cost = analytics.eventCost;
    const avgDayCost = cost !== null && daysCount > 0 ? cost / daysCount : null;

    return [
      { label: 'Количество участников', value: `${personCount}/${capacity}` },
      { label: 'Рейтинг', value: this.formatNullable(rating === 0 ? null : rating) },
      { label: 'Стоимость мероприятия', value: this.formatNullable(cost) },
      { label: 'Средняя стоимость дня', value: this.formatNullable(avgDayCost) },
      { label: 'N-мерность мероприятия', value: String(daysCount) },
    ];
  });

  readonly pricingMetrics = computed<EventManagementAnalyticsMetric[]>(() => {
    const analytics = this.eventAnalytics();

    return [
      { label: 'Фундаментальная цена (1 день)', value: this.formatNullable(analytics.fundamentalPrice1D) },
      { label: 'Фундаментальная цена (N дней)', value: this.formatNullable(analytics.fundamentalPriceNd) },
    ];
  });

  readonly profitMetrics = computed<EventManagementAnalyticsMetric[]>(() => {
    const analytics = this.eventAnalytics();

    return [
      { label: 'Расходы', value: this.formatNullable(analytics.eventCost) },
      { label: 'Баланс (1 день)', value: this.formatNullable(analytics.balance1D) },
      { label: 'Баланс (N дней)', value: this.formatNullable(analytics.balanceNd) },
    ];
  });

  private formatNullable(value: number | null | undefined): string {
    if (value === null || value === undefined) {
      return 'N/A';
    }

    return Number.isInteger(value) ? String(value) : value.toFixed(2);
  }
}
