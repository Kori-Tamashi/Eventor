import { Component, computed, effect, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { RatingModule } from 'primeng/rating';
import { TextareaModule } from 'primeng/textarea';
import { AdminFeedbackDraftValue, AdminFeedbackSavePayload } from './admin-feedback.models';

@Component({
  selector: 'app-admin-feedback-drawer',
  standalone: true,
  imports: [FormsModule, ButtonModule, DrawerModule, RatingModule, TextareaModule],
  templateUrl: './admin-feedback-drawer.html',
  styleUrl: './admin-feedback-drawer.scss',
})
export class AdminFeedbackDrawer {
  readonly visible = input<boolean>(false);
  readonly initialValue = input<AdminFeedbackDraftValue | null>(null);
  readonly isLoading = input<boolean>(false);
  readonly isSaving = input<boolean>(false);
  readonly errorMessage = input<string>('');

  readonly visibleChange = output<boolean>();
  readonly closeRequested = output<void>();
  readonly saveRequested = output<AdminFeedbackSavePayload>();

  readonly commentMaxLen = 250;
  readonly comment = signal<string>('');
  readonly rate = signal<number>(0);

  readonly commentCountLabel = computed(() => `${this.comment().length}/${this.commentMaxLen}`);
  readonly canSave = computed(() => this.comment().trim().length > 0 && this.rate() > 0 && !this.isLoading() && !this.isSaving());

  constructor() {
    effect(() => {
      const value = this.initialValue();
      this.comment.set(value?.comment ?? '');
      this.rate.set(value?.rate ?? 0);
    });
  }

  onDrawerVisibleChange(visible: boolean): void {
    this.visibleChange.emit(visible);
    if (!visible) {
      this.closeRequested.emit();
    }
  }

  onCommentInput(value: string): void {
    this.comment.set(value.slice(0, this.commentMaxLen));
  }

  onRateChange(value: number): void {
    this.rate.set(value ?? 0);
  }

  close(): void {
    this.closeRequested.emit();
    this.visibleChange.emit(false);
  }

  save(): void {
    const comment = this.comment().trim();
    const rate = this.rate();

    if (!comment || rate <= 0) {
      return;
    }

    this.saveRequested.emit({ comment, rate });
  }
}
