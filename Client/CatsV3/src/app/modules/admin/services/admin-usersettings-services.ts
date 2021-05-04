import { HttpClient, HttpParams, HttpErrorResponse, HttpHeaders, HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpBackend } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ModPromiseServiceBase, UserInfo, CurrentUserService } from 'mod-framework';
import { UserInfoExtended } from '../../shared/interfaces/IUserInfoEtended';
import { DataSources } from '../../shared/interfaces/data-source';
import { environment } from 'src/environments/environment';
import { Originator } from '../../originator/models/originator.model';
import { LeadOfficeMember } from 'src/app/models/leadOfficeMember.model';
import { LeadOfficeOfficeManager } from 'src/app/models/LeadOfficeOfficeManager.model';
import { Observable } from 'rxjs/Rx';
import { catchError } from 'rxjs/operators';
import { DLGroupMembers } from 'src/app/models/dLGroupMembers.model';
import { Administration } from 'src/app/models/administration.model';

@Injectable({
  // This service should be created
  // by the root application injector.
  providedIn: 'root'
})
export class AdminUserSettingService extends ModPromiseServiceBase<Originator> {

  private _user: UserInfoExtended;

  constructor(http: HttpClient, private userService: CurrentUserService, private initialDataSources: DataSources) {
    super(http, environment.apiUrl, 'admin', Originator);
    //this.httpClient = new HttpClient(handler);    
  }

  public get user(): any {
    return this.userService.user;
  }

  addCatsLeadOfficeUsers(members: LeadOfficeMember[], managers:LeadOfficeOfficeManager[] , offices: number[]) : Observable<any>{
    let formData = new FormData();

    formData.append('members', JSON.stringify(members));
    formData.append('managers', JSON.stringify(managers));
    formData.append('offices', JSON.stringify(offices)); 
    
    return this.http.post((environment.apiUrl + '/api/admin/addcatsusers'), formData).pipe(
      catchError(this.handleError1)
    )
  } 

  addCatsDLUsers(members: DLGroupMembers[], dlIds: number[]) : Observable<any>{
    let formData = new FormData();
    formData.append('members', JSON.stringify(members));
    formData.append('dlIds', JSON.stringify(dlIds)); 
    
    return this.http.post((environment.apiUrl + '/api/admin/addDlusers'), formData).pipe(
      catchError(this.handleError1)
    )
  } 

  addCatsSupports(members: LeadOfficeMember[], roles: number[]) : Observable<any>{
    let formData = new FormData();
    formData.append('members', JSON.stringify(members));
    formData.append('roles', JSON.stringify(roles));     
    formData.append('offices', JSON.stringify(this.initialDataSources.leadOffices.filter(o => o.name.includes('CATS SUPPORT')).map(x => x.id))); 
    
    return this.http.post((environment.apiUrl + '/api/admin/addSupports'), formData).pipe(
      catchError(this.handleError1)
    )
  } 


  deleteCatsLeadOfficeUsers(userId: number, ismanager:boolean , leadOfficeId: number) : Observable<any>{
    let formData = new FormData();

    formData.append('userId', JSON.stringify(userId));
    formData.append('ismanager', JSON.stringify(ismanager));
    formData.append('leadOfficeId', JSON.stringify(leadOfficeId)); 
    
    return this.http.post((environment.apiUrl + '/api/admin/removecatsusers'), formData).pipe(
      catchError(this.handleError1)
    );      
  } 
  
  deleteCatsSupportUsers(upn: string, roleId: number) : Observable<any>{
    let formData = new FormData();

    formData.append('upn', JSON.stringify(upn));
    formData.append('roleId', JSON.stringify(roleId)); 
    
    return this.http.post((environment.apiUrl + '/api/admin/removesupportusers'), formData).pipe(
      catchError(this.handleError1)
    );      
  }

  deleteCatsDLUsers(userId: number, dlId: number) : Observable<any>{
    let formData = new FormData();

    formData.append('userId', JSON.stringify(userId));
    formData.append('dlId', JSON.stringify(dlId)); 
    
    return this.http.post((environment.apiUrl + '/api/admin/removecatDlsusers'), formData).pipe(
      catchError(this.handleError1)
    );      
  }

  handleError1(handleError: any): Observable<any[]> {
    //handleError.error.error.message
    throw new Error("Method not implemented.");
  }

}