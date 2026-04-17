import { Component, effect, input, output, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DrawerCard } from '../../../drawer-card/drawer-card';
import { EventManagementParticipantRow } from '../../../../core/ui/event-management-drawer-data.service';

@Component({
  selector: 'app-event-management-day-participants-tab',
  standalone: true,
  imports: [ButtonModule, TableModule, DrawerCard],
  templateUrl: './event-management-day-participants-tab.html',
  styleUrl: './event-management-day-participants-tab.scss',
})
export class EventManagementDayParticipantsTab {
  readonly participantRows = input.required<EventManagementParticipantRow[]>();
  readonly isSaving = input<boolean>(false);

  readonly saveRequested = output<EventManagementParticipantRow[]>();
  readonly cancelRequested = output<void>();

  readonly savedRows = signal<EventManagementParticipantRow[]>([]);
  readonly editableRows = signal<EventManagementParticipantRow[]>([]);

  constructor() {
    effect(() => {
      const rows = this.participantRows().map((r) => ({ ...r }));
      this.savedRows.set(rows);
      this.editableRows.set(rows.map((r) => ({ ...r })));
    });
  }

  typeLabel(type: 0 | 1 | 2): string {
    if (type === 0) return 'Простой';
    if (type === 1) return 'VIP';
    return 'Организатор';
  }

  paymentLabel(payment: boolean): string {
    return payment ? 'Оплачено' : 'Не оплачено';
  }

  updateParticipantType(registrationId: string, value: string): void {
    const numericValue = Number(value) as 0 | 1 | 2;

    if (numericValue !== 0 && numericValue !== 1 && numericValue !== 2) {
      return;
    }

    this.editableRows.update((rows) =>
      rows.map((row) => (row.registrationId === registrationId ? { ...row, type: numericValue } : row))
    );
  }

  updateParticipantPayment(registrationId: string, value: string): void {
    const boolValue = value === 'true';

    this.editableRows.update((rows) =>
      rows.map((row) => (row.registrationId === registrationId ? { ...row, payment: boolValue } : row))
    );
  }

  cancel(): void {
    this.editableRows.set(this.savedRows().map((r) => ({ ...r })));
    this.cancelRequested.emit();
  }

  save(): void {
    this.saveRequested.emit(this.editableRows());
  }
}
