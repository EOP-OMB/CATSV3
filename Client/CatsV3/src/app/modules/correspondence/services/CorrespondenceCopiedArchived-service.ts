import { HttpClient, HttpBackend, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { CurrentUserService, ModPromiseServiceBase } from 'mod-framework';
import { from, Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { DataSources } from '../../shared/interfaces/data-source';
import { Correspondence } from '../Models/correspondence.model';
import { CorrespondenceCopiedArchived } from '../Models/correspondenceCopiedArchived.model';


@Injectable({
    // This service should be created
    // by the root application injector.
    providedIn: 'root'
  })
  export class CorrespondenceCopiedArchivedService extends ModPromiseServiceBase<CorrespondenceCopiedArchived> {      

    constructor(http: HttpClient, private http2: HttpClient,  handler: HttpBackend, private userService: CurrentUserService, private initialDataSources: DataSources, private dialog: MatDialog,) {
        super(http, environment.apiUrl, 'correspondenceCopiedArchived', CorrespondenceCopiedArchived);
    }

    loadCorrespondenceCopiedArchived(upn: string): Observable<CorrespondenceCopiedArchived[]> {
  
      let params = new HttpParams().set('upn', upn);
      return  from(this.getAllWithHttpParams(params)); 
    }

    getAllWithHttpParams(params?: HttpParams): Promise<CorrespondenceCopiedArchived[]>{
      return this.http.get<CorrespondenceCopiedArchived[]>(environment.apiUrl + '/api/'+ this.endpoint + '/filter',{params})   .toPromise()  
    }

    createCorrespondenceCopiedArchived(correspondenceCopiedArchived: CorrespondenceCopiedArchived): Observable<CorrespondenceCopiedArchived>{
       
        return from(this.create(correspondenceCopiedArchived)) ;
    }

    updateCorrespondenceCopiedArchived(correspondenceCopiedArchived: CorrespondenceCopiedArchived): Observable<CorrespondenceCopiedArchived>{
      return from(this.create(correspondenceCopiedArchived)) ;
    }

    deleteCorrespondenceCopiedArchived(correspondenceCopiedArchived: CorrespondenceCopiedArchived): Observable<CorrespondenceCopiedArchived>{
       
        return from(this.delete(correspondenceCopiedArchived.id)) ;
    }

    
  }