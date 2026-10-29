import { Component, OnInit, inject } from '@angular/core';
import { PeopleStore } from '@business/people.store';
import { PersonDetail } from './person-detail/person-detail';
import { PersonList } from './person-list/person-list';

/** People list on the left; the selected person's details and duties on the right. */
@Component({
  selector: 'app-people-page',
  imports: [PersonList, PersonDetail],
  templateUrl: './people-page.html',
  styleUrl: './people-page.scss',
})
export class PeoplePage implements OnInit {
  protected readonly store = inject(PeopleStore);

  ngOnInit(): void {
    this.store.loadPeople();
  }
}
