import { DtoBase } from 'mod-framework';
import { Role } from './role.model';
import { Surrogate } from './surrogate.model';

export class CurrentBrowserUser extends DtoBase {
    id:number
    LoginName: string = ''
    PreferredName: string = ''
    Phone: string = ''
    Email: string = ''
    Office: string = ''
    OfficeAcronym: string = ''
    MemberRoleCollection: any[] = []
    MemberGroupsCollection: string[] = []
    MemberExternalGroupsCollection: string[] = []
    MemberCATSOfficeManagerCollection: string[] = []
    MemberOfDevelopers: boolean = false
    MemberOfAdmins: boolean = false
    MemberOfCATSSupport: boolean = false    
    MemberOfCATSReportsAdmins: boolean = false
    MemberOfCATSOfficeManagers: boolean = false
    MemberOfCATSOriginators: boolean = false
    MemberOfCATSOriginatorSurrogate: boolean = false
    MemberOfCATSReviewers: boolean = false
    MemberOfCATSReviewerSurrogate: boolean = false
    MemberOfCATSCorrespondenceUnitTeam: boolean = false
    MemberOfCATSCorrespondenceReadOnly: boolean = false
    MemberOfCATSFYIUsers: boolean = false
    MemberOfCATSCopiedUsers: boolean = false
    SurrogateOriginator: any[] = []
    MySurrogateOriginator: any[] = []
    SurrogateReviewer: any[] = []
    MySurrogateReviewer: any[] = []
    Mycolleagues: any[] = []
    SurrogateActive: boolean = false;
    ImpersonationActive: boolean = false;
    ImpersonationUserLabel: string = '';
    MainActingUserPreferedName: string;
    MainActingUserUPN: string;
    MainActingUserEmail: string;
    SharePointUserID: number = 0;
    CurrentActingRole: string = '';
}