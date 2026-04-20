import { Component, computed, effect, input, output, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import {
  AdminLocationDraftValue,
  AdminLocationEditorMode,
  AdminLocationSavePayload,
} from './admin-location.models';

@Component({
  selector: 'app-admin-location-drawer',
  standalone: true,
  imports: [ButtonModule, DrawerModule, InputTextModule, TextareaModule],
  templateUrl: './admin-location-drawer.html',
  styleUrl: './admin-location-drawer.scss',
})
export class AdminLocationDrawer {
  readonly visible = input<boolean>(false);
  readonly mode = input<AdminLocationEditorMode>('create');
  readonly initialValue = input<AdminLocationDraftValue | null>(null);
  readonly isLoading = input<boolean>(false);
  readonly isSaving = input<boolean>(false);
  readonly errorMessage = input<string>('');

  readonly visibleChange = output<boolean>();
  readonly saveRequested = output<AdminLocationSavePayload>();
  readonly closeRequested = output<void>();

  readonly titleMaxLen = 70;
  readonly descriptionMaxLen = 250;

  readonly title = signal<string>('');
  readonly description = signal<string>('');
  readonly cost = signal<string>('');
  readonly capacity = signal<string>('');

  readonly drawerTitle = computed(() => this.mode() === 'edit' ? 'Редактировать локацию' : 'Создать локацию');
  readonly titleCountLabel = computed(() => `${this.title().length}/${this.titleMaxLen}`);
  readonly descriptionCountLabel = computed(() => `${this.description().length}/${this.descriptionMaxLen}`);
  readonly canSave = computed(() => {
    const title = this.title().trim();
    const cost = this.parseNumber(this.cost());
    const capacity = this.parseInteger(this.capacity());

    return title.length > 0 && cost !== null && capacity !== null && !this.isLoading() && !this.isSaving();
  });

  constructor() {
    effect(() => {
      const value = this.initialValue();

      this.title.set(value?.title ?? '');
      this.description.set(value?.description ?? '');
      this.cost.set(value?.cost ?? '');
      this.capacity.set(value?.capacity ?? '');
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

  onCostInput(value: string): void {
    this.cost.set(value.replace(/[^0-9.,]/g, '').slice(0, 12));
  }

  onCapacityInput(value: string): void {
    this.capacity.set(value.replace(/\D/g, '').slice(0, 6));
  }

  close(): void {
    this.closeRequested.emit();
    this.visibleChange.emit(false);
  }

  save(): void {
    const title = this.title().trim();
    const cost = this.parseNumber(this.cost());
    const capacity = this.parseInteger(this.capacity());

    if (!title || cost === null || capacity === null) {
      return;
    }

    this.saveRequested.emit({
      title,
      description: this.description().trim() || null,
      cost,
      capacity,
    });
  }

  private parseNumber(value: string): number | null {
    const normalized = value.trim().replace(',', '.');
    if (!normalized) {
      return null;
    }

    const parsed = Number(normalized);
    return Number.isFinite(parsed) ? parsed : null;
  }

  private parseInteger(value: string): number | null {
    const trimmed = value.trim();
    if (!trimmed) {
      return null;
    }

    const parsed = Number(trimmed);
    return Number.isInteger(parsed) ? parsed : null;
  }
}
