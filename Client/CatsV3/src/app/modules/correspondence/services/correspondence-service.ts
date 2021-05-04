import { HttpClient, HttpParams, HttpErrorResponse, HttpBackend, HttpEventType, HttpEvent, HttpRequest, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, from } from 'rxjs';
import { Correspondence } from '../Models/correspondence.model';
import { environment } from 'src/environments/environment';
import {  throwError } from 'rxjs';
import { catchError, map, tap, last, delay } from 'rxjs/operators';
import { MatDialog } from '@angular/material/dialog';
import { DialogAlertComponent } from '../../shared/components/dialog-alert/dialog-alert.component';
import { ModPromiseServiceBase, UserInfo, CurrentUserService } from 'mod-framework';
import { FormGroup } from '@angular/forms';
import { UserInfoExtended } from '../../shared/interfaces/IUserInfoEtended';
import { DataSources } from '../../shared/interfaces/data-source';
import { PageEvent } from '@angular/material/paginator';
import { Page, PageRequest } from 'ngx-pagination-data-source/lib/page';
import { UserQuery } from '../../shared/utilities/utility-functions';
import Swal from 'sweetalert2/dist/sweetalert2.all.js';

export interface Employee {
  first_name: string;
  last_name: string;
}

@Injectable({
  // This service should be created
  // by the root application injector.
  providedIn: 'root'
})
export class CorrespondenceService extends ModPromiseServiceBase<Correspondence> {

  public allCorrespondences :Observable<any[]>;
  public singleCorrespondences :Observable<any>;
  public correspondences : Correspondence[];

  filterOption : string;
  searchOption : any;
  filterOptionId : string;
  private _user: UserInfoExtended;

  public progress: number;
  public message: string='';

  //This httpClient is using HttpBackend temporary because the interptor is adding and forcing application/json
  //as an unique content-type. As such the formData will fail with unsupported media type 405/415 error
  //as not being able to set the mutli-part/form-data content type
  //private httpClient: HttpClient;

  constructor(http: HttpClient, private http2: HttpClient,  handler: HttpBackend, private userService: CurrentUserService, private initialDataSources: DataSources, private dialog: MatDialog,) {
    super(http, environment.apiUrl, 'correspondence', Correspondence);
    //this.httpClient = new HttpClient(handler);    
  }

  public get user(): any {
    return this.userService.user;
  }

  page(request: PageRequest<Correspondence>, filter : string, query: UserQuery, previousGlobalSearch?: any): Observable<Page<Correspondence>> {
    // transform request & query into something your server can understand
    // (you might want to refactor this into a utility function)

    // if (query.search.length < 2 && query.search.trim() != ''){
    //   query.search = '';
    // }

    if (previousGlobalSearch != null){
      query.search = previousGlobalSearch.searchCriteria != '' ? previousGlobalSearch.searchCriteria : query.search;
    }
    
    var params : any = {
      filter: filter,
      pageNumber: request.page, 
      pageSize: request.size,
      sortOrder: request.sort.order,
      sortProperty: request.sort.property,
      ...query
    }
    // fetch page over HTTP
    return this.http.get<Page<Correspondence>>(environment.apiUrl + '/api/Correspondence/cdashboard', {params})
    .pipe( catchError(this.handleError2)); 
    // transform the response to the Page type with RxJS map() if necessary
  }

  loadCorrespondenceById(id: number): Observable<Correspondence> {
    this.filterOptionId = id.toString();    
      return from(this.get(id)); 
  }


  /** Return distinct message for sent, upload progress, & response events */
  private getEventMessage(event: HttpEvent<any>, file: File) {
    switch (event.type) {
      case HttpEventType.Sent:
        return `Uploading file "${file.name}" of size ${file.size}.`;

      case HttpEventType.UploadProgress:
        // Compute and show the % done:
        const percentDone = Math.round(100 * event.loaded / event.total);
        return `File "${file.name}" is ${percentDone}% uploaded.`;

      case HttpEventType.Response:
        return `File "${file.name}" was completely uploaded! Your user ID is: "${event.body.userID}"`;

      default:
        return `File "${file.name}" surprising upload event: ${event.type}.`;
    }
  }

  

  sendAttachedEmails(correspondence: Correspondence, files?: File[], isEmailArchived: boolean = false) : Promise<any>{
    
    //set the current user upn
    correspondence.currentUserUPN = this.userService.user.upn;//set the current user upn
    correspondence.currentUserFullName = this.userService.user.displayName;
    //check for files
    let filesToUpload : File[] = files;
    const formData = new FormData();
    const headers = new HttpHeaders({ 'ngsw-bypass': ''});

    if (filesToUpload != undefined){  
      Array.from(filesToUpload).map((file, index) => {
        let filename = correspondence.isFinalDocument ? 'FINAL_' + file.name : 'REF_' + file.name;
        return formData.append('file'+index, file, filename);
      });
    }
    
          
    formData.append('correspondence', JSON.stringify(correspondence)); 

    this.http.get(environment.apiUrl + '/api/lettertype').toPromise();

    return this.http.post((environment.apiUrl + '/api/Correspondence/archiveEmailAttachment'), formData, {
      reportProgress: true,
      observe: 'events'
    }).pipe( catchError(this.handleError2)).toPromise();

  }

  updateMyCorrespondence(correspondence: Correspondence, files?: File[], isEmailArchived: boolean = false) : Observable<any>{
    
    //set the current user upn
    correspondence.currentUserUPN = this.userService.user.upn;//set the current user upn
    correspondence.currentUserFullName = this.userService.user.displayName;
    //check for files
    let filesToUpload : File[] = files;
    const formData = new FormData();
    const headers = new HttpHeaders({ 'ngsw-bypass': ''});

    if (filesToUpload != undefined){  
      Array.from(filesToUpload).map((file, index) => {
        let filename = correspondence.isFinalDocument ? 'FINAL_' + file.name : 'REF_' + file.name;
        return formData.append('file'+index, file, filename);
      });
    }
    
          
    formData.append('correspondence', JSON.stringify(correspondence)); 

    
    return this.http.post((environment.apiUrl + '/api/Correspondence/update'), formData, {
        reportProgress: true,
        observe: 'events'
      }).pipe( catchError(this.handleError2));
  }

  restoreDeletedCorrespondence(correspondence: Correspondence): Observable<Correspondence>{
        
    //set the current user upn
    correspondence.currentUserUPN = this.userService.user.upn;//set the current user upn
    correspondence.currentUserFullName = this.userService.user.displayName;
     
    let formData = new FormData();
           
    formData.append('correspondence', JSON.stringify(correspondence));   
    
    return this.http.post<Correspondence>((environment.apiUrl + '/api/Correspondence/restore'), formData)
      .pipe(x => this.singleCorrespondences = x,catchError(this.handleError2)) ;
    
  }

  updateCorrespondence(correspondence: Correspondence, form?: FormGroup): Observable<Correspondence>{
        
    //set the current user upn
    correspondence.currentUserUPN = this.userService.user.upn;//set the current user upn
    correspondence.currentUserFullName = this.userService.user.displayName;
     
    //check for files
    if (form){
      
      let formData = new FormData();
      let files = form.get("attachedFiles").value;
      for (let index = 0; index <  files.length; index++)  
      {  
        const file =  files[index] as File;  
        formData.append('Files', file);  
      }  

           
      formData.append('correspondence', JSON.stringify(correspondence));   
      
      return this.http.post<Correspondence>((environment.apiUrl + '/api/Correspondence/update'),formData)
       .pipe(x => this.singleCorrespondences = x,catchError(this.handleError2)) ;
    }
    else{
      return from(this.create(correspondence)) ;
    }
    
  }

  handleError1(handleError: any): Observable<Correspondence[]> {
    throw new Error("Method not implemented.");
  }

  handleError2(error: HttpErrorResponse) {
    let errorMessage = 'Unknown error!';
    if (error.error instanceof ErrorEvent) {
      // Client-side errors
      errorMessage = `Error: ${error.error.message}`;
      Swal.fire('', errorMessage, 'error');
    } 
    else {
      // Server-side errors
      if(error.status == 413){
        errorMessage = 'Your request may contain files that exceeded 50 MB total size limit. \n' +  `Error Code: ${error.status}\nMessage: ${error.message}`;
        Swal.fire(errorMessage, 'error');
      }
      if(error.status == 440){
        errorMessage = error.error?.title;
        Swal.fire('Hey!', errorMessage, 'info');
      }
      else{
        errorMessage = error.error?.Message;
        Swal.fire('', errorMessage, 'error');
      }   

    }
    //window.alert(errorMessage);
    
    return throwError(errorMessage);
  }
}