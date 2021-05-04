import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CdashboardComponent } from './components/cdashboard/cdashboard.component';
import { CorrespondenceDetailsComponent } from './components/correspondence-details/correspondence-details.component';
import { CorrespondenceFormComponent } from './components/correspondence-form/correspondence-form.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from '../../modules/shared/shared.module';
import { SafePipe } from 'src/app/safe.pipe';
import { RoleGuardService } from 'src/app/security/role-guard.service';
import { DialogContentPopupComponent } from './components/dialog-content-popup/dialog-content-popup.component';
import { OriginatorModule } from '../originator/originator.module';


const cdashboardRoutes: Routes = [
    { path: '', component: CdashboardComponent, pathMatch: 'full', data:{title:''}},
    { path: 'cdetails', component: CorrespondenceDetailsComponent, data:{title:'Manage Details'} ,canActivate: [RoleGuardService]},
    { path: 'cform', component: CorrespondenceFormComponent, data:{title:'New Letter'},canActivate: [RoleGuardService] }
];


@NgModule({
    declarations: [
        CdashboardComponent,
        CorrespondenceDetailsComponent,
        CorrespondenceFormComponent,
        DialogContentPopupComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        OriginatorModule,
        RouterModule.forChild(cdashboardRoutes)
    ],
    exports: [CorrespondenceDetailsComponent, DialogContentPopupComponent]
})
export class CorrespondenceModule { }
