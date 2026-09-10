import { NavItem } from '../models/nav-item.model';

export const NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
  {
    label: 'Bookings',
    icon: 'calendar',
    children: [
      { label: 'All bookings', route: '/bookings' },
      { label: 'Calendar', route: '/bookings/calendar' },
    ],
  },
  {
    label: 'Galleries',
    icon: 'gallery',
    children: [
      { label: 'All galleries', route: '/galleries' },
      { label: 'Photos', route: '/galleries/photos' },
    ],
  },
  { label: 'Clients', icon: 'users', route: '/clients' },
  {
    label: 'Settings',
    icon: 'settings',
    children: [
      { label: 'Session types', route: '/settings/session-types' },
      { label: 'Users & roles', route: '/settings/users' },
    ],
  },
];
