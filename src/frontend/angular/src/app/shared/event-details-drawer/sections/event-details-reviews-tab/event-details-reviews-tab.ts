import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { RatingModule } from 'primeng/rating';
import { TextareaModule } from 'primeng/textarea';
import { DrawerCard } from '../../../drawer-card/drawer-card';
import { EventDetailsDrawerReview } from '../../event-details-drawer.models';

@Component({
  selector: 'app-event-details-reviews-tab',
  standalone: true,
  imports: [
    FormsModule,
    ButtonModule,
    RatingModule,
    TextareaModule,
    DrawerCard,
  ],
  templateUrl: './event-details-reviews-tab.html',
  styleUrl: './event-details-reviews-tab.scss',
})
export class EventDetailsReviewsTab {
  readonly reviewText = input.required<string>();
  readonly reviewRating = input.required<number>();
  readonly reviewRows = input.required<EventDetailsDrawerReview[]>();
  readonly deletingFeedbackId = input<string | null>(null);
  readonly reviewCountLabel = input.required<string>();
  readonly canSaveReview = input.required<boolean>();

  readonly reviewTextChange = output<string>();
  readonly reviewRatingChange = output<number>();
  readonly reviewCancel = output<void>();
  readonly reviewSave = output<void>();
  readonly reviewDelete = output<string>();

  onReviewInput(value: string): void {
    this.reviewTextChange.emit(value);
  }

  onReviewRatingChange(value: number): void {
    this.reviewRatingChange.emit(value ?? 0);
  }

  cancelReview(): void {
    this.reviewCancel.emit();
  }

  saveReview(): void {
    this.reviewSave.emit();
  }

  deleteReview(feedbackId: string): void {
    this.reviewDelete.emit(feedbackId);
  }
}
