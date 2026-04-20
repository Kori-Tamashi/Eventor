import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { catchError, finalize, Observable, of, shareReplay, tap } from 'rxjs';
import { CurrentUser } from '../api/models/user.models';
import { UsersApiService } from '../api/services/users-api.service';
import { AuthSessionStore } from './auth-session.store';

@Injectable({
  providedIn: 'root',
})
export class CurrentUserStore {
  private readonly usersApiService = inject(UsersApiService);
  private readonly authSessionStore = inject(AuthSessionStore);

  private loadRequest$: Observable<CurrentUser | null> | null = null;

  readonly currentUser = signal<CurrentUser | null>(null);
  readonly isLoading = signal<boolean>(false);
  readonly isAdmin = computed(() => this.currentUser()?.role === 0);

  constructor() {
    effect(() => {
      const token = this.authSessionStore.token();

      if (!token) {
        this.clear();
        return;
      }

      if (!this.currentUser() && !this.isLoading()) {
        this.refresh().subscribe();
      }
    });
  }

  setCurrentUser(user: CurrentUser | null): void {
    this.currentUser.set(user);
  }

  clear(): void {
    this.currentUser.set(null);
    this.loadRequest$ = null;
    this.isLoading.set(false);
  }

  ensureLoaded(): Observable<CurrentUser | null> {
    if (!this.authSessionStore.token()) {
      this.clear();
      return of(null);
    }

    if (this.currentUser()) {
      return of(this.currentUser());
    }

    return this.refresh();
  }

  refresh(): Observable<CurrentUser | null> {
    if (!this.authSessionStore.token()) {
      this.clear();
      return of(null);
    }

    if (this.loadRequest$) {
      return this.loadRequest$;
    }

    this.isLoading.set(true);
    this.loadRequest$ = this.usersApiService.getMe().pipe(
      tap((user) => this.currentUser.set(user)),
      catchError(() => {
        this.currentUser.set(null);
        return of(null);
      }),
      finalize(() => {
        this.isLoading.set(false);
        this.loadRequest$ = null;
      }),
      shareReplay(1),
    );

    return this.loadRequest$;
  }
}
