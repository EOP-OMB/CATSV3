import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, Subject, forkJoin, from } from 'rxjs';
import { DataSources } from '../modules/shared/interfaces/data-source';
import { CurrentUserService, UserInfo, ModPromiseServiceBase } from 'mod-framework';
import { environment } from 'src/environments/environment';
import { ReviewRound } from '../models/reviewRound.model';

const url = environment.apiUrl ;


@Injectable({
    // This service should be created
    // by the root application injector.
    providedIn: 'root'
  })

  export class ReviewRoundService extends ModPromiseServiceBase<ReviewRound> {

    constructor(http: HttpClient,  private userService: CurrentUserService, private initialDataSources: DataSources) {
      super(http, environment.apiUrl, 'reviewRound', ReviewRound);
      //this.httpClient = new HttpClient(handler);    
    }
  
    public get user(): UserInfo {
      return this.userService.user;
    }

    loadReviewRouds(filterValue: string, searchValue: string): Observable<ReviewRound[]> {
        
        return from(this.getAll());
        
      }

  }