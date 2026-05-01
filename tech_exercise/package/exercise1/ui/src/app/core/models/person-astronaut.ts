export interface PersonAstronaut {
  personId: number;
  name: string;
  currentRank: string;
  currentDutyTitle: string;
  careerStartDate: string | null;
  careerEndDate: string | null;
  isRetired: boolean;
}

export const isAstronaut = (person: PersonAstronaut): boolean =>
  !!person.currentRank && person.currentRank.trim().length > 0;
