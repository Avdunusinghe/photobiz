import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { TokenService } from './token.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpTesting: HttpTestingController;
  let tokenService: TokenService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(AuthService);
    httpTesting = TestBed.inject(HttpTestingController);
    tokenService = TestBed.inject(TokenService);
  });

  afterEach(() => httpTesting.verify());

  it('stores the access token on successful login', () => {
    let completed = false;
    service.login('someone', 'secret').subscribe(() => (completed = true));

    const req = httpTesting.expectOne(`${environment.apiUrl}/api/auth/token`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ username: 'someone', password: 'secret' });

    req.flush({ accessToken: 'token-123', expiresAtUtc: '2026-01-01T00:00:00Z' });

    expect(completed).toBe(true);
    expect(tokenService.getToken()).toBe('token-123');
  });

  it('propagates errors without storing a token', () => {
    let caughtError: unknown;
    service.login('someone', 'wrong-password').subscribe({ error: (err) => (caughtError = err) });

    httpTesting
      .expectOne(`${environment.apiUrl}/api/auth/token`)
      .flush({ title: 'Authentication failed.' }, { status: 401, statusText: 'Unauthorized' });

    expect(caughtError).toBeDefined();
    expect(tokenService.getToken()).toBeNull();
  });

  it('reports authentication state based on the stored token', () => {
    expect(service.isAuthenticated()).toBe(false);

    tokenService.setToken('token-123');

    expect(service.isAuthenticated()).toBe(true);
  });

  it('clears the token on logout', () => {
    tokenService.setToken('token-123');

    service.logout();

    expect(tokenService.getToken()).toBeNull();
  });
});
