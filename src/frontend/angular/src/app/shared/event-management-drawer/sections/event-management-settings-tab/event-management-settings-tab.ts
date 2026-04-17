import { Component, computed, effect, input, output, signal } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { ButtonModule } from 'primeng/button';
import { EventManagementLocationOption } from '../../../../core/ui/event-management-drawer-data.service';

@Component({
  selector: 'app-event-management-settings-tab',
  standalone: true,
  imports: [InputTextModule, TextareaModule, ButtonModule],
  templateUrl: './event-management-settings-tab.html',
  styleUrl: './event-management-settings-tab.scss',
})
export class EventManagementSettingsTab {
  readonly titleMaxLen = 70;
  readonly descriptionMaxLen = 250;
  readonly eventTitleValue = input.required<string>();
  readonly descriptionValue = input.required<string>();
  readonly locationIdValue = input.required<string>();
  readonly startDateValue = input.required<string>();
  readonly daysCountValue = input.required<string>();
  readonly markupValue = input.required<string>();
  readonly locations = input.required<EventManagementLocationOption[]>();
  readonly isSaving = input<boolean>(false);
  readonly errorMessage = input<string>('');
  readonly successMessage = input<string>('');

  readonly eventTitleChange = output<string>();
  readonly descriptionChange = output<string>();
  readonly locationIdChange = output<string>();
  readonly startDateChange = output<string>();
  readonly daysCountChange = output<string>();
  readonly markupChange = output<string>();
  readonly cancelRequested = output<void>();
  readonly saveRequested = output<void>();

  readonly eventTitle = signal<string>('');
  readonly description = signal<string>('');
  readonly locationId = signal<string>('');
  readonly date = signal<string>('');
  readonly markup = signal<string>('');

  readonly titleCountLabel = computed(() => `${this.eventTitle().length}/${this.titleMaxLen}`);
  readonly descriptionCountLabel = computed(() => `${this.description().length}/${this.descriptionMaxLen}`);

  constructor() {
    effect(() => {
      this.eventTitle.set(this.eventTitleValue());
      this.description.set(this.descriptionValue());
      this.locationId.set(this.locationIdValue());
      this.date.set(this.startDateValue());
      this.markup.set(this.markupValue());
    });
  }

  onTitleInput(value: string): void {
    const normalized = value.slice(0, this.titleMaxLen);
    this.eventTitle.set(normalized);
    this.eventTitleChange.emit(normalized);
  }

  onDescriptionInput(value: string): void {
    const normalized = value.slice(0, this.descriptionMaxLen);
    this.description.set(normalized);
    this.descriptionChange.emit(normalized);
  }

  onLocationInput(value: string): void {
    this.locationId.set(value);
    this.locationIdChange.emit(value);
  }

  onDateInput(value: string): void {
    this.date.set(value);
    this.startDateChange.emit(value);
  }

  onDaysCountInput(value: string): void {
    this.daysCountChange.emit(value);
  }

  onMarkupInput(value: string): void {
    const normalized = value.replace(/[^0-9.,]/g, '').slice(0, 8);
    this.markup.set(normalized);
    this.markupChange.emit(normalized);
  }

  cancel(): void {
    this.cancelRequested.emit();
  }

  save(): void {
    this.saveRequested.emit();
  }
}
