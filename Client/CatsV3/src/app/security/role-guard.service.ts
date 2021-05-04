import { Injectable } from '@angular/core';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot, CanActivate } from '@angular/router';
import { CurrentUserService, UserInfo } from 'mod-framework';
import { DataSources } from '../modules/shared/interfaces/data-source';
import { LeadOffice } from '../models/LeadOffice.model';
import { Observable } from "rxjs/Rx";
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/take';
import { MainLoaderService } from '../services/main-loader-service';
import { MatDialog } from '@angular/material/dialog';
import { DialogAlertComponent } from '../modules/shared/components/dialog-alert/dialog-alert.component';
import { LeadOfficeMember } from '../models/leadOfficeMember.model';
import { AppConfigService } from '../services/AppConfigService';

@Injectable({
  providedIn: 'root'
})
export class RoleGuardService implements CanActivate {
  constructor(
      private router: Router,
      private userService: CurrentUserService,
      private appConfigService: AppConfigService,
      private mainsvc: MainLoaderService,
      private initialDataSources: DataSources,
      private dialog: MatDialog,
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {      
      
      if (state.url.includes('correspondence') && 
        this.initialDataSources.leadOffices.length > 0 &&   
        this.initialDataSources.currentBrowserUser.LoginName != "")
      {
        return this.getAuthorization(state.url);
      }
      else if (!state.url.includes('correspondence') && 
        this.initialDataSources.leadOffices.length > 0  && 
        this.initialDataSources.dlGroups.length > 0  && 
        this.initialDataSources.reviewRounds.length > 0 && 
        this.initialDataSources.currentBrowserUser.LoginName != "")
      {
        return this.getAuthorization(state.url);
      }
      else{

        return this.appConfigService.load(state.url).then(res => {
            if (!!state){
              return this.getAuthorization(state.url);
            }
            else{
              this.router.navigate(['/']);
            }
          return res;
        });
      }
  }

  getAuthorization(url: string):boolean{
       
      const routeUrl = this.router.url;
      const currentUser = this.userService.user;      
      const user = this.initialDataSources.currentBrowserUser;

      if (currentUser) {

        if (url?.toUpperCase().indexOf('CORRESPONDENCE') != -1){

            if ((user.MemberOfCATSCorrespondenceUnitTeam == true ||
                user.MemberOfAdmins == true ||
                user.MemberOfCATSCorrespondenceReadOnly == true)){
                return true;
            }
            else{
              // not authorised so return false
              this.showDialog('CORRESPONDENCE', url);   
              return false;
            }
        }
        else if (url?.toUpperCase().indexOf('ORIGINATOR') != -1){
            return true;
        }
        else if (url?.toUpperCase().indexOf('REVIEWER') != -1){
            return true;
        }
        else if (url?.toUpperCase().indexOf('ADMIN') != -1){
          if (user.MemberOfAdmins == true || user.MemberOfCATSSupport == true ){
              return true;
          }
          else if (url?.includes('/admin/document-operations')){
            this.showDialog('REPORTS', url); 
            return false;
          }
          else if (this.initialDataSources.currentBrowserUser.MemberOfCATSOfficeManagers){
            return true;          
          }
          else{
            this.showDialog('REPORTS', url); 
            return false;
          }
        }
        else if (url.toUpperCase().indexOf('REPORT') != -1){
          if (user.MemberOfAdmins == true || user.MemberOfCATSReportsAdmins == true){
              return true;
          }
          else{
            // not authorised so return false
            this.showDialog('REPORTS', url);      
            return false            
          }
        }
        else{
          return false
        }

    }
    else{
      // not authorised so return false
      const dialogRef = this.dialog.open(DialogAlertComponent, {
        width:'440px',
        data: 'Your session expired or not allowed to use this resource.\nPlease refersh your browser or contact the adminstrator'
      });
      this.router.navigate(['/forbidden'], { queryParams: { returnUrl: url }});
      return false;
    }
  }

  showDialog(resource: string, url: string){
     // not authorised so return false
     //const dialogRef = this.dialog.open(DialogAlertComponent, {
     //   width:'440px',
     //   data: 'You are not allowed to use this resource (' + resource + ').\nPlease contact the adminstrator'
     // });

    console.log('User does not have access to route -- redirecting...');
    // not logged in so redirect to home page with the return url
    //this.router.parseUrl('forbidden');
    this.router.navigate(['/forbidden']);//this.router.navigate(['/forbidden'], { queryParams: { returnUrl: url }});
  }
}
