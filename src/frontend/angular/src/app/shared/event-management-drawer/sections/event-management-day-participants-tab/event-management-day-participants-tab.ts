import { Component, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DrawerCard } from '../../../drawer-card/drawer-card';

type EventManagementDayParticipantType = 'Простой' | 'VIP' | 'Организатор';
type EventManagementDayParticipantPayment = 'Не оплачено' | 'Оплачено';

type EventManagementDayParticipantRow = {
  id: number;
  name: string;
  type: EventManagementDayParticipantType;
  payment: EventManagementDayParticipantPayment;
};

@Component({
  selector: 'app-event-management-day-participants-tab',
  standalone: true,
  imports: [ButtonModule, TableModule, DrawerCard],
  templateUrl: './event-management-day-participants-tab.html',
  styleUrl: './event-management-day-participants-tab.scss',
})
export class EventManagementDayParticipantsTab {
  readonly participantTypeOptions: EventManagementDayParticipantType[] = [
    'Простой',
    'VIP',
    'Организатор',
  ];

  readonly paymentOptions: EventManagementDayParticipantPayment[] = [
    'Не оплачено',
    'Оплачено',
  ];

  readonly savedRows = signal<EventManagementDayParticipantRow[]>(this.buildInitialRows());
  readonly participantRows = signal<EventManagementDayParticipantRow[]>(this.buildInitialRows());

  updateParticipantType(id: number, value: string): void {
    if (!this.isParticipantType(value)) {
      return;
    }

    this.participantRows.update((rows) =>
      rows.map((row) => (row.id === id ? { ...row, type: value } : row))
    );
  }

  updateParticipantPayment(id: number, value: string): void {
    if (!this.isParticipantPayment(value)) {
      return;
    }

    this.participantRows.update((rows) =>
      rows.map((row) => (row.id === id ? { ...row, payment: value } : row))
    );
  }

  cancel(): void {
    this.participantRows.set(this.cloneRows(this.savedRows()));
  }

  save(): void {
    this.savedRows.set(this.cloneRows(this.participantRows()));
  }

  private buildInitialRows(): EventManagementDayParticipantRow[] {
    return Array.from({ length: 9 }, (_, index) => ({
      id: index + 1,
      name: 'Участник',
      type: 'Простой',
      payment: 'Не оплачено',
    }));
  }

  private cloneRows(rows: EventManagementDayParticipantRow[]): EventManagementDayParticipantRow[] {
    return rows.map((row) => ({ ...row }));
  }

  private isParticipantType(value: string): value is EventManagementDayParticipantType {
    return this.participantTypeOptions.includes(value as EventManagementDayParticipantType);
  }

  private isParticipantPayment(value: string): value is EventManagementDayParticipantPayment {
    return this.paymentOptions.includes(value as EventManagementDayParticipantPayment);
  }
}
