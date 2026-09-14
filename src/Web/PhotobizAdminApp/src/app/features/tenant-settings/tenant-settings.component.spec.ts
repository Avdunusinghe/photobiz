import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ResultDto, SmtpSettingDto, TenantDetailsDto } from './models/tenant.model';
import { TenantSettingsService } from './services/tenant-settings.service';
import { TenantSettingsComponent } from './tenant-settings.component';

function makeTenant(overrides: Partial<TenantDetailsDto> = {}): TenantDetailsDto {
  return {
    id: crypto.randomUUID(),
    tenantKey: 'acme',
    name: 'Acme Studio',
    logoUrl: null,
    customerEmail: 'owner@acme.test',
    customerFirstName: 'Ada',
    customerLastName: 'Lovelace',
    phone: '+1 555 0100',
    address: '1 Street',
    city: 'London',
    country: 'UK',
    planCode: 'pro',
    billingCycle: 'monthly',
    isSubscribed: true,
    subscriptionExpiredOn: null,
    customDomain: null,
    customDomainVerifiedAt: null,
    createdAt: '2026-01-01T00:00:00Z',
    ...overrides,
  };
}

function makeFileSelectedEvent(file: File | null): Event {
  const input = document.createElement('input');
  input.type = 'file';
  if (file) {
    Object.defineProperty(input, 'files', { value: [file], configurable: true });
  }
  return { target: input } as unknown as Event;
}

function makeSmtpSettings(overrides: Partial<SmtpSettingDto> = {}): SmtpSettingDto {
  return {
    id: crypto.randomUUID(),
    host: 'smtp.acmestudio.test',
    port: 587,
    username: 'no-reply@acmestudio.test',
    enableSsl: true,
    fromEmail: 'no-reply@acmestudio.test',
    fromName: 'Acme Studio',
    isEnabled: true,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    ...overrides,
  };
}

describe('TenantSettingsComponent', () => {
  let tenantSettingsService: {
    getTenantDetails: ReturnType<typeof vi.fn>;
    updateTenantDetails: ReturnType<typeof vi.fn>;
    uploadLogo: ReturnType<typeof vi.fn>;
    removeLogo: ReturnType<typeof vi.fn>;
    getSmtpSettings: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    tenantSettingsService = {
      getTenantDetails: vi.fn().mockReturnValue(of(makeTenant())),
      updateTenantDetails: vi.fn(),
      uploadLogo: vi.fn(),
      removeLogo: vi.fn(),
      getSmtpSettings: vi.fn().mockReturnValue(of(null)),
    };

    TestBed.configureTestingModule({
      imports: [TenantSettingsComponent],
      providers: [
        provideHttpClient(),
        { provide: TenantSettingsService, useValue: tenantSettingsService },
      ],
    });
  });

  function render() {
    const fixture = TestBed.createComponent(TenantSettingsComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('loads the current tenant and populates the form', () => {
    tenantSettingsService.getTenantDetails.mockReturnValue(
      of(makeTenant({ name: 'Acme Studio Ltd', logoUrl: 'https://cdn.acmestudio.test/logo.png' })),
    );

    const fixture = render();

    expect(tenantSettingsService.getTenantDetails).toHaveBeenCalledTimes(1);
    expect(fixture.componentInstance['form'].controls.name.value).toBe('Acme Studio Ltd');
    expect(fixture.componentInstance['tenant']()?.logoUrl).toBe(
      'https://cdn.acmestudio.test/logo.png',
    );
    expect(fixture.nativeElement.querySelector('.state')).toBeFalsy();
  });

  it('shows the read-only account info from the loaded tenant', () => {
    tenantSettingsService.getTenantDetails.mockReturnValue(
      of(makeTenant({ tenantKey: 'acme', planCode: 'pro', billingCycle: 'monthly' })),
    );

    const fixture = render();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('acme');
    expect(text).toContain('pro');
  });

  it('shows an error banner when loading fails', () => {
    tenantSettingsService.getTenantDetails.mockReturnValue(
      throwError(() => ({ status: 500, error: { title: 'Boom' } })),
    );

    const fixture = render();

    expect(fixture.nativeElement.querySelector('.flash--error').textContent).toContain('Boom');
  });

  it('does not submit an invalid form', () => {
    const fixture = render();
    const instance = fixture.componentInstance;

    instance['form'].controls.name.setValue('');
    instance['save']();

    expect(tenantSettingsService.updateTenantDetails).not.toHaveBeenCalled();
    expect(instance['form'].controls.name.touched).toBe(true);
  });

  it('saves the form and shows a success flash', () => {
    const updated = makeTenant({ name: 'Acme Studio Ltd' });
    const response: ResultDto<TenantDetailsDto> = {
      success: true,
      message: 'Tenant details updated successfully.',
      data: updated,
    };
    tenantSettingsService.updateTenantDetails.mockReturnValue(of(response));

    const fixture = render();
    const instance = fixture.componentInstance;
    instance['form'].controls.name.setValue('Acme Studio Ltd');

    instance['save']();
    fixture.detectChanges();

    expect(tenantSettingsService.updateTenantDetails).toHaveBeenCalledWith(
      expect.objectContaining({ name: 'Acme Studio Ltd' }),
    );
    expect(instance['flash']()).toBe('Tenant details updated successfully.');
    expect(instance['tenant']()).toEqual(updated);
  });

  it('surfaces a validation error banner when saving fails', () => {
    tenantSettingsService.updateTenantDetails.mockReturnValue(
      throwError(() => ({
        status: 400,
        error: { title: 'Bad request', detail: 'Invalid email.' },
      })),
    );

    const fixture = render();
    fixture.componentInstance['save']();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.flash--error').textContent).toContain(
      'Invalid email.',
    );
  });

  it('uploads a selected logo file and shows a success flash', () => {
    const updated = makeTenant({ logoUrl: 'https://api.photobiz.test/media/Tenant/acme/logo.webp' });
    tenantSettingsService.uploadLogo.mockReturnValue(
      of({ success: true, message: 'Logo uploaded successfully.', data: updated }),
    );

    const fixture = render();
    const instance = fixture.componentInstance;
    const file = new File(['content'], 'logo.png', { type: 'image/png' });

    instance['onLogoFileSelected'](makeFileSelectedEvent(file));

    expect(tenantSettingsService.uploadLogo).toHaveBeenCalledWith(file);
    expect(instance['flash']()).toBe('Logo uploaded successfully.');
    expect(instance['tenant']()).toEqual(updated);
  });

  it('rejects a disallowed file type without calling the service', () => {
    const fixture = render();
    const instance = fixture.componentInstance;
    const file = new File(['content'], 'logo.pdf', { type: 'application/pdf' });

    instance['onLogoFileSelected'](makeFileSelectedEvent(file));

    expect(tenantSettingsService.uploadLogo).not.toHaveBeenCalled();
    expect(instance['logoError']()).toContain('PNG, JPEG, WebP, or GIF');
  });

  it('rejects a file over the 5 MB limit without calling the service', () => {
    const fixture = render();
    const instance = fixture.componentInstance;
    const oversized = new File([new Uint8Array(5 * 1024 * 1024 + 1)], 'logo.png', {
      type: 'image/png',
    });

    instance['onLogoFileSelected'](makeFileSelectedEvent(oversized));

    expect(tenantSettingsService.uploadLogo).not.toHaveBeenCalled();
    expect(instance['logoError']()).toContain('5 MB');
  });

  it('surfaces an error when the upload fails', () => {
    tenantSettingsService.uploadLogo.mockReturnValue(
      throwError(() => ({ status: 400, error: { detail: 'Logo must be 5 MB or smaller.' } })),
    );

    const fixture = render();
    const instance = fixture.componentInstance;
    const file = new File(['content'], 'logo.png', { type: 'image/png' });

    instance['onLogoFileSelected'](makeFileSelectedEvent(file));

    expect(instance['logoError']()).toBe('Logo must be 5 MB or smaller.');
  });

  it('removes the logo and shows a success flash', () => {
    tenantSettingsService.getTenantDetails.mockReturnValue(
      of(makeTenant({ logoUrl: 'https://api.photobiz.test/media/Tenant/acme/logo.webp' })),
    );
    const cleared = makeTenant({ logoUrl: null });
    tenantSettingsService.removeLogo.mockReturnValue(
      of({ success: true, message: 'Logo removed successfully.', data: cleared }),
    );

    const fixture = render();
    const instance = fixture.componentInstance;

    instance['removeLogo']();

    expect(tenantSettingsService.removeLogo).toHaveBeenCalled();
    expect(instance['flash']()).toBe('Logo removed successfully.');
    expect(instance['tenant']()).toEqual(cleared);
  });

  it('loads and displays the SMTP configuration when configured', () => {
    tenantSettingsService.getSmtpSettings.mockReturnValue(
      of(makeSmtpSettings({ host: 'smtp.acmestudio.test', port: 587, fromEmail: 'no-reply@acmestudio.test' })),
    );

    const fixture = render();

    expect(tenantSettingsService.getSmtpSettings).toHaveBeenCalledTimes(1);
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('smtp.acmestudio.test:587');
    expect(text).toContain('no-reply@acmestudio.test');
  });

  it('shows a not-configured message when the tenant has no SMTP settings', () => {
    tenantSettingsService.getSmtpSettings.mockReturnValue(of(null));

    const fixture = render();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Not configured');
  });

  it('shows an error banner when loading SMTP settings fails', () => {
    tenantSettingsService.getSmtpSettings.mockReturnValue(
      throwError(() => ({ status: 500, error: { title: 'SMTP lookup failed' } })),
    );

    const fixture = render();

    const banners = fixture.nativeElement.querySelectorAll('.flash--error');
    const messages = Array.from(banners as NodeListOf<HTMLElement>).map((el) => el.textContent);
    expect(messages.some((text) => text?.includes('SMTP lookup failed'))).toBe(true);
  });

  it('reloads SMTP settings when asked to', () => {
    tenantSettingsService.getSmtpSettings.mockReturnValue(of(makeSmtpSettings()));

    const fixture = render();
    fixture.componentInstance['loadSmtpSettings']();

    expect(tenantSettingsService.getSmtpSettings).toHaveBeenCalledTimes(2);
  });
});
