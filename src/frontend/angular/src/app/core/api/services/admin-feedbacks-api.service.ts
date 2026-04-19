import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AdminFeedbacksService } from '../generated';

@Injectable({
  providedIn: 'root',
})
export class AdminFeedbacksApiService {
  private readonly adminFeedbacksService = inject(AdminFeedbacksService);

  deleteFeedback(feedbackId: string): Observable<void> {
    return this.adminFeedbacksService.deleteApiV1AdminFeedbacks({
      feedbackId,
    }) as Observable<void>;
  }
}
