import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  provideRouter,
  Router,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { AuthService } from '../services/auth.service';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  let authServiceSpy: { isAuthenticated: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    authServiceSpy = { isAuthenticated: vi.fn() };

    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: authServiceSpy }],
    });
  });

  function runGuard() {
    return TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    );
  }

  it('allows activation when authenticated', () => {
    authServiceSpy.isAuthenticated.mockReturnValue(true);

    expect(runGuard()).toBe(true);
  });

  it('redirects to /login when not authenticated', () => {
    authServiceSpy.isAuthenticated.mockReturnValue(false);

    const result = runGuard();

    expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe('/login');
  });
});
