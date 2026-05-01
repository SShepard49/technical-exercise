import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideToastr } from 'ngx-toastr';

import { App } from './app';
import { API_BASE_URL } from './core/tokens';

describe('App shell', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideAnimationsAsync(),
        provideToastr(),
        { provide: API_BASE_URL, useValue: '' },
      ],
    }).compileComponents();
  });

  it('mounts the shell', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('renders the ACTS brand and primary navigation', async () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.brand-acronym')?.textContent).toContain('ACTS');
    const navLabels = Array.from(compiled.querySelectorAll('.primary-nav a')).map((a) =>
      (a.textContent ?? '').trim(),
    );
    expect(navLabels).toContain('Home');
    expect(navLabels).toContain('Roster');
  });
});
