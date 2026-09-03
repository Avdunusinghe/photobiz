import { Injectable } from '@angular/core';

const TOKEN_STORAGE_KEY = 'photobiz_admin_token';

@Injectable({ providedIn: 'root' })
export class TokenService {
  getToken(): string | null {
    return localStorage.getItem(TOKEN_STORAGE_KEY);
  }

  setToken(token: string): void {
    localStorage.setItem(TOKEN_STORAGE_KEY, token);
  }

  clearToken(): void {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
  }
}
