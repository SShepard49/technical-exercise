import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { PersonAstronaut, isAstronaut } from '../../../core/models';

interface Stat {
  readonly id: string;
  readonly label: string;
  readonly icon: string;
  readonly value: number;
  readonly description: string;
}

@Component({
  selector: 'app-stat-cards',
  imports: [MatCardModule, MatIconModule, MatProgressSpinnerModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="stats" aria-label="Astronaut roster summary">
      @for (stat of stats(); track stat.id) {
        <mat-card appearance="outlined" class="stat-card">
          <mat-card-content>
            <div class="stat-card-header">
              <mat-icon aria-hidden="true" class="stat-icon">{{ stat.icon }}</mat-icon>
              <span class="stat-label">{{ stat.label }}</span>
            </div>
            <div class="stat-value" [attr.aria-busy]="loading()">
              @if (loading()) {
                <mat-progress-spinner mode="indeterminate" diameter="28"></mat-progress-spinner>
              } @else if (errored()) {
                <span class="stat-fallback" aria-label="Unavailable">&mdash;</span>
              } @else {
                {{ stat.value }}
              }
            </div>
            <p class="stat-description">{{ stat.description }}</p>
          </mat-card-content>
        </mat-card>
      }
    </section>
  `,
  styleUrl: './stat-cards.scss',
})
export class StatCards {
  readonly people = input<PersonAstronaut[]>([]);
  readonly loading = input<boolean>(false);
  readonly errored = input<boolean>(false);

  readonly stats = computed<Stat[]>(() => {
    const all = this.people();
    const astronauts = all.filter(isAstronaut);
    const retired = astronauts.filter((p) => p.isRetired).length;
    const active = astronauts.length - retired;
    return [
      {
        id: 'total',
        label: 'Total astronauts',
        icon: 'rocket',
        value: astronauts.length,
        description: 'People with a recorded astronaut rank.',
      },
      {
        id: 'active',
        label: 'Active',
        icon: 'rocket_launch',
        value: active,
        description: 'Currently serving on a duty.',
      },
      {
        id: 'retired',
        label: 'Retired',
        icon: 'flag',
        value: retired,
        description: 'Career complete, no further duties.',
      },
    ];
  });
}
