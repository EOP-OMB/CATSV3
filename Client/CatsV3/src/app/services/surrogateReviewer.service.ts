import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, Subject, forkJoin, from } from 'rxjs';
import { DataSources } from '../modules/shared/interfaces/data-source';
import { CurrentUserService, UserInfo, ModPromiseServiceBase } from 'mod-framework';
import { environment } from 'src/environments/environment';
import { Surrogate } from '../models/surrogate.model';

const url = environment.apiUrl ;

@Injectable({
    // This service should be created
    // by the root application injector.
    providedIn: 'root'
  })

  export class SurrogateReviewerService extends ModPromiseServiceBase<Surrogate> {

    private endPoint : string ;
    
    constructor(http: HttpClient,  private userService: CurrentUserService, private initialDataSources: DataSources) {
      super(http, environment.apiUrl, 'SurrogateReviewer', Surrogate);
    }
  
    public get user(): any {
      return this.userService.user;
    }

    async load(userUpn: string) :Promise<any>  {
        let surrogateReviewers = await this.http.get(url + '/api/' + this.endpoint + '/GetSurrogate?search=' + userUpn).toPromise();
  
        return surrogateReviewers as Surrogate[];
    }

    addUpdateSurrogate(surrogate:Surrogate): Observable<Surrogate[]>{
      return this.http.get<Surrogate[]>(environment.apiUrl + '/api/' + this.endpoint + '/updatecreate' ); 
    }

    addSurrogate(surrogate:Surrogate): Observable<Surrogate>{
      return from(this.create(surrogate));
    }

    updateSurrogate(surrogate:Surrogate): Observable<Surrogate>{
      return from(this.create(surrogate));
    }

    loadSurrogateById(id: number): Observable<Surrogate> {         
        return from(this.get(id)); 
    }

    deleteSurrogate(id:number){
      return from(this.delete(id));
    }

  }