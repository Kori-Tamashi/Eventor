import { Component, computed, effect, input, signal } from '@angular/core';
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
      this.description.set('Описание дня');
    });
  }

  onTitleInput(value: string): void {
    this.dayTitle.set(value.slice(0, this.titleMaxLen));
  }

  onDescriptionInput(value: string): void {
    this.description.set(value.slice(0, this.descriptionMaxLen));
  }

  cancel(): void {
    const normalizedTitle = this.initialTitle().trim() || 'Название дня';
    this.dayTitle.set(normalizedTitle);
    this.description.set('Описание дня');
  }

  save(): void {}
}
