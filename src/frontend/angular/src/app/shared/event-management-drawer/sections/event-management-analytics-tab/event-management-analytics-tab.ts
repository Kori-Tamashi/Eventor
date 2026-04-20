import { Component, input } from '@angular/core';
import { EventManagementEventAnalytics } from '../../../../core/ui/event-management-drawer-data.service';

@Component({
  selector: 'app-event-management-analytics-tab',
  standalone: true,
  templateUrl: './event-management-analytics-tab.html',
  styleUrl: './event-management-analytics-tab.scss',
})
export class EventManagementAnalyticsTab {
  readonly eventAnalytics = input.required<EventManagementEventAnalytics>();
  readonly eventRating = input.required<number>();
  readonly eventPersonCount = input.required<number>();
  readonly eventDaysCount = input.required<number>();
  readonly locationCapacity = input.required<number>();

  participantSummary(): string {
    return `${this.eventPersonCount()}/${this.locationCapacity()}`;
  }

  ratingValue(): string {
    const rating = this.eventRating();
    return this.formatNullable(rating === 0 ? null : rating);
  }

  eventCostValue(): string {
    return this.formatNullable(this.eventAnalytics().eventCost);
  }

  avgDayCostValue(): string {
    const cost = this.eventAnalytics().eventCost;
    const daysCount = this.eventDaysCount();
    return this.formatNullable(cost !== null && daysCount > 0 ? cost / daysCount : null);
  }

  daysCountValue(): string {
    return String(this.eventDaysCount());
  }

  fundamentalPrice1DValue(): string {
    return this.formatNullable(this.eventAnalytics().fundamentalPrice1D);
  }

  fundamentalPriceNdValue(): string {
    return this.formatNullable(this.eventAnalytics().fundamentalPriceNd);
  }

  balance1DValue(): string {
    return this.formatNullable(this.eventAnalytics().balance1D);
  }

  balanceNdValue(): string {
    return this.formatNullable(this.eventAnalytics().balanceNd);
  }

  private formatNullable(value: number | null | undefined): string {
    if (value === null || value === undefined) {
      return 'N/A';
    }

    return Number.isInteger(value) ? String(value) : value.toFixed(2);
  }
}
