import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReviewDetailsComponent } from './components/review-details/review-details.component';
import { RdashboardComponent } from './components/rdashboard/rdashboard.component';
import { ClearanceFormComponent } from './components/clearance-form/clearance-form.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from '../../modules/shared/shared.module';
import { RoleGuardService } from 'src/app/security/role-guard.service';
import { ReviewOperationsComponent } from '../admin/components/review-operations/review-operations.component';
import { OriginatorModule } from '../originator/originator.module';

const rdashboardRoutes: Routes = [
    { path: '', component: RdashboardComponent  },
    { path: 'rdetails', component: ReviewDetailsComponent, data:{title:'Review Details'}, canActivate: [RoleGuardService]  },
    { path: 'rmanage', component: ReviewDetailsComponent, data:{title:'Review Manage'}, canActivate: [RoleGuardService]  },
    { path: 'rdraft', component: ReviewDetailsComponent, data:{title:'Review Draft'}, canActivate: [RoleGuardService]  },
    { path: 'rclearance', component: ClearanceFormComponent, data:{title:'Review Details'}, canActivate: [RoleGuardService]   }
];

@NgModule({
    declarations: [
        ReviewDetailsComponent,
        RdashboardComponent,
        ClearanceFormComponent],
    imports: [
        CommonModule,
        SharedModule,
        OriginatorModule,
        RouterModule.forChild(rdashboardRoutes)
    ],
    exports: [ReviewDetailsComponent]
})
export class ReviewerModule { }
