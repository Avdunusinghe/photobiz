import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { IconComponent } from '../../shared/ui/icon/icon.component';
import {
  FooterLinkDto,
  FooterLinkPlatform,
  GalleryTemplate,
  GradientDirection,
  HeaderStyle,
  SiteThemeDto,
} from './models/site-theme.model';
import { SiteThemeService } from './services/site-theme.service';

const HEX_PATTERN = /^#[0-9A-Fa-f]{6}$/;

export const GRADIENT_DIRECTIONS: { value: GradientDirection; label: string; css: string }[] = [
  { value: 'ToRight', label: 'Left to right', css: 'to right' },
  { value: 'ToBottom', label: 'Top to bottom', css: 'to bottom' },
  { value: 'Diagonal', label: 'Diagonal', css: 'to bottom right' },
];

export const HEADER_STYLES: { value: HeaderStyle; label: string; description: string }[] = [
  { value: 'Classic', label: 'Classic', description: 'Logo and nav in a solid top bar.' },
  { value: 'Centered', label: 'Centered', description: 'Logo centered, nav below it.' },
  {
    value: 'TransparentOverHero',
    label: 'Transparent over hero',
    description: 'Header floats over the hero image/gradient.',
  },
];

export const GALLERY_TEMPLATES: { value: GalleryTemplate; label: string; description: string }[] = [
  { value: 'Grid', label: 'Grid', description: 'Even rows and columns.' },
  { value: 'Masonry', label: 'Masonry', description: 'Staggered, Pinterest-style layout.' },
  { value: 'Carousel', label: 'Carousel', description: 'One row, horizontal scroll.' },
  { value: 'Slideshow', label: 'Slideshow', description: 'One large photo at a time.' },
];

export const FONT_OPTIONS = [
  { value: '', label: 'Platform default' },
  { value: 'Inter', label: 'Inter' },
  { value: 'Playfair Display', label: 'Playfair Display' },
  { value: 'Georgia', label: 'Georgia' },
  { value: 'Montserrat', label: 'Montserrat' },
  { value: 'Lora', label: 'Lora' },
];

export const FOOTER_LINK_PLATFORMS: { value: FooterLinkPlatform; label: string }[] = [
  { value: 'Facebook', label: 'Facebook' },
  { value: 'Instagram', label: 'Instagram' },
  { value: 'X', label: 'X (Twitter)' },
  { value: 'Pinterest', label: 'Pinterest' },
  { value: 'LinkedIn', label: 'LinkedIn' },
  { value: 'YouTube', label: 'YouTube' },
  { value: 'TikTok', label: 'TikTok' },
  { value: 'Website', label: 'Website' },
];

@Component({
  selector: 'app-site-theme',
  imports: [ReactiveFormsModule, IconComponent],
  templateUrl: './site-theme.component.html',
  styleUrl: './site-theme.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SiteThemeComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly siteThemeService = inject(SiteThemeService);

  protected readonly gradientDirections = GRADIENT_DIRECTIONS;
  protected readonly headerStyles = HEADER_STYLES;
  protected readonly galleryTemplates = GALLERY_TEMPLATES;
  protected readonly fontOptions = FONT_OPTIONS;
  protected readonly footerLinkPlatforms = FOOTER_LINK_PLATFORMS;
  protected readonly currentYear = new Date().getFullYear();
  protected readonly previewBlocks = [0, 1, 2, 3, 4, 5];

  protected readonly theme = signal<SiteThemeDto | null>(null);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly flash = signal<string | null>(null);

  protected readonly addingLink = signal(false);
  protected readonly linkError = signal<string | null>(null);
  protected readonly editingLinkId = signal<string | null>(null);
  protected readonly linkBusyId = signal<string | null>(null);

  protected readonly form = this.formBuilder.nonNullable.group({
    primaryColor: ['#111827', [Validators.required, Validators.pattern(HEX_PATTERN)]],
    secondaryColor: ['#F97316', [Validators.required, Validators.pattern(HEX_PATTERN)]],
    accentColor: ['#2563EB', [Validators.required, Validators.pattern(HEX_PATTERN)]],
    useGradient: [false],
    gradientStartColor: ['#111827', [Validators.pattern(HEX_PATTERN)]],
    gradientEndColor: ['#2563EB', [Validators.pattern(HEX_PATTERN)]],
    gradientDirection: ['ToRight' as GradientDirection],
    fontFamily: [''],
    headerStyle: ['Classic' as HeaderStyle],
    tagline: ['', [Validators.maxLength(160)]],
    footerText: ['', [Validators.maxLength(500)]],
    footerCopyrightText: ['', [Validators.maxLength(200)]],
    defaultGalleryTemplate: ['Grid' as GalleryTemplate],
  });

  protected readonly addLinkForm = this.formBuilder.nonNullable.group({
    platform: ['Facebook' as FooterLinkPlatform],
    url: ['', [Validators.required]],
  });

  protected readonly editLinkForm = this.formBuilder.nonNullable.group({
    platform: ['Facebook' as FooterLinkPlatform],
    url: ['', [Validators.required]],
    isActive: [true],
  });

  protected readonly formValue = toSignal(this.form.valueChanges, {
    initialValue: this.form.getRawValue(),
  });

  protected readonly previewBackground = computed(() => {
    const value = this.formValue();
    if (value.useGradient) {
      const direction = this.gradientDirections.find((d) => d.value === value.gradientDirection);
      return `linear-gradient(${direction?.css ?? 'to right'}, ${value.gradientStartColor}, ${value.gradientEndColor})`;
    }
    return value.primaryColor;
  });

  protected readonly previewFontFamily = computed(() => {
    const font = this.formValue().fontFamily;
    return font ? `${font}, sans-serif` : null;
  });

  constructor() {
    this.load();
  }

  protected headerStyleDescription(style: HeaderStyle): string {
    return this.headerStyles.find((option) => option.value === style)?.description ?? '';
  }

  protected hasError(field: string): boolean {
    const control = this.form.get(field);
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.siteThemeService.getTheme().subscribe({
      next: (theme) => {
        this.applyTheme(theme);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.error.set(this.describeError(error));
        this.loading.set(false);
      },
    });
  }

  private applyTheme(theme: SiteThemeDto): void {
    this.theme.set(theme);
    this.form.reset({
      primaryColor: theme.primaryColor,
      secondaryColor: theme.secondaryColor,
      accentColor: theme.accentColor,
      useGradient: !!theme.gradientStartColor && !!theme.gradientEndColor,
      gradientStartColor: theme.gradientStartColor ?? theme.primaryColor,
      gradientEndColor: theme.gradientEndColor ?? theme.accentColor,
      gradientDirection: theme.gradientDirection,
      fontFamily: theme.fontFamily ?? '',
      headerStyle: theme.headerStyle,
      tagline: theme.tagline ?? '',
      footerText: theme.footerText ?? '',
      footerCopyrightText: theme.footerCopyrightText ?? '',
      defaultGalleryTemplate: theme.defaultGalleryTemplate,
    });
  }

  protected save(): void {
    if (this.saving()) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    const value = this.form.getRawValue();

    this.siteThemeService
      .updateTheme({
        primaryColor: value.primaryColor,
        secondaryColor: value.secondaryColor,
        accentColor: value.accentColor,
        gradientStartColor: value.useGradient ? value.gradientStartColor : null,
        gradientEndColor: value.useGradient ? value.gradientEndColor : null,
        gradientDirection: value.gradientDirection,
        fontFamily: value.fontFamily.trim() ? value.fontFamily.trim() : null,
        headerStyle: value.headerStyle,
        tagline: value.tagline.trim() ? value.tagline.trim() : null,
        footerText: value.footerText.trim() ? value.footerText.trim() : null,
        footerCopyrightText: value.footerCopyrightText.trim() ? value.footerCopyrightText.trim() : null,
        defaultGalleryTemplate: value.defaultGalleryTemplate,
      })
      .subscribe({
        next: (result) => {
          this.saving.set(false);
          if (result.data) {
            this.applyTheme(result.data);
          }
          this.flash.set(result.message ?? 'Theme updated.');
        },
        error: (error: HttpErrorResponse) => {
          this.saving.set(false);
          this.error.set(this.describeError(error));
        },
      });
  }

  protected startAddingLink(): void {
    this.addLinkForm.reset({ platform: 'Facebook', url: '' });
    this.linkError.set(null);
    this.addingLink.set(true);
  }

  protected cancelAddingLink(): void {
    this.addingLink.set(false);
  }

  protected submitAddLink(): void {
    if (this.addLinkForm.invalid) {
      this.addLinkForm.markAllAsTouched();
      return;
    }

    const value = this.addLinkForm.getRawValue();
    this.linkError.set(null);
    this.linkBusyId.set('new');

    this.siteThemeService.addFooterLink({ platform: value.platform, url: value.url.trim() }).subscribe({
      next: (result) => {
        this.linkBusyId.set(null);
        this.addingLink.set(false);
        if (result.data) {
          this.theme.set(result.data);
        }
        this.flash.set(result.message ?? 'Footer link added.');
      },
      error: (error: HttpErrorResponse) => {
        this.linkBusyId.set(null);
        this.linkError.set(this.describeError(error));
      },
    });
  }

  protected startEditingLink(link: FooterLinkDto): void {
    this.editingLinkId.set(link.id);
    this.linkError.set(null);
    this.editLinkForm.reset({ platform: link.platform, url: link.url, isActive: link.isActive });
  }

  protected cancelEditingLink(): void {
    this.editingLinkId.set(null);
  }

  protected submitEditLink(link: FooterLinkDto): void {
    if (this.editLinkForm.invalid) {
      this.editLinkForm.markAllAsTouched();
      return;
    }

    const value = this.editLinkForm.getRawValue();
    this.linkError.set(null);
    this.linkBusyId.set(link.id);

    this.siteThemeService
      .updateFooterLink(link.id, { platform: value.platform, url: value.url.trim(), isActive: value.isActive })
      .subscribe({
        next: (result) => {
          this.linkBusyId.set(null);
          this.editingLinkId.set(null);
          if (result.data) {
            this.theme.set(result.data);
          }
          this.flash.set(result.message ?? 'Footer link updated.');
        },
        error: (error: HttpErrorResponse) => {
          this.linkBusyId.set(null);
          this.linkError.set(this.describeError(error));
        },
      });
  }

  protected removeLink(link: FooterLinkDto): void {
    this.linkBusyId.set(link.id);
    this.linkError.set(null);

    this.siteThemeService.removeFooterLink(link.id).subscribe({
      next: (result) => {
        this.linkBusyId.set(null);
        if (result.data) {
          this.theme.set(result.data);
        }
        this.flash.set(result.message ?? 'Footer link removed.');
      },
      error: (error: HttpErrorResponse) => {
        this.linkBusyId.set(null);
        this.linkError.set(this.describeError(error));
      },
    });
  }

  protected moveLink(link: FooterLinkDto, direction: -1 | 1): void {
    const links = this.theme()?.footerLinks ?? [];
    const index = links.findIndex((l) => l.id === link.id);
    const targetIndex = index + direction;
    if (index < 0 || targetIndex < 0 || targetIndex >= links.length) {
      return;
    }

    const reordered = [...links];
    [reordered[index], reordered[targetIndex]] = [reordered[targetIndex], reordered[index]];

    this.linkBusyId.set(link.id);
    this.linkError.set(null);

    this.siteThemeService
      .reorderFooterLinks({ orderedFooterLinkIds: reordered.map((l) => l.id) })
      .subscribe({
        next: (result) => {
          this.linkBusyId.set(null);
          if (result.data) {
            this.theme.set(result.data);
          }
        },
        error: (error: HttpErrorResponse) => {
          this.linkBusyId.set(null);
          this.linkError.set(this.describeError(error));
        },
      });
  }

  protected dismissFlash(): void {
    this.flash.set(null);
  }

  private describeError(error: HttpErrorResponse): string {
    if (error.status === 0) {
      return 'Cannot reach the server. Check your connection and try again.';
    }

    const body = error.error as {
      detail?: string;
      title?: string;
      errors?: Record<string, string[]>;
    } | null;

    if (body?.errors) {
      const messages = Object.values(body.errors).flat();
      if (messages.length > 0) {
        return messages.join(' ');
      }
    }

    return body?.detail || body?.title || `Request failed (${error.status}).`;
  }
}
