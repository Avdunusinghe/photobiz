import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter, map } from 'rxjs';
import { NAV_ITEMS } from '../../core/config/nav-items';
import { AuthService } from '../../core/services/auth.service';
import { FooterComponent } from '../../shared/ui/footer/footer.component';
import { LogoComponent } from '../../shared/ui/logo/logo.component';
import { SidebarComponent } from '../../shared/ui/sidebar/sidebar.component';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, LogoComponent, SidebarComponent, FooterComponent],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShellComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly navItems = NAV_ITEMS;

  protected readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects),
    ),
    { initialValue: this.router.url },
  );

  protected signOut(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }
}
