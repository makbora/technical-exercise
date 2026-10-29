import { Routes } from '@angular/router';
import { PeoplePage } from '@features/people/people-page';

export const routes: Routes = [
  { path: '', component: PeoplePage, title: 'People · Stargate ACTS' },
  { path: '**', redirectTo: '' },
];
