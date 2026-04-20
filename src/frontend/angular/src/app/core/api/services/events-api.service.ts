import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin, of } from 'rxjs';
import { map } from 'rxjs/operators';
import type {
  System_DateOnly,
  Web_Dtos_CreateDayRequest,
} from '../generated';
import { EventsService } from '../generated';
import { EventApiModel, EventListFilters } from '../models/event.models';

export type CreateEventPayload = {
  title: string;
  description?: string | null;
  startDate: string;
  locationId: string;
  daysCount: number;
  percent: number;
  createdByUserId: string;
};

export type UpdateEventPayload = {
  title?: string | null;
  description?: string | null;
  startDate?: string | null;
  locationId?: string | null;
  daysCount?: number | null;
  percent?: number | null;
};

@Injectable({
  providedIn: 'root',
})
export class EventsApiService {
  private readonly eventsService = inject(EventsService);

  listEvents(filters: EventListFilters = {}): Observable<EventApiModel[]> {
    return this.eventsService.getApiV1Events({
      titleContains: filters.titleContains,
      pageNumber: filters.pageNumber,
      pageSize: filters.pageSize,
    }) as Observable<EventApiModel[]>;
  }

  listUserEvents(userId: string, filters: EventListFilters = {}): Observable<EventApiModel[]> {
    return this.eventsService.getApiV1EventsUser({
      userId,
      pageNumber: filters.pageNumber,
      pageSize: filters.pageSize,
    }) as Observable<EventApiModel[]>;
  }

  listOrganizedEvents(userId: string, filters: EventListFilters = {}): Observable<EventApiModel[]> {
    return this.eventsService.getApiV1EventsOrganized({
      userId,
      pageNumber: filters.pageNumber,
      pageSize: filters.pageSize,
    }) as Observable<EventApiModel[]>;
  }

  getEvent(eventId: string): Observable<EventApiModel> {
    return this.eventsService.getApiV1Events1({ eventId }) as Observable<EventApiModel>;
  }

  getEventTitleMap(eventIds: string[]): Observable<Record<string, string>> {
    const uniqueIds = Array.from(new Set(eventIds.filter(Boolean)));

    if (uniqueIds.length === 0) {
      return of({});
    }

    return forkJoin(
      uniqueIds.map((eventId) =>
        this.getEvent(eventId).pipe(
          map((event) => ({ id: eventId, title: event.title }))
        )
      )
    ).pipe(
      map((entries) =>
        entries.reduce<Record<string, string>>((acc, entry) => {
          acc[entry.id] = entry.title;
          return acc;
        }, {})
      )
    );
  }

  createEvent(payload: CreateEventPayload): Observable<EventApiModel> {
    return this.eventsService.postApiV1Events({
      requestBody: payload as never,
    }) as Observable<EventApiModel>;
  }

  updateEvent(eventId: string, payload: UpdateEventPayload): Observable<void> {
    return this.eventsService.putApiV1Events({
      eventId,
      requestBody: payload as never,
    }) as Observable<void>;
  }

  deleteEvent(eventId: string): Observable<void> {
    return this.eventsService.deleteApiV1Events({
      eventId,
    }) as Observable<void>;
  }

  createDay(eventId: string, payload: Web_Dtos_CreateDayRequest): Observable<void> {
    return this.eventsService.postApiV1EventsDays({
      eventId,
      requestBody: payload,
    }) as Observable<void>;
  }
}
