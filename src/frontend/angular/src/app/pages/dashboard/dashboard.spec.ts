import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { Dashboard } from './dashboard';
import { EventsApiService } from '../../core/api/services/events-api.service';
import { RegistrationsApiService } from '../../core/api/services/registrations-api.service';
import { UsersApiService } from '../../core/api/services/users-api.service';

describe('Dashboard', () => {
  let component: Dashboard;
  let fixture: ComponentFixture<Dashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [
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
          },
        },
        {
          provide: EventsApiService,
          useValue: {
            getEvent: () => of({
              id: 'event-1',
              title: 'Мероприятие',
              description: 'Описание',
              startDate: '2026-04-19',
              locationId: 'location-1',
              daysCount: 1,
              percent: 10,
              createdByUserId: 'user-1',
              rating: 4.5,
              personCount: 10,
            }),
          },
        },
        {
          provide: RegistrationsApiService,
          useValue: {
            listByUser: () => of([]),
            deleteRegistration: () => of(void 0),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Dashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
