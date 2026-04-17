import { Component, computed, effect, input, output, signal } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-event-management-day-settings-tab',
  standalone: true,
  imports: [InputTextModule, TextareaModule, ButtonModule],
  templateUrl: './event-management-day-settings-tab.html',
  styleUrl: './event-management-day-settings-tab.scss',
})
export class EventManagementDaySettingsTab {
  readonly initialTitle = input<string>('Название дня');
  readonly initialDescription = input<string>('');
  readonly isSaving = input<boolean>(false);
  readonly errorMessage = input<string>('');
  readonly successMessage = input<string>('');

  readonly cancelRequested = output<void>();
  readonly saveRequested = output<{ title: string; description: string }>();

  readonly titleMaxLen = 70;
  readonly descriptionMaxLen = 250;

  readonly dayTitle = signal<string>('Название дня');
  readonly description = signal<string>('Описание дня');

  readonly titleCountLabel = computed(() => `${this.dayTitle().length}/${this.titleMaxLen}`);
  readonly descriptionCountLabel = computed(() => `${this.description().length}/${this.descriptionMaxLen}`);

  constructor() {
    effect(() => {
      const normalizedTitle = this.initialTitle().trim() || 'Название дня';
      this.dayTitle.set(normalizedTitle);
      this.description.set(this.initialDescription());
    });
  }

  onTitleInput(value: string): void {
    this.dayTitle.set(value.slice(0, this.titleMaxLen));
  }

  onDescriptionInput(value: string): void {
    this.description.set(value.slice(0, this.descriptionMaxLen));
  }

  cancel(): void {
    this.cancelRequested.emit();
  }

  save(): void {
    this.saveRequested.emit({
      title: this.dayTitle().trim() || 'Название дня',
      description: this.description().trim(),
    });
  }
}
