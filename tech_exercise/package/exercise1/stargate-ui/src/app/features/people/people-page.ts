import { Component, OnInit, inject, signal } from '@angular/core';
import { NewAstronautDuty } from '@abstractions/models/person';
import { PeopleStore } from '@business/people.store';
import { AddDutyDialog } from './add-duty-dialog/add-duty-dialog';
import { PersonDetail } from './person-detail/person-detail';
import { PersonList } from './person-list/person-list';
import { PersonNameDialog } from './person-name-dialog/person-name-dialog';

type PeopleDialog = 'add-person' | 'add-duty' | 'edit';

/** People list on the left; the selected person's details and duties on the right. */
@Component({
  selector: 'app-people-page',
  imports: [PersonList, PersonDetail, AddDutyDialog, PersonNameDialog],
  templateUrl: './people-page.html',
  styleUrl: './people-page.scss',
})
export class PeoplePage implements OnInit {
  protected readonly store = inject(PeopleStore);
  protected readonly dialog = signal<PeopleDialog | null>(null);

  ngOnInit(): void {
    this.store.loadPeople();
  }

  protected openDialog(dialog: PeopleDialog): void {
    this.store.clearSaveError();
    this.dialog.set(dialog);
  }

  protected closeDialog(): void {
    this.dialog.set(null);
  }

  protected async addPerson(name: string): Promise<void> {
    if (await this.store.addPerson(name)) this.closeDialog();
  }

  protected async addDuty(duty: NewAstronautDuty): Promise<void> {
    if (await this.store.addDuty(duty)) this.closeDialog();
  }

  protected async rename(newName: string): Promise<void> {
    if (await this.store.rename(newName)) this.closeDialog();
  }
}
