import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { PetListComponent } from './components/pet-list/pet-list.component';
import { PetDetailComponent } from './components/pet-detail/pet-detail.component';
import { PetFormComponent } from './components/pet-form/pet-form.component';
import { AdoptionListComponent } from './components/adoption-list/adoption-list.component';
import { NotificationsComponent } from './components/notifications/notifications.component';
import { authGuard, adminGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'pets', component: PetListComponent },
  { path: 'pets/create', component: PetFormComponent, canActivate: [adminGuard] },
  { path: 'pets/:id', component: PetDetailComponent },
  { path: 'pets/:id/edit', component: PetFormComponent, canActivate: [adminGuard] },
  { path: 'adoption-requests', component: AdoptionListComponent, canActivate: [authGuard] },
  { path: 'notifications', component: NotificationsComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '' }
];
