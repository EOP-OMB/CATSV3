import { LetterType } from 'src/app/models/letterType.model';
import { LeadOffice } from 'src/app/models/LeadOffice.model';
import { ExternalUser } from 'src/app/models/externauUser.model';
import { Injectable } from '@angular/core';
import { CurrentBrowserUser } from 'src/app/models/currentBrowserUser.model';
import { ReviewRound } from 'src/app/models/reviewRound.model';
import { DLGroup } from 'src/app/models/dLGroup.model';
import { HelpDocument } from '../../help/Models/helpDocument.model';
import { Administration } from 'src/app/models/administration.model';
import { Role } from 'src/app/models/role.model';
import { Employee } from 'src/app/services/main-loader-service';

@Injectable()
export class DataSources {
     letterTypes: LetterType[] = [];
     leadOffices: LeadOffice[] = [];
     externalUsers:ExternalUser[] = [];
     employees: Employee[] = [];
     reviewRounds: ReviewRound[] = [];
     dlGroups: DLGroup[] = [];
     currentBrowserUser: CurrentBrowserUser = new CurrentBrowserUser();
     allOMBUsers: any[] = [];
     helpDocuments: HelpDocument[] = [];
     adminstrations: Administration[] = [];
     previousDashboard: string = '';
     currentDashboard: string = '';
     roles : Role[] = [];

     reIntializeCurrentBrowserUser():CurrentBrowserUser{
          return new CurrentBrowserUser();
     }

}


