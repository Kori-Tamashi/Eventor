import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { Profile } from './profile';
import { AuthApiService } from '../../core/api/services/auth-api.service';
import { UsersApiService } from '../../core/api/services/users-api.service';
import { UiNotificationsService } from '../../core/ui/ui-notifications.service';

describe('Profile', () => {
  let fixture: ComponentFixture<Profile>;
  let component: Profile;
  let usersApiService: Pick<UsersApiService, 'getMe' | 'updateMe' | 'deleteMe'>;
  let authApiService: Pick<AuthApiService, 'logout'>;
  let uiNotificationsService: Pick<UiNotificationsService, 'success' | 'error' | 'info'>;

  beforeEach(async () => {
    usersApiService = {
      getMe: vi.fn(() => of({
        id: 'user-1',
        name: 'Username',
        phone: '+7 900 000-00-00',
        gender: 0 as const,
        role: 1 as const,
        passwordHash: null,
      })),
      updateMe: vi.fn(() => of(void 0)),
      deleteMe: vi.fn(() => of(void 0)),
    } as unknown as Pick<UsersApiService, 'getMe' | 'updateMe' | 'deleteMe'>;
    authApiService = {
      logout: vi.fn(),
    } as unknown as Pick<AuthApiService, 'logout'>;
    uiNotificationsService = {
      success: vi.fn(),
      error: vi.fn(),
      info: vi.fn(),
    } as unknown as Pick<UiNotificationsService, 'success' | 'error' | 'info'>;

    await TestBed.configureTestingModule({
      imports: [Profile],
      providers: [
        { provide: UsersApiService, useValue: usersApiService },
        { provide: AuthApiService, useValue: authApiService },
        { provide: UiNotificationsService, useValue: uiNotificationsService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Profile);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('creates the component', () => {
    expect(component).toBeTruthy();
  });

  it('loads profile data into the form', () => {
    expect(component.profileForm.getRawValue().name).toBe('Username');
    expect(component.profileForm.getRawValue().phone).toBe('+7 900 000-00-00');
    expect(component.roleLabel()).toBe('Пользователь');
  });

  it('saves profile changes and shows a success notification', () => {
    component.profileForm.patchValue({
      name: ' Updated User ',
      phone: ' +7 911 111-11-11 ',
      gender: 1,
      password: 'secret123',
      confirmPassword: 'secret123',
    });

    component.saveChanges();

    expect(usersApiService.updateMe).toHaveBeenCalledWith({
      name: 'Updated User',
      phone: '+7 911 111-11-11',
      gender: 1,
      password: 'secret123',
    });
    expect(uiNotificationsService.success).toHaveBeenCalledWith('Изменения сохранены.');
    expect(component.saveMessage()).toBe('');
  });

  it('emits session end after successful account deletion', () => {
    const emitSpy = vi.spyOn(component.sessionEnded, 'emit');

    component.deleteAccount();

    expect(usersApiService.deleteMe).toHaveBeenCalled();
    expect(authApiService.logout).toHaveBeenCalled();
    expect(emitSpy).toHaveBeenCalled();
  });

  it('restores initial values when changes are cancelled', () => {
    component.profileForm.patchValue({
      name: 'Changed Name',
      phone: '+7 911 111-11-11',
      gender: 1,
    });

    component.cancelChanges();

    expect(component.profileForm.getRawValue().name).toBe('Username');
    expect(component.profileForm.getRawValue().phone).toBe('+7 900 000-00-00');
    expect(component.profileForm.getRawValue().gender).toBe(0);
  });

  it('maps update 401 to a session error', () => {
    usersApiService.updateMe = vi.fn(() => throwError(() => ({ status: 401 }))) as UsersApiService['updateMe'];
    component.profileForm.patchValue({
      name: 'User',
      phone: '+7 999 000-00-00',
      gender: 0,
    });

    component.saveChanges();

    expect(component.saveMessage()).toBe('Сессия недействительна. Выполните вход снова.');
  });
});
