import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpErrorResponse, HttpClient, HttpHeaders, HttpRequest } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { Correspondence } from '../modules/correspondence/Models/correspondence.model';
import { FormGroup } from '@angular/forms';
import { CurrentUserService } from 'mod-framework';

@Injectable({
  providedIn: 'root'
})

export class FileUploadService {

  constructor(private http: HttpClient, private userService: CurrentUserService) { }

  addUser(correspondence: Correspondence, form?: FormGroup): Observable<any> {

    

    //set the current user upn
    correspondence.currentUserUPN = this.userService.user.upn;//set the current user upn
    correspondence.currentUserFullName = this.userService.user.displayName;

    //check for files
    let filesToUpload : File[] = form.get("attachedFiles").value;
    const formData = new FormData();
    const headers = new HttpHeaders({ 'ngsw-bypass': ''});
      
    Array.from(filesToUpload).map((file, index) => {
      return formData.append('file'+index, file, file.name);
    });
          
    //formData.append('correspondence', JSON.stringify(correspondence)); 
    const req = new HttpRequest('POST',(environment.apiUrl + '/api/Correspondence/update'), formData, {reportProgress: true});
    return this.http.request(req).pipe(
        catchError(this.errorMgmt)
      );

    // return this.http.post((environment.apiUrl + '/api/Correspondence/update'), formData, {
    //   reportProgress: true,
    //   observe: 'events', headers:headers
    // }).pipe(
    //   catchError(this.errorMgmt)
    // )
  }

  errorMgmt(error: HttpErrorResponse) {
    let errorMessage = '';
    if (error.error instanceof ErrorEvent) {
      // Get client-side error
      errorMessage = error.error.message;
    } else {
      // Get server-side error
      errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
    }
    //console.log(errorMessage);
    return throwError(errorMessage);
  }

}