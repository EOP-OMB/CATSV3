import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, Subject, forkJoin, from } from 'rxjs';
import { DataSources } from '../modules/shared/interfaces/data-source';
import { CurrentUserService, UserInfo, ModPromiseServiceBase } from 'mod-framework';
import { environment } from 'src/environments/environment';
import { Administration } from '../models/administration.model';

const url = environment.apiUrl ;

@Injectable({
    // This service should be created
    // by the root application injector.
    providedIn: 'root'
  })

  export class AdministrationService extends ModPromiseServiceBase<Administration> {

    private endPoint : string ;
    
    constructor(http: HttpClient,  private userService: CurrentUserService, private initialDataSources: DataSources) {
      super(http, environment.apiUrl, 'adminstration', Administration);
    }
  
    public get user(): any {
      return this.userService.user;
    }

    async load(userUpn: string) :Promise<any>  {
        let adminstrations= await this.http.get(url + '/api/' + this.endpoint + '/GetAdministration?search=' + userUpn).toPromise();
  
        return adminstrations as Administration[];
    }

    addAdministration(surrogate:Administration): Observable<Administration>{
      return from(this.create(surrogate));
    }

    updateAdministration(surrogate:Administration): Observable<Administration>{
      return from(this.create(surrogate));
    }

    loadAdministrationId(id: number): Observable<Administration> {         
        return from(this.get(id)); 
    }

    deleteAdministration(id:number){
      return from(this.delete(id));
    }

  }