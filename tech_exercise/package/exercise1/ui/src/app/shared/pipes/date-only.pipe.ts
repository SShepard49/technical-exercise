import { Pipe, PipeTransform } from '@angular/core';

/**
 * Formats an API DateOnly string ("YYYY-MM-DD") for display without
 * touching timezones. Returns a fallback when the value is null/empty.
 *
 * We intentionally avoid `new Date('YYYY-MM-DD')`, which is parsed as
 * UTC midnight and can render the previous day in negative-offset
 * locales.
 */
@Pipe({ name: 'dateOnly' })
export class DateOnlyPipe implements PipeTransform {
  private static readonly months = [
    'Jan',
    'Feb',
    'Mar',
    'Apr',
    'May',
    'Jun',
    'Jul',
    'Aug',
    'Sep',
    'Oct',
    'Nov',
    'Dec',
  ];

  transform(value: string | null | undefined, fallback = '\u2014'): string {
    if (!value) {
      return fallback;
    }
    const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(value);
    if (!match) {
      return value;
    }
    const [, year, monthStr, dayStr] = match;
    const monthIndex = Number.parseInt(monthStr, 10) - 1;
    const day = Number.parseInt(dayStr, 10);
    if (monthIndex < 0 || monthIndex > 11 || Number.isNaN(day)) {
      return value;
    }
    return `${DateOnlyPipe.months[monthIndex]} ${day}, ${year}`;
  }
}
