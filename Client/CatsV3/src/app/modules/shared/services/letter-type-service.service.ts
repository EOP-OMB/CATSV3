import { Injectable } from '@angular/core';
import { LetterType } from 'src/app/models/letterType.model';
import { ModPromiseServiceBase } from 'mod-framework';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LetterTypeServiceService extends ModPromiseServiceBase<LetterType> {

  constructor(http: HttpClient) {
      super(http, environment.apiUrl, 'lettertype', LetterType)
  }
}
