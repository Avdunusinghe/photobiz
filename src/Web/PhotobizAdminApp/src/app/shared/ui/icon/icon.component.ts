import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export type IconName =
  | 'dashboard'
  | 'calendar'
  | 'gallery'
  | 'users'
  | 'settings'
  | 'chevron'
  | 'search'
  | 'refresh'
  | 'plus'
  | 'close';

@Component({
  selector: 'app-icon',
  templateUrl: './icon.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconComponent {
  readonly name = input.required<IconName>();
  readonly size = input(20);
}
