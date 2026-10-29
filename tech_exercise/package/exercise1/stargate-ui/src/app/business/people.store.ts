import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, Subscription, firstValueFrom } from 'rxjs';
import { PersonRepository } from '@abstractions/person.repository';
import { AstronautProfile, NewAstronautDuty, PersonSummary } from '@abstractions/models/person';
import { filterPeople } from './person-search';

/** State for the people page: the list of people and the profile of the selected person. */
@Injectable({ providedIn: 'root' })
export class PeopleStore {
  private readonly repository = inject(PersonRepository);
  private profileRequest?: Subscription;

  readonly people = signal<PersonSummary[]>([]);
  readonly peopleLoading = signal(false);
  readonly peopleError = signal<string | null>(null);

  /** Search text for the list. Filtering happens in the browser, over the people already loaded. */
  readonly searchQuery = signal('');
  readonly filteredPeople = computed(() => filterPeople(this.people(), this.searchQuery()));

  readonly selectedName = signal<string | null>(null);
  readonly profile = signal<AstronautProfile | null>(null);
  readonly profileLoading = signal(false);
  readonly profileError = signal<string | null>(null);

  readonly saving = signal(false);
  readonly saveError = signal<string | null>(null);

  loadPeople(): void {
    this.fetchPeople(true);
  }

  select(name: string): void {
    if (name === this.selectedName() && !this.profileError()) return;

    this.selectedName.set(name);
    this.profile.set(null);
    this.fetchProfile(name, true);
  }

  /** Adds a person and selects them. Resolves to true once it is saved. */
  async addPerson(name: string): Promise<boolean> {
    if (!(await this.save(this.repository.addPerson(name)))) return false;
    this.fetchPeople(false);
    this.select(name);
    return true;
  }

  /** Adds a duty to the selected person. Resolves to true once it is saved. */
  async addDuty(duty: NewAstronautDuty): Promise<boolean> {
    const name = this.selectedName();
    if (!name || !(await this.save(this.repository.addDuty(name, duty)))) return false;
    this.refreshInPlace(name);
    return true;
  }

  /** Renames the selected person. Resolves to true once it is saved. */
  async rename(newName: string): Promise<boolean> {
    const name = this.selectedName();
    if (!name || !(await this.save(this.repository.rename(name, newName)))) return false;
    this.refreshInPlace(newName);
    return true;
  }

  clearSaveError(): void {
    this.saveError.set(null);
  }

  private async save(request: Observable<void>): Promise<boolean> {
    this.saving.set(true);
    this.saveError.set(null);

    try {
      await firstValueFrom(request, { defaultValue: undefined });
      return true;
    } catch (e) {
      this.saveError.set((e as Error).message);
      return false;
    } finally {
      this.saving.set(false);
    }
  }

  /**
   * After a change to the selected person, refresh the list (name, current rank and title) and
   * their profile without showing the loading placeholders again.
   */
  private refreshInPlace(name: string): void {
    this.selectedName.set(name);
    this.fetchPeople(false);
    this.fetchProfile(name, false);
  }

  private fetchPeople(showLoading: boolean): void {
    if (showLoading) this.peopleLoading.set(true);
    this.peopleError.set(null);

    this.repository.getAll().subscribe({
      next: (people) => {
        this.people.set([...people].sort((a, b) => a.name.localeCompare(b.name)));
        this.peopleLoading.set(false);
      },
      error: (e: Error) => {
        this.peopleError.set(e.message);
        this.peopleLoading.set(false);
      },
    });
  }

  private fetchProfile(name: string, showLoading: boolean): void {
    // Drop any slower response for a previously clicked person, so it can't overwrite this one.
    this.profileRequest?.unsubscribe();

    if (showLoading) this.profileLoading.set(true);
    this.profileError.set(null);

    this.profileRequest = this.repository.getProfile(name).subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.profileLoading.set(false);
      },
      error: (e: Error) => {
        this.profileError.set(e.message);
        this.profileLoading.set(false);
      },
    });
  }
}
