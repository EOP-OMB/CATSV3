import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DeletedPackagesComponent } from './components/deleted-packages/deleted-packages.component';
import { ReviewOperationsComponent } from './components/review-operations/review-operations.component';
import { DocumentOperationsComponent } from './components/document-operations/document-operations.component';
import { UsersSettingsComponent } from './components/users-settings/users-settings.component';
import { PermissionSettingsComponent } from './components/permission-settings/permission-settings.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { RoleGuardService } from 'src/app/security/role-guard.service';

const adminRoutes: Routes = [
    { path: '', component: DeletedPackagesComponent, data:{title:''}  , canActivate: [RoleGuardService]},
    { path: 'deleted-items', component: DeletedPackagesComponent, data:{title:'Deleted Items'}  , canActivate: [RoleGuardService]},
    { path: 'review-operations', component: ReviewOperationsComponent, data:{title:'Review Operations' }  , canActivate: [RoleGuardService]},
    { path: 'document-operations', component: DocumentOperationsComponent, data:{title:'Document Operations' }  , canActivate: [RoleGuardService]},
    { path: 'users-settings', component: UsersSettingsComponent, data:{title:'User Settings' }  , canActivate: [RoleGuardService]},
    { path: 'permission-settings', component: PermissionSettingsComponent, data:{title:'Permission Settings' }  , canActivate: [RoleGuardService]}
];



@NgModule({
    declarations: [
        DeletedPackagesComponent,
        ReviewOperationsComponent,
        DocumentOperationsComponent,
        UsersSettingsComponent,
        PermissionSettingsComponent],
  imports: [
      SharedModule,
      CommonModule,
      RouterModule.forChild(adminRoutes)
  ]
})
export class AdminModule { }
