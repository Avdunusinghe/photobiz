import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  protected readonly stats = [
    { label: 'Upcoming bookings', value: '—' },
    { label: 'Active galleries', value: '—' },
    { label: 'Clients', value: '—' },
  ];
}
