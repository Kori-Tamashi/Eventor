import { Component, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class Sidebar {
  readonly profileOpen = input<boolean>(false);
  readonly showAdmin = input<boolean>(false);
  readonly profileToggle = output<void>();
  readonly logoutRequested = output<void>();
}
