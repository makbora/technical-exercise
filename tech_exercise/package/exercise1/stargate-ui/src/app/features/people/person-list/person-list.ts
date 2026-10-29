import { Component, input, output } from '@angular/core';
import { PersonSummary, astronautStatus } from '@abstractions/models/person';
import { initials } from '../initials';

@Component({
  selector: 'app-person-list',
  templateUrl: './person-list.html',
  styleUrl: './person-list.scss',
})
export class PersonList {
  readonly people = input.required<PersonSummary[]>();
  readonly selectedName = input<string | null>(null);
  readonly selected = output<string>();

  protected readonly initials = initials;
  protected readonly status = astronautStatus;
}
