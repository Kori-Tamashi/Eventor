import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { FeedbacksService } from '../generated';
import { CreateFeedbackPayload, FeedbackApiModel } from '../models/feedback.models';

@Injectable({
  providedIn: 'root',
})
export class FeedbacksApiService {
  private readonly feedbacksService = inject(FeedbacksService);

  listByEvent(eventId: string, pageNumber?: number, pageSize?: number): Observable<FeedbackApiModel[]> {
    return this.feedbacksService.getApiV1FeedbacksEvent({
      eventId,
      pageNumber,
      pageSize,
    }) as Observable<FeedbackApiModel[]>;
  }

  createFeedback(payload: CreateFeedbackPayload): Observable<FeedbackApiModel> {
    return this.feedbacksService.postApiV1Feedbacks({
      requestBody: payload,
    }) as Observable<FeedbackApiModel>;
  }
}
