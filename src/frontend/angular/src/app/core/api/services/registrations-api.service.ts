import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AdminRegistrationsService } from '../generated';
import { RegistrationApiModel } from '../models/registration.models';

@Injectable({
  providedIn: 'root',
})
export class RegistrationsApiService {
  private readonly adminRegistrationsService = inject(AdminRegistrationsService);

  listByEvent(eventId: string, pageNumber?: number, pageSize?: number): Observable<RegistrationApiModel[]> {
    return this.adminRegistrationsService.getApiV1AdminRegistrations({
      eventId,
      pageNumber,
      pageSize,
    }) as Observable<RegistrationApiModel[]>;
  }
}
