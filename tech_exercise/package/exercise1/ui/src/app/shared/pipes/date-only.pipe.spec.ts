import { describe, it, expect } from 'vitest';

import { DateOnlyPipe } from './date-only.pipe';

describe('DateOnlyPipe', () => {
  const pipe = new DateOnlyPipe();

  it('formats a YYYY-MM-DD value without timezone drift', () => {
    expect(pipe.transform('2022-01-01')).toBe('Jan 1, 2022');
    expect(pipe.transform('2024-12-31')).toBe('Dec 31, 2024');
  });

  it('returns the fallback for null and empty values', () => {
    expect(pipe.transform(null)).toBe('\u2014');
    expect(pipe.transform(undefined)).toBe('\u2014');
    expect(pipe.transform('')).toBe('\u2014');
    expect(pipe.transform(null, '-')).toBe('-');
  });

  it('returns the original input for unrecognized formats', () => {
    expect(pipe.transform('not a date')).toBe('not a date');
    expect(pipe.transform('2024-13-45')).toBe('2024-13-45');
  });
});
