import { Component } from '@angular/core';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';

type OrganizationEventRow = {
  name: string;
  location: string;
  description: string;
  date: string;
  peopleCount: number;
  daysCount: number;
  markup: number;
  rating: number;
}

@Component({
  selector: 'app-organization',
  standalone: true,
  imports: [InputTextModule, ButtonModule, TableModule],
  templateUrl: './organization.html',
  styleUrl: './organization.scss',
})
export class Organization {
  readonly rows: OrganizationEventRow[] = [
    {
      name: 'Название',
      location: 'Локация',
      description: 'Описание',
      date: 'dd.mm.yyyy',
      peopleCount: 0,
      daysCount: 0,
      markup: 0,
      rating: 0,
    },
    {
      name: 'Название',
      location: 'Локация',
      description: 'Описание',
      date: 'dd.mm.yyyy',
      peopleCount: 0,
      daysCount: 0,
      markup: 0,
      rating: 0,
    },
    {
      name: 'Название',
      location: 'Локация',
      description: 'Описание',
      date: 'dd.mm.yyyy',
      peopleCount: 0,
      daysCount: 0,
      markup: 0,
      rating: 0,
    },
  ];
}
