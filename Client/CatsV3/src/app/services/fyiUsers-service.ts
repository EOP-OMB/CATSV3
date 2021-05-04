import { HttpClient, HttpParams, HttpErrorResponse, HttpHeaders, HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpBackend } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, from } from 'rxjs';
import {  throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ModPromiseServiceBase, UserInfo, CurrentUserService } from 'mod-framework';
import { environment } from 'src/environments/environment';
import { FYIUser } from '../modules/originator/models/fyiuser.model';
import { UserInfoExtended } from '../modules/shared/interfaces/IUserInfoEtended';
import { DataSources } from '../modules/shared/interfaces/data-source';
import Swal from 'sweetalert2/dist/sweetalert2.all.js';

@Injectable({
  // This service should be created
  // by the root application injector.
  providedIn: 'root'
})
export class FyiUserService extends ModPromiseServiceBase<FYIUser> {

  public allFyiUserss :Observable<FYIUser[]>;
  public singleOriginator :Observable<FYIUser>;

  filterOption : string;
  searchOption : string;
  filterOptionId : string;
  private _user: UserInfoExtended;

  constructor(http: HttpClient, private userService: CurrentUserService, private initialDataSources: DataSources) {
    super(http, environment.apiUrl, 'fyiuser', FYIUser);
    //this.httpClient = new HttpClient(handler);    
  }

  getOriginatorsWithOptions(catsid: string, round: string, status: string = ''): Observable<FYIUser[]> {
      
    let params = new HttpParams()
      .set('catsid', catsid)
      .set('status', status)
      .set('round', round);
      return  from(this.getDataWithHttpParams(params, 'filterbystatus'));
  }
  

  getDataWithHttpParams(params?: HttpParams, route?: string): Promise<FYIUser[]>{
    return this.http.get<FYIUser[]>(environment.apiUrl + '/api/' + this.endpoint + '/' + route,{params})
      .pipe(x => this.allFyiUserss = x,catchError(this.handleError2)).toPromise()  
  }


  public get user(): any {
    return this.userService.user;
  }
  
  loadOriginatorById(id: number): Observable<FYIUser> {
    this.filterOptionId = id.toString();    
      return from(this.get(id)); 
  }

  updateOriginator(originator: FYIUser): Observable<FYIUser>{
        
    return from(this.create(originator)) ;
    
  }

  handleError1(handleError: any): Observable<FYIUser[]> {
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