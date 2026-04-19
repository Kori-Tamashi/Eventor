import { Component, inject, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { DrawerModule } from 'primeng/drawer';
import { AuthApiService } from '../../core/api/services/auth-api.service';
import { Sidebar } from '../sidebar/sidebar';
import { Profile } from '../../pages/profile/profile';
import { EventDetailsDrawer } from '../../shared/event-details-drawer/event-details-drawer';
import { EventManagementDrawer } from '../../shared/event-management-drawer/event-management-drawer';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    RouterOutlet,
    DrawerModule,
    Sidebar,
    Profile,
    EventDetailsDrawer,
    EventManagementDrawer,
  ],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  private readonly router = inject(Router);
  private readonly authApiService = inject(AuthApiService);

  readonly profileDrawerOpen = signal<boolean>(false);

  toggleProfileDrawer(): void {
    this.profileDrawerOpen.update((value) => !value);
  }

  logout(): void {
    this.profileDrawerOpen.set(false);
    this.authApiService.logout();
    void this.router.navigate(['/auth']);
  }
}
