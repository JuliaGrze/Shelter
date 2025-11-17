import { Routes } from '@angular/router';
import { AnimalList } from './features/animals/animal-list/animal-list';
import { AnimalDetails } from './features/animals/animal-details/animal-details';
import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { roleGuard } from './core/guards/role-guard';
import { AnimalAddForm } from './features/animals/animal-add-form/animal-add-form';
import { SpeciesAddForm } from './features/species/species-add-form/species-add-form';
import { AnimalEditDeleteForm } from './features/animals/animal-edit-delete-form/animal-edit-delete-form';
import { SpeciesManageForm } from './features/species/species-manage-form/species-manage-form';
import { MedicalRecordDueList } from './features/medical/medical-record-due-list/medical-record-due-list';
import { DonateWidget } from './features/donation/donate-widget/donate-widget';
import { DonarWall } from './features/donation/donar-wall/donar-wall';
import { MonthlySum } from './features/donation/monthly-sum/monthly-sum';
import { AdoptionWorkerList } from './features/adoption/adoption-worker-list/adoption-worker-list';
import { ApplicationAdoptionDetailsWorker } from './features/adoption/application-adoption-details-worker/application-adoption-details-worker';
import { MyApplications } from './features/adoption/my-applications/my-applications';
import { ApplyForAdoption } from './features/adoption/apply-for-adoption/apply-for-adoption';
import { DetailsApplicationAdoption } from './features/adoption/details-application-adoption/details-application-adoption';

export const routes: Routes = [
    {path: 'animals', component: AnimalList},
    {path: 'animals/:id', component: AnimalDetails},

    //ADMIN/WORKER
    {
        path: 'worker/animals/new',
        component: AnimalAddForm,
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'Worker']}
    },
    {
        path: 'worker/animals/edit/:id',
        component: AnimalEditDeleteForm,
        canActivate: [roleGuard],
        data: {roles: ['Admin', 'Worker']}
    },
    {
        path: 'worker/species/new',
        component: SpeciesAddForm,
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'Worker']}
    },
    {
        path: 'worker/species/manage',
        component: SpeciesManageForm,
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'Worker']}
    },
    {
        path: 'worker/medical/due',
        component: MedicalRecordDueList,
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'Worker']}
    },
    {
        path: 'worker/donate/monthly/sum',
        component: MonthlySum,
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'Worker']}
    },

    //AUTH - login & register
    {path: 'login', component: Login},
    {path: 'register', component: Register },

    //Donation
    {path: 'donate/widget', component: DonateWidget},   
    {path: 'donate/donar/wall', component: DonarWall},
    
    //Adoption
    {
        path: 'worker/adoption/list',
        component: AdoptionWorkerList,
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'Worker'] }
    },
    {
        path: 'worker/adoption/:id',
        component: ApplicationAdoptionDetailsWorker,
        canActivate: [roleGuard],
        data: { roles: ['Admin', 'Worker'] }
    },
    {
        path: 'adoption/my/applications',
        component: MyApplications,
        canActivate: [roleGuard],
        data: { roles: ['Client','Admin', 'Worker'] }
    },
    {
        path: 'adoption/apply/:id',
        component: ApplyForAdoption,
        canActivate: [roleGuard],
        data: { roles: ['Client','Admin', 'Worker'] }
    },
    {
        path: 'adoption/details/:id',
        component: DetailsApplicationAdoption,
        canActivate: [roleGuard],
        data: { roles: ['Client','Admin', 'Worker'] }
    },



    {path: '**', redirectTo: 'animals'},

];
