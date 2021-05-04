import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of, Subject, forkJoin } from 'rxjs';
import { LeadOffice } from '../models/LeadOffice.model';
import { environment } from 'src/environments/environment';
import { DataSources } from '../modules/shared/interfaces/data-source';
import { LeadOfficeServiceService } from '../modules/shared/services/lead-office-service.service';
import { LetterTypeServiceService } from '../modules/shared/services/letter-type-service.service';
import { ExternauUsersServiceService } from '../modules/shared/services/externau-users-service.service';
import { CurrentBrowserUser } from '../models/currentBrowserUser.model';
import { LeadOfficeMember } from '../models/leadOfficeMember.model';
import { CurrentUserService, UserInfo } from 'mod-framework';
import { Role } from '../models/role.model';
import { SurrogateOriginatorService } from './surrogateOriginator.service';
import { Surrogate } from '../models/surrogate.model';
import { MatDialog } from '@angular/material/dialog';
import { SurrogateReviewerService } from './surrogateReviewer.service';

const apiUrl = 'https://api.angularbootcamp.com';

export interface Employee {
  first_name: string;
  last_name: string;
  city: string;
  company: string;
  createdTime: string;
  departmentId: number
  dept:  string;
  displayName:  string;
  division:  string;
  ein:  string;
  emailAddress:  string;
  givenName:  string;
  id: number
  inactive: boolean
  inactiveDate: Date
  mailNickName:  string;
  managerEin:  string;
  middleName:  string;
  mobilePhone:  string;
  modifiedTime:  string;
  office:  string;
  officePhone:  string;
  postalCode:  string;
  preferredName:  string;
  reportsToEmployeeId: number
  samAccountName:  string;
  state:  string;
  streetAddress:  string;
  surname:  string;
  title:  string;
  type:  string;
  upn:  string;
}

@Injectable({
  // This service should be created
  // by the root application injector.
  providedIn: 'root'
})
export class MainLoaderService {

  public leadOffices: LeadOffice[] = [];
  public currentBrowserUser: CurrentBrowserUser;

  constructor(
    private http: HttpClient, 
    private userService: CurrentUserService,
    private initialDataSources: DataSources,
    private surrogateOriginatorService: SurrogateOriginatorService,
    private surrogateReviewerService: SurrogateReviewerService,
    ) {
      initialDataSources.externalUsers = [];
      initialDataSources.leadOffices = [];
      initialDataSources.letterTypes = [];
      initialDataSources.reviewRounds = [];
      initialDataSources.dlGroups = [];
      initialDataSources.currentBrowserUser = new CurrentBrowserUser();
    }

  public get user(): any {
    return this.userService.user;
  }

  async setCurrentUserMetadata():Promise<any>{

      //Get the Impersonated User
      const impersonationUser = JSON.parse(sessionStorage.getItem('impersonationUser'));
      const surrogateUser = JSON.parse(sessionStorage.getItem('surrogateUser'));
      let currentBrowserUser: any;
      let impersonationUserTitle: string;
      let currentActingRole: string;
      let mainActingUser: string = '';
      let mainActingUserUpn: string;
      let mainActingUserEmail: string;

      //reinitialize the Current browserUser: Will eventually become the logged or the impersonated user
      this.initialDataSources.currentBrowserUser = this.initialDataSources.reIntializeCurrentBrowserUser();
      

      //Set Current Browser User Surrogate, or Impersonated or as Logged User in order of existance 
      if(surrogateUser){
        currentActingRole = surrogateUser.currentActingRole;
        currentBrowserUser = surrogateUser;
        impersonationUserTitle = "Acting As " +  currentActingRole.toUpperCase() + ": <span class='impersonatedUser'> \"" + surrogateUser.displayName + "\"</span>";
        
        mainActingUser = currentBrowserUser.displayName; //impersonationUser ? impersonationUser.displayName : currentBrowserUser.displayName;//this.user.displayName;
        mainActingUserUpn =  currentBrowserUser.upn; //impersonationUser ? impersonationUser.upn : currentBrowserUser.upn; //this.user.upn;
        mainActingUserEmail = this.userService.user.emailAddress;

        if (impersonationUser){      
          impersonationUserTitle = "Viewing As IMPERSONATED USER:<span class='impersonatedUser'> \"" + impersonationUser.displayName + "\"</span>" + "<br>" + "Acting as " +  currentActingRole.toUpperCase() + " for: <span class='impersonatedUser'>  \"" + surrogateUser.displayName + "\"</span>";
        }        
      }
      else if(impersonationUser){
        currentActingRole = 'Impersonated User';      
        currentBrowserUser = impersonationUser;  
        mainActingUser = currentBrowserUser.displayName;
        mainActingUserUpn = currentBrowserUser.upn;
        mainActingUserEmail = this.userService.user.emailAddress;
        impersonationUserTitle = "Acting As " +  currentActingRole.toUpperCase() + ":<span class='impersonatedUser'> \"" + impersonationUser.displayName + "\"</span>"; 
      }
      else{
        currentActingRole = 'Logged User';
        currentBrowserUser = this.user; 
        mainActingUser = currentBrowserUser.displayName;
        mainActingUserUpn = currentBrowserUser.upn;
        mainActingUserEmail = this.userService.user.emailAddress;
        impersonationUserTitle = "";
      }
      
      //Get all the OMB users
      let OMBUsers: any[] = [];
      // this.initialDataSources.leadOffices.forEach(office =>{
      //   office.leadOfficeMembers.forEach(m => {
      //     let user = {upn: m.userUPN, displayName: m.userFullName, officeId:m.leadOfficeId, office: '', emailAddress:'', roleId: m.role?.id, role: m.role?.name, id:m.id};
      //     if (OMBUsers.indexOf(user)== -1){
      //       OMBUsers.push(user);
      //     }
      //   });
      // });

      this.initialDataSources.employees.forEach(emp =>{
        let user = {upn: emp.upn?.toLowerCase().trim(), displayName: emp.displayName, officeId: 0, office: emp.office, emailAddress:emp.emailAddress, roleId: 0, role:'', id:emp.id};
        if (OMBUsers.indexOf(user)== -1){
          OMBUsers.push(user);
        }
      });

      //add the DL group to the OMBUsers
      this.initialDataSources.dlGroups.forEach(g => {
        OMBUsers.push({upn: g.name, displayName: g.name, officeId:0, office: '', emailAddress:'', roleId: 0, role:'DL', id:g.id})
      });

      //remove duplicate
      OMBUsers =  OMBUsers.filter((v,i) => 
        OMBUsers.findIndex(item => item.upn == v.upn) === i
      );
      
      //Get the Current User Surrogate data
      const surrogatesO = await  this.surrogateOriginatorService.load(mainActingUserUpn.toLowerCase().trim());
      const OriSurrogates = surrogatesO.filter(u => u.surrogateUPN?.lastIndexOf(mainActingUserUpn.toLowerCase().trim()) != -1 && OMBUsers.some(omb => u.surrogateUPN.includes(omb.upn))).filter(u => OMBUsers.some(omb => u.catsUserUPN.includes(omb.upn)));
      const MyOriSurrogates = surrogatesO.filter(u => u.catsUserUPN?.lastIndexOf(mainActingUserUpn.toLowerCase().trim()) != -1).filter(u => OMBUsers.some(omb => u.surrogateUPN.includes(omb.upn)));
      const surrogatesR = await  this.surrogateReviewerService.load(mainActingUserUpn.toLowerCase().trim());
      const RevSurrogates = surrogatesR.filter(u => u.surrogateUPN?.lastIndexOf(mainActingUserUpn.toLowerCase().trim()) != -1).filter(u => OMBUsers.some(omb => u.catsUserUPN.includes(omb.upn)));
      const MyRevSurrogates = surrogatesR.filter(u => u.catsUserUPN?.lastIndexOf(mainActingUserUpn.toLowerCase().trim()) != -1).filter(u => OMBUsers.some(omb => u.surrogateUPN.includes(omb.upn)));      

      const currentBrowserUserLeadOffices = this.initialDataSources.leadOffices.filter(o => o.leadOfficeMembers.some(m => m.userUPN.toLowerCase().includes(mainActingUserUpn.toLowerCase())) || o.leadOfficeOfficeManagers.some(m => m.userUPN.toLowerCase().includes(mainActingUserUpn.toLowerCase()))); 
      //Current Browser user assigned offices list
      let memberGroupsCollection = currentBrowserUserLeadOffices.map(o => o.name);        
      let leadOfficeMembers = currentBrowserUserLeadOffices.map(x => x.leadOfficeMembers);
      let leadOfficeOfficeManagers = currentBrowserUserLeadOffices.map(x => x.leadOfficeOfficeManagers);
      
      let officeRoles: any[] = [];
      officeRoles = currentBrowserUserLeadOffices.map(x => {
        var rolesMembers: any[] = x.leadOfficeMembers.filter(y => y.userUPN.toLowerCase().trim().includes( mainActingUserUpn.toLowerCase().trim())).map(y => y.role).map(y => y.name);
        var rolesManagers: any[] = x.leadOfficeOfficeManagers.filter(y => y.userUPN.toLowerCase().trim().includes( mainActingUserUpn.toLowerCase().trim())).map(y => y.role).map(y => y.name);
        rolesMembers = rolesMembers.filter((element, i) => i === rolesMembers.indexOf(element));
        rolesManagers = rolesManagers.filter((element, i) => i === rolesManagers.indexOf(element));
        rolesMembers = [...rolesMembers, ...rolesManagers]
        return { office: x.name, roles: rolesMembers}
      })

      //current browser collegues
      let currentBrowserUserLeadOfficesMembers : any[] = [];
      leadOfficeMembers.forEach(x => {  
          x.forEach(m => {            
            currentBrowserUserLeadOfficesMembers.push(m);
          });
      });
      leadOfficeOfficeManagers.forEach(x => {  
          x.forEach(m => {            
            currentBrowserUserLeadOfficesMembers.push(m);
          });
      });

      //Current Browser user external assigned offices list (assigned as originators)
      let currentBrowserUserExternalLeadOffices : string[] = currentBrowserUserLeadOfficesMembers.filter(x => x.userUPN?.trim().toLowerCase() == mainActingUserUpn.trim().toLowerCase() && x.externalLeadOfficeIds != '')?.map(x => x.externalLeadOfficeIds);
      currentBrowserUserExternalLeadOffices = this.initialDataSources.leadOffices.filter(x => currentBrowserUserExternalLeadOffices?.indexOf(x.id.toString()) != -1).map(o => o.name); 
      
      //set the OMBUsers
      OMBUsers = OMBUsers.sort((a,b) => a.displayName?.localeCompare(b.displayName));//sort by displayName
      this.initialDataSources.allOMBUsers = OMBUsers;
      //set the currentBrowserUser
      this.initialDataSources.currentBrowserUser.id = 0
      this.initialDataSources.currentBrowserUser.LoginName = this.user.upn;//currentBrowserUser.upn.trim();      
      this.initialDataSources.currentBrowserUser.PreferredName = this.user.displayName;
      this.initialDataSources.currentBrowserUser.Email = this.userService.user.emailAddress;
      this.initialDataSources.currentBrowserUser.CurrentActingRole = currentActingRole;
      this.initialDataSources.currentBrowserUser.SurrogateOriginator = OriSurrogates.map(u =>  {return {upn:u.catsUserUPN, displayName: u.catsUser, id:u.id}; }).sort((a, b) => a.displayName < b.displayName ? -1 : a.displayName > b.displayName ? 1 : 0);
      this.initialDataSources.currentBrowserUser.MySurrogateOriginator = MyOriSurrogates.map(u =>  {return {upn:u.surrogateUPN, displayName: u.surrogate, id:u.id}; }).sort((a, b) => a.displayName < b.displayName ? -1 : a.displayName > b.displayName ? 1 : 0);
      this.initialDataSources.currentBrowserUser.SurrogateReviewer = RevSurrogates.map(u =>  {return {upn:u.catsUserUPN, displayName: u.catsUser, id:u.id}; }).sort((a, b) => a.displayName < b.displayName ? -1 : a.displayName > b.displayName ? 1 : 0);
      this.initialDataSources.currentBrowserUser.MySurrogateReviewer = MyRevSurrogates.map(u =>  {return {upn:u.surrogateUPN, displayName: u.surrogate, id:u.id}; }).sort((a, b) => a.displayName < b.displayName ? -1 : a.displayName > b.displayName ? 1 : 0);
      this.initialDataSources.currentBrowserUser.MemberRoleCollection = officeRoles;
      this.initialDataSources.currentBrowserUser.MemberGroupsCollection = memberGroupsCollection;
      this.initialDataSources.currentBrowserUser.MemberCATSOfficeManagerCollection = officeRoles.filter(x => x.roles?.some(r => r?.toLowerCase().includes('manager')))?.map(x => x.office)
      this.initialDataSources.currentBrowserUser.MemberExternalGroupsCollection = currentBrowserUserExternalLeadOffices;
      this.initialDataSources.currentBrowserUser.ImpersonationActive = impersonationUser != null ? true : false;
      this.initialDataSources.currentBrowserUser.ImpersonationUserLabel = impersonationUserTitle;
      this.initialDataSources.currentBrowserUser.MainActingUserPreferedName = mainActingUser;
      this.initialDataSources.currentBrowserUser.MainActingUserUPN = mainActingUserUpn?.toLowerCase().trim();
      this.initialDataSources.currentBrowserUser.MainActingUserEmail = mainActingUserEmail;

       //set Current User Colleagues
       let colleagues = []
       this.initialDataSources.leadOffices.filter(o => this.initialDataSources.currentBrowserUser.MemberGroupsCollection.indexOf(o.name.trim()) != -1)
       .map(x => x.leadOfficeMembers)
       .forEach(x => {x.forEach(d => {colleagues.push(d)})});
       this.initialDataSources.leadOffices.filter(o => this.initialDataSources.currentBrowserUser.MemberGroupsCollection.indexOf(o.name.trim()) != -1)
       .map(x => x.leadOfficeOfficeManagers)
       .forEach(x => {x.forEach(d => {colleagues.push(d)})});       
      this.initialDataSources.currentBrowserUser.Mycolleagues = colleagues.filter((v,i,a)=>a.findIndex(t=>(t.userUPN  === v.userUPN ))===i).sort((a, b) => a.userFullName < b.userFullName ? -1 : a.userFullName > b.userFullName ? 1 : 0);

      const userGroups = this.initialDataSources.currentBrowserUser.MemberGroupsCollection;

      if (officeRoles.some(o => o.roles.some(r => r.trim().toUpperCase().includes('ADMINISTRATOR')))){
        this.initialDataSources.currentBrowserUser.MemberOfAdmins = true;
      }

      
      if (officeRoles.some(o => o.roles.some(r => r.trim().toUpperCase().includes('SUPPORT')))){
        this.initialDataSources.currentBrowserUser.MemberOfCATSSupport = true;
      }
      //Access the Report
      if (officeRoles.some(o => o.roles.some(r => r.trim().toUpperCase().includes('REPORT MANAGER')))){
        this.initialDataSources.currentBrowserUser.MemberOfCATSReportsAdmins = true;
      }

      if (this.initialDataSources.currentBrowserUser.MemberCATSOfficeManagerCollection.length > 0){
        this.initialDataSources.currentBrowserUser.MemberOfCATSOfficeManagers = true;
      }
      
      //Access to Correspondence Dashboard
      if (userGroups.indexOf("CORRESPONDENCE") != -1){
        this.initialDataSources.currentBrowserUser.MemberOfCATSCorrespondenceUnitTeam = true;
      }
      else if (userGroups.indexOf("LA") != -1){
        this.initialDataSources.currentBrowserUser.MemberOfCATSCorrespondenceReadOnly = true;
      }

      
      return null;
     
  }

  getLeadOffices(): LeadOffice[]{
    return this.initialDataSources.leadOffices;
  }
}