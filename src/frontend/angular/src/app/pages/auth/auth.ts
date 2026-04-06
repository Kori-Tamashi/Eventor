import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { map } from 'rxjs/operators';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';

type AuthMode = 'login' | 'register';

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

  readonly authForm = this.fb.group({
    username: [''],
    phone: [''],
    password: [''],
    confirmPassword: [''],
  });

  onSubmit(event: Event): void {
    event.preventDefault();
    this.router.navigate(['/app/dashboard']);
  }

  private normalizeMode(value: string | null): AuthMode {
    return value === 'register' ? 'register' : 'login';
  }
}
