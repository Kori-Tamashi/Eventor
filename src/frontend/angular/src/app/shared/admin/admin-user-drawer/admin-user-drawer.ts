import { Component, computed, effect, input, output, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { InputTextModule } from 'primeng/inputtext';
import { Web_Dtos_Gender, Web_Dtos_UserRole } from '../../../core/api/generated';
import {
  AdminUserDraftValue,
  AdminUserEditorMode,
  AdminUserSavePayload,
} from './admin-user.models';

type SelectOption<TValue> = {
  label: string;
  value: TValue;
};

@Component({
  selector: 'app-admin-user-drawer',
  standalone: true,
  imports: [ButtonModule, DrawerModule, InputTextModule],
  templateUrl: './admin-user-drawer.html',
  styleUrl: './admin-user-drawer.scss',
})
export class AdminUserDrawer {
  readonly visible = input<boolean>(false);
  readonly mode = input<AdminUserEditorMode>('create');
  readonly initialValue = input<AdminUserDraftValue | null>(null);
  readonly isLoading = input<boolean>(false);
  readonly isSaving = input<boolean>(false);
  readonly errorMessage = input<string>('');

  readonly visibleChange = output<boolean>();
  readonly closeRequested = output<void>();
  readonly saveRequested = output<AdminUserSavePayload>();

  readonly genderOptions: SelectOption<Web_Dtos_Gender>[] = [
    { label: 'Мужской', value: 0 },
    { label: 'Женский', value: 1 },
  ];

  readonly roleOptions: SelectOption<Web_Dtos_UserRole>[] = [
    { label: 'Администратор', value: 0 },
    { label: 'Организатор мероприятий', value: 1 },
    { label: 'Гость', value: 2 },
  ];

  readonly titleMaxLen = 70;

  readonly name = signal<string>('');
  readonly phone = signal<string>('');
  readonly gender = signal<Web_Dtos_Gender>(0);
  readonly role = signal<Web_Dtos_UserRole>(2);
  readonly password = signal<string>('');

  readonly drawerTitle = computed(() => this.mode() === 'edit' ? 'Редактировать пользователя' : 'Создать пользователя');
  readonly titleCountLabel = computed(() => `${this.name().length}/${this.titleMaxLen}`);
  readonly passwordRequired = computed(() => this.mode() === 'create');
  readonly canSave = computed(() => {
    const hasPassword = this.passwordRequired() ? this.password().trim().length >= 6 : true;
    return (
      this.name().trim().length > 0 &&
      this.phone().trim().length > 0 &&
      hasPassword &&
      !this.isLoading() &&
      !this.isSaving()
    );
  });

  constructor() {
    effect(() => {
      const value = this.initialValue();

      this.name.set(value?.name ?? '');
      this.phone.set(value?.phone ?? '');
      this.gender.set(value?.gender ?? 0);
      this.role.set(value?.role ?? 2);
      this.password.set(value?.password ?? '');
    });
  }

  onDrawerVisibleChange(visible: boolean): void {
    this.visibleChange.emit(visible);
    if (!visible) {
      this.closeRequested.emit();
    }
  }

  onNameInput(value: string): void {
    this.name.set(value.slice(0, this.titleMaxLen));
  }

  onPhoneInput(value: string): void {
    this.phone.set(value.slice(0, 32));
  }

  onGenderChange(value: string): void {
    this.gender.set(Number(value) as Web_Dtos_Gender);
  }

  onRoleChange(value: string): void {
    this.role.set(Number(value) as Web_Dtos_UserRole);
  }

  onPasswordInput(value: string): void {
    this.password.set(value.slice(0, 64));
  }

  close(): void {
    this.closeRequested.emit();
    this.visibleChange.emit(false);
  }

  save(): void {
    const password = this.password().trim();

    if (!this.canSave()) {
      return;
    }

    this.saveRequested.emit({
      name: this.name().trim(),
      phone: this.phone().trim(),
      gender: this.gender(),
      role: this.role(),
      password: password || undefined,
    });
  }
}
