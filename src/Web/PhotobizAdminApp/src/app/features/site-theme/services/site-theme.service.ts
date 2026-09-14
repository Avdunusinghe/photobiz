import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  AddFooterLinkRequest,
  ReorderFooterLinksRequest,
  ResultDto,
  SiteThemeDto,
  UpdateFooterLinkRequest,
  UpdateSiteThemeRequest,
} from '../models/site-theme.model';

@Injectable({ providedIn: 'root' })
export class SiteThemeService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/tenant/theme`;

  /** GET /api/tenant/theme — the calling tenant's own portfolio site theme. */
  getTheme(): Observable<SiteThemeDto> {
    return this.http.get<SiteThemeDto>(this.baseUrl);
  }

  /** PUT /api/tenant/theme — replaces the calling tenant's theme. Footer links are managed separately. */
  updateTheme(request: UpdateSiteThemeRequest): Observable<ResultDto<SiteThemeDto>> {
    return this.http.put<ResultDto<SiteThemeDto>>(this.baseUrl, request);
  }

  /** POST /api/tenant/theme/footer-links */
  addFooterLink(request: AddFooterLinkRequest): Observable<ResultDto<SiteThemeDto>> {
    return this.http.post<ResultDto<SiteThemeDto>>(`${this.baseUrl}/footer-links`, request);
  }

  /** PUT /api/tenant/theme/footer-links/{id} */
  updateFooterLink(id: string, request: UpdateFooterLinkRequest): Observable<ResultDto<SiteThemeDto>> {
    return this.http.put<ResultDto<SiteThemeDto>>(`${this.baseUrl}/footer-links/${id}`, request);
  }

  /** DELETE /api/tenant/theme/footer-links/{id} */
  removeFooterLink(id: string): Observable<ResultDto<SiteThemeDto>> {
    return this.http.delete<ResultDto<SiteThemeDto>>(`${this.baseUrl}/footer-links/${id}`);
  }

  /** PUT /api/tenant/theme/footer-links/reorder */
  reorderFooterLinks(request: ReorderFooterLinksRequest): Observable<ResultDto<SiteThemeDto>> {
    return this.http.put<ResultDto<SiteThemeDto>>(`${this.baseUrl}/footer-links/reorder`, request);
  }
}
