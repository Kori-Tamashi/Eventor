import { Component } from '@angular/core';
import { TabsModule } from 'primeng/tabs';
import { AdminEventsSection } from '../../shared/admin/admin-events-section/admin-events-section';
import { AdminItemsSection } from '../../shared/admin/admin-items-section/admin-items-section';
import { AdminLocationsSection } from '../../shared/admin/admin-locations-section/admin-locations-section';
import { AdminMenusSection } from '../../shared/admin/admin-menus-section/admin-menus-section';
import { AdminUsersSection } from '../../shared/admin/admin-users-section/admin-users-section';

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [TabsModule, AdminLocationsSection, AdminItemsSection, AdminMenusSection, AdminUsersSection, AdminEventsSection],
  templateUrl: './admin.html',
  styleUrl: './admin.scss',
})
export class Admin {}
