import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { AdminUsersService } from '../generated';
import { CurrentUser } from '../models/user.models';

@Injectable({
  providedIn: 'root',
})
export class AdminUsersApiService {
  private readonly adminUsersService = inject(AdminUsersService);

  getUser(userId: string): Observable<CurrentUser> {
    return this.adminUsersService.getApiV1AdminUsers1({ userId }) as Observable<CurrentUser>;
  }

  getUserNameMap(userIds: string[]): Observable<Record<string, string>> {
    const uniqueIds = Array.from(new Set(userIds.filter(Boolean)));

    if (uniqueIds.length === 0) {
      return of({});
    }

    return forkJoin(
      uniqueIds.map((userId) =>
        this.getUser(userId).pipe(
          map((user) => ({ id: userId, name: user.name }))
        )
      )
    ).pipe(
      map((entries) =>
        entries.reduce<Record<string, string>>((acc, entry) => {
          acc[entry.id] = entry.name;
          return acc;
        }, {})
      )
    );
  }
}
