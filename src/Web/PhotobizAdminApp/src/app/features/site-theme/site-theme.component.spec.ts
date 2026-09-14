import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { FooterLinkDto, ResultDto, SiteThemeDto } from './models/site-theme.model';
import { SiteThemeService } from './services/site-theme.service';
import { SiteThemeComponent } from './site-theme.component';

function makeFooterLink(overrides: Partial<FooterLinkDto> = {}): FooterLinkDto {
  return {
    id: crypto.randomUUID(),
    platform: 'Facebook',
    url: 'https://facebook.com/acme',
    displayOrder: 0,
    isActive: true,
    ...overrides,
  };
}

function makeTheme(overrides: Partial<SiteThemeDto> = {}): SiteThemeDto {
  return {
    id: crypto.randomUUID(),
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
    ...overrides,
  };
}

describe('SiteThemeComponent', () => {
  let siteThemeService: {
    getTheme: ReturnType<typeof vi.fn>;
    updateTheme: ReturnType<typeof vi.fn>;
    addFooterLink: ReturnType<typeof vi.fn>;
    updateFooterLink: ReturnType<typeof vi.fn>;
    removeFooterLink: ReturnType<typeof vi.fn>;
    reorderFooterLinks: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    siteThemeService = {
      getTheme: vi.fn().mockReturnValue(of(makeTheme())),
      updateTheme: vi.fn(),
      addFooterLink: vi.fn(),
      updateFooterLink: vi.fn(),
      removeFooterLink: vi.fn(),
      reorderFooterLinks: vi.fn(),
    };

    TestBed.configureTestingModule({
      imports: [SiteThemeComponent],
      providers: [provideHttpClient(), { provide: SiteThemeService, useValue: siteThemeService }],
    });
  });

  function render() {
    const fixture = TestBed.createComponent(SiteThemeComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('loads the current theme and populates the form', () => {
    siteThemeService.getTheme.mockReturnValue(
      of(makeTheme({ primaryColor: '#AA3311', tagline: 'Weddings in Colombo' })),
    );

    const fixture = render();

    expect(siteThemeService.getTheme).toHaveBeenCalledTimes(1);
    expect(fixture.componentInstance['form'].controls.primaryColor.value).toBe('#AA3311');
    expect(fixture.componentInstance['form'].controls.tagline.value).toBe('Weddings in Colombo');
  });

  it('treats a theme with both gradient colors as gradient-enabled', () => {
    siteThemeService.getTheme.mockReturnValue(
      of(makeTheme({ gradientStartColor: '#111111', gradientEndColor: '#222222' })),
    );

    const fixture = render();

    expect(fixture.componentInstance['form'].controls.useGradient.value).toBe(true);
  });

  it('shows an error banner when loading fails', () => {
    siteThemeService.getTheme.mockReturnValue(throwError(() => ({ status: 500, error: { title: 'Boom' } })));

    const fixture = render();

    expect(fixture.nativeElement.querySelector('.flash--error').textContent).toContain('Boom');
  });

  it('does not submit an invalid form', () => {
    const fixture = render();
    const instance = fixture.componentInstance;

    instance['form'].controls.primaryColor.setValue('not-a-color');
    instance['save']();

    expect(siteThemeService.updateTheme).not.toHaveBeenCalled();
    expect(instance['form'].controls.primaryColor.touched).toBe(true);
  });

  it('saves the form, sending null gradient colors when the toggle is off', () => {
    const updated = makeTheme({ primaryColor: '#ABCDEF' });
    siteThemeService.updateTheme.mockReturnValue(
      of({ success: true, message: 'Theme updated successfully.', data: updated }),
    );

    const fixture = render();
    const instance = fixture.componentInstance;
    instance['form'].controls.primaryColor.setValue('#ABCDEF');

    instance['save']();

    expect(siteThemeService.updateTheme).toHaveBeenCalledWith(
      expect.objectContaining({ primaryColor: '#ABCDEF', gradientStartColor: null, gradientEndColor: null }),
    );
    expect(instance['flash']()).toBe('Theme updated successfully.');
  });

  it('sends the gradient colors when the toggle is on', () => {
    siteThemeService.updateTheme.mockReturnValue(of({ success: true, message: null, data: makeTheme() }));

    const fixture = render();
    const instance = fixture.componentInstance;
    instance['form'].controls.useGradient.setValue(true);
    instance['form'].controls.gradientStartColor.setValue('#111111');
    instance['form'].controls.gradientEndColor.setValue('#222222');

    instance['save']();

    expect(siteThemeService.updateTheme).toHaveBeenCalledWith(
      expect.objectContaining({ gradientStartColor: '#111111', gradientEndColor: '#222222' }),
    );
  });

  it('adds a footer link', () => {
    const updated = makeTheme({ footerLinks: [makeFooterLink({ platform: 'Instagram' })] });
    siteThemeService.addFooterLink.mockReturnValue(
      of({ success: true, message: 'Footer link added successfully.', data: updated }),
    );

    const fixture = render();
    const instance = fixture.componentInstance;
    instance['startAddingLink']();
    instance['addLinkForm'].setValue({ platform: 'Instagram', url: 'https://instagram.com/acme' });

    instance['submitAddLink']();

    expect(siteThemeService.addFooterLink).toHaveBeenCalledWith({
      platform: 'Instagram',
      url: 'https://instagram.com/acme',
    });
    expect(instance['theme']()).toEqual(updated);
    expect(instance['addingLink']()).toBe(false);
  });

  it('removes a footer link', () => {
    const link = makeFooterLink();
    siteThemeService.getTheme.mockReturnValue(of(makeTheme({ footerLinks: [link] })));
    const updated = makeTheme({ footerLinks: [] });
    siteThemeService.removeFooterLink.mockReturnValue(
      of({ success: true, message: 'Footer link removed successfully.', data: updated }),
    );

    const fixture = render();
    const instance = fixture.componentInstance;

    instance['removeLink'](link);

    expect(siteThemeService.removeFooterLink).toHaveBeenCalledWith(link.id);
    expect(instance['theme']()!.footerLinks).toHaveLength(0);
  });

  it('reorders footer links by swapping with the target index', () => {
    const first = makeFooterLink({ id: 'a', displayOrder: 0 });
    const second = makeFooterLink({ id: 'b', displayOrder: 1 });
    siteThemeService.getTheme.mockReturnValue(of(makeTheme({ footerLinks: [first, second] })));
    siteThemeService.reorderFooterLinks.mockReturnValue(
      of({ success: true, message: null, data: makeTheme({ footerLinks: [second, first] }) }),
    );

    const fixture = render();
    const instance = fixture.componentInstance;

    instance['moveLink'](first, 1);

    expect(siteThemeService.reorderFooterLinks).toHaveBeenCalledWith({ orderedFooterLinkIds: ['b', 'a'] });
  });

  it('surfaces an error banner when saving the theme fails', () => {
    siteThemeService.updateTheme.mockReturnValue(
      throwError(() => ({ status: 400, error: { title: 'Bad request', detail: 'Invalid color.' } })),
    );

    const fixture = render();
    fixture.componentInstance['save']();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.flash--error').textContent).toContain('Invalid color.');
  });
});
