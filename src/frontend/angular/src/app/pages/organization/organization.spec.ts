import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { Organization } from './organization';
import { EventDetailsDrawerStore } from '../../core/ui/event-details-drawer.store';
import { EventManagementDrawerStore } from '../../core/ui/event-management-drawer.store';
import { EventsApiService } from '../../core/api/services/events-api.service';
import { LocationsApiService } from '../../core/api/services/locations-api.service';
import { UiNotificationsService } from '../../core/ui/ui-notifications.service';
import { UsersApiService } from '../../core/api/services/users-api.service';

describe('Organization', () => {
  let fixture: ComponentFixture<Organization>;
  let component: Organization;
  let eventsApiService: Pick<EventsApiService, 'listOrganizedEvents' | 'deleteEvent'>;
  let eventDetailsDrawerStore: Pick<EventDetailsDrawerStore, 'openForEvent'>;
  let eventManagementDrawerStore: Pick<EventManagementDrawerStore, 'open'> & { isOpen: () => boolean };
  let uiNotificationsService: Pick<UiNotificationsService, 'success' | 'error' | 'info'>;

  beforeEach(async () => {
    if (!(globalThis as any).ResizeObserver) {
      (globalThis as any).ResizeObserver = class {
        observe() {}
        unobserve() {}
        disconnect() {}
      };
    }

    eventsApiService = {
      listOrganizedEvents: vi.fn(() => of([
        {
          id: 'event-1',
          title: 'Орг мероприятие',
          description: 'Описание',
          startDate: '2026-04-19',
          locationId: 'location-1',
          daysCount: 2,
          percent: 12,
          createdByUserId: 'user-1',
          rating: 4.2,
          personCount: 8,
        },
      ])),
      deleteEvent: vi.fn(() => of(void 0)),
    } as unknown as Pick<EventsApiService, 'listOrganizedEvents' | 'deleteEvent'>;
    eventDetailsDrawerStore = {
      openForEvent: vi.fn(),
    } as unknown as Pick<EventDetailsDrawerStore, 'openForEvent'>;
    eventManagementDrawerStore = {
      open: vi.fn(),
      isOpen: () => false,
    } as unknown as Pick<EventManagementDrawerStore, 'open'> & { isOpen: () => boolean };
    uiNotificationsService = {
      success: vi.fn(),
      error: vi.fn(),
      info: vi.fn(),
    } as unknown as Pick<UiNotificationsService, 'success' | 'error' | 'info'>;

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
        { provide: EventsApiService, useValue: eventsApiService },
        {
          provide: LocationsApiService,
          useValue: {
            getLocationTitleMap: () => of({ 'location-1': 'Локация 1' }),
          },
        },
        { provide: EventDetailsDrawerStore, useValue: eventDetailsDrawerStore },
        { provide: EventManagementDrawerStore, useValue: eventManagementDrawerStore },
        { provide: UiNotificationsService, useValue: uiNotificationsService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Organization);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('creates the component', () => {
    expect(component).toBeTruthy();
  });

  it('loads organized events and resolves location titles', () => {
    expect(component.allRows().length).toBe(1);
    expect(component.allRows()[0].name).toBe('Орг мероприятие');
    expect(component.allRows()[0].location).toBe('Локация 1');
  });

  it('opens create event drawer with create context', () => {
    component.openCreateEventDrawer();

    expect(eventManagementDrawerStore.open).toHaveBeenCalledWith({
      mode: 'create',
      source: 'organization',
      viewerRole: 'user',
      title: 'Создать мероприятие',
    });
  });

  it('opens manage event drawer with event context', () => {
    const row = component.allRows()[0];

    component.openManageEventDrawer(row);

    expect(eventManagementDrawerStore.open).toHaveBeenCalledWith({
      mode: 'manage',
      source: 'organization',
      viewerRole: 'user',
      title: 'Орг мероприятие',
      eventId: 'event-1',
    });
  });

  it('deletes event and shows notification', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    const row = component.allRows()[0];

    component.deleteEvent(row);

    expect(eventsApiService.deleteEvent).toHaveBeenCalledWith('event-1');
    expect(uiNotificationsService.success).toHaveBeenCalledWith('Мероприятие удалено.');
    expect(component.allRows().length).toBe(0);
  });
});
