import { Routes } from '@angular/router';

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
        loadComponent: () => import('./pages/admin/admin').then((m) => m.Admin)
      },
    ]
  },
  {
    path: '**',
    redirectTo: '',
  }
];
