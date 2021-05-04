import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HelpPageComponent } from './components/help-page/help-page.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { PdfViewerModule } from 'ng2-pdf-viewer';

const adminRoutes: Routes = [
    { path: '', component: HelpPageComponent }
];



@NgModule({
  declarations: [HelpPageComponent],
  imports: [
    PdfViewerModule,
      SharedModule,
      CommonModule,
      RouterModule.forChild(adminRoutes)
  ]
})
export class HelpModule { }
