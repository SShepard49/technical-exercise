import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import {
  AstronautDutiesResult,
  AstronautDutyService,
} from '../../core/services/astronaut-duty.service';
import { isAstronaut } from '../../core/models';
import { DateOnlyPipe } from '../../shared/pipes/date-only.pipe';
import { StatusChip } from '../../shared/ui/status-chip/status-chip';
import { EmptyState } from '../../shared/ui/empty-state/empty-state';
import { DutyTimeline } from './components/duty-timeline';

@Component({
  selector: 'app-person-detail-page',
  imports: [
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    DateOnlyPipe,
    StatusChip,
    EmptyState,
    DutyTimeline,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './person-detail.page.html',
  styleUrl: './person-detail.page.scss',
})
export class PersonDetailPage {
  private readonly astronautDuties = inject(AstronautDutyService);

  /** Injected by `withComponentInputBinding()`. Decoded route param value. */
  readonly name = input.required<string>();

  protected readonly loading = signal(true);
  protected readonly errored = signal(false);
  protected readonly result = signal<AstronautDutiesResult | null>(null);

  protected readonly person = computed(() => this.result()?.person ?? null);
  protected readonly duties = computed(() => this.result()?.duties ?? []);
  protected readonly hasAstronautRecord = computed(() => {
    const person = this.person();
    return person !== null && isAstronaut(person);
  });

  constructor() {
    effect(() => {
      const name = this.name();
      this.load(name);
    });
  }

  private load(name: string): void {
    this.loading.set(true);
    this.errored.set(false);
    this.result.set(null);
    this.astronautDuties.getByName(name).subscribe({
      next: (data) => {
        this.result.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.errored.set(true);
      },
    });
  }
}
