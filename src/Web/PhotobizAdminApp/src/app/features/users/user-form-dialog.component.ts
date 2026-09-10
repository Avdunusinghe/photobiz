import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { IconComponent } from '../../shared/ui/icon/icon.component';
import { ROLE_NAMES, UserDto } from './models/user.model';
import { UserService } from './services/user.service';

export type UserFormMode = 'create' | 'edit';

export interface UserFormSaved {
  message: string;
}

const MOBILE_PATTERN = /^\+?[0-9\s\-()]{7,32}$/;

function nonEmptyArray(control: AbstractControl): ValidationErrors | null {
  return Array.isArray(control.value) && control.value.length > 0 ? null : { required: true };
}

@Component({
  selector: 'app-user-form-dialog',
  imports: [ReactiveFormsModule, IconComponent],
  templateUrl: './user-form-dialog.component.html',
  styleUrl: './user-form-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '(document:keydown.escape)': 'dismiss.emit()' },
})
export class UserFormDialogComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly userService = inject(UserService);

  readonly mode = input.required<UserFormMode>();
  readonly user = input<UserDto | null>(null);

  readonly saved = output<UserFormSaved>();
  readonly dismiss = output<void>();

  protected readonly allRoles = ROLE_NAMES;
  protected readonly submitting = signal(false);
  protected readonly formError = signal<string | null>(null);

  protected form = this.buildForm(null, 'create');

  ngOnInit(): void {
    this.form = this.buildForm(this.user(), this.mode());
  }

  protected get isEdit(): boolean {
    return this.mode() === 'edit';
  }

  protected hasError(field: string): boolean {
    const control = this.form.get(field);
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  protected toggleRole(role: string): void {
    const control = this.form.controls.roles;
    const next = control.value.includes(role)
      ? control.value.filter((r) => r !== role)
      : [...control.value, role];
    control.setValue(next);
    control.markAsTouched();
  }

  protected submit(): void {
    if (this.submitting()) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.formError.set(null);

    const value = this.form.getRawValue();
    const mobileNumber = value.mobileNumber.trim() ? value.mobileNumber.trim() : null;

    const request$ = this.isEdit
      ? this.userService.updateUser(this.user()!.id, {
          username: value.username.trim(),
          firstName: value.firstName.trim(),
          lastName: value.lastName.trim(),
          email: value.email.trim(),
          mobileNumber,
          isActive: value.isActive,
          roles: value.roles,
          password: value.password ? value.password : null,
        })
      : this.userService.createUser({
          username: value.username.trim(),
          password: value.password,
          firstName: value.firstName.trim(),
          lastName: value.lastName.trim(),
          email: value.email.trim(),
          mobileNumber,
          isActive: value.isActive,
          roles: value.roles,
        });

    request$.subscribe({
      next: (result) => {
        this.submitting.set(false);
        this.saved.emit({
          message: result.message ?? (this.isEdit ? 'User updated.' : 'User created.'),
        });
      },
      error: (error: HttpErrorResponse) => {
        this.submitting.set(false);
        this.formError.set(this.describeError(error));
      },
    });
  }

  private buildForm(user: UserDto | null, mode: UserFormMode) {
    const passwordValidators =
      mode === 'edit'
        ? [Validators.minLength(8), Validators.maxLength(128)]
        : [Validators.required, Validators.minLength(8), Validators.maxLength(128)];

    return this.formBuilder.nonNullable.group({
      username: [user?.username ?? '', [Validators.required, Validators.maxLength(256)]],
      firstName: [user?.firstName ?? '', [Validators.required, Validators.maxLength(128)]],
      lastName: [user?.lastName ?? '', [Validators.required, Validators.maxLength(128)]],
      email: [
        user?.email ?? '',
        [Validators.required, Validators.email, Validators.maxLength(256)],
      ],
      mobileNumber: [
        user?.mobileNumber ?? '',
        [Validators.maxLength(32), Validators.pattern(MOBILE_PATTERN)],
      ],
      password: ['', passwordValidators],
      isActive: [user?.isActive ?? true],
      roles: this.formBuilder.nonNullable.control<string[]>(user?.roles ?? [], {
        validators: [nonEmptyArray],
      }),
    });
  }

  private describeError(error: HttpErrorResponse): string {
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

    return (
      body?.detail ||
      body?.title ||
      (error.status === 0
        ? 'Cannot reach the server. Check your connection and try again.'
        : `Request failed (${error.status}).`)
    );
  }
}
