import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';

import { DutyTimeline } from './duty-timeline';
import { AstronautDuty } from '../../../core/models';

const duties: AstronautDuty[] = [
  {
    id: 3,
    personId: 1,
    rank: 'COL',
    dutyTitle: 'Commander',
    dutyStartDate: '2024-01-01',
    dutyEndDate: null,
  },
  {
    id: 2,
    personId: 1,
    rank: 'MAJ',
    dutyTitle: 'Pilot',
    dutyStartDate: '2022-06-15',
    dutyEndDate: '2023-12-31',
  },
];

describe('DutyTimeline', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [DutyTimeline] });
  });

  it('renders one timeline entry per duty', () => {
    const fixture = TestBed.createComponent(DutyTimeline);
    fixture.componentRef.setInput('duties', duties);
    fixture.detectChanges();

    const items = fixture.nativeElement.querySelectorAll('.entry');
    expect(items.length).toBe(2);
  });

  it('marks the entry with no end date as Current', () => {
    const fixture = TestBed.createComponent(DutyTimeline);
    fixture.componentRef.setInput('duties', duties);
    fixture.detectChanges();

    const currentBadges = fixture.nativeElement.querySelectorAll('.badge.is-current');
    expect(currentBadges.length).toBe(1);

    const firstEntry = fixture.nativeElement.querySelector('.entry');
    expect(firstEntry?.classList.contains('is-current')).toBe(true);
    expect(firstEntry?.textContent).toContain('Commander');
    expect(firstEntry?.textContent).toContain('Present');
  });

  it('renders an end date for past duties', () => {
    const fixture = TestBed.createComponent(DutyTimeline);
    fixture.componentRef.setInput('duties', duties);
    fixture.detectChanges();

    const entries = fixture.nativeElement.querySelectorAll('.entry');
    const past = entries[1]!;
    expect(past.textContent).toContain('Pilot');
    expect(past.textContent).toMatch(/Dec 31, 2023/);
    expect(past.classList.contains('is-current')).toBe(false);
  });

  it('renders an empty list when no duties are provided', () => {
    const fixture = TestBed.createComponent(DutyTimeline);
    fixture.componentRef.setInput('duties', []);
    fixture.detectChanges();

    const items = fixture.nativeElement.querySelectorAll('.entry');
    expect(items.length).toBe(0);
  });
});
