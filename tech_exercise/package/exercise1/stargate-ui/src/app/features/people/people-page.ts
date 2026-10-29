import { Component, OnInit, inject, signal } from '@angular/core';
import { NewAstronautDuty } from '@abstractions/models/person';
import { PeopleStore } from '@business/people.store';
import { AddDutyDialog } from './add-duty-dialog/add-duty-dialog';
import { EditPersonDialog } from './edit-person-dialog/edit-person-dialog';
import { PersonDetail } from './person-detail/person-detail';
import { PersonList } from './person-list/person-list';

type PeopleDialog = 'add-duty' | 'edit';

/** People list on the left; the selected person's details and duties on the right. */
@Component({
  selector: 'app-people-page',
  imports: [PersonList, PersonDetail, AddDutyDialog, EditPersonDialog],
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

  protected async addDuty(duty: NewAstronautDuty): Promise<void> {
    if (await this.store.addDuty(duty)) this.closeDialog();
  }

  protected async rename(newName: string): Promise<void> {
    if (await this.store.rename(newName)) this.closeDialog();
  }
}
