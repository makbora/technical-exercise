import { TestBed } from '@angular/core/testing';
import { Observable, Subject } from 'rxjs';
import { PersonRepository } from '@abstractions/person.repository';
import { AstronautProfile, NewAstronautDuty, PersonSummary } from '@abstractions/models/person';
import { PeopleStore } from './people.store';

/** Hands back Subjects so each test decides when, and in what order, responses arrive. */
class FakePersonRepository extends PersonRepository {
  readonly people$ = new Subject<PersonSummary[]>();
  readonly profiles = new Map<string, Subject<AstronautProfile>>();
  readonly save$ = new Subject<void>();
  readonly addedDuties: [string, NewAstronautDuty][] = [];
  readonly renames: [string, string][] = [];
  getAllCalls = 0;

  getAll(): Observable<PersonSummary[]> {
    this.getAllCalls++;
    return this.people$;
  }

  getProfile(name: string): Observable<AstronautProfile> {
    const profile$ = new Subject<AstronautProfile>();
    this.profiles.set(name, profile$);
    return profile$;
  }

  addDuty(name: string, duty: NewAstronautDuty): Observable<void> {
    this.addedDuties.push([name, duty]);
    return this.save$;
  }

  rename(name: string, newName: string): Observable<void> {
    this.renames.push([name, newName]);
    return this.save$;
  }

  succeedSave(): void {
    this.save$.next();
    this.save$.complete();
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

  describe('saving', () => {
    const duty: NewAstronautDuty = { rank: 'LTC', title: 'Commander', startDate: '2025-01-01' };

    beforeEach(() => {
      store.select('Jane Doe');
      repository.profiles.get('Jane Doe')!.next({ person: person('Jane Doe'), duties: [] });
    });

    it('adds a duty to the selected person, then refreshes without the loading placeholders', async () => {
      const saved = store.addDuty(duty);
      expect(store.saving()).toBe(true);

      repository.succeedSave();

      expect(await saved).toBe(true);
      expect(repository.addedDuties).toEqual([['Jane Doe', duty]]);
      expect(store.saving()).toBe(false);
      expect(repository.getAllCalls).toBe(1);
      expect(store.peopleLoading()).toBe(false);
      expect(store.profileLoading()).toBe(false);
      expect(store.profile()?.person.name).toBe('Jane Doe');
    });

    it('reports a failed save and keeps the current profile', async () => {
      const saved = store.addDuty(duty);

      repository.save$.error(new Error('Person with this name already has a duty scheduled'));

      expect(await saved).toBe(false);
      expect(store.saveError()).toBe('Person with this name already has a duty scheduled');
      expect(store.saving()).toBe(false);
      expect(repository.getAllCalls).toBe(0);
      expect(store.profile()?.person.name).toBe('Jane Doe');
    });

    it('renames the selected person and keeps them selected under the new name', async () => {
      const saved = store.rename('Jane Smith');
      repository.succeedSave();

      expect(await saved).toBe(true);
      expect(repository.renames).toEqual([['Jane Doe', 'Jane Smith']]);
      expect(store.selectedName()).toBe('Jane Smith');
      expect(repository.profiles.has('Jane Smith')).toBe(true);
    });

    it('clears a previous save error', async () => {
      const saved = store.rename('Sam Carter');
      repository.save$.error(new Error('Person with this name already exists'));
      await saved;

      store.clearSaveError();

      expect(store.saveError()).toBeNull();
    });
  });
});
