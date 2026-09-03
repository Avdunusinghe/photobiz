import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let authServiceSpy: { login: ReturnType<typeof vi.fn> };
  let router: Router;

  beforeEach(() => {
    authServiceSpy = { login: vi.fn() };

    TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy },
      ],
    });

    router = TestBed.inject(Router);
  });

  it('does not submit when the username is empty', () => {
    const fixture = TestBed.createComponent(LoginComponent);
    fixture.detectChanges();
    fixture.componentInstance['form'].controls.password.setValue('secret');

    fixture.componentInstance['submit']();

    expect(authServiceSpy.login).not.toHaveBeenCalled();
  });

  it('does not submit when the password is empty', () => {
    const fixture = TestBed.createComponent(LoginComponent);
    fixture.detectChanges();
    fixture.componentInstance['form'].controls.username.setValue('someone');

    fixture.componentInstance['submit']();

    expect(authServiceSpy.login).not.toHaveBeenCalled();
  });

  it('logs in and navigates to the dashboard on success', () => {
    authServiceSpy.login.mockReturnValue(of(undefined));
    const navigateSpy = vi.spyOn(router, 'navigateByUrl');

    const fixture = TestBed.createComponent(LoginComponent);
    fixture.detectChanges();
    fixture.componentInstance['form'].controls.username.setValue('someone');
    fixture.componentInstance['form'].controls.password.setValue('secret');

    fixture.componentInstance['submit']();

    expect(authServiceSpy.login).toHaveBeenCalledWith('someone', 'secret');
    expect(navigateSpy).toHaveBeenCalledWith('/dashboard');
  });

  it('shows an error message when login fails with invalid credentials', () => {
    authServiceSpy.login.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 401 })));

    const fixture = TestBed.createComponent(LoginComponent);
    fixture.detectChanges();
    fixture.componentInstance['form'].controls.username.setValue('someone');
    fixture.componentInstance['form'].controls.password.setValue('wrong');

    fixture.componentInstance['submit']();

    expect(fixture.componentInstance['errorMessage']()).toBe('Incorrect username or password.');
  });

  it('shows a validation-specific message on a 400 response', () => {
    authServiceSpy.login.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 400 })));

    const fixture = TestBed.createComponent(LoginComponent);
    fixture.detectChanges();
    fixture.componentInstance['form'].controls.username.setValue('someone');
    fixture.componentInstance['form'].controls.password.setValue('secret');

    fixture.componentInstance['submit']();

    expect(fixture.componentInstance['errorMessage']()).toBe('Enter a username and password.');
  });
});
