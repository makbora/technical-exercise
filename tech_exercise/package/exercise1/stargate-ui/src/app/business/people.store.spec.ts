import { TestBed } from '@angular/core/testing';
import { Observable, Subject } from 'rxjs';
import { PersonRepository } from '@abstractions/person.repository';
import { AstronautProfile, PersonSummary } from '@abstractions/models/person';
import { PeopleStore } from './people.store';

/** Hands back Subjects so each test decides when, and in what order, responses arrive. */
class FakePersonRepository extends PersonRepository {
  readonly people$ = new Subject<PersonSummary[]>();
  readonly profiles = new Map<string, Subject<AstronautProfile>>();

  getAll(): Observable<PersonSummary[]> {
    return this.people$;
  }

  getProfile(name: string): Observable<AstronautProfile> {
    const profile$ = new Subject<AstronautProfile>();
    this.profiles.set(name, profile$);
    return profile$;
  }
}

function person(name: string): PersonSummary {
  return {
    id: name.length,
    name,
    currentRank: null,
    currentDutyTitle: null,
    careerStartDate: null,
    careerEndDate: null,
  };
}

describe('PeopleStore', () => {
  let repository: FakePersonRepository;
  let store: PeopleStore;

  beforeEach(() => {
    repository = new FakePersonRepository();
    TestBed.configureTestingModule({
      providers: [{ provide: PersonRepository, useValue: repository }],
    });
    store = TestBed.inject(PeopleStore);
  });

  it('loads people sorted by name', () => {
    store.loadPeople();
    expect(store.peopleLoading()).toBe(true);

    repository.people$.next([person('Sam Carter'), person('Jane Doe')]);

    expect(store.peopleLoading()).toBe(false);
    expect(store.people().map((p) => p.name)).toEqual(['Jane Doe', 'Sam Carter']);
  });

  it('reports a failure to load people', () => {
    store.loadPeople();

    repository.people$.error(new Error('Could not reach the Stargate API. Is it running?'));

    expect(store.peopleLoading()).toBe(false);
    expect(store.peopleError()).toBe('Could not reach the Stargate API. Is it running?');
  });

  it('loads the profile of the selected person', () => {
    store.select('Jane Doe');
    expect(store.selectedName()).toBe('Jane Doe');
    expect(store.profileLoading()).toBe(true);

    const profile = { person: person('Jane Doe'), duties: [] };
    repository.profiles.get('Jane Doe')!.next(profile);

    expect(store.profileLoading()).toBe(false);
    expect(store.profile()).toEqual(profile);
  });

  it('ignores a slow response for a previously selected person', () => {
    store.select('Jane Doe');
    store.select('Sam Carter');

    repository.profiles.get('Sam Carter')!.next({ person: person('Sam Carter'), duties: [] });
    repository.profiles.get('Jane Doe')!.next({ person: person('Jane Doe'), duties: [] });

    expect(store.selectedName()).toBe('Sam Carter');
    expect(store.profile()?.person.name).toBe('Sam Carter');
  });

  it('reports a failure to load a profile and allows retrying the same person', () => {
    store.select('Jane Doe');
    repository.profiles.get('Jane Doe')!.error(new Error('Person with this name does not exist'));

    expect(store.profileError()).toBe('Person with this name does not exist');

    store.select('Jane Doe');

    expect(store.profileError()).toBeNull();
    expect(store.profileLoading()).toBe(true);
  });
});
