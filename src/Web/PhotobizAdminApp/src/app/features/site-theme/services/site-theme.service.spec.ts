import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { ResultDto, SiteThemeDto } from '../models/site-theme.model';
import { SiteThemeService } from './site-theme.service';

describe('SiteThemeService', () => {
  const baseUrl = `${environment.apiUrl}/api/tenant/theme`;
  let service: SiteThemeService;
  let httpTesting: HttpTestingController;

  const sampleTheme: SiteThemeDto = {
    id: '11111111-1111-1111-1111-111111111111',
    primaryColor: '#111827',
    secondaryColor: '#F97316',
    accentColor: '#2563EB',
    gradientStartColor: null,
    gradientEndColor: null,
    gradientDirection: 'ToRight',
    fontFamily: null,
    headerStyle: 'Classic',
    tagline: null,
    footerText: null,
    footerCopyrightText: null,
    defaultGalleryTemplate: 'Grid',
    footerLinks: [],
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(SiteThemeService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('fetches the current theme', () => {
    let received: SiteThemeDto | undefined;
    service.getTheme().subscribe((result) => (received = result));

    const req = httpTesting.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');

    req.flush(sampleTheme);
    expect(received).toEqual(sampleTheme);
  });

  it('puts an update request', () => {
    const response: ResultDto<SiteThemeDto> = {
      success: true,
      message: 'Theme updated successfully.',
      data: sampleTheme,
    };

    let received: ResultDto<SiteThemeDto> | undefined;
    service
      .updateTheme({
        primaryColor: '#111827',
        secondaryColor: '#F97316',
        accentColor: '#2563EB',
        gradientStartColor: null,
        gradientEndColor: null,
        gradientDirection: 'ToRight',
        fontFamily: null,
        headerStyle: 'Classic',
        tagline: null,
        footerText: null,
        footerCopyrightText: null,
        defaultGalleryTemplate: 'Grid',
      })
      .subscribe((r) => (received = r));

    const req = httpTesting.expectOne(baseUrl);
    expect(req.request.method).toBe('PUT');

    req.flush(response);
    expect(received).toEqual(response);
  });

  it('posts a new footer link', () => {
    const response: ResultDto<SiteThemeDto> = { success: true, message: null, data: sampleTheme };

    let received: ResultDto<SiteThemeDto> | undefined;
    service
      .addFooterLink({ platform: 'Instagram', url: 'https://instagram.com/acme' })
      .subscribe((r) => (received = r));

    const req = httpTesting.expectOne(`${baseUrl}/footer-links`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ platform: 'Instagram', url: 'https://instagram.com/acme' });

    req.flush(response);
    expect(received).toEqual(response);
  });

  it('puts a footer link update', () => {
    const response: ResultDto<SiteThemeDto> = { success: true, message: null, data: sampleTheme };

    let received: ResultDto<SiteThemeDto> | undefined;
    service
      .updateFooterLink('link-1', { platform: 'Facebook', url: 'https://facebook.com/acme', isActive: false })
      .subscribe((r) => (received = r));

    const req = httpTesting.expectOne(`${baseUrl}/footer-links/link-1`);
    expect(req.request.method).toBe('PUT');

    req.flush(response);
    expect(received).toEqual(response);
  });

  it('deletes a footer link', () => {
    const response: ResultDto<SiteThemeDto> = { success: true, message: null, data: sampleTheme };

    let received: ResultDto<SiteThemeDto> | undefined;
    service.removeFooterLink('link-1').subscribe((r) => (received = r));

    const req = httpTesting.expectOne(`${baseUrl}/footer-links/link-1`);
    expect(req.request.method).toBe('DELETE');

    req.flush(response);
    expect(received).toEqual(response);
  });

  it('puts a footer link reorder request', () => {
    const response: ResultDto<SiteThemeDto> = { success: true, message: null, data: sampleTheme };

    let received: ResultDto<SiteThemeDto> | undefined;
    service.reorderFooterLinks({ orderedFooterLinkIds: ['a', 'b'] }).subscribe((r) => (received = r));

    const req = httpTesting.expectOne(`${baseUrl}/footer-links/reorder`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ orderedFooterLinkIds: ['a', 'b'] });

    req.flush(response);
    expect(received).toEqual(response);
  });
});
