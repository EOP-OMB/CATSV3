import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomePageComponent } from './components/home-page/home-page.component';
import { Routes, RouterModule } from '@angular/router';
import { MaterialModule } from 'src/app/material-module';

const homeRoutes: Routes = [
    { path: '', component: HomePageComponent }
];



@NgModule({
  declarations: [HomePageComponent],
  imports: [
      CommonModule,MaterialModule,
      RouterModule.forChild(homeRoutes)
  ]
})
export class HomeModule { }
