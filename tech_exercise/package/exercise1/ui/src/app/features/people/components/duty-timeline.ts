import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

import { AstronautDuty } from '../../../core/models';
import { DateOnlyPipe } from '../../../shared/pipes/date-only.pipe';

interface TimelineEntry {
  readonly duty: AstronautDuty;
  readonly isCurrent: boolean;
  readonly isRetiredEntry: boolean;
}

@Component({
  selector: 'app-duty-timeline',
  imports: [MatIconModule, DateOnlyPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ol class="timeline" aria-label="Astronaut duty timeline">
      @for (entry of entries(); track entry.duty.id) {
        <li class="entry" [class.is-current]="entry.isCurrent">
          <div class="marker" aria-hidden="true">
            <mat-icon>{{
              entry.isRetiredEntry
                ? 'flag'
                : entry.isCurrent
                  ? 'rocket_launch'
                  : 'history'
            }}</mat-icon>
          </div>
          <div class="content">
            <div class="content-head">
              <h3 class="title">{{ entry.duty.dutyTitle }}</h3>
              @if (entry.isCurrent) {
                <span class="badge is-current" aria-label="Current duty">Current</span>
              }
            </div>
            <p class="rank">Rank: {{ entry.duty.rank }}</p>
            <p class="dates">
              <span class="date-range">
                <span class="material-icons" aria-hidden="true">calendar_today</span>
                {{ entry.duty.dutyStartDate | dateOnly }}
                &nbsp;&ndash;&nbsp;
                {{ entry.duty.dutyEndDate ? (entry.duty.dutyEndDate | dateOnly) : 'Present' }}
              </span>
            </p>
          </div>
        </li>
      }
    </ol>
  `,
  styleUrl: './duty-timeline.scss',
})
export class DutyTimeline {
  readonly duties = input.required<AstronautDuty[]>();

  readonly entries = computed<TimelineEntry[]>(() =>
    this.duties().map((duty) => ({
      duty,
      isCurrent: duty.dutyEndDate === null,
      isRetiredEntry: duty.dutyTitle?.toUpperCase() === 'RETIRED',
    })),
  );
}
