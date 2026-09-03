import { TestBed } from '@angular/core/testing';
import { TokenService } from './token.service';

describe('TokenService', () => {
  let service: TokenService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(TokenService);
  });

  it('returns null when no token is stored', () => {
    expect(service.getToken()).toBeNull();
  });

  it('returns the token that was set', () => {
    service.setToken('abc123');

    expect(service.getToken()).toBe('abc123');
  });

  it('clears the stored token', () => {
    service.setToken('abc123');

    service.clearToken();

    expect(service.getToken()).toBeNull();
  });
});
