import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { IconComponent } from '../../shared/ui/icon/icon.component';
import { SmtpSettingDto, TenantDetailsDto } from './models/tenant.model';
import { TenantSettingsService } from './services/tenant-settings.service';

const MAX_LOGO_SIZE_BYTES = 5 * 1024 * 1024;
const ALLOWED_LOGO_TYPES = ['image/png', 'image/jpeg', 'image/webp', 'image/gif'];

@Component({
  selector: 'app-tenant-settings',
  imports: [ReactiveFormsModule, IconComponent],
  templateUrl: './tenant-settings.component.html',
  styleUrl: './tenant-settings.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TenantSettingsComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly tenantSettingsService = inject(TenantSettingsService);

  protected readonly tenant = signal<TenantDetailsDto | null>(null);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly uploadingLogo = signal(false);
  protected readonly removingLogo = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly flash = signal<string | null>(null);
  protected readonly logoPreviewFailed = signal(false);
  protected readonly logoError = signal<string | null>(null);

  protected readonly smtpSettings = signal<SmtpSettingDto | null>(null);
  protected readonly smtpLoading = signal(true);
  protected readonly smtpError = signal<string | null>(null);

  protected readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(256)]],
    customerFirstName: ['', [Validators.required, Validators.maxLength(128)]],
    customerLastName: ['', [Validators.required, Validators.maxLength(128)]],
    customerEmail: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    phone: ['', [Validators.required, Validators.maxLength(32)]],
    address: ['', [Validators.required, Validators.maxLength(256)]],
    city: ['', [Validators.required, Validators.maxLength(128)]],
    country: ['', [Validators.required, Validators.maxLength(128)]],
  });

  constructor() {
    this.load();
    this.loadSmtpSettings();
  }

  protected hasError(field: string): boolean {
    const control = this.form.get(field);
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.tenantSettingsService.getTenantDetails().subscribe({
      next: (tenant) => {
        this.tenant.set(tenant);
        this.form.reset({
          name: tenant.name,
          customerFirstName: tenant.customerFirstName,
          customerLastName: tenant.customerLastName,
          customerEmail: tenant.customerEmail,
          phone: tenant.phone,
          address: tenant.address,
          city: tenant.city,
          country: tenant.country,
        });
        this.logoPreviewFailed.set(false);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.error.set(this.describeError(error));
        this.loading.set(false);
      },
    });
  }

  protected loadSmtpSettings(): void {
    this.smtpLoading.set(true);
    this.smtpError.set(null);

    this.tenantSettingsService.getSmtpSettings().subscribe({
      next: (smtp) => {
        this.smtpSettings.set(smtp);
        this.smtpLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.smtpError.set(this.describeError(error));
        this.smtpLoading.set(false);
      },
    });
  }

  protected onLogoPreviewError(): void {
    this.logoPreviewFailed.set(true);
  }

  protected onLogoFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';

    if (!file) {
      return;
    }

    this.logoError.set(null);

    if (!ALLOWED_LOGO_TYPES.includes(file.type)) {
      this.logoError.set('Logo must be a PNG, JPEG, WebP, or GIF image.');
      return;
    }
    if (file.size > MAX_LOGO_SIZE_BYTES) {
      this.logoError.set('Logo must be 5 MB or smaller.');
      return;
    }

    this.uploadingLogo.set(true);

    this.tenantSettingsService.uploadLogo(file).subscribe({
      next: (result) => {
        this.uploadingLogo.set(false);
        if (result.data) {
          this.tenant.set(result.data);
        }
        this.logoPreviewFailed.set(false);
        this.flash.set(result.message ?? 'Logo uploaded.');
      },
      error: (error: HttpErrorResponse) => {
        this.uploadingLogo.set(false);
        this.logoError.set(this.describeError(error));
      },
    });
  }

  protected removeLogo(): void {
    if (this.uploadingLogo() || this.removingLogo()) {
      return;
    }

    this.removingLogo.set(true);
    this.logoError.set(null);

    this.tenantSettingsService.removeLogo().subscribe({
      next: (result) => {
        this.removingLogo.set(false);
        if (result.data) {
          this.tenant.set(result.data);
        }
        this.logoPreviewFailed.set(false);
        this.flash.set(result.message ?? 'Logo removed.');
      },
      error: (error: HttpErrorResponse) => {
        this.removingLogo.set(false);
        this.logoError.set(this.describeError(error));
      },
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

    this.tenantSettingsService
      .updateTenantDetails({
        name: value.name.trim(),
        customerEmail: value.customerEmail.trim(),
        customerFirstName: value.customerFirstName.trim(),
        customerLastName: value.customerLastName.trim(),
        phone: value.phone.trim(),
        address: value.address.trim(),
        city: value.city.trim(),
        country: value.country.trim(),
      })
      .subscribe({
        next: (result) => {
          this.saving.set(false);
          if (result.data) {
            this.tenant.set(result.data);
          }
          this.flash.set(result.message ?? 'Tenant details updated.');
        },
        error: (error: HttpErrorResponse) => {
          this.saving.set(false);
          this.error.set(this.describeError(error));
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
