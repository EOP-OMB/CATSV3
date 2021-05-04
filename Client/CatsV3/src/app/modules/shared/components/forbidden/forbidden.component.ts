import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AppConfigService } from 'src/app/services/AppConfigService';
import { MainLoaderService } from 'src/app/services/main-loader-service';
import { DataSources } from '../../interfaces/data-source';

@Component({
  selector: 'app-forbidden',
  templateUrl: './forbidden.component.html',
  styleUrls: ['./forbidden.component.scss']
})
export class ForbiddenComponent implements OnInit {

  actingUser: string = "";
  actingUserRole: string = ""
  isCorresponceTeam: boolean = false;
  isOriginator: boolean = false;
  isReviewer: boolean = false;
  isSurrogateOriginator: boolean = false;
  isSurrogateReviewer: boolean = false;

  constructor(private router: Router,
    private mainLoaderService: MainLoaderService,
    private appConfigService: AppConfigService,
    public initialDataSources: DataSources) { }


  ngOnInit(): void {
    this.actingUserRole = this.initialDataSources.currentBrowserUser.CurrentActingRole;
    this.actingUser = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;
    
    this.isCorresponceTeam = this.initialDataSources.currentBrowserUser.MemberOfCATSCorrespondenceUnitTeam || this.initialDataSources.currentBrowserUser.MemberOfCATSCorrespondenceReadOnly ? true : false;;
    this.isOriginator = this.initialDataSources.currentBrowserUser.MemberOfCATSOriginators;
    this.isSurrogateOriginator = this.initialDataSources.currentBrowserUser.MemberOfCATSOriginatorSurrogate;
    this.isReviewer = this.initialDataSources.currentBrowserUser.MemberOfCATSReviewers;
    this.isSurrogateReviewer = this.initialDataSources.currentBrowserUser.MemberOfCATSReviewerSurrogate;
  }

  onCancelImpersonation(){
      sessionStorage.removeItem('impersonationUser');
      let url: string = this.router.url.indexOf('forbidden') != -1 ? '/home' : this.router.url;
      this.appConfigService.load().then(res => {
        this.initialDataSources.currentBrowserUser.ImpersonationUserLabel = "";
        this.router.navigateByUrl(url);
      }); 
  }

}
