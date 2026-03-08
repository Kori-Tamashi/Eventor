import { Component } from '@angular/core';
import { RouterLink} from '@angular/router';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './auth.html',
  styleUrl: './auth.scss',
})
export class Auth {

}
