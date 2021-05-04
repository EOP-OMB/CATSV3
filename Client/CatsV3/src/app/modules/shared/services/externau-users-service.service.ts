import { Injectable } from '@angular/core';
import { ModPromiseServiceBase } from 'mod-framework';
import { ExternalUser } from 'src/app/models/externauUser.model';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ExternauUsersServiceService  extends ModPromiseServiceBase<ExternalUser> {

  constructor(http: HttpClient) {
      super(http, environment.apiUrl, 'externaluser', ExternalUser)
  }
}

