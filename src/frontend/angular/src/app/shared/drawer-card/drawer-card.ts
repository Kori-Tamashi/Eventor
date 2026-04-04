import { Component, input } from '@angular/core';
import {Drawer} from 'primeng/drawer';

type DrawerCardVariant = 'stat' | 'content';

@Component({
  selector: 'app-drawer-card',
  standalone: true,
  imports: [],
  templateUrl: './drawer-card.html',
  styleUrl: './drawer-card.scss',
})
export class DrawerCard {
  readonly title = input<string>('');
  readonly variant = input<DrawerCardVariant>('content');
}
