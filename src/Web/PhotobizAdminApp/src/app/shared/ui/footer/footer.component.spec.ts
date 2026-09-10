import { TestBed } from '@angular/core/testing';
import { FooterComponent } from './footer.component';

describe('FooterComponent', () => {
  it('renders the current year in the copyright text', () => {
    TestBed.configureTestingModule({ imports: [FooterComponent] });

    const fixture = TestBed.createComponent(FooterComponent);
    fixture.detectChanges();

    const text = fixture.nativeElement.querySelector('.footer__text').textContent;
    expect(text).toContain(String(new Date().getFullYear()));
    expect(text).toContain('Photobiz');
  });
});
