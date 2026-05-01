import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';

import { PeopleService } from '../../core/services/people.service';
import { PersonAstronaut } from '../../core/models';
import { HomeHero } from './components/hero';
import { StatCards } from './components/stat-cards';

@Component({
  selector: 'app-home-page',
  imports: [HomeHero, StatCards],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="home">
      <app-home-hero />
      <app-stat-cards
        [people]="people()"
        [loading]="loading()"
        [errored]="errored()"
      />
    </section>
  `,
  styleUrl: './home.page.scss',
})
export class HomePage {
  private readonly peopleService = inject(PeopleService);

  protected readonly people = signal<PersonAstronaut[]>([]);
  protected readonly loading = signal(true);
  protected readonly errored = signal(false);

  constructor() {
    this.peopleService.getAll().subscribe({
      next: (people) => {
        this.people.set(people);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.errored.set(true);
      },
    });
  }
}
