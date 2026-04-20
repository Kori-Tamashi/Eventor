import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { Dashboard } from './dashboard';
import { EventDetailsDrawerStore } from '../../core/ui/event-details-drawer.store';
import { EventsApiService } from '../../core/api/services/events-api.service';
import { RegistrationsApiService } from '../../core/api/services/registrations-api.service';
import { UiNotificationsService } from '../../core/ui/ui-notifications.service';
import { UsersApiService } from '../../core/api/services/users-api.service';

describe('Dashboard', () => {
  let fixture: ComponentFixture<Dashboard>;
  let component: Dashboard;
  let eventsApiService: Pick<EventsApiService, 'getEvent'>;
  let registrationsApiService: Pick<RegistrationsApiService, 'listByUser' | 'deleteRegistration'>;
  let eventDetailsDrawerStore: Pick<EventDetailsDrawerStore, 'openForEvent'>;
  let uiNotificationsService: Pick<UiNotificationsService, 'success' | 'error' | 'info'>;

  beforeEach(async () => {
    eventsApiService = {
      getEvent: vi.fn((eventId: string) => of({
        id: eventId,
        title: eventId === 'event-1' ? 'Первое мероприятие' : 'Второе мероприятие',
        description: 'Описание',
        startDate: '2026-04-19',
        locationId: 'location-1',
        daysCount: 2,
        percent: 10,
        createdByUserId: 'user-1',
        rating: 4.5,
        personCount: 10,
      })),
    } as unknown as Pick<EventsApiService, 'getEvent'>;
    registrationsApiService = {
      listByUser: vi.fn(() => of([
        {
          id: 'registration-1',
          eventId: 'event-1',
          userId: 'user-1',
          type: 0 as const,
          payment: false,
          days: [{ id: 'day-1', sequenceNumber: 1, title: 'День 1' }],
        },
        {
          id: 'registration-2',
          eventId: 'event-2',
          userId: 'user-1',
          type: 2 as const,
          payment: true,
          days: [],
        },
      ])),
      deleteRegistration: vi.fn(() => of(void 0)),
    } as unknown as Pick<RegistrationsApiService, 'listByUser' | 'deleteRegistration'>;
    eventDetailsDrawerStore = {
      openForEvent: vi.fn(),
    } as unknown as Pick<EventDetailsDrawerStore, 'openForEvent'>;
    uiNotificationsService = {
      success: vi.fn(),
      error: vi.fn(),
      info: vi.fn(),
    } as unknown as Pick<UiNotificationsService, 'success' | 'error' | 'info'>;

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
        { provide: EventsApiService, useValue: eventsApiService },
        { provide: RegistrationsApiService, useValue: registrationsApiService },
        { provide: EventDetailsDrawerStore, useValue: eventDetailsDrawerStore },
        { provide: UiNotificationsService, useValue: uiNotificationsService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Dashboard);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('creates the component', () => {
    expect(component).toBeTruthy();
  });

  it('loads and maps only cancelable registrations', () => {
    expect(component.currentUsername()).toBe('Username');
    expect(component.allRows().length).toBe(1);
    expect(component.allRows()[0].eventTitle).toBe('Первое мероприятие');
    expect(component.allRows()[0].typeLabel).toBe('Простой');
    expect(component.allRows()[0].daysLabel).toBe('День 1');
  });

  it('opens the details drawer for the selected event', () => {
    const row = component.allRows()[0];

    component.openDetails(row);

    expect(eventDetailsDrawerStore.openForEvent).toHaveBeenCalledWith({
      eventId: 'event-1',
      source: 'dashboard',
      viewerRole: 'user',
      fallbackTitle: 'Первое мероприятие',
    });
  });

  it('cancels a registration and shows a notification', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    const row = component.allRows()[0];

    component.cancelRegistration(row);

    expect(registrationsApiService.deleteRegistration).toHaveBeenCalledWith('registration-1');
    expect(uiNotificationsService.success).toHaveBeenCalledWith('Регистрация отменена.');
    expect(component.allRows().length).toBe(0);
  });

  it('does not cancel organizer rows', () => {
    component.cancelRegistration({
      registrationId: 'registration-2',
      eventId: 'event-2',
      eventTitle: 'Второе мероприятие',
      description: 'Описание',
      date: '19.04.2026',
      type: 2,
      typeLabel: 'Организатор',
      paymentLabel: 'Оплачено',
      daysLabel: 'Дни не выбраны',
      rating: 4.5,
      canCancel: false,
    });

    expect(registrationsApiService.deleteRegistration).not.toHaveBeenCalledWith('registration-2');
  });
});
