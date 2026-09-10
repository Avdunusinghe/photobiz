import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ShellComponent } from './shell.component';

describe('ShellComponent', () => {
  let authServiceSpy: { logout: ReturnType<typeof vi.fn> };
  let router: Router;

  beforeEach(() => {
    authServiceSpy = { logout: vi.fn() };

    TestBed.configureTestingModule({
      imports: [ShellComponent],
      providers: [provideRouter([]), { provide: AuthService, useValue: authServiceSpy }],
    });

    router = TestBed.inject(Router);
  });

  it('creates and renders the sidebar and footer', () => {
    const fixture = TestBed.createComponent(ShellComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('app-sidebar')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('app-footer')).toBeTruthy();
  });

  it('logs out and navigates to /login on sign out', () => {
    const navigateSpy = vi.spyOn(router, 'navigateByUrl');
    const fixture = TestBed.createComponent(ShellComponent);
    fixture.detectChanges();

    fixture.componentInstance['signOut']();

    expect(authServiceSpy.logout).toHaveBeenCalled();
    expect(navigateSpy).toHaveBeenCalledWith('/login');
  });
});
