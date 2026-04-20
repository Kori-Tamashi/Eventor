import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { Auth } from './auth';
import { AuthApiService } from '../../core/api/services/auth-api.service';

describe('Auth', () => {
  let fixture: ComponentFixture<Auth>;
  let component: Auth;
  let authApiService: Pick<AuthApiService, 'login' | 'register'>;
  let router: Router;
  let routeStub: { queryParamMap: Observable<ReturnType<typeof convertToParamMap>> };

  async function createComponent(mode: 'login' | 'register' = 'login'): Promise<void> {
    routeStub.queryParamMap = of(convertToParamMap(mode === 'register' ? { mode } : {}));

    fixture = TestBed.createComponent(Auth);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  }

  beforeEach(async () => {
    authApiService = {
      login: vi.fn(() => of({ accessToken: 'token' })),
      register: vi.fn(() => of(void 0)),
    } as unknown as Pick<AuthApiService, 'login' | 'register'>;

    routeStub = {
      queryParamMap: of(convertToParamMap({})),
    };

    await TestBed.configureTestingModule({
      imports: [Auth],
      providers: [
        provideRouter([]),
        { provide: AuthApiService, useValue: authApiService },
        { provide: ActivatedRoute, useValue: routeStub },
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  it('creates the component', async () => {
    await createComponent();
    expect(component).toBeTruthy();
  });

  it('shows validation message for invalid login submit', async () => {
    await createComponent();

    component.onSubmit(new Event('submit'));

    expect(component.errorMessage()).toBe('Заполните номер телефона и пароль.');
    expect(authApiService.login).not.toHaveBeenCalled();
  });

  it('logs in and navigates to dashboard', async () => {
    await createComponent();
    component.authForm.patchValue({
      phone: ' +7 999 123 45 67 ',
      password: 'secret123',
    });

    component.onSubmit(new Event('submit'));

    expect(authApiService.login).toHaveBeenCalledWith({
      phone: '+7 999 123 45 67',
      password: 'secret123',
    });
    expect(router.navigate).toHaveBeenCalledWith(['/app/dashboard']);
  });

  it('shows password mismatch message in register mode', async () => {
    await createComponent('register');
    component.authForm.patchValue({
      name: 'User',
      phone: '+7 999 123 45 67',
      password: 'secret123',
      confirmPassword: 'different',
    });

    component.onSubmit(new Event('submit'));

    expect(component.errorMessage()).toBe('Пароли не совпадают.');
    expect(authApiService.register).not.toHaveBeenCalled();
  });

  it('registers and then logs in in register mode', async () => {
    await createComponent('register');
    component.authForm.patchValue({
      name: ' Test User ',
      phone: ' +7 999 123 45 67 ',
      gender: 1,
      password: 'secret123',
      confirmPassword: 'secret123',
    });

    component.onSubmit(new Event('submit'));

    expect(authApiService.register).toHaveBeenCalledWith({
      name: 'Test User',
      phone: '+7 999 123 45 67',
      gender: 1,
      password: 'secret123',
    });
    expect(authApiService.login).toHaveBeenCalledWith({
      phone: '+7 999 123 45 67',
      password: 'secret123',
    });
    expect(router.navigate).toHaveBeenCalledWith(['/app/dashboard']);
  });

  it('maps login 401 to a user-facing error', async () => {
    authApiService.login = vi.fn(() => throwError(() => ({ status: 401 }))) as AuthApiService['login'];
    await createComponent();
    component.authForm.patchValue({
      phone: '+7 999 123 45 67',
      password: 'secret123',
    });

    component.onSubmit(new Event('submit'));

    expect(component.errorMessage()).toBe('Неверный номер телефона или пароль.');
  });
});
