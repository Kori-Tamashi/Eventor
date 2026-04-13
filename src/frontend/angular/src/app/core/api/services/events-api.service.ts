import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
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
}
