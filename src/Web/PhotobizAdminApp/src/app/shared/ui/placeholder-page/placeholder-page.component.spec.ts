import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { PlaceholderPageComponent } from './placeholder-page.component';

describe('PlaceholderPageComponent', () => {
  function createComponent(data: Record<string, unknown>) {
    TestBed.configureTestingModule({
      imports: [PlaceholderPageComponent],
      providers: [{ provide: ActivatedRoute, useValue: { data: of(data) } }],
    });

    const fixture = TestBed.createComponent(PlaceholderPageComponent);
    fixture.detectChanges();
    return fixture;
  }

  it('renders the title from route data', () => {
    const fixture = createComponent({ title: 'All Bookings' });

    expect(fixture.nativeElement.querySelector('.placeholder__title').textContent).toContain(
      'All Bookings',
    );
  });

  it('falls back to a default title when none is provided', () => {
    const fixture = createComponent({});

    expect(fixture.nativeElement.querySelector('.placeholder__title').textContent).toContain(
      'Coming soon',
    );
  });
});
