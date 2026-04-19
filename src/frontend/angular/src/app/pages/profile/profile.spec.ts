import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { of } from 'rxjs';

import { Profile } from './profile';
import { AuthApiService } from '../../core/api/services/auth-api.service';
import { UsersApiService } from '../../core/api/services/users-api.service';

describe('Profile', () => {
  let component: Profile;
  let fixture: ComponentFixture<Profile>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Profile],
      providers: [
        provideHttpClient(),
        {
          provide: UsersApiService,
          useValue: {
            getMe: () => of({
              id: 'user-1',
              name: 'Username',
              phone: '+7 900 000-00-00',
              gender: 0,
              role: 1,
              passwordHash: null,
            }),
            updateMe: () => of(void 0),
            deleteMe: () => of(void 0),
          },
        },
        {
          provide: AuthApiService,
          useValue: {
            logout: () => undefined,
          },
        },
      ],
    })
    .compileComponents();

    fixture = TestBed.createComponent(Profile);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
