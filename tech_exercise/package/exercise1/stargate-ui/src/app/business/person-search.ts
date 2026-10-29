import { PersonSummary } from '@abstractions/models/person';

/**
 * Narrows the people already loaded in the browser; never calls the API.
 * Every word in the query has to appear in the person's name, current rank or current duty
 * title, ignoring case and accents ("o'neill commander" finds Jack O'Neill if he commands).
 */
export function filterPeople(people: PersonSummary[], query: string): PersonSummary[] {
  const terms = normalize(query).split(/\s+/).filter(Boolean);
  if (terms.length === 0) return people;

  return people.filter((person) => {
    const text = normalize(
      [person.name, person.currentRank, person.currentDutyTitle].filter(Boolean).join(' '),
    );
    return terms.every((term) => text.includes(term));
  });
}

function normalize(value: string): string {
  return value
    .normalize('NFD')
    .replace(/\p{Diacritic}/gu, '')
    .toLocaleLowerCase();
}
