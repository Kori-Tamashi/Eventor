import {
  EnvironmentProviders,
  InjectionToken,
  inject,
  makeEnvironmentProviders,
  provideEnvironmentInitializer,
} from '@angular/core';
import { AuthSessionStore } from '../auth/auth-session.store';
import { OpenAPI } from './generated';

export const EVENTOR_API_BASE_URL = new InjectionToken<string>('EVENTOR_API_BASE_URL');

function getDefaultBaseUrl(): string {
  if (typeof window === 'undefined') {
    return '';
  }

  const isLocalHost = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';
  const isAngularDevServer = window.location.port === '4200';

  if (isLocalHost && isAngularDevServer) {
    return 'http://localhost:8080';
  }

  return '';
}

export function provideEventorApi(
  baseUrl = getDefaultBaseUrl()
): EnvironmentProviders {
  return makeEnvironmentProviders([
    {
      provide: EVENTOR_API_BASE_URL,
      useValue: baseUrl,
    },
    provideEnvironmentInitializer(() => {
      const authSessionStore = inject(AuthSessionStore);
      const apiBaseUrl = inject(EVENTOR_API_BASE_URL);

      OpenAPI.BASE = apiBaseUrl;
      OpenAPI.TOKEN = async () => authSessionStore.token() ?? '';
    }),
  ]);
}
