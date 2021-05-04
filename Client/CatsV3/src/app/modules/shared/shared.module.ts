import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ClearancesheetComponent } from './components/clearancesheet/clearancesheet.component';
import { Routes, RouterModule } from '@angular/router';
import { DashboardContainerComponent } from './components/dashboard-container/dashboard-container.component';
import {MaterialModule} from '../../material-module';
import {CdkDetailRowDirective} from '../../Directives/cdk-detail-row.directive';
import {FormsModule} from '@angular/forms';
import { CdkTableModule} from '@angular/cdk/table';
import {SafePipe} from '../../safe.pipe';
import { DatePipe } from '@angular/common';
import { BrowserModule } from '@angular/platform-browser';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { ReactiveFormsModule } from '@angular/forms';
import { DialogPromptComponent } from './components/dialog-prompt/dialog-prompt.component';
import { DialogAlertComponent } from './components/dialog-alert/dialog-alert.component';
import { DialogContentHolderComponent } from './components/dialog-content-holder/dialog-content-holder.component';
import { ReplacePipe } from 'src/app/miscellaneous/pipes/replace.pipe';
import { SelectAutocompleteComponent } from './components/select-autocomplete/select-autocomplete.component';
import { SelectAutocompletePlaceholderComponent } from './components/select-autocomplete-placeholder/select-autocomplete-placeholder.component';
import { SelectSearchComponent } from './components/select-search/select-search.component';
import { MatSelectSearchComponent } from './components/mat-select-search/mat-select-search.component';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { ForbiddenComponent } from './components/forbidden/forbidden.component';
import { SummaryComponent } from './components/summary/summary.component';
import { NgxMatFileInputModule } from '@angular-material-components/file-input';
import { HttpClientModule } from '@angular/common/http';
import { ClearancesheetDialogContentPopupComponent } from './components/clearancesheet-DialogContent-popup/Clearancesheet-DialogContent-popup';
@NgModule({
    declarations: [
        ClearancesheetComponent, 
        DashboardContainerComponent,
        CdkDetailRowDirective,
        SelectSearchComponent,
        MatSelectSearchComponent,
        SafePipe,
        ReplacePipe,
        DialogPromptComponent,
        DialogAlertComponent,
        DialogContentHolderComponent,
        SelectAutocompleteComponent,
        SelectAutocompletePlaceholderComponent,
        ForbiddenComponent,
        SummaryComponent,
        ClearancesheetDialogContentPopupComponent,
        
    ],
    imports: [
        
        MatTableModule,
        MatSortModule,
        CommonModule,
        MaterialModule,
        RouterModule,
        FormsModule,
        ReactiveFormsModule,
        CdkTableModule,
        NgxMatSelectSearchModule,
        NgxMatFileInputModule,
        HttpClientModule,
    ],
    exports: [
        ClearancesheetComponent,
        DashboardContainerComponent,
        SelectAutocompleteComponent,
        SelectAutocompletePlaceholderComponent,
        SelectSearchComponent,
        MatSelectSearchComponent,
        ForbiddenComponent,
        SummaryComponent,
        CdkDetailRowDirective,
        MaterialModule,
        HttpClientModule,
        FormsModule,
        CdkTableModule,
        NgxMatSelectSearchModule,
        NgxMatFileInputModule,
        SafePipe,ReplacePipe]
})
export class SharedModule { }
