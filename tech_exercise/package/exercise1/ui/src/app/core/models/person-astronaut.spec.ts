import { describe, it, expect } from 'vitest';

import { PersonAstronaut, isAstronaut } from './person-astronaut';

const make = (overrides: Partial<PersonAstronaut> = {}): PersonAstronaut => ({
  personId: 1,
  name: 'Test Person',
  currentRank: '',
  currentDutyTitle: '',
  careerStartDate: null,
  careerEndDate: null,
  isRetired: false,
  ...overrides,
});

describe('isAstronaut', () => {
  it('returns true for a person with a non-empty rank', () => {
    expect(isAstronaut(make({ currentRank: 'CAPT' }))).toBe(true);
  });

  it('returns false when rank is missing or whitespace', () => {
    expect(isAstronaut(make({ currentRank: '' }))).toBe(false);
    expect(isAstronaut(make({ currentRank: '   ' }))).toBe(false);
  });
});
