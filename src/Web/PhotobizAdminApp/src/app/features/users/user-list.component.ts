import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, map } from 'rxjs';
import { IconComponent } from '../../shared/ui/icon/icon.component';
import { ROLE_NAMES, UserDto } from './models/user.model';
import { UserService } from './services/user.service';
import { UserFormDialogComponent, UserFormMode, UserFormSaved } from './user-form-dialog.component';

type StatusFilter = 'any' | 'active' | 'inactive';

@Component({
  selector: 'app-user-list',
  imports: [ReactiveFormsModule, DatePipe, IconComponent, UserFormDialogComponent],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserListComponent {
  private readonly userService = inject(UserService);

  protected readonly roles = ROLE_NAMES;
  protected readonly pageSizeOptions = [10, 20, 50];

  protected readonly searchControl = new FormControl('', { nonNullable: true });
  protected readonly roleFilter = signal('');
  protected readonly statusFilter = signal<StatusFilter>('any');
  protected readonly pageNumber = signal(1);
  protected readonly pageSize = signal(20);

  protected readonly users = signal<UserDto[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly totalPages = signal(0);
  protected readonly loading = signal(false);
  protected readonly loaded = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly flash = signal<string | null>(null);

  protected readonly selectedId = signal<string | null>(null);
  protected readonly selectedUser = computed(
    () => this.users().find((user) => user.id === this.selectedId()) ?? null,
  );

  protected readonly dialogMode = signal<'closed' | UserFormMode>('closed');
  protected readonly formMode = computed<UserFormMode>(() =>
    this.dialogMode() === 'edit' ? 'edit' : 'create',
  );
  protected readonly confirmingDelete = signal(false);
  protected readonly deleting = signal(false);

  protected readonly hasFilters = computed(
    () =>
      this.searchControl.value.trim().length > 0 ||
      this.roleFilter().length > 0 ||
      this.statusFilter() !== 'any',
  );
  protected readonly rangeStart = computed(() =>
    this.totalCount() === 0 ? 0 : (this.pageNumber() - 1) * this.pageSize() + 1,
  );
  protected readonly rangeEnd = computed(() =>
    Math.min(this.pageNumber() * this.pageSize(), this.totalCount()),
  );

  constructor() {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        map((value) => value.trim()),
        distinctUntilChanged(),
        takeUntilDestroyed(),
      )
      .subscribe(() => {
        this.pageNumber.set(1);
        this.load();
      });

    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.userService
      .getUsers({
        searchText: this.searchControl.value,
        role: this.roleFilter() || null,
        isActive: this.statusFilter() === 'any' ? null : this.statusFilter() === 'active',
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize(),
      })
      .subscribe({
        next: (page) => {
          this.users.set(page.items);
          this.totalCount.set(page.totalCount);
          this.totalPages.set(page.totalPages);
          if (!page.items.some((user) => user.id === this.selectedId())) {
            this.selectedId.set(null);
          }
          this.loading.set(false);
          this.loaded.set(true);
        },
        error: (error: HttpErrorResponse) => {
          this.error.set(this.describeError(error));
          this.loading.set(false);
          this.loaded.set(true);
        },
      });
  }

  protected refresh(): void {
    this.load();
  }

  protected setRoleFilter(value: string): void {
    this.roleFilter.set(value);
    this.pageNumber.set(1);
    this.load();
  }

  protected setStatusFilter(value: StatusFilter): void {
    this.statusFilter.set(value);
    this.pageNumber.set(1);
    this.load();
  }

  protected setPageSize(value: number): void {
    this.pageSize.set(value);
    this.pageNumber.set(1);
    this.load();
  }

  protected clearFilters(): void {
    this.searchControl.setValue('', { emitEvent: false });
    this.roleFilter.set('');
    this.statusFilter.set('any');
    this.pageNumber.set(1);
    this.load();
  }

  protected previousPage(): void {
    if (this.pageNumber() > 1) {
      this.pageNumber.update((page) => page - 1);
      this.load();
    }
  }

  protected nextPage(): void {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update((page) => page + 1);
      this.load();
    }
  }

  protected toggleSelection(user: UserDto): void {
    this.selectedId.set(this.selectedId() === user.id ? null : user.id);
  }

  protected fullName(user: UserDto): string {
    return `${user.firstName} ${user.lastName}`.trim();
  }

  protected openCreate(): void {
    this.flash.set(null);
    this.dialogMode.set('create');
  }

  protected openEdit(): void {
    if (this.selectedUser()) {
      this.flash.set(null);
      this.dialogMode.set('edit');
    }
  }

  protected closeDialog(): void {
    this.dialogMode.set('closed');
  }

  protected onDialogSaved(result: UserFormSaved): void {
    this.dialogMode.set('closed');
    this.flash.set(result.message);
    this.load();
  }

  protected askDelete(): void {
    if (this.selectedUser()) {
      this.confirmingDelete.set(true);
    }
  }

  protected cancelDelete(): void {
    this.confirmingDelete.set(false);
  }

  protected confirmDelete(): void {
    const user = this.selectedUser();
    if (!user || this.deleting()) {
      return;
    }

    this.deleting.set(true);

    this.userService.deleteUser(user.id).subscribe({
      next: (result) => {
        this.deleting.set(false);
        this.confirmingDelete.set(false);
        this.selectedId.set(null);
        this.flash.set(result.message ?? `User “${user.username}” was deleted.`);
        if (this.users().length === 1 && this.pageNumber() > 1) {
          this.pageNumber.update((page) => page - 1);
        }
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.deleting.set(false);
        this.confirmingDelete.set(false);
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
    const body = error.error as { detail?: string; title?: string } | null;
    return body?.detail || body?.title || `Request failed (${error.status}).`;
  }
}
