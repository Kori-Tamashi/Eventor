import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { map } from 'rxjs/operators';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';

type AuthMode = 'login' | 'register';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [RouterLink, InputTextModule, ButtonModule, PasswordModule],
  templateUrl: './auth.html',
  styleUrl: './auth.scss',
})
export class Auth {
  private readonly route = inject(ActivatedRoute);

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

  onSubmit(event: Event): void {
    event.preventDefault();
  }

  private normalizeMode(value: string | null): AuthMode {
    return value === 'register' ? 'register' : 'login';
  }
}
