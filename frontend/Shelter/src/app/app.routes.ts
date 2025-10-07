import { Routes } from '@angular/router';
import { AnimalList } from './features/animals/animal-list/animal-list';
import { AnimalDetails } from './features/animals/animal-details/animal-details';
import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';

export const routes: Routes = [
    {path: 'animals', component: AnimalList},
    {path: 'animals/:id', component: AnimalDetails},

    //AUTH - login & register
    {path: 'login', component: Login},
    {path: 'register', component: Register },

    {path: '**', redirectTo: 'animals'}

];
