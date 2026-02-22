import { Component, signal } from '@angular/core';
// import { RouterOutlet } from '@angular/router';
// import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-root',
  standalone: true,
  // imports: [RouterOutlet, ButtonModule],
  imports: [],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('eventor-frontend');
}
