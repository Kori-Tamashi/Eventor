import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import type { Web_Dtos_UpdateUserRequest } from '../generated';
import { UsersService } from '../generated';
import { CurrentUser, UpdateCurrentUserPayload } from '../models/user.models';

@Injectable({
  providedIn: 'root',
})
export class UsersApiService {
  private readonly usersService = inject(UsersService);

  getMe(): Observable<CurrentUser> {
    return this.usersService.getApiV1UsersMe() as Observable<CurrentUser>;
  }

  updateMe(payload: UpdateCurrentUserPayload): Observable<void> {
    return this.usersService.putApiV1UsersMe({
      requestBody: payload as Web_Dtos_UpdateUserRequest,
    }) as Observable<void>;
  }

  deleteMe(): Observable<void> {
    return this.usersService.deleteApiV1UsersMe() as Observable<void>;
  }
}
