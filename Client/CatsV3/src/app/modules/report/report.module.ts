import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportViewerComponentCATS } from './components/report-viewer/report-viewer-cats.component';
import { Routes, RouterModule } from '@angular/router';
import { ReportLetterTypePerformanceMetricsComponent } from './components/report-letter-type-performance-metrics/report-letter-type-performance-metrics.component';
import { ReportLeadofficePerformanceMetricsComponent } from './components/report-leadoffice-performance-metrics/report-leadoffice-performance-metrics.component';
import { ReportReviewroundPerformanceMetricsComponent } from './components/report-reviewround-performance-metrics/report-reviewround-performance-metrics.component';
import { ReportStatisticsComponent } from './components/report-statistics/report-statistics.component';
import { SharedModule } from '../shared/shared.module';
import { ReportViewerModule } from 'ngx-ssrs-reportviewer';
import { IframeAutoHeightDirective } from 'src/app/Directives/IframeAutoHeightDirective,directive';
import { RoleGuardService } from 'src/app/security/role-guard.service';

const reportRoutes: Routes = [
    { path: '', component: ReportViewerComponentCATS, data:{title:'Main Report'}  ,canActivate: [RoleGuardService]}
];

@NgModule({
  declarations: [IframeAutoHeightDirective,ReportViewerComponentCATS, ReportStatisticsComponent, ReportLetterTypePerformanceMetricsComponent, ReportLeadofficePerformanceMetricsComponent, ReportReviewroundPerformanceMetricsComponent],
  imports: [
      CommonModule,
      SharedModule,
      ReportViewerModule,
      RouterModule.forChild(reportRoutes)
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class ReportModule { }
