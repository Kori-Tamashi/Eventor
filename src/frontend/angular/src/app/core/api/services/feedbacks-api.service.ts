import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import type { Domain_Enums_FeedbackSortByRate } from '../generated';
import { FeedbacksService } from '../generated';
import { CreateFeedbackPayload, FeedbackApiModel, UpdateFeedbackPayload } from '../models/feedback.models';

@Injectable({
  providedIn: 'root',
})
export class FeedbacksApiService {
  private readonly feedbacksService = inject(FeedbacksService);

  listFeedbacks(filters: {
    registrationId?: string;
    sortByRate?: Domain_Enums_FeedbackSortByRate;
    pageNumber?: number;
    pageSize?: number;
  } = {}): Observable<FeedbackApiModel[]> {
    return this.feedbacksService.getApiV1Feedbacks({
      registrationId: filters.registrationId,
      sortByRate: filters.sortByRate,
      pageNumber: filters.pageNumber,
      pageSize: filters.pageSize,
    }) as Observable<FeedbackApiModel[]>;
  }

  listByEvent(eventId: string, pageNumber?: number, pageSize?: number): Observable<FeedbackApiModel[]> {
    return this.feedbacksService.getApiV1FeedbacksEvent({
      eventId,
      pageNumber,
      pageSize,
    }) as Observable<FeedbackApiModel[]>;
  }

  getFeedback(feedbackId: string): Observable<FeedbackApiModel> {
    return this.feedbacksService.getApiV1Feedbacks1({
      feedbackId,
    }) as Observable<FeedbackApiModel>;
  }

  createFeedback(payload: CreateFeedbackPayload): Observable<FeedbackApiModel> {
    return this.feedbacksService.postApiV1Feedbacks({
      requestBody: payload,
    }) as Observable<FeedbackApiModel>;
  }

  updateFeedback(feedbackId: string, payload: UpdateFeedbackPayload): Observable<FeedbackApiModel> {
    return this.feedbacksService.putApiV1Feedbacks({
      feedbackId,
      requestBody: payload,
    }) as Observable<FeedbackApiModel>;
  }

  deleteFeedback(feedbackId: string): Observable<void> {
    return this.feedbacksService.deleteApiV1Feedbacks({
      feedbackId,
    }) as Observable<void>;
  }
}
