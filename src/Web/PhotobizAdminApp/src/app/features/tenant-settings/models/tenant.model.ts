/** Mirrors `Photobiz.Application.Common.Models.ResultDto` / `ResultDto<T>`. */
export interface ResultDto<T = never> {
  success: boolean;
  message: string | null;
  data?: T;
}

/** Mirrors `Photobiz.Application.Features.Tenants.Common.TenantDetailsDto`. */
export interface TenantDetailsDto {
  id: string;
  tenantKey: string;
  name: string;
  logoUrl: string | null;
  customerEmail: string;
  customerFirstName: string;
  customerLastName: string;
  phone: string;
  address: string;
  city: string;
  country: string;
  planCode: string;
  billingCycle: string;
  isSubscribed: boolean;
  subscriptionExpiredOn: string | null;
  customDomain: string | null;
  customDomainVerifiedAt: string | null;
  createdAt: string;
}

/** Body for `PUT /api/tenant`. Logo is managed separately via upload/remove. */
export interface UpdateTenantDetailsRequest {
  name: string;
  customerEmail: string;
  customerFirstName: string;
  customerLastName: string;
  phone: string;
  address: string;
  city: string;
  country: string;
}

/**
 * Mirrors `Photobiz.Application.Features.SmtpSettings.Common.SmtpSettingDto`. Deliberately has no
 * password field — the API never sends the credential back once saved.
 */
export interface SmtpSettingDto {
  id: string;
  host: string;
  port: number;
  username: string;
  enableSsl: boolean;
  fromEmail: string;
  fromName: string | null;
  isEnabled: boolean;
  createdAt: string;
  updatedAt: string | null;
}
