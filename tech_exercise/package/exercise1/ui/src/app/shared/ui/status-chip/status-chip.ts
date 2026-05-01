import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

import { PersonAstronaut } from '../../../core/models';

export type AstronautStatus = 'active' | 'retired';

interface StatusDescriptor {
  readonly label: string;
  readonly icon: string;
  readonly cssClass: string;
}

@Component({
  selector: 'app-status-chip',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span
      class="status-chip"
      [class]="descriptor().cssClass"
      [attr.aria-label]="'Astronaut status: ' + descriptor().label"
    >
      <span class="material-icons" aria-hidden="true">{{ descriptor().icon }}</span>
      <span class="label">{{ descriptor().label }}</span>
    </span>
  `,
  styleUrl: './status-chip.scss',
})
export class StatusChip {
  readonly person = input.required<PersonAstronaut>();

  readonly descriptor = computed<StatusDescriptor>(() =>
    this.person().isRetired
      ? { label: 'Retired', icon: 'flag', cssClass: 'is-retired' }
      : { label: 'Active', icon: 'rocket_launch', cssClass: 'is-active' },
  );
}
