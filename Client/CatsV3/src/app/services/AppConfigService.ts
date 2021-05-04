import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import 'rxjs/add/operator/toPromise';
import { map } from 'rxjs/operator/map';
import { Observable, forkJoin, throwError } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Employee, MainLoaderService } from './main-loader-service';
import { DataSources } from '../modules/shared/interfaces/data-source';
import { LetterType } from '../models/letterType.model';
import { LeadOffice } from '../models/LeadOffice.model';
import { ExternalUser } from '../models/externauUser.model';
import { ReviewRound } from '../models/reviewRound.model';
import { DLGroup } from '../models/dLGroup.model';
import { HelpDocument } from '../modules/help/Models/helpDocument.model';
import { Administration } from '../models/administration.model';
import { Role } from '../models/role.model';

@Injectable()
export class AppConfigService {

  public version: string;
  public impersonationLabelValue: string;

  constructor(public http: HttpClient, public mainLoaderService: MainLoaderService, public initialDataSources: DataSources) {}

  loadInitialSettings(): Observable<any[]>{
    let lettertype = this.http.get(environment.apiUrl + '/api/lettertype').toPromise();
    let leadoffice = this.http.get(environment.apiUrl + '/api/leadoffice').toPromise();
    let externalusers = this.http.get(environment.apiUrl + '/api/externaluser').toPromise();
    let reviewRound = this.http.get(environment.apiUrl + '/api/reviewRound').toPromise();
    let dlGroups =  this.http.get(environment.apiUrl + '/api/dlgroup').toPromise();
    let helpMenusItems =  this.http.get(environment.apiUrl + '/api/helpdocument').toPromise();

    return forkJoin([lettertype, leadoffice, externalusers, reviewRound, dlGroups, helpMenusItems]);
  }

  loadData() :Promise<any>  {

      const promise = this.http.get('/assets/app.config.json')
        .toPromise()
        .then(data => {
          Object.assign(this, data);
          return data;
        });

      return promise;
  }

  async load(url: string = '') :Promise<any>  {

      const promiseLetterType = await this.http.get(environment.apiUrl + '/api/lettertype').toPromise().then(data => 
        {this.initialDataSources.letterTypes = data as LetterType[]; 
          this.initialDataSources.letterTypes = this.initialDataSources.letterTypes.sort((a, b) => (a.name > b.name) ? 1 : -1); return data;}).catch(this.handleError2) ;

      const promiseLeadOffice = await this.http.get(environment.apiUrl + '/api/leadoffice').toPromise().then(data => 
        {
          this.initialDataSources.leadOffices = data as LeadOffice[]; return data;}).catch(function(error) {
          //alert("The error is handled, continue normally " + error.message);        
        });

        const promiseEmployee = await this.http.get(environment.apiUrl + '/api/employee').toPromise().then(data => 
          {this.initialDataSources.employees = data as Employee[]; return data;}).catch(function(error) {
            //alert("The error is handled, continue normally " + error.message);        
          });

      const promiseExternalOffice = await this.http.get(environment.apiUrl + '/api/externaluser').toPromise().then(data => 
        {this.initialDataSources.externalUsers = data as ExternalUser[]; return data;}).catch(function(error) {
          //alert("The error is handled, continue normally " + error.message);        
        });

      const promiseReviewRound = url.includes('originator') ?  await this.http.get(environment.apiUrl + '/api/reviewRound').toPromise().then(data => 
        {this.initialDataSources.reviewRounds = data as ReviewRound[]; return data;}).catch(function(error) {
          //alert("The error is handled, continue normally " + error.message);        
        }) : [new ReviewRound()];

      const promiseDLGroup = await this.http.get(environment.apiUrl + '/api/dlgroup').toPromise().then(data => 
        {this.initialDataSources.dlGroups = data as DLGroup[]; return data;}).catch(function(error) {
          //alert("The error is handled, continue normally " + error.message);        
        });

      const promiseReviewRoundList = await this.http.get(environment.apiUrl + '/api/reviewround').toPromise().then(data => 
          {this.initialDataSources.reviewRounds = data as ReviewRound[]; return data;}).catch(function(error) {
            //alert("The error is handled, continue normally " + error.message);        
          });

      const promiseHelpMenus = await this.http.get(environment.apiUrl + '/api/helpdocument').toPromise().then(data => 
          {this.initialDataSources.helpDocuments = data as HelpDocument[]; return data;}).catch(function(error) {
            //alert("The error is handled, continue normally " + error.message);        
          });

      const promiseAdministrations = await this.http.get(environment.apiUrl + '/api/administration').toPromise().then(data => 
          {this.initialDataSources.adminstrations = data as Administration[]; return data;}).catch(function(error) {
          });

      const promiseRoles = await this.http.get(environment.apiUrl + '/api/role').toPromise().then(data => 
          {this.initialDataSources.roles = data as Role[]; return data;}).catch(function(error) {
          });

      await this.mainLoaderService.setCurrentUserMetadata();

      return this.initialDataSources;
  }

  

  handleError2(error:  Response | any) {
    let errorMessage = 'Unknown error!';
    if (error.error instanceof ErrorEvent) {
      // Client-side errors
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side errors
      errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
    }

    window.alert(errorMessage);
    return (errorMessage);
  }
  
}
