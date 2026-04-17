import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AuthSessionStore {
  private readonly storageKey = 'eventor.auth.token';

  readonly token = signal<string | null>(this.readToken());

  setToken(token: string): void {
    this.token.set(token);
    this.writeToken(token);
  }

  clearToken(): void {
    this.token.set(null);
    this.removeToken();
  }

  private readToken(): string | null {
    if (typeof localStorage === 'undefined') {
      return null;
    }

    return localStorage.getItem(this.storageKey);
  }

  private writeToken(token: string): void {
    if (typeof localStorage === 'undefined') {
      return;
    }

    localStorage.setItem(this.storageKey, token);
  }

  private removeToken(): void {
    if (typeof localStorage === 'undefined') {
      return;
    }

    localStorage.removeItem(this.storageKey);
  }
}
