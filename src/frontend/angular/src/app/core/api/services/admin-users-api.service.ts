import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import type {
  Web_Dtos_CreateUserRequest,
  Web_Dtos_Gender,
  Web_Dtos_UpdateUserRequest,
  Web_Dtos_UserRole,
} from '../generated';
import { AdminUsersService } from '../generated';
import { CurrentUser } from '../models/user.models';

@Injectable({
  providedIn: 'root',
})
export class AdminUsersApiService {
  private static readonly EMPTY_GUID = '00000000-0000-0000-0000-000000000000';
  private readonly adminUsersService = inject(AdminUsersService);

  listUsers(filters: {
    nameContains?: string;
    phone?: string;
    role?: Web_Dtos_UserRole;
    gender?: Web_Dtos_Gender;
    pageNumber?: number;
    pageSize?: number;
  } = {}): Observable<CurrentUser[]> {
    return this.adminUsersService.getApiV1AdminUsers({
      nameContains: filters.nameContains,
      phone: filters.phone,
      role: filters.role,
      gender: filters.gender,
      pageNumber: filters.pageNumber,
      pageSize: filters.pageSize,
    }) as Observable<CurrentUser[]>;
  }

  getUser(userId: string): Observable<CurrentUser> {
    return this.adminUsersService.getApiV1AdminUsers1({ userId }) as Observable<CurrentUser>;
  }

  createUser(payload: Web_Dtos_CreateUserRequest): Observable<CurrentUser> {
    return this.adminUsersService.postApiV1AdminUsers({
      requestBody: payload,
    }) as Observable<CurrentUser>;
  }

  updateUser(userId: string, payload: Web_Dtos_UpdateUserRequest): Observable<void> {
    return this.adminUsersService.putApiV1AdminUsers({
      userId,
      requestBody: payload,
    }) as Observable<void>;
  }

  deleteUser(userId: string): Observable<void> {
    return this.adminUsersService.deleteApiV1AdminUsers({
      userId,
    }) as Observable<void>;
  }

  getUserNameMap(userIds: string[]): Observable<Record<string, string>> {
    const uniqueIds = Array.from(
      new Set(
        userIds.filter(
          (userId): userId is string =>
            !!userId && userId !== AdminUsersApiService.EMPTY_GUID
        )
      )
    );

    if (uniqueIds.length === 0) {
      return of({});
    }

    return forkJoin(
      uniqueIds.map((userId) =>
        this.getUser(userId).pipe(
          map((user) => ({ id: userId, name: user.name })),
          catchError(() => of(null))
        )
      )
    ).pipe(
      map((entries) =>
        entries.reduce<Record<string, string>>((acc, entry) => {
          if (!entry) {
            return acc;
          }

          acc[entry.id] = entry.name;
          return acc;
        }, {})
      )
    );
  }
}
