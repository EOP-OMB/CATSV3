import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OdashboardComponent } from './components/odashboard/odashboard.component';
import { PendingLettersComponent } from './components/pending-letters/pending-letters.component';
import { LaunchRoundFormComponent } from './components/launch-round-form/launch-round-form.component';
import { NewCollaborationFormComponent } from './components/new-collaboration-form/new-collaboration-form.component';
import { CollaborationDetailsComponent } from './components/collaboration-details/collaboration-details.component';
import { Routes, RouterModule } from '@angular/router';
import {SharedModule} from '../shared/shared.module';
import { RoleGuardService } from 'src/app/security/role-guard.service';

const odashboardRoutes: Routes = [
    { path: '', component: OdashboardComponent,data:{title:''} },
    { path: 'opending', component: NewCollaborationFormComponent,data:{title:'Pending'} },
    { path: 'manage', component: NewCollaborationFormComponent,data:{title:'Manage Existing'} },
    { path: 'olaunch', component: NewCollaborationFormComponent,data:{title:'Next Round'} },
    { path: 'onewcollaboration', component: NewCollaborationFormComponent,data:{title:'New Collaboration'} },
    { path: 'ocollaborationdetails', component: NewCollaborationFormComponent,data:{title:'Collaboration Details'} }
];



@NgModule({
    declarations: [
        OdashboardComponent,
        PendingLettersComponent,
        LaunchRoundFormComponent,
        NewCollaborationFormComponent,
        CollaborationDetailsComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        RouterModule.forChild(odashboardRoutes)
    ],
    exports: [NewCollaborationFormComponent]
})
export class OriginatorModule { }
