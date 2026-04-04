import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { Organization } from './organization';

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
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(Organization);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
