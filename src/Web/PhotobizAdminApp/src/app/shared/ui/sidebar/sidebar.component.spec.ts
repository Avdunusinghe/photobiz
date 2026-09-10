import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { NavItem } from '../../../core/models/nav-item.model';
import { SidebarComponent } from './sidebar.component';

describe('SidebarComponent', () => {
  const items: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
    {
      label: 'Bookings',
      icon: 'calendar',
      children: [
        { label: 'All bookings', route: '/bookings' },
        { label: 'Calendar', route: '/bookings/calendar' },
      ],
    },
  ];

  function createComponent(currentUrl: string) {
    TestBed.configureTestingModule({
      imports: [SidebarComponent],
      providers: [provideRouter([])],
    });

    const fixture = TestBed.createComponent(SidebarComponent);
    fixture.componentRef.setInput('items', items);
    fixture.componentRef.setInput('currentUrl', currentUrl);
    fixture.detectChanges();
    return fixture;
  }

  it('creates and renders one top-level entry per item', () => {
    const fixture = createComponent('/dashboard');

    const topLevel = fixture.nativeElement.querySelectorAll('nav.sidebar__nav > *');
    expect(topLevel.length).toBe(items.length);
  });

  it('does not render children until the group is expanded', () => {
    const fixture = createComponent('/dashboard');
    expect(fixture.nativeElement.querySelectorAll('.nav-item--child').length).toBe(0);

    fixture.componentInstance['toggleGroup']('Bookings');
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.nav-item--child').length).toBe(2);

    fixture.componentInstance['toggleGroup']('Bookings');
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelectorAll('.nav-item--child').length).toBe(0);
  });

  it('marks a single-level item active only when its route matches the current url', () => {
    const fixture = createComponent('/dashboard');

    expect(fixture.componentInstance['isActive']('/dashboard')).toBe(true);
    expect(fixture.componentInstance['isActive']('/bookings')).toBe(false);
  });

  it('marks a group active when a child route matches the current url', () => {
    const fixture = createComponent('/bookings/calendar');

    expect(fixture.componentInstance['isGroupActive'](items[1])).toBe(true);
    expect(fixture.componentInstance['isGroupActive'](items[0])).toBe(false);
  });

  it('collapses to icon-only mode and hides labels', () => {
    const fixture = createComponent('/dashboard');
    expect(fixture.nativeElement.querySelector('.nav-item__label')).toBeTruthy();

    fixture.componentInstance['toggleCollapsed']();
    fixture.detectChanges();

    expect(
      fixture.nativeElement.querySelector('.sidebar').classList.contains('sidebar--collapsed'),
    ).toBe(true);
    expect(fixture.nativeElement.querySelector('.nav-item__label')).toBeFalsy();
  });
});
