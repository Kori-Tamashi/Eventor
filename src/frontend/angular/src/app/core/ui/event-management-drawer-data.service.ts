import { Injectable, inject } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, concatMap, map, switchMap } from 'rxjs/operators';
import { DayApiModel } from '../api/models/day.models';
import { EventApiModel } from '../api/models/event.models';
import { FeedbackApiModel } from '../api/models/feedback.models';
import { RegistrationApiModel } from '../api/models/registration.models';
import { AdminUsersApiService } from '../api/services/admin-users-api.service';
import { DaysApiService } from '../api/services/days-api.service';
import { EconomyApiService } from '../api/services/economy-api.service';
import { EventsApiService } from '../api/services/events-api.service';
import { FeedbacksApiService } from '../api/services/feedbacks-api.service';
import { ItemsApiService, ItemApiModel } from '../api/services/items-api.service';
import { LocationsApiService } from '../api/services/locations-api.service';
import { MenusApiService, MenuItemApiModel } from '../api/services/menus-api.service';
import { RegistrationsApiService } from '../api/services/registrations-api.service';
import { UsersApiService } from '../api/services/users-api.service';
import { CreateFeedbackPayload } from '../api/models/feedback.models';

export type { ItemApiModel, MenuItemApiModel };

export type EventManagementLocationOption = {
  id: string;
  title: string;
};

export type EventManagementParticipantRow = {
  registrationId: string;
  userId: string;
  name: string;
  type: 0 | 1 | 2;
  payment: boolean;
};

export type EventManagementReviewRow = {
  feedbackId: string;
  person: string;
  comment: string;
  rating: number;
  isOwnedByCurrentUser: boolean;
};

export type EventManagementEventAnalytics = {
  eventCost: number | null;
  fundamentalPrice1D: number | null;
  fundamentalPriceNd: number | null;
  balance1D: number | null;
  balanceNd: number | null;
};

export type EventManagementDayData = {
  id: string;
  number: number;
  title: string;
  description: string;
  menuId: string;
  price: string;
  participantsCount: string;
  participantRows: EventManagementParticipantRow[];
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
  reviewRows: EventManagementReviewRow[];
  currentUserRegistrationId: string | null;
  eventAnalytics: EventManagementEventAnalytics;
  eventRating: number;
  personCount: number;
  locationCapacity: number;
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
  private readonly feedbacksApiService = inject(FeedbacksApiService);
  private readonly adminUsersApiService = inject(AdminUsersApiService);
  private readonly itemsApiService = inject(ItemsApiService);

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
        reviewRows: [],
        currentUserRegistrationId: null,
        eventAnalytics: {
          eventCost: null,
          fundamentalPrice1D: null,
          fundamentalPriceNd: null,
          balance1D: null,
          balanceNd: null,
        },
        eventRating: 0,
        personCount: 0,
        locationCapacity: 0,
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
      feedbacks: this.feedbacksApiService.listByEvent(eventId).pipe(catchError(() => of([] as FeedbackApiModel[]))),
    }).pipe(
      switchMap(({ currentUser, locations, event, days, registrations, feedbacks }) => {
        const userIds = registrations.map((r) => r.userId);
        const selectedLocation = locations.find((l) => l.id === event.locationId);

        return forkJoin({
          dayPriceMap: this.economyApiService.getDayPriceMap(days.map((d) => d.id)).pipe(catchError(() => of({} as Record<string, number>))),
          userMap: this.adminUsersApiService.getUserNameMap(userIds).pipe(catchError(() => of({} as Record<string, string>))),
          eventCost: this.economyApiService.getEventCost(eventId).pipe(catchError(() => of(null))),
          fundamentalPrice1D: this.economyApiService.getFundamentalPrice1D(eventId).pipe(catchError(() => of(null))),
          fundamentalPriceNd: this.economyApiService.getFundamentalPriceNd(eventId).pipe(catchError(() => of(null))),
          balance1D: this.economyApiService.getEventBalance1D(eventId).pipe(catchError(() => of(null))),
          balanceNd: this.economyApiService.getEventBalanceNd(eventId).pipe(catchError(() => of(null))),
        }).pipe(
          map(({ dayPriceMap, userMap, eventCost, fundamentalPrice1D, fundamentalPriceNd, balance1D, balanceNd }) => ({
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
            dayRows: this.buildDayRows(days, registrations, dayPriceMap, userMap),
            currentUserId: currentUser.id,
            currentUsername: currentUser.name,
            reviewRows: this.buildReviewRows(feedbacks, registrations, userMap, currentUser.id),
            currentUserRegistrationId: registrations.find((r) => r.userId === currentUser.id)?.id ?? null,
            eventAnalytics: { eventCost, fundamentalPrice1D, fundamentalPriceNd, balance1D, balanceNd },
            eventRating: event.rating,
            personCount: event.personCount,
            locationCapacity: selectedLocation?.capacity ?? 0,
          }))
        );
      })
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
        switchMap(() => this.ensureOrganizerRegistration(payload.eventId as string, payload.currentUserId)),
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
          switchMap(() => this.ensureOrganizerRegistration(event.id, payload.currentUserId)),
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

  saveParticipants(
    updates: Array<{ registrationId: string; type: 0 | 1 | 2; payment: boolean }>,
    _eventId: string,
  ): Observable<void> {
    if (updates.length === 0) {
      return of(void 0);
    }

    return forkJoin(
      updates.map((u) =>
        this.registrationsApiService.updateRegistration(u.registrationId, {
          type: u.type,
          payment: u.payment,
        })
      )
    ).pipe(map(() => void 0));
  }

  loadMenuItems(menuId: string): Observable<{ menuItems: MenuItemApiModel[]; availableItems: ItemApiModel[] }> {
    return forkJoin({
      menuItems: this.menusApiService.listMenuItems(menuId).pipe(catchError(() => of([] as MenuItemApiModel[]))),
      availableItems: this.itemsApiService.listItems().pipe(catchError(() => of([] as ItemApiModel[]))),
    });
  }

  saveMenuItems(
    menuId: string,
    currentItems: MenuItemApiModel[],
    updatedItems: MenuItemApiModel[],
  ): Observable<void> {
    const currentMap = new Map(currentItems.map((i) => [i.itemId, i]));
    const updatedMap = new Map(updatedItems.map((i) => [i.itemId, i]));

    const operations: Array<Observable<unknown>> = [];

    for (const updated of updatedItems) {
      const current = currentMap.get(updated.itemId);

      if (!current) {
        operations.push(this.menusApiService.addMenuItem(menuId, updated.itemId, updated.amount));
      } else if (current.amount !== updated.amount) {
        operations.push(this.menusApiService.updateMenuItemAmount(menuId, updated.itemId, updated.amount));
      }
    }

    for (const current of currentItems) {
      if (!updatedMap.has(current.itemId)) {
        operations.push(this.menusApiService.removeMenuItem(menuId, current.itemId));
      }
    }

    if (operations.length === 0) {
      return of(void 0);
    }

    return forkJoin(operations).pipe(map(() => void 0));
  }

  createReview(payload: CreateFeedbackPayload, person: string): Observable<EventManagementReviewRow> {
    return this.feedbacksApiService.createFeedback(payload).pipe(
      map((feedback) => ({
        feedbackId: feedback.id,
        person,
        comment: feedback.comment,
        rating: feedback.rate,
        isOwnedByCurrentUser: true,
      }))
    );
  }

  deleteReview(feedbackId: string): Observable<void> {
    return this.feedbacksApiService.deleteFeedback(feedbackId);
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

  private ensureOrganizerRegistration(eventId: string, userId: string): Observable<void> {
    return this.registrationsApiService.listByEvent(eventId).pipe(
      switchMap((registrations) => {
        const hasOrganizerRegistration = registrations.some(
          (registration) => registration.userId === userId && registration.type === 2
        );

        if (hasOrganizerRegistration) {
          return of(void 0);
        }

        return this.registrationsApiService.createRegistration({
          eventId,
          userId,
          type: 2,
          payment: true,
          dayIds: [],
        }).pipe(map(() => void 0));
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
    dayPriceMap: Record<string, number>,
    userMap: Record<string, string>,
  ): EventManagementDayData[] {
    return [...days]
      .sort((a, b) => a.sequenceNumber - b.sequenceNumber)
      .map((day) => {
        const dayRegistrations = registrations.filter((r) =>
          r.days.some((d) => d.id === day.id)
        );

        return {
          id: day.id,
          number: day.sequenceNumber,
          title: day.title,
          description: day.description ?? '',
          menuId: day.menuId,
          price: this.formatPrice(dayPriceMap[day.id]),
          participantsCount: String(dayRegistrations.length),
          participantRows: dayRegistrations.map((r, index) => ({
            registrationId: r.id,
            userId: r.userId,
            name: userMap[r.userId] ?? `Участник ${index + 1}`,
            type: r.type,
            payment: r.payment,
          })),
        };
      });
  }

  private buildReviewRows(
    feedbacks: FeedbackApiModel[],
    registrations: RegistrationApiModel[],
    userMap: Record<string, string>,
    currentUserId: string,
  ): EventManagementReviewRow[] {
    const registrationById = registrations.reduce<Record<string, RegistrationApiModel>>((acc, r) => {
      acc[r.id] = r;
      return acc;
    }, {});

    return feedbacks.map((feedback) => {
      const registration = registrationById[feedback.registrationId];
      const person = registration ? (userMap[registration.userId] ?? 'Участник') : 'Участник';
      return {
        feedbackId: feedback.id,
        person,
        comment: feedback.comment,
        rating: feedback.rate,
        isOwnedByCurrentUser: registration?.userId === currentUserId,
      };
    });
  }

  private mapDateOnly(value: string): string {
    return value;
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
