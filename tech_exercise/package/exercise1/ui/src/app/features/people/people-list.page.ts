import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  ViewChild,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTableDataSource } from '@angular/material/table';
import { MatChipSelectionChange, MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';

import { PeopleService } from '../../core/services/people.service';
import { PersonAstronaut, isAstronaut } from '../../core/models';
import { DateOnlyPipe } from '../../shared/pipes/date-only.pipe';
import { StatusChip } from '../../shared/ui/status-chip/status-chip';
import { EmptyState } from '../../shared/ui/empty-state/empty-state';

type StatusFilter = 'active' | 'retired';
const DEFAULT_FILTERS: ReadonlySet<StatusFilter> = new Set<StatusFilter>(['active', 'retired']);
const FILTER_STORAGE_KEY = 'peopleList.statusFilters';

@Component({
  selector: 'app-people-list-page',
  imports: [
    RouterLink,
    FormsModule,
    MatCardModule,
    MatSortModule,
    MatTableModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    DateOnlyPipe,
    StatusChip,
    EmptyState,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './people-list.page.html',
  styleUrl: './people-list.page.scss',
})
export class PeopleListPage {
  private readonly peopleService = inject(PeopleService);

  @ViewChild(MatSort)
  set matSort(sort: MatSort | undefined) {
    if (sort) {
      this.dataSource.sort = sort;
    }
  }

  protected readonly displayedColumns = [
    'name',
    'status',
    'currentRank',
    'currentDuty',
    'careerStart',
  ] as const;

  protected readonly loading = signal(true);
  protected readonly errored = signal(false);
  protected readonly searchTerm = signal('');
  protected readonly activeFilters = signal<Set<StatusFilter>>(this.readPersistedFilters());
  protected readonly dataSource = new MatTableDataSource<PersonAstronaut>([]);

  private readonly astronauts = signal<PersonAstronaut[]>([]);

  protected readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const filters = this.activeFilters();
    return this.astronauts().filter((p) => {
      const status: StatusFilter = p.isRetired ? 'retired' : 'active';
      if (!filters.has(status)) {
        return false;
      }
      return term.length === 0 || p.name.toLowerCase().includes(term);
    });
  });

  protected readonly hasResults = computed(() => this.filtered().length > 0);
  protected readonly hasAnyAstronauts = computed(() => this.astronauts().length > 0);

  constructor() {
    this.dataSource.sortingDataAccessor = (person, property) => {
      switch (property) {
        case 'status':
          return person.isRetired ? 'retired' : 'active';
        case 'currentDuty':
          return person.currentDutyTitle ?? '';
        case 'careerStart':
          return person.careerStartDate ? new Date(person.careerStartDate).getTime() : 0;
        default: {
          const value = person[property as keyof PersonAstronaut];
          return typeof value === 'string' || typeof value === 'number' ? value : '';
        }
      }
    };
    effect(() => {
      this.dataSource.data = this.filtered();
    });
    this.load();
  }

  protected onSearch(value: string): void {
    this.searchTerm.set(value);
  }

  protected isFilterSelected(filter: StatusFilter): boolean {
    return this.activeFilters().has(filter);
  }

  onFilterSelectionChange(filter: StatusFilter, event: MatChipSelectionChange): void {
    if (!event.isUserInput) return;
    this.activeFilters.update((set) => {
      const next = new Set(set);
      if (event.selected) {
        next.add(filter);
      } else {
        next.delete(filter);
      }
      this.persistFilters(next);
      return next;
    });
  }

  protected retry(): void {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.errored.set(false);
    this.peopleService.getAll().subscribe({
      next: (people) => {
        this.astronauts.set(people.filter(isAstronaut));
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.errored.set(true);
      },
    });
  }

  private readPersistedFilters(): Set<StatusFilter> {
    if (typeof sessionStorage === 'undefined') {
      return new Set(DEFAULT_FILTERS);
    }

    const raw = sessionStorage.getItem(FILTER_STORAGE_KEY);
    if (!raw) {
      return new Set(DEFAULT_FILTERS);
    }

    try {
      const parsed = JSON.parse(raw);
      if (!Array.isArray(parsed)) {
        return new Set(DEFAULT_FILTERS);
      }

      const hydrated = parsed.filter((value): value is StatusFilter => this.isStatusFilter(value));
      if (hydrated.length === 0) {
        return new Set(DEFAULT_FILTERS);
      }

      return new Set(hydrated);
    } catch {
      return new Set(DEFAULT_FILTERS);
    }
  }

  private persistFilters(filters: Set<StatusFilter>): void {
    if (typeof sessionStorage === 'undefined') {
      return;
    }

    const values = [...filters].filter((value): value is StatusFilter => this.isStatusFilter(value));
    if (values.length === 0) {
      sessionStorage.removeItem(FILTER_STORAGE_KEY);
      return;
    }

    sessionStorage.setItem(FILTER_STORAGE_KEY, JSON.stringify(values));
  }

  private isStatusFilter(value: unknown): value is StatusFilter {
    return value === 'active' || value === 'retired';
  }
}
