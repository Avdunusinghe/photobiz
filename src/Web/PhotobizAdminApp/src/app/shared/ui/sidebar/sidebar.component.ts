import { ChangeDetectionStrategy, Component, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NavItem } from '../../../core/models/nav-item.model';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, IconComponent],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent {
  readonly items = input.required<NavItem[]>();
  readonly currentUrl = input.required<string>();

  protected readonly collapsed = signal(false);
  protected readonly expandedGroups = signal<ReadonlySet<string>>(new Set());

  protected toggleCollapsed(): void {
    this.collapsed.update((value) => !value);
  }

  protected toggleGroup(label: string): void {
    this.expandedGroups.update((current) => {
      const next = new Set(current);
      if (next.has(label)) {
        next.delete(label);
      } else {
        next.add(label);
      }
      return next;
    });
  }

  protected isGroupExpanded(label: string): boolean {
    return this.expandedGroups().has(label);
  }

  protected isActive(route: string): boolean {
    return this.currentUrl() === route;
  }

  protected isGroupActive(item: NavItem): boolean {
    return (item.children ?? []).some((child) => this.isActive(child.route));
  }
}
