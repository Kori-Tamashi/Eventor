import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { AdminEventsSection } from './admin-events-section';
import { AdminEventsApiService } from '../../../core/api/services/admin-events-api.service';
import { AdminUsersApiService } from '../../../core/api/services/admin-users-api.service';
import { EventDetailsDrawerStore } from '../../../core/ui/event-details-drawer.store';
import { EventManagementDrawerStore } from '../../../core/ui/event-management-drawer.store';
import { LocationsApiService } from '../../../core/api/services/locations-api.service';
import { UiNotificationsService } from '../../../core/ui/ui-notifications.service';

describe('AdminEventsSection', () => {
  let fixture: ComponentFixture<AdminEventsSection>;
  let component: AdminEventsSection;
  let adminEventsApiService: Pick<AdminEventsApiService, 'listEvents' | 'deleteEvent'>;
  let eventDetailsDrawerStore: Pick<EventDetailsDrawerStore, 'openForEvent'>;
  let eventManagementDrawerStore: Pick<EventManagementDrawerStore, 'open'>;
  let uiNotificationsService: Pick<UiNotificationsService, 'success' | 'error' | 'info'>;

  beforeEach(async () => {
    if (!(globalThis as any).ResizeObserver) {
      (globalThis as any).ResizeObserver = class {
        observe() {}
        unobserve() {}
        disconnect() {}
      };
    }

    adminEventsApiService = {
      listEvents: vi.fn(() => of([
        {
          id: 'event-1',
          title: 'Admin Event',
          description: 'Описание',
          startDate: '2026-04-19',
          locationId: 'location-1',
          daysCount: 2,
          percent: 15,
          createdByUserId: '00000000-0000-0000-0000-000000000000',
          rating: 4.7,
          personCount: 20,
        },
      ])),
      deleteEvent: vi.fn(() => of(void 0)),
    } as unknown as Pick<AdminEventsApiService, 'listEvents' | 'deleteEvent'>;
    eventDetailsDrawerStore = {
      openForEvent: vi.fn(),
    } as unknown as Pick<EventDetailsDrawerStore, 'openForEvent'>;
    eventManagementDrawerStore = {
      open: vi.fn(),
    } as unknown as Pick<EventManagementDrawerStore, 'open'>;
    uiNotificationsService = {
      success: vi.fn(),
      error: vi.fn(),
      info: vi.fn(),
    } as unknown as Pick<UiNotificationsService, 'success' | 'error' | 'info'>;

    await TestBed.configureTestingModule({
      imports: [AdminEventsSection],
      providers: [
        { provide: AdminEventsApiService, useValue: adminEventsApiService },
        {
          provide: LocationsApiService,
          useValue: {
            getLocationTitleMap: () => of({ 'location-1': 'Локация 1' }),
          },
        },
        {
          provide: AdminUsersApiService,
          useValue: {
            getUserNameMap: () => of({}),
          },
        },
        { provide: EventDetailsDrawerStore, useValue: eventDetailsDrawerStore },
        { provide: EventManagementDrawerStore, useValue: eventManagementDrawerStore },
        { provide: UiNotificationsService, useValue: uiNotificationsService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminEventsSection);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('loads events and falls back to "Не указан" organizer', () => {
    expect(component.allRows().length).toBe(1);
    expect(component.allRows()[0].organizer).toBe('Не указан');
  });

  it('opens details drawer for a row', () => {
    const row = component.allRows()[0];

    component.openDetails(row);

    expect(eventDetailsDrawerStore.openForEvent).toHaveBeenCalledWith({
      eventId: 'event-1',
      source: 'organization',
      viewerRole: 'superuser',
      fallbackTitle: 'Admin Event',
    });
  });

  it('opens management drawer in settings, days and reviews tabs', () => {
    const row = component.allRows()[0];

    component.openManageDrawer(row);
    component.openDaysDrawer(row);
    component.openReviewsDrawer(row);

    expect(eventManagementDrawerStore.open).toHaveBeenCalledWith(expect.objectContaining({
      eventId: 'event-1',
      initialTab: 'settings',
    }));
    expect(eventManagementDrawerStore.open).toHaveBeenCalledWith(expect.objectContaining({
      eventId: 'event-1',
      initialTab: 'days',
    }));
    expect(eventManagementDrawerStore.open).toHaveBeenCalledWith(expect.objectContaining({
      eventId: 'event-1',
      initialTab: 'reviews',
    }));
  });

  it('deletes event and shows a notification', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    const row = component.allRows()[0];

    component.deleteEvent(row);

    expect(adminEventsApiService.deleteEvent).toHaveBeenCalledWith('event-1');
    expect(uiNotificationsService.success).toHaveBeenCalledWith('Мероприятие удалено.');
    expect(component.allRows().length).toBe(0);
  });
});
