import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import type {
  Web_Dtos_CreateRegistrationRequest,
  Web_Dtos_UpdateRegistrationRequest,
} from '../generated';
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

  createRegistration(payload: Web_Dtos_CreateRegistrationRequest): Observable<RegistrationApiModel> {
    return this.registrationsService.postApiV1Registrations({
      requestBody: payload,
    }) as Observable<RegistrationApiModel>;
  }

  listByUser(userId: string, pageNumber?: number, pageSize?: number): Observable<RegistrationApiModel[]> {
    return this.registrationsService.getApiV1RegistrationsUser({
      userId,
      pageNumber,
      pageSize,
    }) as Observable<RegistrationApiModel[]>;
  }

  getRegistration(registrationId: string): Observable<RegistrationApiModel> {
    return this.registrationsService.getApiV1Registrations({
      registrationId,
    }) as Observable<RegistrationApiModel>;
  }

  updateRegistration(
    registrationId: string,
    payload: Web_Dtos_UpdateRegistrationRequest,
  ): Observable<RegistrationApiModel> {
    return this.registrationsService.putApiV1Registrations({
      registrationId,
      requestBody: payload,
    }) as Observable<RegistrationApiModel>;
  }

  deleteRegistration(registrationId: string): Observable<void> {
    return this.registrationsService.deleteApiV1Registrations({
      registrationId,
    }) as Observable<void>;
  }
}
