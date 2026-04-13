import { Component, computed, signal } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DrawerCard } from '../../../drawer-card/drawer-card';

type EventManagementDayMenuObject = 'Объект 1' | 'Объект 2' | 'Объект 3' | 'Объект 4';

type EventManagementDayMenuRow = {
  id: number;
  objectName: EventManagementDayMenuObject;
  quantity: string;
};

@Component({
  selector: 'app-event-management-day-menu-tab',
  standalone: true,
  imports: [InputTextModule, ButtonModule, TableModule, DrawerCard],
  templateUrl: './event-management-day-menu-tab.html',
  styleUrl: './event-management-day-menu-tab.scss',
})
export class EventManagementDayMenuTab {
  readonly objectOptions: EventManagementDayMenuObject[] = [
    'Объект 1',
    'Объект 2',
    'Объект 3',
    'Объект 4',
  ];

  readonly savedRows = signal<EventManagementDayMenuRow[]>(this.buildInitialRows());
  readonly menuRows = signal<EventManagementDayMenuRow[]>(this.buildInitialRows());

  readonly draftObject = signal<EventManagementDayMenuObject>('Объект 1');
  readonly draftQuantity = signal<string>('1');
  readonly editingRowId = signal<number | null>(null);
  readonly selectedRowIds = signal<number[]>([]);

  readonly isEditing = computed(() => this.editingRowId() !== null);
  readonly canSubmit = computed(() => this.draftQuantity().trim().length > 0);
  readonly canDelete = computed(() => this.selectedRowIds().length > 0);
  readonly canEdit = computed(() => this.selectedRowIds().length === 1);
  readonly areAllRowsSelected = computed(() => {
    const rows = this.menuRows();
    return rows.length > 0 && this.selectedRowIds().length === rows.length;
  });

  onDraftObjectChange(value: string): void {
    if (!this.isObjectOption(value)) {
      return;
    }

    this.draftObject.set(value);
  }

  onDraftQuantityInput(value: string): void {
    this.draftQuantity.set(this.normalizeQuantity(value));
  }

  submit(): void {
    if (!this.canSubmit()) {
      return;
    }

    const quantity = this.draftQuantity().trim();
    const objectName = this.draftObject();
    const editingId = this.editingRowId();

    if (editingId !== null) {
      this.menuRows.update((rows) =>
        rows.map((row) =>
          row.id === editingId
            ? {
                ...row,
                objectName,
                quantity,
              }
            : row
        )
      );
      this.selectedRowIds.set([editingId]);
      this.resetDraft();
      return;
    }

    const nextRow: EventManagementDayMenuRow = {
      id: this.nextRowId(),
      objectName,
      quantity,
    };

    this.menuRows.update((rows) => [nextRow, ...rows]);
    this.selectedRowIds.set([nextRow.id]);
    this.resetDraft();
  }

  startEditing(): void {
    if (!this.canEdit()) {
      return;
    }

    const rowId = this.selectedRowIds()[0];
    const row = this.menuRows().find((candidate) => candidate.id === rowId);

    if (!row) {
      return;
    }

    this.editingRowId.set(row.id);
    this.draftObject.set(row.objectName);
    this.draftQuantity.set(row.quantity);
  }

  deleteSelected(): void {
    const selectedIds = new Set(this.selectedRowIds());

    if (selectedIds.size === 0) {
      return;
    }

    this.menuRows.update((rows) => rows.filter((row) => !selectedIds.has(row.id)));

    if (this.editingRowId() !== null && selectedIds.has(this.editingRowId()!)) {
      this.resetDraft();
    }

    this.selectedRowIds.set([]);
  }

  toggleAllRows(checked: boolean): void {
    if (!checked) {
      this.selectedRowIds.set([]);
      return;
    }

    this.selectedRowIds.set(this.menuRows().map((row) => row.id));
  }

  toggleRowSelection(id: number, checked: boolean): void {
    if (checked) {
      this.selectedRowIds.update((ids) => (ids.includes(id) ? ids : [...ids, id]));
      return;
    }

    this.selectedRowIds.update((ids) => ids.filter((currentId) => currentId !== id));
  }

  isRowSelected(id: number): boolean {
    return this.selectedRowIds().includes(id);
  }

  cancel(): void {
    this.menuRows.set(this.cloneRows(this.savedRows()));
    this.selectedRowIds.set([]);
    this.resetDraft();
  }

  save(): void {
    this.savedRows.set(this.cloneRows(this.menuRows()));
    this.selectedRowIds.set([]);
    this.resetDraft();
  }

  submitButtonLabel(): string {
    return this.isEditing() ? 'Обновить' : 'Добавить';
  }

  private normalizeQuantity(value: string): string {
    return value.replace(/\D/g, '').slice(0, 3);
  }

  private nextRowId(): number {
    return this.menuRows().reduce((max, row) => Math.max(max, row.id), 0) + 1;
  }

  private resetDraft(): void {
    this.editingRowId.set(null);
    this.draftObject.set('Объект 1');
    this.draftQuantity.set('1');
  }

  private buildInitialRows(): EventManagementDayMenuRow[] {
    return [
      { id: 1, objectName: 'Объект 1', quantity: '2' },
      { id: 2, objectName: 'Объект 2', quantity: '4' },
      { id: 3, objectName: 'Объект 3', quantity: '1' },
      { id: 4, objectName: 'Объект 1', quantity: '3' },
      { id: 5, objectName: 'Объект 4', quantity: '2' },
    ];
  }

  private cloneRows(rows: EventManagementDayMenuRow[]): EventManagementDayMenuRow[] {
    return rows.map((row) => ({ ...row }));
  }

  private isObjectOption(value: string): value is EventManagementDayMenuObject {
    return this.objectOptions.includes(value as EventManagementDayMenuObject);
  }
}
