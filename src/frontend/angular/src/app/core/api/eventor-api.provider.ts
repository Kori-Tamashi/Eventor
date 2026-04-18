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

export function provideEventorApi(
  baseUrl = 'http://localhost:8081'
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
