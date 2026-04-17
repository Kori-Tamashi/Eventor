import { Component, computed, input, output, signal } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { ButtonModule } from 'primeng/button';

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
  readonly defaultDaysCount = '5';

  readonly daysCount = input.required<string>();
  readonly daysCountChange = output<string>();

  readonly eventTitle = signal<string>('Название мероприятия');
  readonly description = signal<string>('Описание');
  readonly location = signal<string>('Локация');
  readonly date = signal<string>('dd.mm.yyyy');
  readonly markup = signal<string>('NNN.NN');
  readonly participantBudget = signal<string>('NNN.NN');

  readonly titleCountLabel = computed(() => `${this.eventTitle().length}/${this.titleMaxLen}`);
  readonly descriptionCountLabel = computed(() => `${this.description().length}/${this.descriptionMaxLen}`);

  onTitleInput(value: string): void {
    this.eventTitle.set(value.slice(0, this.titleMaxLen));
  }

  onDescriptionInput(value: string): void {
    this.description.set(value.slice(0, this.descriptionMaxLen));
  }

  onLocationInput(value: string): void {
    this.location.set(value);
  }

  onDateInput(value: string): void {
    this.date.set(value);
  }

  onDaysCountInput(value: string): void {
    this.daysCountChange.emit(value);
  }

  onMarkupInput(value: string): void {
    this.markup.set(value);
  }

  onParticipantBudgetInput(value: string): void {
    this.participantBudget.set(value);
  }

  cancel(): void {
    this.eventTitle.set('Название мероприятия');
    this.description.set('Описание');
    this.location.set('Локация');
    this.date.set('dd.mm.yyyy');
    this.markup.set('NNN.NN');
    this.participantBudget.set('NNN.NN');
    this.daysCountChange.emit(this.defaultDaysCount);
  }

  save(): void {}
}
