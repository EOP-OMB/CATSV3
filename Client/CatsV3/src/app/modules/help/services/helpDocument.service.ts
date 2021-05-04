import { Injectable } from '@angular/core';
import { ModPromiseServiceBase } from 'mod-framework';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { HelpDocument } from '../Models/helpDocument.model';

@Injectable({
  providedIn: 'root'
})
export class HelpDocumentService  extends ModPromiseServiceBase<HelpDocument> {

  constructor(http: HttpClient) {
      super(http, environment.apiUrl, 'helpDocument', HelpDocument)
  }
}


