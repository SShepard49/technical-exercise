import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-home-hero',
  imports: [RouterLink, MatButtonModule, MatIconModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="hero" aria-labelledby="hero-headline">
      <p class="overline">ACTS &middot; Astronaut Career Tracking System</p>
      <h1 id="hero-headline" class="headline">Track every duty, rank, and milestone.</h1>
      <p class="tagline">A live record of every person who has served as an astronaut.</p>
      <div class="cta">
        <a mat-flat-button color="primary" routerLink="/people">
          <mat-icon aria-hidden="true">groups</mat-icon>
          View roster
        </a>
      </div>
    </section>
  `,
  styleUrl: './hero.scss',
})
export class HomeHero {}
