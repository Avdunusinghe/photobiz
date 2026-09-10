import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CreateUserRequest,
  GetUsersParams,
  PagedResult,
  ResultDto,
  UpdateUserRequest,
  UserDto,
} from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/users`;

  /** GET /api/users — paged, filtered by search text / role / active state. */
  getUsers(params: GetUsersParams = {}): Observable<PagedResult<UserDto>> {
    let httpParams = new HttpParams();

    if (params.searchText?.trim()) {
      httpParams = httpParams.set('searchText', params.searchText.trim());
    }
    if (params.role) {
      httpParams = httpParams.set('role', params.role);
    }
    if (params.isActive !== undefined && params.isActive !== null) {
      httpParams = httpParams.set('isActive', params.isActive);
    }
    if (params.pageNumber !== undefined) {
      httpParams = httpParams.set('pageNumber', params.pageNumber);
    }
    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', params.pageSize);
    }

    return this.http.get<PagedResult<UserDto>>(this.baseUrl, { params: httpParams });
  }

  /** POST /api/users — create a user. */
  createUser(request: CreateUserRequest): Observable<ResultDto<UserDto>> {
    return this.http.post<ResultDto<UserDto>>(this.baseUrl, request);
  }

  /** PUT /api/users/{id} — update a user; omit `password` to keep the current one. */
  updateUser(id: string, request: UpdateUserRequest): Observable<ResultDto<UserDto>> {
    return this.http.put<ResultDto<UserDto>>(`${this.baseUrl}/${id}`, request);
  }

  /** DELETE /api/users/{id} — remove a user and their role assignments. */
  deleteUser(id: string): Observable<ResultDto> {
    return this.http.delete<ResultDto>(`${this.baseUrl}/${id}`);
  }
}
