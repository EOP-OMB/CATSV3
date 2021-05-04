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
import { MatDialog } from '@angular/material/dialog';
import { CATSPackage } from '../../shared/utilities/utility-functions';
import Swal from 'sweetalert2/dist/sweetalert2.all.js';

@Injectable({
  // This service should be created
  // by the root application injector.
  providedIn: 'root'
})
export class ReviewerService extends ModPromiseServiceBase<Reviewer> {

  filterOption : string;
  searchOption : string;
  filterOptionId : string;
  private _user: UserInfoExtended;

  public allReviews :Observable<Reviewer[]>;
  public singleReview :Observable<Reviewer>;

  constructor(http: HttpClient, private userService: CurrentUserService, private initialDataSources: DataSources,  private dialog: MatDialog,) {
    super(http, environment.apiUrl, 'reviewer', Reviewer);
    //this.httpClient = new HttpClient(handler);    
  }

  public get user(): any {
    return this.userService.user;
  }

loadReviewersWithOptions(option: string, round: string): Observable<Reviewer[]> {
      
    let params = new HttpParams()
      .set('upn', this.initialDataSources.currentBrowserUser.MainActingUserUPN?.replace('i:0e.t|adfs|',''))
      .set('option', option)
      .set('round', round);
      return  from(this.getReviewerDataWithHttpParams(params, 'filter'));
  }

  getCompletedReviewersWithOptions(catsid: string, round: string, status: string = ''): Observable<Reviewer[]> {
      
    let params = new HttpParams()
      .set('catsid', catsid)
      .set('status', status)
      .set('round', round);
      return  from(this.getReviewerDataWithHttpParams(params, 'filterbystatus'));
  }

  loadRevieweFromCollaborationrById(id: number): Observable<Reviewer> {
    this.filterOptionId = id.toString();    
      return from(this.get(id)); 
  }


  loadReviewerById(id: number): Observable<Reviewer> {
    this.filterOptionId = id.toString();    
      return from(this.get(id)); 
  }

  updateReviewer(reviewer: Reviewer): Observable<Reviewer>{
        
    return from(this.create(reviewer)) ;
    
  }

  getReviewerDataWithHttpParams(params?: HttpParams, route?: string): Promise<Reviewer[]>{
    return this.http.get<Reviewer[]>(environment.apiUrl + '/api/' + this.endpoint + '/' + route,{params})
      .pipe(x => this.allReviews = x,catchError(this.handleError2)).toPromise()  
  }

  handleError1(handleError: any): Observable<Reviewer[]> {
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