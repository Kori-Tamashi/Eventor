import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AdminRegistrationsService, RegistrationsService } from '../generated';
import { RegistrationApiModel } from '../models/registration.models';

@Injectable({
  providedIn: 'root',
})
export class RegistrationsApiService {
  private readonly adminRegistrationsService = inject(AdminRegistrationsService);
  private readonly registrationsService = inject(RegistrationsService);

  listByEvent(eventId: string, pageNumber?: number, pageSize?: number): Observable<RegistrationApiModel[]> {
    return this.adminRegistrationsService.getApiV1AdminRegistrations({
      eventId,
      pageNumber,
      pageSize,
    }) as Observable<RegistrationApiModel[]>;
  }

  updateRegistration(
    registrationId: string,
    payload: { type?: 0 | 1 | 2; payment?: boolean },
  ): Observable<RegistrationApiModel> {
    return this.registrationsService.putApiV1Registrations({
      registrationId,
      requestBody: payload,
    }) as Observable<RegistrationApiModel>;
  }
}
