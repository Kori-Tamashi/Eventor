import { Injectable, inject } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { AdminUsersApiService } from '../api/services/admin-users-api.service';
import { DaysApiService } from '../api/services/days-api.service';
import { EconomyApiService } from '../api/services/economy-api.service';
import { EventsApiService } from '../api/services/events-api.service';
import { FeedbacksApiService } from '../api/services/feedbacks-api.service';
import { RegistrationsApiService } from '../api/services/registrations-api.service';
import { UsersApiService } from '../api/services/users-api.service';
import {
  EventDetailsDrawerContext,
  EventDetailsDrawerDay,
  EventDetailsDrawerReview,
  EventDetailsDrawerSource,
  EventDetailsDrawerViewerRole,
} from '../../shared/event-details-drawer/event-details-drawer.models';
import { DayApiModel } from '../api/models/day.models';
import { CreateFeedbackPayload } from '../api/models/feedback.models';
import { RegistrationApiModel } from '../api/models/registration.models';

export type LoadEventDetailsParams = {
  eventId: string;
  source: EventDetailsDrawerSource;
  viewerRole: EventDetailsDrawerViewerRole;
};

export type LoadedEventDetails = {
  context: EventDetailsDrawerContext;
  currentUsername: string;
};

@Injectable({
  providedIn: 'root',
})
export class EventDetailsDrawerDataService {
  private readonly eventsApiService = inject(EventsApiService);
  private readonly daysApiService = inject(DaysApiService);
  private readonly feedbacksApiService = inject(FeedbacksApiService);
  private readonly registrationsApiService = inject(RegistrationsApiService);
  private readonly usersApiService = inject(UsersApiService);
  private readonly adminUsersApiService = inject(AdminUsersApiService);
  private readonly economyApiService = inject(EconomyApiService);

  loadEventDetails(params: LoadEventDetailsParams): Observable<LoadedEventDetails> {
    return forkJoin({
      event: this.eventsApiService.getEvent(params.eventId),
      days: this.daysApiService.listByEvent(params.eventId),
      feedbacks: this.feedbacksApiService.listByEvent(params.eventId),
      registrations: this.registrationsApiService.listByEvent(params.eventId),
      currentUser: this.usersApiService.getMe(),
    }).pipe(
      switchMap(({ event, days, feedbacks, registrations, currentUser }) => {
        const userIds = registrations.map((registration) => registration.userId);

        return forkJoin({
          userMap: this.adminUsersApiService.getUserNameMap(userIds).pipe(catchError(() => of({}))),
          dayPriceMap: this.economyApiService.getDayPriceMap(days.map((day) => day.id)).pipe(catchError(() => of({}))),
        }).pipe(
          map(({ userMap, dayPriceMap }) => {
            const registrationById = registrations.reduce<Record<string, RegistrationApiModel>>((acc, registration) => {
              acc[registration.id] = registration;
              return acc;
            }, {});

            const currentUserRegistration = registrations.find(
              (registration) => registration.userId === currentUser.id
            );

            const dayRows = this.buildDayRows(days, registrations, userMap, dayPriceMap);
            const reviewRows = feedbacks.map((feedback) =>
              this.mapReviewRow(feedback.registrationId, feedback.comment, feedback.rate, registrationById, userMap)
            );

            return {
              currentUsername: currentUser.name,
              context: {
                eventId: event.id,
                source: params.source,
                viewerRole: params.viewerRole,
                data: {
                  title: event.title,
                  daysCount: days.length,
                  participantsCount: registrations.length,
                  dayRows,
                  reviewRows,
                  currentUserRegistrationId: currentUserRegistration?.id ?? null,
                },
              },
            };
          })
        );
      })
    );
  }

  createReview(payload: CreateFeedbackPayload, person: string): Observable<EventDetailsDrawerReview> {
    return this.feedbacksApiService.createFeedback(payload).pipe(
      map((feedback) => ({
        person,
        comment: feedback.comment,
        rating: feedback.rate,
      }))
    );
  }

  private buildDayRows(
    days: DayApiModel[],
    registrations: RegistrationApiModel[],
    userMap: Record<string, string>,
    dayPriceMap: Record<string, number>
  ): EventDetailsDrawerDay[] {
    return [...days]
      .sort((a, b) => a.sequenceNumber - b.sequenceNumber)
      .map((day) => {
        const dayRegistrations = registrations.filter((registration) =>
          registration.days.some((registrationDay) => registrationDay.id === day.id)
        );

        return {
          id: day.id,
          name: day.title,
          price: this.formatPrice(dayPriceMap[day.id]),
          participants: dayRegistrations.length,
          participantRows: dayRegistrations.map((registration, index) => ({
            name: userMap[registration.userId] ?? `Участник ${index + 1}`,
            payment: registration.payment ? 'Оплачено' : 'Не оплачено',
          })),
        };
      });
  }

  private mapReviewRow(
    registrationId: string,
    comment: string,
    rating: number,
    registrationById: Record<string, RegistrationApiModel>,
    userMap: Record<string, string>
  ): EventDetailsDrawerReview {
    const registration = registrationById[registrationId];
    const person = registration ? userMap[registration.userId] ?? 'Участник' : 'Участник';

    return {
      person,
      comment,
      rating,
    };
  }

  private formatPrice(price: number | undefined): string {
    if (typeof price !== 'number' || Number.isNaN(price)) {
      return 'N/A';
    }

    return Number.isInteger(price) ? String(price) : price.toFixed(2);
  }
}
