import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import {
  ResultDto,
  SmtpSettingDto,
  TenantDetailsDto,
  UpdateTenantDetailsRequest,
} from '../models/tenant.model';
import { TenantSettingsService } from './tenant-settings.service';

describe('TenantSettingsService', () => {
  const baseUrl = `${environment.apiUrl}/api/tenant`;
  let service: TenantSettingsService;
  let httpTesting: HttpTestingController;

  const sampleTenant: TenantDetailsDto = {
    id: '11111111-1111-1111-1111-111111111111',
    tenantKey: 'acme',
    name: 'Acme Studio',
    logoUrl: 'https://cdn.acmestudio.test/logo.png',
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
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(TenantSettingsService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('fetches the current tenant details', () => {
    let received: TenantDetailsDto | undefined;
    service.getTenantDetails().subscribe((result) => (received = result));

    const req = httpTesting.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');

    req.flush(sampleTenant);
    expect(received).toEqual(sampleTenant);
  });

  it('puts an update request', () => {
    const body: UpdateTenantDetailsRequest = {
      name: 'Acme Studio Ltd',
      customerEmail: 'hello@acmestudio.test',
      customerFirstName: 'Grace',
      customerLastName: 'Hopper',
      phone: '+1 555 0199',
      address: '221B Baker Street',
      city: 'Manchester',
      country: 'UK',
    };
    const response: ResultDto<TenantDetailsDto> = {
      success: true,
      message: 'Tenant details updated successfully.',
      data: { ...sampleTenant, ...body },
    };

    let received: ResultDto<TenantDetailsDto> | undefined;
    service.updateTenantDetails(body).subscribe((r) => (received = r));

    const req = httpTesting.expectOne(baseUrl);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(body);

    req.flush(response);
    expect(received).toEqual(response);
  });

  it('posts a logo file as multipart form data', () => {
    const file = new File(['content'], 'logo.png', { type: 'image/png' });
    const response: ResultDto<TenantDetailsDto> = {
      success: true,
      message: 'Logo uploaded successfully.',
      data: sampleTenant,
    };

    let received: ResultDto<TenantDetailsDto> | undefined;
    service.uploadLogo(file).subscribe((r) => (received = r));

    const req = httpTesting.expectOne(`${baseUrl}/logo`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toBeInstanceOf(FormData);
    expect((req.request.body as FormData).get('file')).toBe(file);

    req.flush(response);
    expect(received).toEqual(response);
  });

  it('sends a delete request to remove the logo', () => {
    const response: ResultDto<TenantDetailsDto> = {
      success: true,
      message: 'Logo removed successfully.',
      data: { ...sampleTenant, logoUrl: null },
    };

    let received: ResultDto<TenantDetailsDto> | undefined;
    service.removeLogo().subscribe((r) => (received = r));

    const req = httpTesting.expectOne(`${baseUrl}/logo`);
    expect(req.request.method).toBe('DELETE');

    req.flush(response);
    expect(received).toEqual(response);
  });

  it('fetches the current tenant SMTP settings', () => {
    const smtp: SmtpSettingDto = {
      id: '22222222-2222-2222-2222-222222222222',
      host: 'smtp.acmestudio.test',
      port: 587,
      username: 'no-reply@acmestudio.test',
      enableSsl: true,
      fromEmail: 'no-reply@acmestudio.test',
      fromName: 'Acme Studio',
      isEnabled: true,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: null,
    };

    let received: SmtpSettingDto | null | undefined;
    service.getSmtpSettings().subscribe((result) => (received = result));

    const req = httpTesting.expectOne(`${baseUrl}/smtp`);
    expect(req.request.method).toBe('GET');

    req.flush(smtp);
    expect(received).toEqual(smtp);
  });

  it('resolves to null when SMTP is not configured', () => {
    let received: SmtpSettingDto | null | undefined;
    service.getSmtpSettings().subscribe((result) => (received = result));

    const req = httpTesting.expectOne(`${baseUrl}/smtp`);
    req.flush(null as unknown as SmtpSettingDto);

    expect(received).toBeNull();
  });
});
