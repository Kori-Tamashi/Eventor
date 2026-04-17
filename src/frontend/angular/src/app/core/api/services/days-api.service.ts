import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import type { Web_Dtos_UpdateDayRequest } from '../generated';
import { DaysService, EventsService } from '../generated';
import { DayApiModel } from '../models/day.models';

@Injectable({
  providedIn: 'root',
})
export class DaysApiService {
  private readonly daysService = inject(DaysService);
  private readonly eventsService = inject(EventsService);

  listByEvent(eventId: string, pageNumber?: number, pageSize?: number): Observable<DayApiModel[]> {
    return this.eventsService.getApiV1EventsDays({
      eventId,
      pageNumber,
      pageSize,
    }) as Observable<DayApiModel[]>;
  }

  getDay(dayId: string): Observable<DayApiModel> {
    return this.daysService.getApiV1Days1({ dayId }) as Observable<DayApiModel>;
  }

  updateDay(dayId: string, payload: Web_Dtos_UpdateDayRequest): Observable<void> {
    return this.daysService.putApiV1Days({
      dayId,
      requestBody: payload,
    }) as Observable<void>;
  }

  deleteDay(dayId: string): Observable<void> {
    return this.daysService.deleteApiV1Days({ dayId }) as Observable<void>;
  }
}
