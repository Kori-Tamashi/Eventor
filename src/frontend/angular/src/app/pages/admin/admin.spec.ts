import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { Admin } from './admin';
import { ItemsApiService } from '../../core/api/services/items-api.service';
import { LocationsApiService } from '../../core/api/services/locations-api.service';
import { MenusApiService } from '../../core/api/services/menus-api.service';
import { AdminEventsApiService } from '../../core/api/services/admin-events-api.service';
import { AdminFeedbacksApiService } from '../../core/api/services/admin-feedbacks-api.service';
import { AdminUsersApiService } from '../../core/api/services/admin-users-api.service';
import { FeedbacksApiService } from '../../core/api/services/feedbacks-api.service';
import { RegistrationsApiService } from '../../core/api/services/registrations-api.service';
import { EventsApiService } from '../../core/api/services/events-api.service';
import { EventDetailsDrawerStore } from '../../core/ui/event-details-drawer.store';
import { EventManagementDrawerStore } from '../../core/ui/event-management-drawer.store';

describe('Admin', () => {
  let component: Admin;
  let fixture: ComponentFixture<Admin>;

  beforeEach(async () => {
    if (!(globalThis as unknown as { ResizeObserver?: unknown }).ResizeObserver) {
      (globalThis as unknown as { ResizeObserver: typeof ResizeObserver }).ResizeObserver = class {
        observe() {}
        unobserve() {}
        disconnect() {}
      } as unknown as typeof ResizeObserver;
    }

    await TestBed.configureTestingModule({
      imports: [Admin],
      providers: [
        provideRouter([]),
        {
          provide: LocationsApiService,
          useValue: {
            listLocations: () => of([]),
            getLocation: () => of({
              id: 'location-1',
              title: 'Локация',
              description: 'Описание',
              cost: 1000,
              capacity: 100,
            }),
            createLocation: () => of({
              id: 'location-1',
              title: 'Локация',
              description: 'Описание',
              cost: 1000,
              capacity: 100,
            }),
            updateLocation: () => of(void 0),
            deleteLocation: () => of(void 0),
          },
        },
        {
          provide: ItemsApiService,
          useValue: {
            listItems: () => of([]),
            getItem: () => of({
              id: 'item-1',
              title: 'Объект',
              cost: 1000,
            }),
            createItem: () => of({
              id: 'item-1',
              title: 'Объект',
              cost: 1000,
            }),
            updateItem: () => of(void 0),
            deleteItem: () => of(void 0),
          },
        },
        {
          provide: MenusApiService,
          useValue: {
            listMenus: () => of([]),
            getMenu: () => of({
              id: 'menu-1',
              title: 'Меню',
              description: 'Описание',
              menuItems: [],
            }),
            createMenu: () => of({
              id: 'menu-1',
              title: 'Меню',
              description: 'Описание',
              menuItems: [],
            }),
            listMenuItems: () => of([]),
            getMenuItemAmount: () => of(1),
            updateMenu: () => of(void 0),
            deleteMenu: () => of(void 0),
            addMenuItem: () => of(void 0),
            updateMenuItemAmount: () => of(void 0),
            removeMenuItem: () => of(void 0),
          },
        },
        {
          provide: AdminUsersApiService,
          useValue: {
            listUsers: () => of([]),
            getUser: () => of({
              id: 'user-1',
              name: 'Username',
              phone: '+7 900 000-00-00',
              gender: 0,
              role: 1,
              passwordHash: null,
            }),
            createUser: () => of({
              id: 'user-1',
              name: 'Username',
              phone: '+7 900 000-00-00',
              gender: 0,
              role: 1,
              passwordHash: null,
            }),
            updateUser: () => of(void 0),
            deleteUser: () => of(void 0),
          },
        },
        {
          provide: AdminEventsApiService,
          useValue: {
            listEvents: () => of([]),
            deleteEvent: () => of(void 0),
          },
        },
        {
          provide: FeedbacksApiService,
          useValue: {
            listFeedbacks: () => of([]),
            listByEvent: () => of([]),
            getFeedback: () => of({
              id: 'feedback-1',
              registrationId: 'registration-1',
              comment: 'Комментарий',
              rate: 4,
            }),
            createFeedback: () => of({
              id: 'feedback-1',
              registrationId: 'registration-1',
              comment: 'Комментарий',
              rate: 4,
            }),
            updateFeedback: () => of({
              id: 'feedback-1',
              registrationId: 'registration-1',
              comment: 'Комментарий',
              rate: 4,
            }),
            deleteFeedback: () => of(void 0),
          },
        },
        {
          provide: AdminFeedbacksApiService,
          useValue: {
            deleteFeedback: () => of(void 0),
          },
        },
        {
          provide: RegistrationsApiService,
          useValue: {
            getRegistration: () => of({
              id: 'registration-1',
              eventId: 'event-1',
              userId: 'user-1',
              type: 0,
              payment: false,
              days: [],
            }),
          },
        },
        {
          provide: EventsApiService,
          useValue: {
            getEventTitleMap: () => of({}),
          },
        },
        {
          provide: EventDetailsDrawerStore,
          useValue: {
            openForEvent: () => void 0,
          },
        },
        {
          provide: EventManagementDrawerStore,
          useValue: {
            open: () => void 0,
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Admin);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
