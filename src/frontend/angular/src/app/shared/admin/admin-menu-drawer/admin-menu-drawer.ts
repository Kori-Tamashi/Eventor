import { Component, computed, effect, input, output, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { TextareaModule } from 'primeng/textarea';
import { ItemApiModel } from '../../../core/api/services/items-api.service';
import {
  AdminMenuDrawerData,
  AdminMenuEditorItem,
  AdminMenuEditorMode,
  AdminMenuSavePayload,
} from './admin-menu.models';

@Component({
  selector: 'app-admin-menu-drawer',
  standalone: true,
  imports: [ButtonModule, DrawerModule, InputTextModule, TextareaModule, TableModule],
  templateUrl: './admin-menu-drawer.html',
  styleUrl: './admin-menu-drawer.scss',
})
export class AdminMenuDrawer {
  readonly visible = input<boolean>(false);
  readonly mode = input<AdminMenuEditorMode>('create');
  readonly data = input<AdminMenuDrawerData | null>(null);
  readonly isLoading = input<boolean>(false);
  readonly isSaving = input<boolean>(false);
  readonly errorMessage = input<string>('');

  readonly visibleChange = output<boolean>();
  readonly closeRequested = output<void>();
  readonly saveRequested = output<AdminMenuSavePayload>();

  readonly titleMaxLen = 70;
  readonly descriptionMaxLen = 250;

  readonly title = signal<string>('');
  readonly description = signal<string>('');
  readonly rows = signal<AdminMenuEditorItem[]>([]);
  readonly draftItemId = signal<string>('');
  readonly draftAmount = signal<string>('1');
  readonly editingItemId = signal<string | null>(null);

  readonly drawerTitle = computed(() => this.mode() === 'edit' ? 'Редактировать меню' : 'Создать меню');
  readonly titleCountLabel = computed(() => `${this.title().length}/${this.titleMaxLen}`);
  readonly descriptionCountLabel = computed(() => `${this.description().length}/${this.descriptionMaxLen}`);
  readonly availableItems = computed(() => this.data()?.availableItems ?? []);
  readonly isEditingRow = computed(() => this.editingItemId() !== null);
  readonly canSubmitItem = computed(() => this.draftItemId().length > 0 && this.draftAmount().trim().length > 0);
  readonly canSave = computed(() => {
    return this.title().trim().length > 0 && !this.isLoading() && !this.isSaving();
  });

  constructor() {
    effect(() => {
      const data = this.data();
      const items = data?.draft.items ?? [];

      this.title.set(data?.draft.title ?? '');
      this.description.set(data?.draft.description ?? '');
      this.rows.set(items.map((item) => ({ ...item })));
      this.editingItemId.set(null);
      this.draftItemId.set(data?.availableItems[0]?.id ?? '');
      this.draftAmount.set('1');
    });
  }

  onDrawerVisibleChange(visible: boolean): void {
    this.visibleChange.emit(visible);
    if (!visible) {
      this.closeRequested.emit();
    }
  }

  onTitleInput(value: string): void {
    this.title.set(value.slice(0, this.titleMaxLen));
  }

  onDescriptionInput(value: string): void {
    this.description.set(value.slice(0, this.descriptionMaxLen));
  }

  onDraftItemChange(value: string): void {
    this.draftItemId.set(value);
  }

  onDraftAmountInput(value: string): void {
    this.draftAmount.set(value.replace(/\D/g, '').slice(0, 4));
  }

  submitItem(): void {
    if (!this.canSubmitItem()) {
      return;
    }

    const itemId = this.draftItemId();
    const amount = this.draftAmount().trim();
    const item = this.availableItems().find((candidate) => candidate.id === itemId);
    const title = item?.title ?? itemId;

    if (this.editingItemId()) {
      const editingId = this.editingItemId()!;
      this.rows.update((rows) =>
        rows.map((row) => row.itemId === editingId ? { itemId, title, amount } : row)
      );
      this.resetItemDraft();
      return;
    }

    if (this.rows().some((row) => row.itemId === itemId)) {
      this.rows.update((rows) =>
        rows.map((row) => row.itemId === itemId ? { ...row, amount } : row)
      );
    } else {
      this.rows.update((rows) => [...rows, { itemId, title, amount }]);
    }

    this.resetItemDraft();
  }

  editItem(row: AdminMenuEditorItem): void {
    this.editingItemId.set(row.itemId);
    this.draftItemId.set(row.itemId);
    this.draftAmount.set(row.amount);
  }

  deleteItem(row: AdminMenuEditorItem): void {
    this.rows.update((rows) => rows.filter((candidate) => candidate.itemId !== row.itemId));

    if (this.editingItemId() === row.itemId) {
      this.resetItemDraft();
    }
  }

  close(): void {
    this.closeRequested.emit();
    this.visibleChange.emit(false);
  }

  save(): void {
    const title = this.title().trim();
    if (!title) {
      return;
    }

    this.saveRequested.emit({
      title,
      description: this.description().trim() || null,
      items: this.rows().map((row) => ({
        itemId: row.itemId,
        title: row.title,
        amount: Number(row.amount) || 0,
      })),
    });
  }

  private resetItemDraft(): void {
    this.editingItemId.set(null);
    this.draftItemId.set(this.availableItems()[0]?.id ?? '');
    this.draftAmount.set('1');
  }
}
