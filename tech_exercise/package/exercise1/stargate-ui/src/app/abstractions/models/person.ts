/** A person as shown in the people list, with their current astronaut details if they have any. */
export interface PersonSummary {
  id: number;
  name: string;
  currentRank: string | null;
  currentDutyTitle: string | null;
  careerStartDate: string | null;
  careerEndDate: string | null;
}

export interface AstronautDuty {
  id: number;
  rank: string;
  title: string;
  startDate: string;
  /** Null for the person's current duty. */
  endDate: string | null;
}

/** Everything known about one person: their details and their duties, newest first. */
export interface AstronautProfile {
  person: PersonSummary;
  duties: AstronautDuty[];
}

export const RETIRED_DUTY_TITLE = 'RETIRED';

export type AstronautStatus = 'active' | 'retired' | 'none';

export function astronautStatus(person: PersonSummary): AstronautStatus {
  if (!person.careerStartDate) return 'none';
  return person.currentDutyTitle === RETIRED_DUTY_TITLE ? 'retired' : 'active';
}
