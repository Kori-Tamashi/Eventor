import { Component, output } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [InputTextModule, ButtonModule],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
})
export class Profile {
  readonly closeRequested = output<void>();

  closeDrawer(): void {
    this.closeRequested.emit();
  }
}
