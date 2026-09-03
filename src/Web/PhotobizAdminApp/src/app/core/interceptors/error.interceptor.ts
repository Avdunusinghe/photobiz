import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { TokenService } from '../services/token.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        tokenService.clearToken();
        router.navigate(['/login']);
      } else if (error.status === 0) {
        console.error('Network error - API unreachable', error);
      } else {
        console.error(`API error ${error.status}:`, error.error ?? error.message);
      }

      return throwError(() => error);
    }),
  );
};
