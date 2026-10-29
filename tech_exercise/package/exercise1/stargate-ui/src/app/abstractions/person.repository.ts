import { Observable } from 'rxjs';
import { AstronautProfile, NewAstronautDuty, PersonSummary } from './models/person';

/**
 * Where people and their astronaut records come from.
 * An abstract class rather than an interface so it can be used as an Angular injection token.
 * Implementations report failures as an `Error` whose message is safe to show to the user.
 */
export abstract class PersonRepository {
  abstract getAll(): Observable<PersonSummary[]>;
  abstract getProfile(name: string): Observable<AstronautProfile>;
  abstract addPerson(name: string): Observable<void>;
  abstract addDuty(name: string, duty: NewAstronautDuty): Observable<void>;
  abstract rename(name: string, newName: string): Observable<void>;
}
