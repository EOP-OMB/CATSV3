import { Injectable } from '@angular/core';
import { ModPromiseServiceBase } from 'mod-framework';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { DLGroup } from 'src/app/models/dLGroup.model';

@Injectable({
  providedIn: 'root'
})
export class DlGroupsService  extends ModPromiseServiceBase<DLGroup> {

  constructor(http: HttpClient) {
      super(http, environment.apiUrl, 'dlgroup', DLGroup)
  }
}


