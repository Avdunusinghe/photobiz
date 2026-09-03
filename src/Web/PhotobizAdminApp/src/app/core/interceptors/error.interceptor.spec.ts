import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { TokenService } from '../services/token.service';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor', () => {
  let httpClient: HttpClient;
  let httpTesting: HttpTestingController;
  let tokenService: TokenService;
  let router: Router;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    tokenService = TestBed.inject(TokenService);
    router = TestBed.inject(Router);
  });

  afterEach(() => httpTesting.verify());

  it('clears the token and navigates to /login on a 401 response', () => {
    tokenService.setToken('stale-token');
    const navigateSpy = vi.spyOn(router, 'navigate');

    httpClient.get('/api/test').subscribe({ error: () => {} });

    httpTesting
      .expectOne('/api/test')
      .flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    expect(tokenService.getToken()).toBeNull();
    expect(navigateSpy).toHaveBeenCalledWith(['/login']);
  });

  it('rethrows non-401 errors without touching the stored token', () => {
    tokenService.setToken('valid-token');
    let caughtError: unknown;

    httpClient.get('/api/test').subscribe({ error: (err) => (caughtError = err) });

    httpTesting
      .expectOne('/api/test')
      .flush('Server error', { status: 500, statusText: 'Internal Server Error' });

    expect(caughtError).toBeDefined();
    expect(tokenService.getToken()).toBe('valid-token');
  });
});
