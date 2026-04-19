import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { Organization } from './organization';
import { EventsApiService } from '../../core/api/services/events-api.service';
import { LocationsApiService } from '../../core/api/services/locations-api.service';
import { UsersApiService } from '../../core/api/services/users-api.service';

describe('Organization', () => {
  let component: Organization;
  let fixture: ComponentFixture<Organization>;

  beforeEach(async () => {
    if (!(globalThis as any).ResizeObserver) {
      (globalThis as any).ResizeObserver = class {
        observe() {}
        unobserve() {}
        disconnect() {}
      };
    }

    await TestBed.configureTestingModule({
      imports: [Organization],
      providers: [
        provideRouter([]),
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
            listOrganizedEvents: () => of([]),
            deleteEvent: () => of(void 0),
          },
        },
        {
          provide: LocationsApiService,
          useValue: {
            getLocationTitleMap: () => of({}),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Organization);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
