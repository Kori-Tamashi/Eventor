import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import type {
  System_DateOnly,
  Web_Dtos_CreateDayRequest,
  Web_Dtos_CreateEventRequest,
  Web_Dtos_UpdateEventRequest,
} from '../generated';
import { EventsService } from '../generated';
import { EventApiModel, EventListFilters } from '../models/event.models';

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

  createEvent(payload: Web_Dtos_CreateEventRequest): Observable<EventApiModel> {
    return this.eventsService.postApiV1Events({
      requestBody: payload,
    }) as Observable<EventApiModel>;
  }

  updateEvent(eventId: string, payload: Web_Dtos_UpdateEventRequest): Observable<void> {
    return this.eventsService.putApiV1Events({
      eventId,
      requestBody: payload,
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
