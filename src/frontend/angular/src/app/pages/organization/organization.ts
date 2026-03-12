import { Component } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule} from 'primeng/button';

@Component({
  selector: 'app-organization',
  standalone: true,
  imports: [InputTextModule, ButtonModule],
  templateUrl: './organization.html',
  styleUrl: './organization.scss',
})
export class Organization {

}
