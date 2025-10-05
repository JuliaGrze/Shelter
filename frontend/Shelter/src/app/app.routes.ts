import { Routes } from '@angular/router';
import { AnimalList } from './features/animals/animal-list/animal-list';
import { AnimalDetails } from './features/animals/animal-details/animal-details';

export const routes: Routes = [
    {path: 'animals', component: AnimalList},
    {path: 'animals/:id', component: AnimalDetails},
    {path: '**', redirectTo: 'animals'}

];
