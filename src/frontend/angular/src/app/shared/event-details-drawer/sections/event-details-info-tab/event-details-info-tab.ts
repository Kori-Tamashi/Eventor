import { Component, computed, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DrawerCard } from '../../../drawer-card/drawer-card';
import {
  EventDetailsDrawerDay,
  EventDetailsDrawerRegistrationType,
} from '../../event-details-drawer.models';

@Component({
  selector: 'app-event-details-info-tab',
  standalone: true,
  imports: [ButtonModule, TableModule, DrawerCard],
  templateUrl: './event-details-info-tab.html',
  styleUrl: './event-details-info-tab.scss',
})
export class EventDetailsInfoTab {
  readonly daysCount = input.required<number>();
  readonly participantsCount = input.required<number>();
  readonly dayRows = input.required<EventDetailsDrawerDay[]>();
  readonly currentUserId = input<string | null>(null);
  readonly currentUserRegistrationId = input<string | null>(null);
  readonly currentUserRegistrationPayment = input<boolean | null>(null);
  readonly registrationType = input.required<EventDetailsDrawerRegistrationType>();
  readonly registrationDayIds = input.required<string[]>();
  readonly canSaveRegistration = input<boolean>(false);
  readonly isSavingRegistration = input<boolean>(false);
  readonly registrationMessage = input<string>('');

  readonly dayParticipantsOpen = output<EventDetailsDrawerDay>();
  readonly registrationTypeChange = output<EventDetailsDrawerRegistrationType>();
  readonly registrationDayToggle = output<{ dayId: string; checked: boolean }>();
  readonly registrationSave = output<void>();
  readonly registrationDelete = output<void>();

  readonly isCurrentUserRegistered = computed(() => this.currentUserRegistrationId() !== null);
  readonly paymentLabel = computed(() => {
    const payment = this.currentUserRegistrationPayment();
    if (payment === null) {
      return null;
    }

    return payment ? 'Оплачено' : 'Не оплачено';
  });

  openDayParticipants(day: EventDetailsDrawerDay): void {
    this.dayParticipantsOpen.emit(day);
  }

  onRegistrationTypeChange(value: string): void {
    this.registrationTypeChange.emit(Number(value) as EventDetailsDrawerRegistrationType);
  }

  onRegistrationDayToggle(dayId: string, checked: boolean): void {
    this.registrationDayToggle.emit({ dayId, checked });
  }

  saveRegistration(): void {
    this.registrationSave.emit();
  }

  deleteRegistration(): void {
    this.registrationDelete.emit();
  }

  isDaySelected(dayId: string): boolean {
    return this.registrationDayIds().includes(dayId);
  }
}
