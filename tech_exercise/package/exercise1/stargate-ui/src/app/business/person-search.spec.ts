import { PersonSummary } from '@abstractions/models/person';
import { filterPeople } from './person-search';

function person(
  name: string,
  currentRank: string | null = null,
  currentDutyTitle: string | null = null,
): PersonSummary {
  return {
    id: name.length,
    name,
    currentRank,
    currentDutyTitle,
    careerStartDate: null,
    careerEndDate: null,
  };
}

const people = [
  person('Jane Doe', 'MAJ', 'Commander'),
  person('John Doe', 'MAJ', 'RETIRED'),
  person('Sam Carter', 'COL', 'Commander'),
  person('Daniel Jackson'),
  person('Teal’c', 'CIV', 'Specialist'),
  person('Zoë Washburne', 'CPT', 'Pilot'),
];

const names = (query: string) => filterPeople(people, query).map((p) => p.name);

describe('filterPeople', () => {
  it('returns everyone for a blank query', () => {
    expect(filterPeople(people, '   ')).toBe(people);
  });

  it('matches part of a name, ignoring case', () => {
    expect(names('DOE')).toEqual(['Jane Doe', 'John Doe']);
    expect(names('ack')).toEqual(['Daniel Jackson']);
  });

  it('matches the current rank and duty title', () => {
    expect(names('commander')).toEqual(['Jane Doe', 'Sam Carter']);
    expect(names('maj')).toEqual(['Jane Doe', 'John Doe']);
  });

  it('requires every word to match, in any order', () => {
    expect(names('commander doe')).toEqual(['Jane Doe']);
    expect(names('doe   retired')).toEqual(['John Doe']);
    expect(names('carter pilot')).toEqual([]);
  });

  it('ignores accents', () => {
    expect(names('zoe')).toEqual(['Zoë Washburne']);
  });

  it('treats a missing rank or title as empty, not as the text "null"', () => {
    expect(names('null')).toEqual([]);
  });
});
