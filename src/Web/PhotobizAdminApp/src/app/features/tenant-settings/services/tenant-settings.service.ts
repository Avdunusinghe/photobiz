import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ResultDto,
  SmtpSettingDto,
  TenantDetailsDto,
  UpdateTenantDetailsRequest,
} from '../models/tenant.model';

@Injectable({ providedIn: 'root' })
export class TenantSettingsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/tenant`;

  /** GET /api/tenant — the calling user's own tenant; there's no id, the API resolves it from the JWT. */
  getTenantDetails(): Observable<TenantDetailsDto> {
    return this.http.get<TenantDetailsDto>(this.baseUrl);
  }

  /** PUT /api/tenant — updates the calling user's own tenant's profile. */
  updateTenantDetails(
    request: UpdateTenantDetailsRequest,
  ): Observable<ResultDto<TenantDetailsDto>> {
    return this.http.put<ResultDto<TenantDetailsDto>>(this.baseUrl, request);
  }

  /** POST /api/tenant/logo — uploads (or replaces) the calling user's own tenant's logo. */
  uploadLogo(file: File): Observable<ResultDto<TenantDetailsDto>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ResultDto<TenantDetailsDto>>(`${this.baseUrl}/logo`, formData);
  }

  /** DELETE /api/tenant/logo — removes the calling user's own tenant's logo. */
  removeLogo(): Observable<ResultDto<TenantDetailsDto>> {
    return this.http.delete<ResultDto<TenantDetailsDto>>(`${this.baseUrl}/logo`);
  }

  /** GET /api/tenant/smtp — the calling user's own tenant's SMTP configuration, or `null` if not yet configured. */
  getSmtpSettings(): Observable<SmtpSettingDto | null> {
    return this.http.get<SmtpSettingDto | null>(`${this.baseUrl}/smtp`);
  }
}
