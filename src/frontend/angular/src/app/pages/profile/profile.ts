import { Component, computed, inject, output, signal } from '@angular/core';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { finalize } from 'rxjs';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { UsersApiService } from '../../core/api/services/users-api.service';
import {
  CurrentUser,
  UpdateCurrentUserPayload,
} from '../../core/api/models/user.models';
import { Web_Dtos_Gender } from '../../core/api/generated';

type GenderOption = {
  label: string;
  value: Web_Dtos_Gender;
};

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [ReactiveFormsModule, InputTextModule, ButtonModule],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
})
export class Profile {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly usersApiService = inject(UsersApiService);

  readonly closeRequested = output<void>();

  readonly genderOptions: GenderOption[] = [
    { label: 'Мужской', value: 0 },
    { label: 'Женский', value: 1 },
  ];

  readonly isLoading = signal<boolean>(true);
  readonly isSaving = signal<boolean>(false);
  readonly loadErrorMessage = signal<string>('');
  readonly saveMessage = signal<string>('');
  readonly passwordEditorOpen = signal<boolean>(false);
  readonly currentUser = signal<CurrentUser | null>(null);
  readonly initialProfileValue = signal<UpdateCurrentUserPayload | null>(null);

  readonly profileForm = this.fb.group(
    {
      name: ['', [Validators.required]],
      phone: ['', [Validators.required]],
      gender: [0 as Web_Dtos_Gender, [Validators.required]],
      password: ['', [Validators.minLength(6)]],
      confirmPassword: [''],
    },
    {
      validators: [this.passwordsMatchValidator],
    }
  );

  readonly profileInitials = computed(() => {
    const name = this.profileForm.controls.name.value.trim() || this.currentUser()?.name || 'User';
    return name
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part.charAt(0).toUpperCase())
      .join('');
  });

  readonly roleLabel = computed(() => {
    switch (this.currentUser()?.role) {
      case 0:
        return 'Администратор';
      case 1:
        return 'Организатор мероприятий';
      case 2:
        return 'Гость';
      default:
        return 'Не указано';
    }
  });

  readonly canSave = computed(() => {
    if (this.isLoading() || this.isSaving()) {
      return false;
    }

    const controls = this.profileForm.controls;
    return (
      !!controls.name.value.trim() &&
      !!controls.phone.value.trim() &&
      !controls.password.invalid &&
      !this.profileForm.errors?.['passwordMismatch']
    );
  });

  constructor() {
    this.loadProfile();
  }

  closeDrawer(): void {
    this.closeRequested.emit();
  }

  togglePasswordEditor(): void {
    this.passwordEditorOpen.update((value) => !value);

    if (!this.passwordEditorOpen()) {
      this.profileForm.controls.password.setValue('');
      this.profileForm.controls.confirmPassword.setValue('');
    }
  }

  cancelChanges(): void {
    const initialValue = this.initialProfileValue();

    if (!initialValue) {
      return;
    }

    this.profileForm.patchValue({
      name: initialValue.name ?? '',
      phone: initialValue.phone ?? '',
      gender: initialValue.gender ?? 0,
      password: '',
      confirmPassword: '',
    });
    this.passwordEditorOpen.set(false);
    this.saveMessage.set('');
  }

  saveChanges(): void {
    if (!this.canSave()) {
      this.saveMessage.set(this.profileForm.errors?.['passwordMismatch']
        ? 'Пароли не совпадают.'
        : 'Заполните обязательные поля профиля.');
      return;
    }

    const { name, phone, gender, password } = this.profileForm.getRawValue();
    const payload: UpdateCurrentUserPayload = {
      name: name.trim(),
      phone: phone.trim(),
      gender,
      password: password || undefined,
    };

    this.saveMessage.set('');
    this.isSaving.set(true);

    this.usersApiService.updateMe(payload)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          const currentUser = this.currentUser();

          if (currentUser) {
            this.currentUser.set({
              ...currentUser,
              name: payload.name ?? currentUser.name,
              phone: payload.phone ?? currentUser.phone,
              gender: payload.gender ?? currentUser.gender,
            });
          }

          this.initialProfileValue.set({
            name: payload.name,
            phone: payload.phone,
            gender: payload.gender,
          });
          this.profileForm.controls.password.setValue('');
          this.profileForm.controls.confirmPassword.setValue('');
          this.passwordEditorOpen.set(false);
          this.saveMessage.set('Изменения сохранены.');
        },
        error: (error: unknown) => {
          this.saveMessage.set(this.mapErrorMessage(error));
        },
      });
  }

  private loadProfile(): void {
    this.isLoading.set(true);
    this.loadErrorMessage.set('');

    this.usersApiService.getMe()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (user) => {
          this.currentUser.set(user);
          this.initialProfileValue.set({
            name: user.name,
            phone: user.phone,
            gender: user.gender,
          });
          this.profileForm.patchValue({
            name: user.name,
            phone: user.phone,
            gender: user.gender,
            password: '',
            confirmPassword: '',
          });
        },
        error: (error: unknown) => {
          this.loadErrorMessage.set(this.mapErrorMessage(error));
        },
      });
  }

  private passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value ?? '';
    const confirmPassword = control.get('confirmPassword')?.value ?? '';

    if (!password && !confirmPassword) {
      return null;
    }

    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  private mapErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 401) {
      return 'Сессия недействительна. Выполните вход снова.';
    }

    if (status === 404) {
      return 'Профиль пользователя не найден.';
    }

    if (status === 400) {
      return 'Не удалось сохранить изменения. Проверьте введенные данные.';
    }

    return 'Сервер временно недоступен. Повторите попытку позже.';
  }

  private extractStatusCode(error: unknown): number | null {
    if (typeof error !== 'object' || error === null || !('status' in error)) {
      return null;
    }

    const status = (error as { status?: unknown }).status;
    return typeof status === 'number' ? status : null;
  }
}
