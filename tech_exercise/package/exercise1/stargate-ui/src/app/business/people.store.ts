import { Injectable, inject, signal } from '@angular/core';
import { Subscription } from 'rxjs';
import { PersonRepository } from '@abstractions/person.repository';
import { AstronautProfile, PersonSummary } from '@abstractions/models/person';

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

  loadPeople(): void {
    this.peopleLoading.set(true);
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

  select(name: string): void {
    if (name === this.selectedName() && !this.profileError()) return;

    // Drop any slower response for a previously clicked person, so it can't overwrite this one.
    this.profileRequest?.unsubscribe();

    this.selectedName.set(name);
    this.profile.set(null);
    this.profileLoading.set(true);
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
