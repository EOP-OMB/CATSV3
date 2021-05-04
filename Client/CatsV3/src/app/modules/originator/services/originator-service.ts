import { HttpClient, HttpParams, HttpErrorResponse, HttpHeaders, HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpBackend } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, from } from 'rxjs';
import {  throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ModPromiseServiceBase, UserInfo, CurrentUserService } from 'mod-framework';
import { UserInfoExtended } from '../../shared/interfaces/IUserInfoEtended';
import { DataSources } from '../../shared/interfaces/data-source';
import { Originator } from '../models/originator.model';
import { Correspondence } from '../../correspondence/Models/correspondence.model';
import { Collaboration } from '../models/collaboration.model';
import { Reviewer } from '../../reviewer/models/reviewer.model';
import { FYIUser } from '../models/fyiuser.model';
import { environment } from 'src/environments/environment';
import Swal from 'sweetalert2/dist/sweetalert2.all.js';

@Injectable({
  // This service should be created
  // by the root application injector.
  providedIn: 'root'
})
export class OriginatorService extends ModPromiseServiceBase<Originator> {

  public allOriginators :Observable<Originator[]>;
  public singleOriginator :Observable<Originator>;

  filterOption : string;
  searchOption : string;
  filterOptionId : string;
  private _user: UserInfoExtended;

  constructor(http: HttpClient, private userService: CurrentUserService, private initialDataSources: DataSources) {
    super(http, environment.apiUrl, 'originator', Originator);
    //this.httpClient = new HttpClient(handler);    
  }

  public get user(): any {
    return this.userService.user;
  }

  getOriginatorsWithOptions(catsid: string, round: string, status: string = ''): Observable<Originator[]> {
      
    let params = new HttpParams()
      .set('catsid', catsid)
      .set('status', status)
      .set('round', round);
      return  from(this.getDataWithHttpParams(params, 'filterbystatus'));
  }
  

  getDataWithHttpParams(params?: HttpParams, route?: string): Promise<Originator[]>{
    return this.http.get<Originator[]>(environment.apiUrl + '/api/' + this.endpoint + '/' + route,{params})
      .pipe(x => this.allOriginators = x,catchError(this.handleError2)).toPromise()  
  }

  
  loadOriginatorById(id: number): Observable<Originator> {
    this.filterOptionId = id.toString();    
      return from(this.get(id)); 
  }

  updateOriginator(originator: Originator): Observable<Originator>{
        
    return from(this.create(originator)) ;
    
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
        Swal.fire(errorMessage, 'error');
      }
      if(error.status == 440){
        errorMessage = error.error?.title;
        Swal.fire('Hey  ' + this.initialDataSources.currentBrowserUser.PreferredName + '!', errorMessage, 'info');
      }
      else{
        errorMessage = error.error?.Message;
        Swal.fire(errorMessage, 'error');
      }
      
    }
    return throwError(errorMessage);
  }
}