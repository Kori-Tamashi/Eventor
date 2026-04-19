import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AdminEventsService } from '../generated';
import type { System_DateOnly } from '../generated';
import { EventApiModel } from '../models/event.models';

@Injectable({
  providedIn: 'root',
})
export class AdminEventsApiService {
  private readonly adminEventsService = inject(AdminEventsService);

  listEvents(filters: {
    locationId?: string;
    startDateFrom?: string | System_DateOnly;
    startDateTo?: string | System_DateOnly;
    titleContains?: string;
    pageNumber?: number;
    pageSize?: number;
  } = {}): Observable<EventApiModel[]> {
    return this.adminEventsService.getApiV1AdminEvents({
      locationId: filters.locationId,
      startDateFrom: this.toDateOnly(filters.startDateFrom),
      startDateTo: this.toDateOnly(filters.startDateTo),
      titleContains: filters.titleContains,
      pageNumber: filters.pageNumber,
      pageSize: filters.pageSize,
    }) as Observable<EventApiModel[]>;
  }

  deleteEvent(eventId: string): Observable<void> {
    return this.adminEventsService.deleteApiV1AdminEvents({
      eventId,
    }) as Observable<void>;
  }

  private toDateOnly(value?: string | System_DateOnly): System_DateOnly | undefined {
    if (!value) {
      return undefined;
    }

    if (typeof value !== 'string') {
      return value;
    }

    const [year, month, day] = value.split('-').map((part) => Number(part));
    if (!year || !month || !day) {
      return undefined;
    }

    return { year, month, day };
  }
}
