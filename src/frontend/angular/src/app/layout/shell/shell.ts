import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { DrawerModule } from 'primeng/drawer';
import { Sidebar } from '../sidebar/sidebar';
import { Profile } from '../../pages/profile/profile';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, DrawerModule, Sidebar, Profile],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  readonly profileDrawerOpen = signal<boolean>(false);

  toggleProfileDrawer(): void {
    this.profileDrawerOpen.update((value) => !value);
  }
}
