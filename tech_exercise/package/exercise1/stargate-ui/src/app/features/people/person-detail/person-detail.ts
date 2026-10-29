import { Component, computed, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AstronautProfile, RETIRED_DUTY_TITLE, astronautStatus } from '@abstractions/models/person';
import { initials } from '../initials';

@Component({
  selector: 'app-person-detail',
  imports: [DatePipe],
  templateUrl: './person-detail.html',
  styleUrl: './person-detail.scss',
})
export class PersonDetail {
  readonly profile = input.required<AstronautProfile>();
  readonly addDuty = output<void>();
  readonly edit = output<void>();

  protected readonly person = computed(() => this.profile().person);
  protected readonly status = computed(() => astronautStatus(this.person()));
  protected readonly initials = computed(() => initials(this.person().name));
  protected readonly retiredTitle = RETIRED_DUTY_TITLE;
}
