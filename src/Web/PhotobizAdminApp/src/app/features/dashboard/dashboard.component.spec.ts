import { TestBed } from '@angular/core/testing';
import { DashboardComponent } from './dashboard.component';

describe('DashboardComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [DashboardComponent],
    });
  });

  it('creates', () => {
    const fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance).toBeTruthy();
  });

  it('renders one stat card per stat', () => {
    const fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();

    const cards = fixture.nativeElement.querySelectorAll('.stat-card');
    expect(cards.length).toBe(fixture.componentInstance['stats'].length);
  });
});
