import { HttpClient, HttpParams, HttpErrorResponse, HttpHeaders, HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpBackend } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, from } from 'rxjs';
import {  throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ModPromiseServiceBase, UserInfo, CurrentUserService } from 'mod-framework';
import { UserInfoExtended } from '../../shared/interfaces/IUserInfoEtended';
import { DataSources } from '../../shared/interfaces/data-source';
import { Reviewer } from '../../reviewer/models/reviewer.model';
import { environment } from 'src/environments/environment';
import { Collaboration } from '../../originator/models/collaboration.model';
import { Originator } from '../../originator/models/originator.model';
import { Correspondence } from '../../correspondence/Models/correspondence.model';
import { FYIUser } from '../../originator/models/fyiuser.model';
import { CATSPackage, UserQuery } from '../utilities/utility-functions';
import { PageRequest, Page } from 'ngx-pagination-data-source';
import Swal from 'sweetalert2/dist/sweetalert2.all.js';

@Injectable({
  // This service should be created
  // by the root application injector.
  providedIn: 'root'
})
export class CollaborationService extends ModPromiseServiceBase<Collaboration> {

  public allCollaborations :Observable<Collaboration[]>;
  public singleCollaboration :Observable<Collaboration>;

  filterOption : string;
  searchOption : string;
  filterOptionId : string;
  route : string;
  private _user: UserInfoExtended;

  constructor(http: HttpClient, private userService: CurrentUserService, private initialDataSources: DataSources) {
    super(http, environment.apiUrl, 'collaboration', Collaboration);
    //this.httpClient = new HttpClient(handler);    
  }

  public get user(): any {
    return this.userService.user;
  }
  
  page(request: PageRequest<Correspondence>, filter : string, query: UserQuery, dataOption = ''): Observable<Page<Correspondence>> {
    // transform request & query into something your server can understand
    // (you might want to refactor this into a utility function)

    // if (query.search.length < 3 && query.search.trim() != ''){
    //   query.search = '';
    // }

    let currentUserOffices: String[] = this.initialDataSources.currentBrowserUser.MemberGroupsCollection;
    //add the external offices to the current user assigned offices
    this.initialDataSources.currentBrowserUser.MemberExternalGroupsCollection.forEach(o =>{if(currentUserOffices?.indexOf(o) == -1)  currentUserOffices.push(o)});
    
    var params : any = {
      roles: '',
      dataoption: dataOption,
      offices:currentUserOffices.join(','),
      userid: this.initialDataSources.currentBrowserUser.MainActingUserUPN,
      officemanageroffices: this.initialDataSources.currentBrowserUser?.MemberCATSOfficeManagerCollection.join(','),
      filter: filter,
      pageNumber: request.page, 
      pageSize: request.size,
      sortOrder: request.sort.order,
      sortProperty: request.sort.property,
      ...query
    }

    
    // fetch page over HTTP
    return this.http.get<Page<Correspondence>>(environment.apiUrl + '/api/Correspondence/odashboard', {params})
    .pipe( catchError(this.handleError2));
    // transform the response to the Page type with RxJS map() if necessary
  }

  updateCATSPackage(cATSPackage: CATSPackage, files?: File[]) : Observable<any>{
    
    //set the current user upn
    cATSPackage.correspondence.currentUserUPN = this.userService.user.upn;//set the current user upn
    cATSPackage.correspondence.currentUserFullName = this.userService.user.displayName;

    //check for files
    let reviewfilesToUpload : File[] = cATSPackage.reviewFiles;
    let referencefilesToUpload : File[] = cATSPackage.referenceFiles;
    let finalfilesToUpload : File[] = cATSPackage.finalFiles;
    const formData = new FormData();
    const headers = new HttpHeaders({ 'ngsw-bypass': ''});
      
    Array.from(reviewfilesToUpload).map((file, index) => {
      let filename = 'REV_' + file.name;
      return formData.append('review'+ index, file, filename);
    });

    Array.from(referencefilesToUpload).map((file, index) => {
      let filename = 'REF_' +  file.name;
      return formData.append('reference'+ index, file, filename);
    });

    Array.from(finalfilesToUpload).map((file, index) => {
      let filename = 'FINAL_' +  file.name;
      return formData.append('final'+ index, file, filename);
    });

    formData.append('correspondence', JSON.stringify(cATSPackage.correspondence)); 
    formData.append('collaboration', JSON.stringify(cATSPackage.collaboration));
    formData.append('originators', JSON.stringify(cATSPackage.originators));
    formData.append('reviewers', JSON.stringify(cATSPackage.reviewers));
    formData.append('fyiusers', JSON.stringify(cATSPackage.fyiusers));

    return this.http.post((environment.apiUrl + '/api/' + this.endpoint + '/update'), formData, {
      reportProgress: true,
      observe: 'events'
    }).pipe(
      catchError(this.handleError2)
            
    )
  }

  loadCollaborationById(id: number): Observable<Collaboration> {
    this.filterOptionId = id.toString();    
      return from(this.get(id)); 
  }


  loadCollaborationByCorrespondenceId(id: number): Observable<Collaboration> {
    this.route = 'getCorrespondence';
    let params = new HttpParams()
      .set('correspondenceId', id.toString());   
    return this.http.get<Collaboration>(environment.apiUrl + '/api/' + this.endpoint + '/' + this.route,{params})
    .pipe( catchError(this.handleError2)); 
  }

  updateCollaboration(originator: Collaboration): Observable<Collaboration>{
        
    return from(this.create(originator)) ;
    
  }

  getDataWithHttpParams(params?: HttpParams): Promise<Collaboration[]>{
    return this.http.get<Collaboration[]>(environment.apiUrl + '/api/' + this.endpoint + '/' + this.route,{params})
      .pipe(x => this.allCollaborations = x,catchError(this.handleError2)).toPromise()  
  }

  handleError1(handleError: any): Observable<Correspondence[]> {
    throw new Error("Method not implemented.");
  }

  handleError2(error: HttpErrorResponse) {
    let errorMessage = 'Unknown error!';
    if (error.error instanceof ErrorEvent) {
      // Client-side errors
      errorMessage = `Error: ${error.error.message}`;
      Swal.fire(errorMessage, 'error');
    } 
    else {
      // Server-side errors
      if(error.status == 413){
        errorMessage = 'Your request may contain files that exceeded 50 MB total size limit. \n' +  `Error Code: ${error.status}\nMessage: ${error.message}`;
        Swal.fire('', errorMessage, 'error');
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
    return throwError(errorMessage);
  }


}

