import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import {
  CreateUserRequest,
  PagedResult,
  ResultDto,
  UpdateUserRequest,
  UserDto,
} from '../models/user.model';
import { UserService } from './user.service';

describe('UserService', () => {
  const baseUrl = `${environment.apiUrl}/api/users`;
  let service: UserService;
  let httpTesting: HttpTestingController;

  const sampleUser: UserDto = {
    id: '11111111-1111-1111-1111-111111111111',
    username: 'jdoe',
    firstName: 'Jane',
    lastName: 'Doe',
    email: 'jane@example.com',
    mobileNumber: null,
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    roles: ['Admin'],
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(UserService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('requests a page of users without params by default', () => {
    const page: PagedResult<UserDto> = {
      items: [sampleUser],
      totalCount: 1,
      pageNumber: 1,
      pageSize: 20,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    };

    let received: PagedResult<UserDto> | undefined;
    service.getUsers().subscribe((result) => (received = result));

    const req = httpTesting.expectOne((r) => r.url === baseUrl);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.keys()).toEqual([]);

    req.flush(page);
    expect(received).toEqual(page);
  });

  it('only serialises provided filters and trims the search text', () => {
    service
      .getUsers({
        searchText: '  ada  ',
        role: 'Photographer',
        isActive: false,
        pageNumber: 2,
        pageSize: 10,
      })
      .subscribe();

    const req = httpTesting.expectOne((r) => r.url === baseUrl);
    expect(req.request.params.get('searchText')).toBe('ada');
    expect(req.request.params.get('role')).toBe('Photographer');
    expect(req.request.params.get('isActive')).toBe('false');
    expect(req.request.params.get('pageNumber')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('10');

    req.flush({
      items: [],
      totalCount: 0,
      pageNumber: 2,
      pageSize: 10,
      totalPages: 0,
      hasPreviousPage: true,
      hasNextPage: false,
    });
  });

  it('omits blank or nullish filters', () => {
    service.getUsers({ searchText: '   ', role: null, isActive: null }).subscribe();

    const req = httpTesting.expectOne((r) => r.url === baseUrl);
    expect(req.request.params.keys()).toEqual([]);
    req.flush({
      items: [],
      totalCount: 0,
      pageNumber: 1,
      pageSize: 20,
      totalPages: 0,
      hasPreviousPage: false,
      hasNextPage: false,
    });
  });

  it('posts a create request', () => {
    const body: CreateUserRequest = {
      username: 'jdoe',
      password: 'sup3r-secret',
      firstName: 'Jane',
      lastName: 'Doe',
      email: 'jane@example.com',
      mobileNumber: null,
      isActive: true,
      roles: ['Admin'],
    };
    const response: ResultDto<UserDto> = {
      success: true,
      message: 'User created successfully.',
      data: sampleUser,
    };

    let received: ResultDto<UserDto> | undefined;
    service.createUser(body).subscribe((r) => (received = r));

    const req = httpTesting.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(body);

    req.flush(response);
    expect(received).toEqual(response);
  });

  it('puts an update request to the user id url', () => {
    const body: UpdateUserRequest = {
      username: 'jdoe',
      firstName: 'Jane',
      lastName: 'Doe',
      email: 'jane@example.com',
      mobileNumber: '+1 555 0100',
      isActive: false,
      roles: ['Admin', 'Assistant'],
    };

    service.updateUser(sampleUser.id, body).subscribe();

    const req = httpTesting.expectOne(`${baseUrl}/${sampleUser.id}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(body);

    req.flush({ success: true, message: 'User updated successfully.', data: sampleUser });
  });

  it('deletes by user id', () => {
    let received: ResultDto | undefined;
    service.deleteUser(sampleUser.id).subscribe((r) => (received = r));

    const req = httpTesting.expectOne(`${baseUrl}/${sampleUser.id}`);
    expect(req.request.method).toBe('DELETE');

    const response: ResultDto = { success: true, message: 'User deleted successfully.' };
    req.flush(response);
    expect(received).toEqual(response);
  });
});
