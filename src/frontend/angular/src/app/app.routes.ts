import { Routes } from '@angular/router';
import { adminCanActivate, adminCanMatch } from './core/auth/admin.guard';
import { appCanActivate, appCanMatch } from './core/auth/app.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/home/home').then((m) => m.Home),
  },
  {
    path: 'auth',
    loadComponent: () =>
      import('./pages/auth/auth').then((m) => m.Auth),
  },
  {
    path: 'app',
    canActivate: [appCanActivate],
    canMatch: [appCanMatch],
    loadComponent: () =>
      import('./layout/shell/shell').then((m) => m.Shell),
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./pages/dashboard/dashboard').then((m) => m.Dashboard)
      },
      {
        path: 'events',
        loadComponent: () => import('./pages/events/events').then((m) => m.Events)
      },
      {
        path: 'organization',
        loadComponent: () => import('./pages/organization/organization').then((m) => m.Organization)
      },
      {
        path: 'admin',
        canActivate: [adminCanActivate],
        canMatch: [adminCanMatch],
        loadComponent: () => import('./pages/admin/admin').then((m) => m.Admin)
      },
    ]
  },
  {
    path: '**',
    redirectTo: '',
  }
];
