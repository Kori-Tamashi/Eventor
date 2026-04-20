import { Injectable, signal } from '@angular/core';

export type UiNotificationKind = 'success' | 'error' | 'info';

export type UiNotification = {
  id: number;
  kind: UiNotificationKind;
  message: string;
};

@Injectable({
  providedIn: 'root',
})
export class UiNotificationsService {
  private nextId = 1;
  readonly notifications = signal<UiNotification[]>([]);

  success(message: string, durationMs = 1800): void {
    this.push('success', message, durationMs);
  }

  error(message: string, durationMs = 2200): void {
    this.push('error', message, durationMs);
  }

  info(message: string, durationMs = 1800): void {
    this.push('info', message, durationMs);
  }

  dismiss(id: number): void {
    this.notifications.update((items) => items.filter((item) => item.id !== id));
  }

  private push(kind: UiNotificationKind, message: string, durationMs: number): void {
    const id = this.nextId++;
    this.notifications.update((items) => [...items, { id, kind, message }]);

    if (typeof window !== 'undefined') {
      window.setTimeout(() => this.dismiss(id), durationMs);
    }
  }
}
