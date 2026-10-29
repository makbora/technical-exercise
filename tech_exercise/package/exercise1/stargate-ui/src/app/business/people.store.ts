import { Injectable, inject, signal } from '@angular/core';
import { Observable, Subscription, firstValueFrom } from 'rxjs';
import { PersonRepository } from '@abstractions/person.repository';
import { AstronautProfile, NewAstronautDuty, PersonSummary } from '@abstractions/models/person';

/** State for the people page: the list of people and the profile of the selected person. */
@Injectable({ providedIn: 'root' })
export class PeopleStore {
  private readonly repository = inject(PersonRepository);
  private profileRequest?: Subscription;

  readonly people = signal<PersonSummary[]>([]);
  readonly peopleLoading = signal(false);
  readonly peopleError = signal<string | null>(null);

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

  /** Adds a duty to the selected person. Resolves to true once it is saved. */
  addDuty(duty: NewAstronautDuty): Promise<boolean> {
    const name = this.selectedName();
    if (!name) return Promise.resolve(false);
    return this.save(this.repository.addDuty(name, duty), name);
  }

  /** Renames the selected person. Resolves to true once it is saved. */
  rename(newName: string): Promise<boolean> {
    const name = this.selectedName();
    if (!name) return Promise.resolve(false);
    return this.save(this.repository.rename(name, newName), newName);
  }

  clearSaveError(): void {
    this.saveError.set(null);
  }

  private async save(request: Observable<void>, nameAfterSave: string): Promise<boolean> {
    this.saving.set(true);
    this.saveError.set(null);

    try {
      await firstValueFrom(request, { defaultValue: undefined });
    } catch (e) {
      this.saveError.set((e as Error).message);
      return false;
    } finally {
      this.saving.set(false);
    }

    // The save changes the list (name, current rank and title) and the profile, so refresh both
    // in place rather than showing the loading placeholders again.
    this.selectedName.set(nameAfterSave);
    this.fetchPeople(false);
    this.fetchProfile(nameAfterSave, false);
    return true;
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
