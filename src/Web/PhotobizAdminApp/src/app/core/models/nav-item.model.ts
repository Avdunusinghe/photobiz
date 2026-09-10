import { IconName } from '../../shared/ui/icon/icon.component';

export interface NavChildItem {
  label: string;
  route: string;
}

export interface NavItem {
  label: string;
  icon: IconName;
  route?: string;
  children?: NavChildItem[];
}
