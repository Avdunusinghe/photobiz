/** Role names as defined by the API (`Photobiz.Domain.Entities.RoleNames`). */
export const ROLE_NAMES = ['Admin', 'Photographer', 'Assistant'] as const;

export type RoleName = (typeof ROLE_NAMES)[number];

/** Mirrors `Photobiz.Application.Features.Users.Common.UserDto`. */
export interface UserDto {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  email: string;
  mobileNumber: string | null;
  isActive: boolean;
  createdAt: string;
  roles: string[];
}

/** Mirrors `Photobiz.Application.Common.Models.PagedResult<T>`. */
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/** Mirrors `Photobiz.Application.Common.Models.ResultDto` / `ResultDto<T>`. */
export interface ResultDto<T = never> {
  success: boolean;
  message: string | null;
  data?: T;
}

/** Query parameters accepted by `GET /api/users`. */
export interface GetUsersParams {
  searchText?: string | null;
  role?: RoleName | string | null;
  isActive?: boolean | null;
  pageNumber?: number;
  pageSize?: number;
}

/** Body for `POST /api/users`. */
export interface CreateUserRequest {
  username: string;
  password: string;
  firstName: string;
  lastName: string;
  email: string;
  mobileNumber: string | null;
  isActive: boolean;
  roles: string[];
}

/** Body for `PUT /api/users/{id}`. A blank/omitted `password` leaves it unchanged. */
export interface UpdateUserRequest {
  username: string;
  firstName: string;
  lastName: string;
  email: string;
  mobileNumber: string | null;
  isActive: boolean;
  roles: string[];
  password?: string | null;
}
