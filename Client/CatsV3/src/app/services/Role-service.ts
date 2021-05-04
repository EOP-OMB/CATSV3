import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, Subject, forkJoin, from } from 'rxjs';
import { DataSources } from '../modules/shared/interfaces/data-source';
import { CurrentUserService, UserInfo, ModPromiseServiceBase } from 'mod-framework';
import { environment } from 'src/environments/environment';
import { Administration } from '../models/administration.model';
import { Role } from '../models/role.model';

const url = environment.apiUrl ;

@Injectable({
    // This service should be created
    // by the root application injector.
    providedIn: 'root'
  })

  export class RoleService extends ModPromiseServiceBase<Role> {

    private endPoint : string ;
    
    constructor(http: HttpClient,  private userService: CurrentUserService, private initialDataSources: DataSources) {
      super(http, environment.apiUrl, 'adminstration', Role);
    }
  
    public get user(): any {
      return this.userService.user;
    }

    async load(userUpn: string) :Promise<any>  {
        let roles = await this.http.get(url + '/api/' + this.endpoint + '/Role?search=' + userUpn).toPromise();
  
        return roles as Role[];
    }

    addAdministration(role:Role): Observable<Role>{
      return from(this.create(role));
    }

    updateAdministration(role:Role): Observable<Role>{
      return from(this.create(role));
    }

    loadAdministrationId(id: number): Observable<Role> {         
        return from(this.get(id)); 
    }

    deleteAdministration(id:number){
      return from(this.delete(id));
    }

  }