import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, Subject, forkJoin, from } from 'rxjs';
import { DataSources } from '../modules/shared/interfaces/data-source';
import { CurrentUserService, UserInfo, ModPromiseServiceBase } from 'mod-framework';
import { environment } from 'src/environments/environment';
import { Folder } from '../models/folder.model';
import { CATSPackage } from '../modules/shared/utilities/utility-functions';
import { catchError } from 'rxjs/operators';

const url = environment.apiUrl ;

@Injectable({
    // This service should be created
    // by the root application injector.
    providedIn: 'root'
  })

  export class FolderService extends ModPromiseServiceBase<Folder> {

    private endPoint : string ;
    
    constructor(http: HttpClient,  private userService: CurrentUserService, private initialDataSources: DataSources) {
      super(http, environment.apiUrl, 'folder', Folder);
    }
  
    public get user(): any {
      return this.userService.user;
    }

    async load(userUpn: string) :Promise<any>  {

        let FolderOriginators = await this.http.get(url + '/api/' + this.endpoint + '/GetFolder?search=' + userUpn).toPromise();
  
        return FolderOriginators as Folder[];
    }

    addFolder(Folder:Folder): Observable<Folder>{
      return from(this.create(Folder));
    }

    addDocumentToFolder(folder:Folder, cATSPackage: CATSPackage): Observable<any>{
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

      formData.append('folder', JSON.stringify(folder));
      
      return this.http.post((environment.apiUrl + '/api/' + this.endpoint + '/addnewdocuments'), formData, {
        reportProgress: true,
        observe: 'events'
      }).pipe(
        catchError(this.handleError)
      )
    }
 

    updateFolder(Folder:Folder): Observable<Folder>{
      return from(this.create(Folder));
    }

    loadFolderById(id: number): Observable<Folder> {         
        return from(this.get(id)); 
    }

    deleteFolder(id:number){
      return from(this.delete(id));
    } 
    
    handleError2(handleError2: any): any {
      throw new Error('Method not implemented.');
    }

  }