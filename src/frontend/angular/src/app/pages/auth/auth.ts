import { Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize, switchMap } from 'rxjs';
import { map } from 'rxjs/operators';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { AuthApiService } from '../../core/api/services/auth-api.service';
import { Web_Dtos_Gender } from '../../core/api/generated';

type AuthMode = 'login' | 'register';
type GenderOption = {
  label: string;
  value: Web_Dtos_Gender;
};

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [
    RouterLink,
    ReactiveFormsModule,
    InputTextModule,
    ButtonModule,
    PasswordModule
  ],
  templateUrl: './auth.html',
  styleUrl: './auth.scss',
})
export class Auth {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly authApiService = inject(AuthApiService);

  private readonly routeMode = toSignal(
    this.route.queryParamMap.pipe(
      map((params) => this.normalizeMode(params.get('mode')))
    ),
    { initialValue: 'login' as AuthMode }
  );

  readonly mode = computed(() => this.routeMode());
  readonly isRegisterMode = computed(() => this.mode() === 'register');

  readonly title = computed(() =>
    this.isRegisterMode() ? 'Создать аккаунт Eventor' : 'Войти в Eventor'
  );

  readonly submitLabel = computed(() =>
    this.isRegisterMode() ? 'Зарегистрироваться' : 'Войти'
  );

  readonly footerText = computed(() =>
    this.isRegisterMode() ? 'Уже есть аккаунт?' : 'Впервые в Eventor?'
  );

  readonly footerActionText = computed(() =>
    this.isRegisterMode() ? 'Войти' : 'Создать аккаунт'
  );

  readonly footerQueryParams = computed(() => ({
    mode: this.isRegisterMode() ? 'login' : 'register',
  }));

  readonly passwordAutocomplete = computed(() =>
    this.isRegisterMode() ? 'new-password' : 'current-password'
  );

  readonly genderOptions: GenderOption[] = [
    { label: 'Мужской', value: 0 },
    { label: 'Женский', value: 1 },
  ];

  readonly isSubmitting = signal<boolean>(false);
  readonly errorMessage = signal<string>('');

  readonly authForm = this.fb.group({
    name: ['', [Validators.required]],
    phone: ['', [Validators.required]],
    gender: [0 as Web_Dtos_Gender, [Validators.required]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: [''],
  }, {
    validators: [this.passwordsMatchValidator],
  });

  constructor() {
    effect(() => {
      this.isRegisterMode();
      this.errorMessage.set('');
      this.authForm.controls.password.setValue('');
      this.authForm.controls.confirmPassword.setValue('');
    });
  }

  onSubmit(event: Event): void {
    event.preventDefault();

    if (this.isSubmitting()) {
      return;
    }

    const controls = this.authForm.controls;
    const isRegister = this.isRegisterMode();
    const isInvalid = isRegister
      ? (
          !controls.name.value.trim() ||
          !controls.phone.value.trim() ||
          !controls.password.value ||
          !controls.confirmPassword.value ||
          !!this.authForm.errors?.['passwordMismatch'] ||
          controls.password.invalid
        )
      : (
          !controls.phone.value.trim() ||
          !controls.password.value
        );

    if (isInvalid) {
      this.errorMessage.set(this.buildValidationMessage());
      return;
    }

    this.errorMessage.set('');
    this.isSubmitting.set(true);

    const { name, phone, gender, password } = this.authForm.getRawValue();

    const request$ = this.isRegisterMode()
      ? this.authApiService.register({
          name: name.trim(),
          phone: phone.trim(),
          gender,
          password,
        }).pipe(
          switchMap(() =>
            this.authApiService.login({
              phone: phone.trim(),
              password,
            })
          )
        )
      : this.authApiService.login({
          phone: phone.trim(),
          password,
        });

    request$
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: () => {
          this.router.navigate(['/app/dashboard']);
        },
        error: (error: unknown) => {
          this.errorMessage.set(this.mapErrorMessage(error));
        },
      });
  }

  private normalizeMode(value: string | null): AuthMode {
    return value === 'register' ? 'register' : 'login';
  }

  private passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value ?? '';
    const confirmPassword = control.get('confirmPassword')?.value ?? '';

    if (!confirmPassword) {
      return null;
    }

    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  private buildValidationMessage(): string {
    if (!this.isRegisterMode()) {
      return 'Заполните номер телефона и пароль.';
    }

    if (this.authForm.errors?.['passwordMismatch']) {
      return 'Пароли не совпадают.';
    }

    if (this.authForm.controls.password.invalid) {
      return 'Пароль должен содержать не менее 6 символов.';
    }

    return 'Заполните все обязательные поля для регистрации.';
  }

  private mapErrorMessage(error: unknown): string {
    const status = this.extractStatusCode(error);

    if (status === 401) {
      return 'Неверный номер телефона или пароль.';
    }

    if (status === 400) {
      return this.isRegisterMode()
        ? 'Не удалось зарегистрироваться. Проверьте введенные данные.'
        : 'Не удалось выполнить вход. Проверьте введенные данные.';
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
