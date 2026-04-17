import { Component, computed, effect, input, output, signal } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DrawerCard } from '../../../drawer-card/drawer-card';
import { ItemApiModel, MenuItemApiModel } from '../../../../core/ui/event-management-drawer-data.service';

type MenuTabRow = {
  itemId: string;
  title: string;
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
  readonly initialMenuItems = input.required<MenuItemApiModel[]>();
  readonly availableItems = input.required<ItemApiModel[]>();
  readonly isLoading = input<boolean>(false);
  readonly isSaving = input<boolean>(false);

  readonly saveRequested = output<MenuItemApiModel[]>();
  readonly cancelRequested = output<void>();

  readonly savedRows = signal<MenuTabRow[]>([]);
  readonly menuRows = signal<MenuTabRow[]>([]);

  readonly draftItemId = signal<string>('');
  readonly draftQuantity = signal<string>('1');
  readonly editingItemId = signal<string | null>(null);
  readonly selectedItemIds = signal<string[]>([]);

  readonly isEditing = computed(() => this.editingItemId() !== null);
  readonly canSubmit = computed(() => this.draftQuantity().trim().length > 0 && this.draftItemId().length > 0);
  readonly canDelete = computed(() => this.selectedItemIds().length > 0);
  readonly canEdit = computed(() => this.selectedItemIds().length === 1);
  readonly areAllRowsSelected = computed(() => {
    const rows = this.menuRows();
    return rows.length > 0 && this.selectedItemIds().length === rows.length;
  });

  constructor() {
    effect(() => {
      const items = this.initialMenuItems();
      const rows: MenuTabRow[] = items.map((item) => ({
        itemId: item.itemId,
        title: item.title,
        quantity: String(item.amount),
      }));
      this.savedRows.set(rows);
      this.menuRows.set(rows.map((r) => ({ ...r })));
      this.resetDraft();
    });

    effect(() => {
      const available = this.availableItems();
      if (available.length > 0 && !this.draftItemId()) {
        this.draftItemId.set(available[0].id);
      }
    });
  }

  onDraftItemChange(value: string): void {
    this.draftItemId.set(value);
  }

  onDraftQuantityInput(value: string): void {
    this.draftQuantity.set(this.normalizeQuantity(value));
  }

  submit(): void {
    if (!this.canSubmit()) {
      return;
    }

    const quantity = this.draftQuantity().trim();
    const itemId = this.draftItemId();
    const editingId = this.editingItemId();
    const item = this.availableItems().find((i) => i.id === itemId);
    const title = item?.title ?? itemId;

    if (editingId !== null) {
      this.menuRows.update((rows) =>
        rows.map((row) =>
          row.itemId === editingId ? { ...row, itemId, title, quantity } : row
        )
      );
      this.selectedItemIds.set([itemId]);
      this.resetDraft();
      return;
    }

    if (this.menuRows().some((r) => r.itemId === itemId)) {
      this.menuRows.update((rows) =>
        rows.map((row) => (row.itemId === itemId ? { ...row, quantity } : row))
      );
    } else {
      this.menuRows.update((rows) => [...rows, { itemId, title, quantity }]);
    }

    this.selectedItemIds.set([itemId]);
    this.resetDraft();
  }

  startEditing(): void {
    if (!this.canEdit()) {
      return;
    }

    const itemId = this.selectedItemIds()[0];
    const row = this.menuRows().find((r) => r.itemId === itemId);

    if (!row) {
      return;
    }

    this.editingItemId.set(row.itemId);
    this.draftItemId.set(row.itemId);
    this.draftQuantity.set(row.quantity);
  }

  deleteSelected(): void {
    const selectedIds = new Set(this.selectedItemIds());

    if (selectedIds.size === 0) {
      return;
    }

    this.menuRows.update((rows) => rows.filter((row) => !selectedIds.has(row.itemId)));

    if (this.editingItemId() !== null && selectedIds.has(this.editingItemId()!)) {
      this.resetDraft();
    }

    this.selectedItemIds.set([]);
  }

  toggleAllRows(checked: boolean): void {
    if (!checked) {
      this.selectedItemIds.set([]);
      return;
    }

    this.selectedItemIds.set(this.menuRows().map((row) => row.itemId));
  }

  toggleRowSelection(itemId: string, checked: boolean): void {
    if (checked) {
      this.selectedItemIds.update((ids) => (ids.includes(itemId) ? ids : [...ids, itemId]));
      return;
    }

    this.selectedItemIds.update((ids) => ids.filter((id) => id !== itemId));
  }

  isRowSelected(itemId: string): boolean {
    return this.selectedItemIds().includes(itemId);
  }

  cancel(): void {
    this.menuRows.set(this.savedRows().map((r) => ({ ...r })));
    this.selectedItemIds.set([]);
    this.resetDraft();
    this.cancelRequested.emit();
  }

  save(): void {
    this.saveRequested.emit(
      this.menuRows().map((row) => ({
        itemId: row.itemId,
        title: row.title,
        amount: Number(row.quantity) || 0,
      }))
    );
  }

  submitButtonLabel(): string {
    return this.isEditing() ? 'Обновить' : 'Добавить';
  }

  private normalizeQuantity(value: string): string {
    return value.replace(/\D/g, '').slice(0, 3);
  }

  private resetDraft(): void {
    this.editingItemId.set(null);
    const available = this.availableItems();
    this.draftItemId.set(available.length > 0 ? available[0].id : '');
    this.draftQuantity.set('1');
  }
}
