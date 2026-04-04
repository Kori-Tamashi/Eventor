import { Component, input } from '@angular/core';

type DrawerCardVariant = 'default' | 'stat' | 'table';

@Component({
  selector: 'app-drawer-card',
  standalone: true,
  templateUrl: './drawer-card.html',
  styleUrl: './drawer-card.scss',
})
export class DrawerCard {
  readonly title = input<string>('');
  readonly variant = input<DrawerCardVariant>('default');
}
