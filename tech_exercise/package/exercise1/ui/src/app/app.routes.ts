import { Routes } from '@angular/router';
import { environment } from '../environments/environment';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    title: 'ACTS · Mission Control',
    loadComponent: () => import('./features/home/home.page').then((m) => m.HomePage),
  },
  {
    path: 'people',
    title: 'ACTS · Roster',
    loadComponent: () =>
      import('./features/people/people-list.page').then((m) => m.PeopleListPage),
  },
  {
    path: 'api-error-test',
    title: 'API Error Test',
    canMatch: [() => !environment.production],
    loadComponent: () =>
      import('./features/api-error-test/api-error-test.page').then((m) => m.ApiErrorTestPage),
  },
  {
    path: 'people/:name',
    title: 'ACTS · Astronaut detail',
    loadComponent: () =>
      import('./features/people/person-detail.page').then((m) => m.PersonDetailPage),
  },
  { path: '**', redirectTo: '' },
];
