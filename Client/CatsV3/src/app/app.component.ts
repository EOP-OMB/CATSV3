import { Component, EventEmitter, Output, OnInit, ChangeDetectorRef } from '@angular/core';
import * as config from '../assets/menu-config.json';
import { ModSideMenuConfig } from 'mod-framework';;
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { filter, map } from "rxjs/operators";
import { MainLoaderService } from './services/main-loader-service';
import { DataSources } from './modules/shared/interfaces/data-source';
import { RoleGuardService } from './security/role-guard.service';
import { EventEmitterService } from './services/event-emitter.service';
import { checkSupportedBrowser, setIncomingDashboardInRootComponent } from './modules/shared/utilities/utility-functions';
import { MatDialog } from '@angular/material/dialog';
import { AppConfigService } from './services/AppConfigService';
import Swal from 'sweetalert2/dist/sweetalert2.all.js';
@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
    public menuConfig: ModSideMenuConfig;
    dashboardTitle: string = "";
    dashboardIconSource = "../../../../../assets/images/CDashIcon.png";
    toggleImpersonateTitle: string = "";
    actingUserRole: string = "";
    showPageIcon : boolean = true;
    enableColumnCofig: boolean = false;
    enableUserPanelfig: boolean = false;
    enableNewLetterButton: boolean = false;
    parentRoute : string;
    routePath : string;    

    inComingDashboard: string = '';
    
    public constructor(
        private router: Router,
        private cdr: ChangeDetectorRef,
        private route: ActivatedRoute,
        private dialog: MatDialog,
        private appConfigService: AppConfigService,
        private eventEmitterService: EventEmitterService,
        private mainLoaderService: MainLoaderService,
        private roleGuardService: RoleGuardService,
        public initialDataSources: DataSources) {
        this.menuConfig = (config as any).default;

        router.events.subscribe((url: any) =>        
        {
            if (url.url){
                if (url.urlAfterRedirects){    
                    this.dashboardTitle = url.urlAfterRedirects;   
                    this.dashboardTitle = this.dashboardTitle.replace("-"," ")   ;               
                    this.setIconSource();
                    //setIncomingDashboardInRootComponent(this.dashboardTitle, this.eventEmitterService);
                }
            }
        });
    }

    ngOnInit() { 

        //setting the incoming dashboard options  
        if (this.eventEmitterService.subsIncomingDashboard != undefined) {    
            this.eventEmitterService.subsIncomingDashboard = null;          
        }  
        this.eventEmitterService.subsIncomingDashboard = this.eventEmitterService.invokeIncomingDashboardFunction.subscribe((dashboardTitle:string) => {  
            if (dashboardTitle != ''){
                const previousDashboard = this.initialDataSources.previousDashboard;  
                this.initialDataSources.previousDashboard =  dashboardTitle;
                this.initialDataSources.currentDashboard =  dashboardTitle;  
                this.inComingDashboard = dashboardTitle;
                
                let surrogate: any = sessionStorage.getItem('surrogateUser');
                
                //remove surrogate option when user changes the dashboard
                if (previousDashboard != dashboardTitle && surrogate != null)      {
                    this.onSurrogateChangesDashboard(dashboardTitle, previousDashboard); 
                }  
                else{                 
                    //Dashboards RouteGuard checking if the current user is allowed
                    if (!this.roleGuardService.getAuthorization(dashboardTitle)){
                      this.router.navigateByUrl('/forbidden');
                    }                    
                }     
            }
        });

        const impersonationUser = JSON.parse(sessionStorage.getItem('impersonationUser'));
        const surrogateUser = JSON.parse(sessionStorage.getItem('surrogateUser'));

        this.actingUserRole = this.initialDataSources.currentBrowserUser.CurrentActingRole;

        if(surrogateUser){
            this.actingUserRole = surrogateUser.currentActingRole;            
            this.initialDataSources.currentBrowserUser.CurrentActingRole = this.actingUserRole;
            this.initialDataSources.currentBrowserUser.ImpersonationUserLabel = "Acting As " +  this.actingUserRole.toUpperCase() + ": <span class='impersonatedUser'> \"" + surrogateUser.displayName + "\"</span>";
            this.initialDataSources.currentBrowserUser.PreferredName = impersonationUser ? impersonationUser.displayName :surrogateUser.displayName;
    
            if (impersonationUser){  
                this.initialDataSources.currentBrowserUser.ImpersonationUserLabel = "Viewing As IMPERSONATED USER:<span class='impersonatedUser'> \"" + impersonationUser.displayName + "\"</span>" + "<br>" + "Acting as " +  this.actingUserRole.toUpperCase() + " for: <span class='impersonatedUser'>  \"" + surrogateUser.displayName + "\"</span>";
            } 
          }
          else if(impersonationUser){         
            this.actingUserRole = 'Impersonated User';      
            this.initialDataSources.currentBrowserUser.CurrentActingRole = this.actingUserRole;  
            this.initialDataSources.currentBrowserUser.ImpersonationUserLabel = "Acting As " +  this.actingUserRole.toUpperCase() + ": <span class='impersonatedUser'> \"" + impersonationUser.displayName ;
            this.initialDataSources.currentBrowserUser.PreferredName = impersonationUser.displayName;
        }
        else{
            this.initialDataSources.currentBrowserUser.CurrentActingRole = 'Logged User'
        } 
    
        //check if the browser is supported
        if (!checkSupportedBrowser(this.dialog, this.router)) return;
    }

    ngAfterViewChecked(){
        //your code to update the model
        this.cdr.detectChanges();
     }

    setIconSource() {//inComingDashboard.includes('correspondence') || inComingDashboard.includes('originator') || inComingDashboard.includes('reviewer')
        
        if (this.dashboardTitle.indexOf("correspondence") != -1){
            this.showPageIcon = true;
            this.dashboardIconSource = "../../../../../assets/images/CDashIcon.png";
            this.enableColumnCofig = true;
            this.enableNewLetterButton = true;
        
            if (this.initialDataSources.currentBrowserUser.MemberOfAdmins == true || this.initialDataSources.currentBrowserUser.MemberOfCATSSupport == true){
                this.enableUserPanelfig = true;
            }
            else{
                this.enableUserPanelfig = false;
            }    
        }
        else if (this.dashboardTitle.indexOf("originator") != -1){
            this.showPageIcon = true;
            this.dashboardIconSource = "../../../../../assets/images/ODashIcon.png";
            this.enableColumnCofig = true;   
            this.enableNewLetterButton = true;
            this.enableUserPanelfig = true;  
        
            // if (this.initialDataSources.currentBrowserUser.MemberOfAdmins == true || this.initialDataSources.currentBrowserUser.MemberOfCATSSupport == true){
            //     this.enableUserPanelfig = true;
            // }
            // else{
            //     this.enableUserPanelfig = false;
            // }                  
        }
        else if (this.dashboardTitle.indexOf("reviewer") != -1){
            this.showPageIcon = true;
            this.dashboardIconSource = "../../../../../assets/images/RDashIcon.png";  
            this.enableColumnCofig = true;   
            this.enableNewLetterButton = false; 
            this.enableUserPanelfig = true; 
        
            // if (this.initialDataSources.currentBrowserUser.MemberOfAdmins == true || this.initialDataSources.currentBrowserUser.MemberOfCATSSupport == true){
            //     this.enableUserPanelfig = true;
            // }
            // else{
            //     this.enableUserPanelfig = false;
            // }                 
        }
        else{
            this.showPageIcon = false;
            this.dashboardIconSource = "";
            this.inComingDashboard = "";
            this.enableColumnCofig = false;  
            this.enableNewLetterButton = false;  
            this.enableUserPanelfig = false;
        }
             
    }

    evaluateTitle(dashTitle){
        if (this.dashboardTitle.indexOf(dashTitle) != -1){
            return true;
        }
        else{
            return false;
        }
    }

    async onCancelImpersonation(){
        const impersonationUser = JSON.parse(sessionStorage.getItem('impersonationUser'));
        const surrogateUser = JSON.parse(sessionStorage.getItem('surrogateUser'));

        if (surrogateUser){
            this.clearSessionStorage("surrogateUser");
        }
        else{
            this.clearSessionStorage("impersonationUser");
            this.clearSessionStorage("surrogateUser");
        }

        let url: string = this.dashboardTitle.includes('forbidden') == true ? '/home' : this.router.url;
        this.appConfigService.load().then(res => {
            this.ngOnInit();
            this.router.navigateByUrl(url);
        }); 
    }
    // open the coluns settings panel
    onOpenColumnsSettingsClick(){
        this.eventEmitterService.onOpenColumnsSettingsClick();
    }

    //open the impersonation panel
    onOpenImpersonationSettingsClick(){
        this.eventEmitterService.onOpenImpersonationSettingsClick();
    }

    onCreateNewLetterClick(){
        this.eventEmitterService.onCreateNewLetterClick();
    }

    onUserOption(event: Event){
        let d = event;
    }

    onHelpOption(event: Event){
        let d = event;
        this.router.navigateByUrl('/help?selectedhelp=' + event)
    }

    onSearch(event: Event){

    }

    //Making sure that the surrogate session is cleared when user changes the Dashboard
    async onSurrogateChangesDashboard(inComingDashboard: string, previousDashboard = ''){
        
        if(inComingDashboard != previousDashboard ){
            this.clearSessionStorage('surrogateUser');
            this.clearSessionStorage('cATSSelectedDashboardOptions');
            
            this.appConfigService.load().then(res => {
                this.inComingDashboard = inComingDashboard;
                this.ngOnInit();
                                
                //Dashboards RouteGuard checking if the current user is allowed
                if (!this.roleGuardService.getAuthorization(this.parentRoute)){
                    this.router.navigateByUrl('/forbidden');
                }   
                else{
                    this.router.navigateByUrl(inComingDashboard);
                }
            });
        }
        else{
            this.inComingDashboard = inComingDashboard;
        }
        
    }

    clearSessionStorage(key: string){
        sessionStorage.removeItem(key);
    }
}

