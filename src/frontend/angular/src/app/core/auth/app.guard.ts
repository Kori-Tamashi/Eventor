import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router, UrlTree } from '@angular/router';
import { map, Observable, of } from 'rxjs';
import { AuthSessionStore } from './auth-session.store';
import { CurrentUserStore } from './current-user.store';

function checkAppAccess(): Observable<boolean | UrlTree> {
  const router = inject(Router);
  const authSessionStore = inject(AuthSessionStore);
  const currentUserStore = inject(CurrentUserStore);

  if (!authSessionStore.token()) {
    return of(router.createUrlTree(['/auth']));
  }

  return currentUserStore.ensureLoaded().pipe(
    map((user) => user ? true : router.createUrlTree(['/auth']))
  );
}

export const appCanActivate: CanActivateFn = () => checkAppAccess();
export const appCanMatch: CanMatchFn = () => checkAppAccess();
