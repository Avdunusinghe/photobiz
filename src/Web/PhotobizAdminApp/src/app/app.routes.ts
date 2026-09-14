import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

const placeholderPage = () =>
  import('./shared/ui/placeholder-page/placeholder-page.component').then(
    (m) => m.PlaceholderPageComponent,
  );

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    loadComponent: () => import('./layout/shell/shell.component').then((m) => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      { path: 'bookings', loadComponent: placeholderPage, data: { title: 'All Bookings' } },
      {
        path: 'bookings/calendar',
        loadComponent: placeholderPage,
        data: { title: 'Booking Calendar' },
      },
      { path: 'galleries', loadComponent: placeholderPage, data: { title: 'All Galleries' } },
      { path: 'galleries/photos', loadComponent: placeholderPage, data: { title: 'Photos' } },
      { path: 'clients', loadComponent: placeholderPage, data: { title: 'Clients' } },
      {
        path: 'settings/tenant',
        loadComponent: () =>
          import('./features/tenant-settings/tenant-settings.component').then(
            (m) => m.TenantSettingsComponent,
          ),
        data: { title: 'Business Profile' },
      },
      {
        path: 'settings/theme',
        loadComponent: () =>
          import('./features/site-theme/site-theme.component').then((m) => m.SiteThemeComponent),
        data: { title: 'Website Theme' },
      },
      {
        path: 'settings/session-types',
        loadComponent: placeholderPage,
        data: { title: 'Session Types' },
      },
      {
        path: 'settings/users',
        loadComponent: () =>
          import('./features/users/user-list.component').then((m) => m.UserListComponent),
        data: { title: 'Users & Roles' },
      },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },
];
