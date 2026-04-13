import { Injectable, inject } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, concatMap, map, switchMap } from 'rxjs/operators';
import { DayApiModel } from '../api/models/day.models';
import { EventApiModel } from '../api/models/event.models';
import { RegistrationApiModel } from '../api/models/registration.models';
import { DaysApiService } from '../api/services/days-api.service';
import { EconomyApiService } from '../api/services/economy-api.service';
import { EventsApiService } from '../api/services/events-api.service';
import { LocationsApiService } from '../api/services/locations-api.service';
import { MenusApiService } from '../api/services/menus-api.service';
import { RegistrationsApiService } from '../api/services/registrations-api.service';
import { UsersApiService } from '../api/services/users-api.service';

export type EventManagementLocationOption = {
  id: string;
  title: string;
};

export type EventManagementDayData = {
  id: string;
  number: number;
  title: string;
  description: string;
  menuId: string;
  price: string;
  participantsCount: string;
};

export type EventManagementFormData = {
  eventId: string | null;
  title: string;
  description: string;
  locationId: string;
  startDate: string;
  daysCount: string;
  markup: string;
  locations: EventManagementLocationOption[];
  dayRows: EventManagementDayData[];
  currentUserId: string;
  currentUsername: string;
};

export type SaveEventManagementPayload = {
  eventId: string | null;
  currentUserId: string;
  title: string;
  description: string;
  locationId: string;
  startDate: string;
  daysCount: number;
  markup: number;
};

export type SaveManagementDayPayload = {
  eventId: string;
  dayId: string;
  title: string;
  description: string;
  menuId: string;
  sequenceNumber: number;
};

@Injectable({
  providedIn: 'root',
})
export class EventManagementDrawerDataService {
  private readonly usersApiService = inject(UsersApiService);
  private readonly locationsApiService = inject(LocationsApiService);
  private readonly eventsApiService = inject(EventsApiService);
  private readonly daysApiService = inject(DaysApiService);
  private readonly menusApiService = inject(MenusApiService);
  private readonly economyApiService = inject(EconomyApiService);
  private readonly registrationsApiService = inject(RegistrationsApiService);

  loadCreateData(): Observable<EventManagementFormData> {
    return forkJoin({
      currentUser: this.usersApiService.getMe(),
      locations: this.locationsApiService.listLocations(),
    }).pipe(
      map(({ currentUser, locations }) => ({
        eventId: null,
        title: '',
        description: '',
        locationId: locations[0]?.id ?? '',
        startDate: this.formatDateForInput(new Date()),
        daysCount: '1',
        markup: '0',
        locations: locations.map((location) => ({
          id: location.id,
          title: location.title,
        })),
        dayRows: [],
        currentUserId: currentUser.id,
        currentUsername: currentUser.name,
      }))
    );
  }

  loadEventData(eventId: string): Observable<EventManagementFormData> {
    return forkJoin({
      currentUser: this.usersApiService.getMe(),
      locations: this.locationsApiService.listLocations(),
      event: this.eventsApiService.getEvent(eventId),
      days: this.daysApiService.listByEvent(eventId),
      registrations: this.registrationsApiService.listByEvent(eventId),
    }).pipe(
      switchMap(({ currentUser, locations, event, days, registrations }) =>
        this.economyApiService.getDayPriceMap(days.map((day) => day.id)).pipe(
          catchError(() => of({})),
          map((dayPriceMap) => ({
            eventId: event.id,
            title: event.title,
            description: event.description ?? '',
            locationId: event.locationId,
            startDate: this.formatDateStringForInput(event.startDate),
            daysCount: String(event.daysCount),
            markup: this.formatMarkup(event.percent),
            locations: locations.map((location) => ({
              id: location.id,
              title: location.title,
            })),
            dayRows: this.buildDayRows(days, registrations, dayPriceMap),
            currentUserId: currentUser.id,
            currentUsername: currentUser.name,
          }))
        )
      )
    );
  }

  saveEvent(payload: SaveEventManagementPayload): Observable<EventManagementFormData> {
    if (payload.eventId) {
      return this.eventsApiService.updateEvent(payload.eventId, {
        title: payload.title,
        description: payload.description,
        startDate: this.mapDateOnly(payload.startDate),
        locationId: payload.locationId,
        daysCount: payload.daysCount,
        percent: payload.markup,
      }).pipe(
        switchMap(() => this.syncDays(payload.eventId as string, payload.daysCount)),
        switchMap(() => this.loadEventData(payload.eventId as string))
      );
    }

    return this.eventsApiService.createEvent({
      title: payload.title,
      description: payload.description,
      startDate: this.mapDateOnly(payload.startDate),
      locationId: payload.locationId,
      daysCount: payload.daysCount,
      percent: payload.markup,
      createdByUserId: payload.currentUserId,
    }).pipe(
      switchMap((event: EventApiModel) =>
        this.syncDays(event.id, payload.daysCount).pipe(
          switchMap(() => this.loadEventData(event.id))
        )
      )
    );
  }

  saveDay(payload: SaveManagementDayPayload): Observable<EventManagementFormData> {
    return this.daysApiService.updateDay(payload.dayId, {
      title: payload.title,
      description: payload.description,
      menuId: payload.menuId,
      sequenceNumber: payload.sequenceNumber,
    }).pipe(
      switchMap(() => this.loadEventData(payload.eventId))
    );
  }

  private syncDays(eventId: string, targetDaysCount: number): Observable<void> {
    return this.daysApiService.listByEvent(eventId).pipe(
      switchMap((days) => {
        if (days.length === targetDaysCount) {
          return of(void 0);
        }

        if (days.length < targetDaysCount) {
          const missingCount = targetDaysCount - days.length;
          const nextSequenceStart = days.length + 1;

          return this.runSequentially(
            Array.from({ length: missingCount }, (_, index) => {
              const sequenceNumber = nextSequenceStart + index;
              const title = `День ${sequenceNumber}`;

              return () => this.menusApiService.createMenu(title, '').pipe(
                switchMap((menu) =>
                  this.eventsApiService.createDay(eventId, {
                    menuId: menu.id,
                    title,
                    description: '',
                    sequenceNumber,
                  })
                ),
                map(() => void 0)
              );
            })
          );
        }

        const daysToDelete = [...days]
          .sort((a, b) => b.sequenceNumber - a.sequenceNumber)
          .slice(0, days.length - targetDaysCount);

        return this.runSequentially(
          daysToDelete.map((day) => () => this.daysApiService.deleteDay(day.id))
        );
      })
    );
  }

  private runSequentially(operations: Array<() => Observable<unknown>>): Observable<void> {
    if (operations.length === 0) {
      return of(void 0);
    }

    return operations.reduce<Observable<unknown>>(
      (stream, operation) => stream.pipe(concatMap(() => operation())),
      of(void 0)
    ).pipe(map(() => void 0));
  }

  private buildDayRows(
    days: DayApiModel[],
    registrations: RegistrationApiModel[],
    dayPriceMap: Record<string, number>
  ): EventManagementDayData[] {
    return [...days]
      .sort((a, b) => a.sequenceNumber - b.sequenceNumber)
      .map((day) => ({
        id: day.id,
        number: day.sequenceNumber,
        title: day.title,
        description: day.description ?? '',
        menuId: day.menuId,
        price: this.formatPrice(dayPriceMap[day.id]),
        participantsCount: String(
          registrations.filter((registration) =>
            registration.days.some((registrationDay) => registrationDay.id === day.id)
          ).length
        ),
      }));
  }

  private mapDateOnly(value: string): { year: number; month: number; day: number } {
    const [year, month, day] = value.split('-').map(Number);
    return {
      year,
      month,
      day,
    };
  }

  private formatDateStringForInput(value: string): string {
    return value;
  }

  private formatDateForInput(value: Date): string {
    const year = value.getFullYear();
    const month = String(value.getMonth() + 1).padStart(2, '0');
    const day = String(value.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private formatMarkup(value: number): string {
    return Number.isInteger(value) ? String(value) : value.toFixed(2);
  }

  private formatPrice(price: number | undefined): string {
    if (typeof price !== 'number' || Number.isNaN(price)) {
      return 'N/A';
    }

    return Number.isInteger(price) ? String(price) : price.toFixed(2);
  }
}
