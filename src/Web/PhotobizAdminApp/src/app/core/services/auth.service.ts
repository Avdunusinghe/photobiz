import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TokenService } from './token.service';

interface TokenResponse {
  accessToken: string;
  expiresAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenService = inject(TokenService);

  login(username: string, password: string): Observable<void> {
    return this.http
      .post<TokenResponse>(`${environment.apiUrl}/api/auth/token`, { username, password })
      .pipe(
        tap((response) => this.tokenService.setToken(response.accessToken)),
        map(() => undefined),
      );
  }

  logout(): void {
    this.tokenService.clearToken();
  }

  isAuthenticated(): boolean {
    return this.tokenService.getToken() !== null;
  }
}
