import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { Events } from './events';
import { EventsApiService } from '../../core/api/services/events-api.service';

describe('Events', () => {
  let component: Events;
  let fixture: ComponentFixture<Events>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Events],
      providers: [
        {
          provide: EventsApiService,
          useValue: {
            listEvents: () => of([]),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Events);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
