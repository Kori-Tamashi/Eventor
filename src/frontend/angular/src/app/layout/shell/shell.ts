import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { DrawerModule } from 'primeng/drawer';
import { AuthApiService } from '../../core/api/services/auth-api.service';
import { CurrentUserStore } from '../../core/auth/current-user.store';
import { Sidebar } from '../sidebar/sidebar';
import { Profile } from '../../pages/profile/profile';
import { EventDetailsDrawer } from '../../shared/event-details-drawer/event-details-drawer';
import { EventManagementDrawer } from '../../shared/event-management-drawer/event-management-drawer';
import { UiNotifications } from '../../shared/ui/ui-notifications/ui-notifications';

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
    UiNotifications,
  ],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly authApiService = inject(AuthApiService);
  private readonly currentUserStore = inject(CurrentUserStore);

  readonly profileDrawerOpen = signal<boolean>(false);
  readonly isAdmin = this.currentUserStore.isAdmin;
  readonly routeAnimationPhase = signal<'enter-a' | 'enter-b'>('enter-a');

  constructor() {
    this.currentUserStore.ensureLoaded().subscribe();

    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.routeAnimationPhase.update((phase) => phase === 'enter-a' ? 'enter-b' : 'enter-a');
      });
  }

  toggleProfileDrawer(): void {
    this.profileDrawerOpen.update((value) => !value);
  }

  logout(): void {
    this.profileDrawerOpen.set(false);
    this.authApiService.logout();
    void this.router.navigate(['/auth']);
  }
}
