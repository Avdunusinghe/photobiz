import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { LogoComponent } from '../../shared/ui/logo/logo.component';

@Component({
  selector: 'app-dashboard',
  imports: [LogoComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly stats = [
    { label: 'Upcoming bookings', value: '—' },
    { label: 'Active galleries', value: '—' },
    { label: 'Clients', value: '—' },
  ];

  protected signOut(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }
}
