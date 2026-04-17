import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import type { Web_Dtos_LoginRequest, Web_Dtos_RegisterRequest } from '../generated';
import { AuthService } from '../generated';
import { AuthSessionStore } from '../../auth/auth-session.store';
import {
  AuthTokenResponse,
  LoginPayload,
  RegisterPayload,
} from '../models/auth.models';

@Injectable({
  providedIn: 'root',
})
export class AuthApiService {
  private readonly authService = inject(AuthService);
  private readonly authSessionStore = inject(AuthSessionStore);

  register(payload: RegisterPayload): Observable<unknown> {
    return this.authService.postApiV1AuthRegister({
      requestBody: payload as Web_Dtos_RegisterRequest,
    });
  }

  login(payload: LoginPayload): Observable<AuthTokenResponse> {
    return this.authService.postApiV1AuthLogin({
      requestBody: payload as Web_Dtos_LoginRequest,
    }).pipe(
      tap((response) => {
        const token = (response as AuthTokenResponse).token;

        if (token) {
          this.authSessionStore.setToken(token);
        }
      })
    ) as Observable<AuthTokenResponse>;
  }

  logout(): void {
    this.authSessionStore.clearToken();
  }
}
