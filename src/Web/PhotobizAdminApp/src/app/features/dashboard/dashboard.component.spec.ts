import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { DashboardComponent } from './dashboard.component';

describe('DashboardComponent', () => {
  let authServiceSpy: { logout: ReturnType<typeof vi.fn> };
  let router: Router;

  beforeEach(() => {
    authServiceSpy = { logout: vi.fn() };

    TestBed.configureTestingModule({
      imports: [DashboardComponent],
      providers: [provideRouter([]), { provide: AuthService, useValue: authServiceSpy }],
    });

    router = TestBed.inject(Router);
  });

  it('creates', () => {
    const fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance).toBeTruthy();
  });

  it('logs out and navigates to /login on sign out', () => {
    const navigateSpy = vi.spyOn(router, 'navigateByUrl');
    const fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    fixture.componentInstance['signOut']();

    expect(authServiceSpy.logout).toHaveBeenCalled();
    expect(navigateSpy).toHaveBeenCalledWith('/login');
  });
});
