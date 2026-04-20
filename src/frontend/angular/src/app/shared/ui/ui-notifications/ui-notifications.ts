import { Component, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { UiNotificationsService } from '../../../core/ui/ui-notifications.service';

@Component({
  selector: 'app-ui-notifications',
  standalone: true,
  imports: [ButtonModule],
  templateUrl: './ui-notifications.html',
  styleUrl: './ui-notifications.scss',
})
export class UiNotifications {
  readonly notificationsService = inject(UiNotificationsService);
  readonly notifications = this.notificationsService.notifications;

  dismiss(id: number): void {
    this.notificationsService.dismiss(id);
  }
}
