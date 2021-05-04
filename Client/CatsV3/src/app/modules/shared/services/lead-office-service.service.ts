import { Injectable } from '@angular/core';
import { ModPromiseServiceBase } from 'mod-framework';
import { LeadOffice } from 'src/app/models/LeadOffice.model';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LeadOfficeServiceService  extends ModPromiseServiceBase<LeadOffice> {

  constructor(http: HttpClient) {
      super(http, environment.apiUrl, 'leadoffice', LeadOffice)
  }
}
