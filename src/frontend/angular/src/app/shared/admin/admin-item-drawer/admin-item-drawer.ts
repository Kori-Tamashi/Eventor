import { Component, computed, effect, input, output, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { InputTextModule } from 'primeng/inputtext';
import {
  AdminItemDraftValue,
  AdminItemEditorMode,
  AdminItemSavePayload,
} from './admin-item.models';

@Component({
  selector: 'app-admin-item-drawer',
  standalone: true,
  imports: [ButtonModule, DrawerModule, InputTextModule],
  templateUrl: './admin-item-drawer.html',
  styleUrl: './admin-item-drawer.scss',
})
export class AdminItemDrawer {
  readonly visible = input<boolean>(false);
  readonly mode = input<AdminItemEditorMode>('create');
  readonly initialValue = input<AdminItemDraftValue | null>(null);
  readonly isLoading = input<boolean>(false);
  readonly isSaving = input<boolean>(false);
  readonly errorMessage = input<string>('');

  readonly visibleChange = output<boolean>();
  readonly closeRequested = output<void>();
  readonly saveRequested = output<AdminItemSavePayload>();

  readonly titleMaxLen = 70;

  readonly title = signal<string>('');
  readonly cost = signal<string>('');

  readonly drawerTitle = computed(() => this.mode() === 'edit' ? 'Редактировать объект' : 'Создать объект');
  readonly titleCountLabel = computed(() => `${this.title().length}/${this.titleMaxLen}`);
  readonly canSave = computed(() => {
    const title = this.title().trim();
    const cost = this.parseNumber(this.cost());
    return title.length > 0 && cost !== null && !this.isLoading() && !this.isSaving();
  });

  constructor() {
    effect(() => {
      const value = this.initialValue();
      this.title.set(value?.title ?? '');
      this.cost.set(value?.cost ?? '');
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

  onCostInput(value: string): void {
    this.cost.set(value.replace(/[^0-9.,]/g, '').slice(0, 12));
  }

  close(): void {
    this.closeRequested.emit();
    this.visibleChange.emit(false);
  }

  save(): void {
    const title = this.title().trim();
    const cost = this.parseNumber(this.cost());

    if (!title || cost === null) {
      return;
    }

    this.saveRequested.emit({ title, cost });
  }

  private parseNumber(value: string): number | null {
    const normalized = value.trim().replace(',', '.');
    if (!normalized) {
      return null;
    }

    const parsed = Number(normalized);
    return Number.isFinite(parsed) ? parsed : null;
  }
}
