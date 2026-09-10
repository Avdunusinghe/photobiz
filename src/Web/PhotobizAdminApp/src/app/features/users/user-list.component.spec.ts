import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { PagedResult, ResultDto, UserDto } from './models/user.model';
import { UserService } from './services/user.service';
import { UserListComponent } from './user-list.component';

function makeUser(overrides: Partial<UserDto> = {}): UserDto {
  return {
    id: crypto.randomUUID(),
    username: 'jdoe',
    firstName: 'Jane',
    lastName: 'Doe',
    email: 'jane@example.com',
    mobileNumber: null,
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
    roles: ['Admin'],
    ...overrides,
  };
}

function makePage(
  items: UserDto[],
  overrides: Partial<PagedResult<UserDto>> = {},
): PagedResult<UserDto> {
  return {
    items,
    totalCount: items.length,
    pageNumber: 1,
    pageSize: 20,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
    ...overrides,
  };
}

describe('UserListComponent', () => {
  let userService: {
    getUsers: ReturnType<typeof vi.fn>;
    createUser: ReturnType<typeof vi.fn>;
    updateUser: ReturnType<typeof vi.fn>;
    deleteUser: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    userService = {
      getUsers: vi.fn().mockReturnValue(of(makePage([makeUser()]))),
      createUser: vi.fn(),
      updateUser: vi.fn(),
      deleteUser: vi.fn(),
    };

    TestBed.configureTestingModule({
      imports: [UserListComponent],
      providers: [provideHttpClient(), { provide: UserService, useValue: userService }],
    });
  });

  function render() {
    const fixture = TestBed.createComponent(UserListComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('loads and renders a row per user on init', () => {
    const fixture = render();

    expect(userService.getUsers).toHaveBeenCalledTimes(1);
    expect(fixture.nativeElement.querySelectorAll('.table__row').length).toBe(1);
  });

  it('shows the empty state when there are no users and no filters', () => {
    userService.getUsers.mockReturnValue(of(makePage([])));

    const fixture = render();

    expect(fixture.nativeElement.querySelector('.state__title').textContent).toContain(
      'No users yet',
    );
  });

  it('passes role and status filters and resets to page 1', () => {
    const fixture = render();
    fixture.componentInstance['pageNumber'].set(3);

    fixture.componentInstance['setRoleFilter']('Photographer');
    fixture.componentInstance['setStatusFilter']('inactive');

    expect(userService.getUsers).toHaveBeenLastCalledWith(
      expect.objectContaining({ role: 'Photographer', isActive: false, pageNumber: 1 }),
    );
  });

  it('debounces the search box before reloading', () => {
    const fixture = render();
    userService.getUsers.mockClear();
    vi.useFakeTimers();

    fixture.componentInstance['searchControl'].setValue('ada');
    vi.advanceTimersByTime(100);
    expect(userService.getUsers).not.toHaveBeenCalled();

    vi.advanceTimersByTime(250);
    expect(userService.getUsers).toHaveBeenCalledWith(
      expect.objectContaining({ searchText: 'ada', pageNumber: 1 }),
    );

    vi.useRealTimers();
  });

  it('enables row actions on selection and deletes through the service', () => {
    const user = makeUser({ username: 'target' });
    userService.getUsers.mockReturnValue(of(makePage([user])));
    const deleteResult: ResultDto = { success: true, message: 'User deleted successfully.' };
    userService.deleteUser.mockReturnValue(of(deleteResult));

    const fixture = render();
    const instance = fixture.componentInstance;

    instance['toggleSelection'](user);
    fixture.detectChanges();
    expect(instance['selectedUser']()).toEqual(user);

    instance['askDelete']();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.confirm')).toBeTruthy();

    instance['confirmDelete']();

    expect(userService.deleteUser).toHaveBeenCalledWith(user.id);
    expect(instance['flash']()).toBe('User deleted successfully.');
    expect(instance['selectedId']()).toBeNull();
  });

  it('surfaces an error banner when loading fails', () => {
    userService.getUsers.mockReturnValue(
      throwError(() => ({ status: 500, error: { title: 'Boom' } })),
    );

    const fixture = render();

    expect(fixture.nativeElement.querySelector('.flash--error').textContent).toContain('Boom');
  });

  it('advances to the next page', () => {
    userService.getUsers.mockReturnValue(
      of(makePage([makeUser()], { totalCount: 40, totalPages: 2 })),
    );

    const fixture = render();
    fixture.componentInstance['nextPage']();

    expect(userService.getUsers).toHaveBeenLastCalledWith(
      expect.objectContaining({ pageNumber: 2 }),
    );
  });
});
