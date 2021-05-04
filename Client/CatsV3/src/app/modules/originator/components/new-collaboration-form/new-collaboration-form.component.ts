import { Component, OnInit, ViewChild, NgZone, ElementRef, Input } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { MatStepper, MatHorizontalStepper } from '@angular/material/stepper';
import {CdkTextareaAutosize} from '@angular/cdk/text-field';
import { take, startWith, map } from 'rxjs/operators';
import { MatSelect } from '@angular/material/select';
import { ThemePalette, ErrorStateMatcher } from '@angular/material/core';
import { DataSources } from 'src/app/modules/shared/interfaces/data-source';
import { OutPutOptionsSelected } from 'src/app/modules/shared/components/select-autocomplete/select-autocomplete.component';
import { STEPPER_GLOBAL_OPTIONS } from '@angular/cdk/stepper';
import { ActivatedRoute, Router } from '@angular/router';
import { CorrespondenceService } from 'src/app/modules/correspondence/services/correspondence-service';
import { Correspondence } from 'src/app/modules/correspondence/Models/correspondence.model';
import { DatePipe } from '@angular/common';
import { getUserRoles, getStringFromHtml, getMembersFullnames, getMembersUpns, getMembersUpnAndFullnames, CATSPackage } from 'src/app/modules/shared/utilities/utility-functions';
import { MatSlideToggle } from '@angular/material/slide-toggle';
import { DashboardsConfigurations } from 'src/app/modules/shared/Data/dashboards.configurations';
import { MatDialog } from '@angular/material/dialog';
import { DialogPromptComponent } from 'src/app/modules/shared/components/dialog-prompt/dialog-prompt.component';
import { Reviewer } from 'src/app/modules/reviewer/models/reviewer.model';
import { Originator } from '../../models/originator.model';
import { FYIUser } from '../../models/fyiuser.model';
import { HttpEventType } from '@angular/common/http';
import { Collaboration } from '../../models/collaboration.model';
import { CollaborationService } from 'src/app/modules/shared/services/collaboration-service';
import { OriginatorService } from '../../services/originator-service';
import { ReviewerService } from 'src/app/modules/reviewer/services/reviewer.service';
import { FyiUserService } from 'src/app/services/fyiUsers-service';
import { EventEmitterService } from 'src/app/services/event-emitter.service';
import { AppConfigService } from 'src/app/services/AppConfigService';
import Swal from 'sweetalert2';
@Component({
  selector: 'app-new-collaboration-form',
  templateUrl: './new-collaboration-form.component.html',
  styleUrls: ['./new-collaboration-form.component.scss'],
  providers: [DatePipe, {provide: STEPPER_GLOBAL_OPTIONS, useValue: {displayDefaultIndicatorType: false}}]
})
export class NewCollaborationFormComponent implements OnInit {

  catsPackageData: CATSPackage = new CATSPackage();
  correspodenceData : Correspondence = new Correspondence();
  
  //Initial previous Saved Data from DB
  initialReviewers: string[] = [];
  initialFyiUsers: string[] = [];
  initialOriginators: string[] = [];

  catsId: string = "";
  currentRound : string = "";
  completedRounds: string[] = [];
  reviewers: string[] = [];
  reviewersToRemind: string[] = [];
  completedReviewers: string[] = [];
  draftReviewers: string[] = [];
  areControlsEditable: boolean = true;
  isResponseRequired: boolean = true; //indicating that the item as been created by the Correspondence Team, not by the originator using new Collaboration  
  isLaunchRoundActivated: boolean = false;
  isLeadPending: boolean = false;
  isEditable : boolean = true;
  userRole : string = "";
  errors: string[] = [];

  isSummaryDocDetailsIconON: boolean  = true;
  isSummaryAttachmentIconON: boolean  = true;
  isSummaryMembersIconON: boolean  = true;
  isSummaryRoundSelectIconON: boolean  = true;
  isManageExistingUpdatable: boolean = false;
  
  columnsConfigurations :DashboardsConfigurations;

  //Users to be selected as Originator, Reviewers or FYI
  ombUsersfilteredOptions: Observable<string[]>;
  selectedOriginators: any[] = [] ;
  selectedOriginatorsObs: Observable<any[]>; 
  selectedReviewers: any[] = [] ;
  selectedReviewersObs: Observable<string[]>; 
  selectedFyiUsersObs: Observable<string[]>;  
  selectedFyiUsers: any[] = [] ;
  leadOfficeUsersOptionsObs : Observable<any[] | any>; 
  leadOfficeUsersOptions : any[] = []; 
  leadOfficeUsersOriginatorOptionsObs : Observable<any[] | any>; 
  leadOfficeUsersOriginatorOptions: any[] = [] ;
  leadOfficeUsersSelectedOptions : any[] = []; 
  leadOffices: string[] = [];

  letterTypeSelected: string = "";
  letterTypeSelectedHtmlContent: string = "";
  letterTypes: string[] = [];
  reviewRounds: string[] = [];
  reviewRoundsSelected: string[] = [];

  isLinear : boolean;
  documentDetailsFormGroup: FormGroup;
  instructionsAttachDocFormGroup: FormGroup;
  membersAssignmentFormGroup: FormGroup;
  membersAssignmentFormGroupHint: FormGroup;
  roundsSelectionFormGroup: FormGroup;
  roundsSelectionFormGroupHint: FormGroup;
  summaryForms: any[] = [];
  changeDetectionValue: number = 0;

  boilerplateSelectedValue: string = "No";

  //fileupload settings
  multiple: boolean = true;
  accept: string; 
  color: ThemePalette = 'primary';
  emptyAr: string[] = [];

  itemId: number = 0;  
  parentRoute: string;
  routePath : string = "";
  routeTitle: string = "";

  @ViewChild('autosize') autosize: CdkTextareaAutosize;
  @ViewChild('form1') form1:HTMLFormElement;
  @ViewChild('form2') form2:HTMLFormElement; 
  @ViewChild('form3') form3:HTMLFormElement; 
  @ViewChild('form4') form4:HTMLFormElement; 
  @ViewChild('form5') form5:HTMLFormElement; 
  @ViewChild('form6') form6:HTMLFormElement;   
  @ViewChild("doctype", {static: false}) doctype: MatSelect ;
  @ViewChild("leadingoffice", {static: false}) leadingoffice: MatSelect ;
  @ViewChild("boile", {static: false}) boile : MatSelect;
  @ViewChild("boiler", {static: false}) boiler : MatSlideToggle;
  @ViewChild("myoriginators", {static: false}) myoriginators : HTMLElement;
  @ViewChild("myreviewers", {static: false}) myreviewers : HTMLElement;
  @ViewChild("myfyiusers", {static: false}) myfyiusers : HTMLElement;
  @ViewChild("reviewdocumentcontrol", {static: false}) reviewdocumentfilecontrol : HTMLElement;
  @ViewChild("referencedocumentcontrol", {static: false}) referencedocumentfilecontrol : HTMLElement;  
  @ViewChild('stepper') stepper: MatHorizontalStepper;

  @Input() inComingColloborationId = 0;
  stepperSelectedStepIndex: number = 0;

  /** Returns a FormArray with the name 'formArray'. */
  //get formArray(): AbstractControl | null { return this.formGroup.get('formArray'); }

  constructor(
    private elementRef: ElementRef,
    private router: Router,
    private route: ActivatedRoute,
    private appConfigService: AppConfigService,
    private eventEmitterService: EventEmitterService,
    private correspondenceService: CorrespondenceService,
    private originatorService: OriginatorService,
    private reviewerService: ReviewerService,
    private fyiUserService: FyiUserService,
    private collaborationService: CollaborationService,
    public dialog: MatDialog,
    private _formBuilder: FormBuilder, 
    private ngZone: NgZone,
    private datePipe: DatePipe = new DatePipe('en-US'),
    private initialDataSources: DataSources) {
    
    //console.log('Called Constructor');
    this.route.queryParams.subscribe(params => {
        this.itemId = params['id'] ? params['id'] : 0;
    });

    this.route.parent?.url.subscribe((urlPath) => {
        this.parentRoute = urlPath[urlPath.length - 1].path;
    })

    this.route.url.subscribe(x => {
      this.routePath = x.length > 0 ?  x[0]?.path : "";
    });

    this.route.data.subscribe(x => {
      this.routeTitle = x ? x.title : "";
    });

    //set the stepper step to the last step when the component is called by a dialog from another component
    if (this.parentRoute == undefined){
      this.stepperSelectedStepIndex = 4;
    }

    this.isLinear = this.itemId == 0 ? false : true;//isLeadPending == false

    this.documentDetailsFormGroup = this._formBuilder.group({
      catsid: ['0000-0000-0000'],
      correspondentName: [''],
      letterSubject: ['', Validators.required],
      letterTypeName: ['', Validators.required],
      leadOfficeName: ['', Validators.required],
      boilerplate: ['', Validators.required]
    });

    this.instructionsAttachDocFormGroup = this._formBuilder.group({
      reviewDocuments: ['', Validators.required],
      referenceDocuments: [''],
      finalDocuments: [''],// this field is required only when the reviewer is finalizing the package
      summaryMaterialBackground: ['', Validators.required],
      reviewInstructions: [''],
      completedReviewers:['']
    });    
    
    this.roundsSelectionFormGroup = this._formBuilder.group({
      currentRoundEndDate: ['', Validators.required],
      currentReview:['', Validators.required],
    }); 

    this.roundsSelectionFormGroupHint = this._formBuilder.group({
      completedRounds:[this.emptyAr]
    }); 

    this.membersAssignmentFormGroup = this._formBuilder.group({
      originators: [this.emptyAr, Validators.required],
      reviewers: [this.emptyAr, Validators.required],
      fyiUsers: [this.emptyAr],
    }); 
    
    this.membersAssignmentFormGroupHint = this._formBuilder.group({
      completedUsers:[this.emptyAr],
      draftUsers:[this.emptyAr]
    }); 

  }

  async ngOnInit() {    

    //setting the incoming dashboard options  
    // send the dahsboard title to the root component (appComponent)
    this.setIncomingDashboardInRootComponent();

    if (this.initialDataSources.allOMBUsers == undefined || this.initialDataSources.allOMBUsers == null || 
      this.initialDataSources.currentBrowserUser.LoginName == '' || this.initialDataSources.currentBrowserUser.LoginName == undefined){
      await this.appConfigService.load();
    }

    //initialize the observable as they are serving as input to the 
    //child component SelectAutocompleteComponent

    let currentUserUpn : string = this.initialDataSources.currentBrowserUser.LoginName;
    this.leadOfficeUsersOptions = this.getMemberOptionsObjects(this.initialDataSources.allOMBUsers?.map(u => u.upn));
    this.leadOfficeUsersOptionsObs = of(this.leadOfficeUsersOptions);
    this.leadOfficeUsersOriginatorOptions = this.leadOfficeUsersOptions?.filter(u => u.upn.indexOf('DL-') == -1);
    this.leadOfficeUsersOriginatorOptionsObs = of(this.leadOfficeUsersOriginatorOptions);
    this.leadOffices = this.initialDataSources.leadOffices.map(x => x.name?.trim());
    this.letterTypes = this.initialDataSources.letterTypes.map(x => x.name);
    this.reviewRounds = this.initialDataSources.reviewRounds.map(x => x.name);
    this.membersAssignmentFormGroup.controls['originators'].setValue(currentUserUpn != undefined ?[currentUserUpn] : []); // making sure the current user is automatically an originator
    this.selectedOriginatorsObs = of(this.getMemberOptionsObjects(this.membersAssignmentFormGroup.controls['originators'].value))// of(this.membersAssignmentFormGroup.controls['originators'].value);
    this.selectedReviewersObs = of([]);
    this.selectedFyiUsersObs = of([]);


    this.columnsConfigurations = new DashboardsConfigurations(this.datePipe);
    
    this.ombUsersfilteredOptions =  this.documentDetailsFormGroup .controls['correspondentName'].valueChanges
      .pipe(
        startWith(''),
        map(value => this._filter(value? value: ''))
      );

    this.documentDetailsFormGroup.valueChanges.subscribe(val=>{
      //console.log(val);  

      if(!this.checkIfStepIsEmpty(val)){
        this.setConfirmDetails(val, this.form1) ;//this.setFormControls(this.form1);  
        this.changeDetectionValue += 1;   
        this.letterTypeSelected = this.documentDetailsFormGroup.controls['letterTypeName'] .value; 
        this.letterTypeSelectedHtmlContent =   this.initialDataSources.letterTypes.filter(x => x.name == this.letterTypeSelected ).map(x => x.htmlContent).join();
      }      
    });

    this.instructionsAttachDocFormGroup.valueChanges.subscribe(val=>{
      //console.log(val); 
      if(!this.checkIfStepIsEmpty(val)){
        this.setConfirmDetails(val, this.form2) ;//this.setFormControls(this.form2); 
        this.changeDetectionValue += 1;
      }
    }); 
    
    this.roundsSelectionFormGroup.valueChanges.subscribe(val=>{
      //console.log(val); 
      this.reviewRoundsSelected = val.currentReview ? val.currentReview : [];
      this.activateMoveRounds();
      this.setConfirmDetails(val, this.form4) ; 
      this.changeDetectionValue += 1;
    });    
    
    this.roundsSelectionFormGroupHint.valueChanges.subscribe(val=>{
      //console.log(val); 
      this.setConfirmDetails(val, this.form5) ; 
      this.changeDetectionValue += 1;
    });  

    this.membersAssignmentFormGroup.valueChanges.subscribe(val=>{
      //console.log(val);
      this.setConfirmDetails(val, this.form3) ;//this.setFormControls(this.form3); 
      this.changeDetectionValue += 1;

      if (this.routePath != ''){
        this.onActivate_ManageUpdate(val);
      }
    });

    this.membersAssignmentFormGroupHint.valueChanges.subscribe(val=>{
      //console.log(val);
      this.setConfirmDetails(val, this.form6) ; 
      this.changeDetectionValue += 1;
    });

    this.setEdit_Icon_Summary_Page();

    //inComingColloborationId is send as @input from the Correspondence Dashboard to view the review details
    if (this.inComingColloborationId > 0){
      this.itemId = this.inComingColloborationId;
    }

    if (this.itemId > 0 ){
      if (this.routePath == 'opending'){
        this.correspondenceService
        .loadCorrespondenceById(this.itemId)
        .subscribe(response => {
          this.catsPackageData.correspondence = response; 
            if(this.catsPackageData.correspondence.collaboration != null){
              this.catsPackageData.correspondence.collaboration.reviewers = this.catsPackageData.correspondence.collaboration.reviewers.filter(x => x.roundName == this.catsPackageData.correspondence.currentReview);
              this.catsPackageData.correspondence.collaboration.fYIUsers = this.catsPackageData.correspondence.collaboration.fYIUsers.filter(x => x.roundName == this.catsPackageData.correspondence.currentReview);
              this.catsPackageData.correspondence.collaboration.originators = this.catsPackageData.correspondence.collaboration.originators.filter(x => x.roundName == this.catsPackageData.correspondence.currentReview);
              Object.assign(this.catsPackageData.collaboration, response.collaboration);
            }

            if (this.parentRoute == 'reviewer'){
              let status = this.catsPackageData.correspondence.reviewStatus == 'Draft' || this.catsPackageData.correspondence.reviewStatus == 'Completed' ? this.catsPackageData.correspondence.reviewStatus : 'Not Completed';
              this.reviewerService
              .getCompletedReviewersWithOptions(this.catsPackageData.correspondence.catsid, this.catsPackageData.correspondence.currentReview, status)
              .subscribe(response => {
                  let reviewers = response;
                  this.catsPackageData.reviewers = reviewers;
                  this.catsPackageData.correspondence.collaboration = reviewers.find(x => x.collaboration.correspondence.id == this.catsPackageData.correspondence.id).collaboration;
                  this.catsPackageData.collaboration = reviewers.find(x => x.collaboration.correspondence.id == this.catsPackageData.correspondence.id).collaboration;
                  
                  this.loadDetailsOnInit();
              });
            }
            else{
              this.loadDetailsOnInit();
            }
            
        });
      }
      else{
        this.correspondenceService
        .loadCorrespondenceById(this.itemId)
        .subscribe(response => { 
            this.catsPackageData.correspondence = response;
            this.catsPackageData.correspondence.collaboration.reviewers = this.catsPackageData.correspondence.collaboration.reviewers.filter(x => x.roundName == this.catsPackageData.correspondence.currentReview);
            this.catsPackageData.correspondence.collaboration.fYIUsers = this.catsPackageData.correspondence.collaboration.fYIUsers.filter(x => x.roundName == this.catsPackageData.correspondence.currentReview);
            this.catsPackageData.correspondence.collaboration.originators = this.catsPackageData.correspondence.collaboration.originators.filter(x => x.roundName == this.catsPackageData.correspondence.currentReview);
            Object.assign(this.catsPackageData.collaboration, this.catsPackageData.correspondence.collaboration);

            if (this.parentRoute == 'reviewer'){
              let status: string = 'Not Completed';
              if(this.catsPackageData.correspondence?.reviewStatus.toLowerCase().trim().includes('draft') || this.catsPackageData.correspondence?.reviewStatus.toLowerCase().trim().includes('Completed')){
                status = this.catsPackageData.correspondence.reviewStatus
              }
              else if(this.catsPackageData.correspondence?.reviewStatus.toLowerCase().trim().includes('finished')){
                status = 'Completed'
              }
              this.loadDetailsOnInit();
              // this.reviewerService
              // .getCompletedReviewersWithOptions(this.catsPackageData.correspondence.catsid, this.catsPackageData.correspondence.currentReview, status)
              // .subscribe(response => {
              //     let reviewers = response;
              //     this.catsPackageData.reviewers = reviewers;
              //     this.catsPackageData.correspondence.collaboration = reviewers.find(x => x.collaboration.correspondence.id == this.catsPackageData.correspondence.id).collaboration;
              //     this.catsPackageData.collaboration = reviewers.find(x => x.collaboration.correspondence.id == this.catsPackageData.correspondence.id).collaboration;
                  
              //     this.loadDetailsOnInit();
              // });
            }
            else{
              this.loadDetailsOnInit();
            }
            
        });
      }
    }
    else{
      this.isLinear = true;      
      this.activateMoveRounds();
    }
    
  }    

  setIncomingDashboardInRootComponent(){
    this.eventEmitterService.onIncomingDashboard('');
  }

  ngAfterViewInit() {  
    
  } 

  ngAfterContentChecked(){
    //this helps to have  Boilerplate value assigned to the summary form by default. otherwise it cames as empty value
    if (this.itemId == 0){
      this.documentDetailsFormGroup.controls['boilerplate'].setValue('No');
    }    
  }

  ngAfterViewChecked(){
    var el = this.elementRef.nativeElement.querySelectorAll('.error-nav');
    if(el.length > 0) {
      el?.forEach(element => {
        const onclick = element.getAttribute('listener');//making sure that the handler is not registered again
        if (onclick != 'true'){
          const step = element.getAttribute('step');
          element.setAttribute('listener', 'true');
          element.addEventListener('click', this.onsetSelecetedStep.bind(this, Number(step)));
        }        
      });
    } 
  }

  checkIfControlInvalid(frm: FormGroup, controlname: string): boolean{
    return frm.controls[controlname].valid;
  }

  setErrorListFormInvalid(formName: string){
    this.errors = [];
    
    if (this.routeTitle == "Collaboration Details"){
      return;
    }   

    //if (formName == 'documentDetailsFormGroup'){
      for (const field in this.documentDetailsFormGroup.controls) { // 'field' is a string
        const control = this.documentDetailsFormGroup.get(field); // 'control' is a FormControl  
        let label = this.columnsConfigurations.getColumnsLabel(field,'correspondence') ;
          
        if (field == 'boilerplate'){
          label = "Boilerplate";
        }

        if (control.invalid){
          this.errors.push("<span class='error-nav' step='0'>" + label + "</span>" );
        }
      }

    //if (formName == 'instructionsAttachDocFormGroup'){
      for (const field in this.instructionsAttachDocFormGroup.controls) { // 'field' is a string
        const control = this.instructionsAttachDocFormGroup.get(field); // 'control' is a FormControl 
        let label = this.columnsConfigurations.getColumnsLabel(field,'correspondence') ;

        if (field == 'summaryMaterialBackground' || field == 'reviewInstructions'){
          label = this.columnsConfigurations.getColumnsLabel(field,'originator');
        } 

        if (control.invalid){
          this.errors.push("<span class='error-nav' step='1'>" + label + "</span>" );
        }
      }
    //}

    //if (formName == 'roundsSelectionFormGroup'){
      for (const field in this.roundsSelectionFormGroup.controls) { // 'field' is a string
      const control = this.roundsSelectionFormGroup.get(field); // 'control' is a FormControl 
      const label = this.columnsConfigurations.getColumnsLabel(field,'originator') 
        if (control.invalid){
          this.errors.push("<span class='error-nav' step='3'>" + label + "</span>" );
        }
      }
    //}

    //if (formName == 'membersAssignmentFormGroup'){
      for (const field in this.membersAssignmentFormGroup.controls) { // 'field' is a string
        const control = this.membersAssignmentFormGroup.get(field); // 'control' is a FormControl 
        const label = this.columnsConfigurations.getColumnsLabel(field,'correspondence') ;
        if (control.invalid){
          this.errors.push("<span class='error-nav' step='2'>" + label + "</span>" );
        }
      }
    //}
  }


  checkIfStepIsEmpty(frmValue: any): boolean{
    let isempty : boolean = true;
    Object.keys(frmValue)?.forEach(key => {
      
      if (Array.isArray(frmValue[key])){
        if (frmValue[key].length > 0){
          isempty = false;
        }
      }
      else{
        if (frmValue[key] != ''){
          isempty = false;
        }
      }
    });   

    return isempty;
  }

  activateMoveRounds(){
    this.isLaunchRoundActivated = this.routeTitle?.toLowerCase() == 'next round' ? true : false; //this.itemId == 0 || this.itemId == undefined || this.currentRound == '' || this.reviewRoundsSelected?.join() != this.currentRound ? true: false;
  }

  loadDetailsOnInit(){

    //this.details.disable();//Disable the form for all the userss except LA and Correspondence Team
    //bind the control

    this.catsPackageData.correspondence = getUserRoles(this.catsPackageData.correspondence,this.initialDataSources.currentBrowserUser);
    
    this.documentDetailsFormGroup.patchValue({
      catsid: this.catsPackageData.correspondence.catsid,
      correspondentName:this.catsPackageData.correspondence.correspondentName,
      letterSubject:this.catsPackageData.correspondence.letterSubject,
      letterTypeName:this.catsPackageData.correspondence.letterTypeName,
      leadOfficeName: this.catsPackageData.correspondence.leadOfficeName,
      boilerplate: this.catsPackageData.collaboration?.boilerPlate != undefined ? this.catsPackageData.collaboration?.boilerPlate?.trim() : 'No',
    }); 
    
    this.instructionsAttachDocFormGroup.patchValue({
      reviewDocuments: getStringFromHtml(this.catsPackageData.correspondence.reviewDocuments) != '' ? this.catsPackageData.correspondence.reviewDocuments : '',
      referenceDocuments:getStringFromHtml(this.catsPackageData.correspondence.referenceDocuments) ? this.catsPackageData.correspondence.referenceDocuments : '',
      finalDocuments:getStringFromHtml(this.catsPackageData.correspondence.finalDocuments) ? this.catsPackageData.correspondence.finalDocuments : '',
      summaryMaterialBackground:this.catsPackageData.collaboration?.summaryMaterialBackground != undefined ? this.catsPackageData.collaboration?.summaryMaterialBackground : '' ,
      reviewInstructions: this.catsPackageData.collaboration?.reviewInstructions != undefined ? this.catsPackageData.collaboration?.reviewInstructions: ''
    }); 

    this.roundsSelectionFormGroup.patchValue({
      currentRoundEndDate: this.catsPackageData.collaboration?.currentRoundEndDate != undefined? this.datePipe.transform(new Date(this.catsPackageData.collaboration?.currentRoundEndDate) ,'yyyy-MM-dd'): ''  ,
      currentReview: this.catsPackageData.correspondence.currentReview ? this.catsPackageData.correspondence.currentReview?.trim() == 'NONE' || this.catsPackageData.correspondence.currentReview?.trim() == '' ? [] : this.catsPackageData.correspondence.currentReview?.split(','): [],
    }); 

    this.roundsSelectionFormGroupHint.patchValue({
      completedRounds:this.catsPackageData.collaboration?.completedRounds != undefined ? this.catsPackageData.collaboration?.completedRounds?.split(','): []
    }); 

    //check for inactive users, but previously assigned to the item
    if (this.catsPackageData.collaboration.currentOriginatorsIds != '')
      this.checkIfSelectedUsersActive(this.catsPackageData.collaboration.currentOriginatorsIds?.split(';'), this.leadOfficeUsersOriginatorOptions, 'originators');
    if (this.catsPackageData.collaboration.currentReviewersIds != '')
      this.checkIfSelectedUsersActive(this.catsPackageData.collaboration.currentReviewersIds?.split(';'), this.leadOfficeUsersSelectedOptions, 'reviewers');
    if (this.catsPackageData.collaboration.currentFYIUsersIds != '')        
      this.checkIfSelectedUsersActive(this.catsPackageData.collaboration.currentFYIUsersIds?.split(';'), this.leadOfficeUsersSelectedOptions,'fyiusers');
    
    this.membersAssignmentFormGroup.patchValue({
      originators: this.catsPackageData.collaboration.currentOriginatorsIds != undefined && this.catsPackageData.collaboration.currentOriginatorsIds?.trim() != '' ? this.catsPackageData.collaboration.currentOriginatorsIds?.split(';'): [this.initialDataSources.currentBrowserUser.MainActingUserUPN] ,
      reviewers:this.catsPackageData.collaboration.currentReviewersIds != undefined && this.routePath != 'olaunch' ? this.catsPackageData.collaboration.currentReviewersIds?.split(';'): [] ,
      fyiUsers:this.catsPackageData.collaboration?.currentFYIUsersIds  != undefined && this.routePath != 'olaunch' ? this.catsPackageData.collaboration?.currentFYIUsersIds?.split(';'): [],
    });
    
    this.membersAssignmentFormGroupHint.patchValue({
      completedUsers:this.catsPackageData.collaboration?.completedReviewersIds != undefined ? this.catsPackageData.collaboration?.completedReviewersIds?.split(';'): [],
      draftUsers:this.catsPackageData.collaboration?.draftReviewersIds != undefined ? this.catsPackageData.collaboration?.draftReviewersIds?.split(';'): []
    });

    this.catsId = this.catsPackageData.correspondence.catsid;
    this.userRole = this.catsPackageData.correspondence.officeRole;
    this.isResponseRequired = this.catsPackageData.correspondence.isPendingLeadOffice ? this.catsPackageData.correspondence.isPendingLeadOffice : true;
    this.isLeadPending = this.catsPackageData.correspondence.isPendingLeadOffice;
    this.currentRound = this.catsPackageData.correspondence.currentReview;
    this.reviewers = this.catsPackageData.correspondence.isUnderReview != true ? [] :  this.catsPackageData.correspondence.collaboration.reviewers.map(r => r.reviewerUPN);

    
    this.reviewersToRemind =this.catsPackageData.correspondence.isUnderReview != true ? [] : this.catsPackageData.correspondence.collaboration.completedReviewersIdsCount == 0 ? this.reviewers : 
    this.catsPackageData.correspondence.collaboration.reviewers.map(r => r.reviewerUPN).filter(r => this.catsPackageData.correspondence.collaboration.completedReviewersIds.includes(r.trim()) == false &&  this.catsPackageData.correspondence.collaboration.draftReviewersIds.includes(r.trim()) == false);  
    
    if (this.routePath != 'opending'){
      this.completedRounds = this.catsPackageData.collaboration ? this.catsPackageData.collaboration.completedRounds? this.catsPackageData.collaboration.completedRounds.split(','):[] : [];
      this.completedReviewers = this.catsPackageData.collaboration ? this.catsPackageData.collaboration.completedReviewers? this.catsPackageData.collaboration.completedReviewers.split(';'):[] : [];
      this.draftReviewers =  this.catsPackageData.collaboration ? this.catsPackageData.collaboration.draftReviewers? this.catsPackageData.collaboration.draftReviewersIds.split(';'):[] : [];
      this.reviewers = this.catsPackageData.collaboration ? this.catsPackageData.collaboration.currentReviewers? this.catsPackageData.collaboration.currentReviewers.split(';'):[] : [];
      if (this.routePath == 'olaunch'){
        //Initial previous Saved Data
        this.initialOriginators = this.initialFyiUsers = this.initialReviewers = [];
      }  
      else{//Initial previous Saved Data
        this.initialReviewers = this.catsPackageData.collaboration?.currentReviewersIds?.split(';');
        this.initialFyiUsers = this.catsPackageData.collaboration?.currentFYIUsersIds?.split(';');
        this.initialOriginators = this.catsPackageData.collaboration?.currentOriginatorsIds?.split(';');
      } 
    } 

    //Set Members (Originator,Reviewer, FYI) select controls data sources
    this.selectedOriginatorsObs = of(this.getMemberOptionsObjects(this.membersAssignmentFormGroup.controls['originators'].value));//of(this.membersAssignmentFormGroup.controls['originators'].value);
    this.selectedReviewersObs = of(this.getMemberOptionsObjects(this.membersAssignmentFormGroup.controls['reviewers'].value));//of(this.membersAssignmentFormGroup.controls['reviewers'].value);
    this.selectedFyiUsersObs = of(this.getMemberOptionsObjects(this.membersAssignmentFormGroup.controls['fyiUsers'].value));//of(this.membersAssignmentFormGroup.controls['fyiUsers'].value);

    this.reviewRoundsSelected = this.roundsSelectionFormGroup.controls['currentReview'].value;
    
    this.activateMoveRounds();

    //make few control no edItable based on item status
    this.setControls_Editable();

    this.setStepperDefautSettings();
  }

  getMemberOptionsObjects(options: any[]): any{
    return this.initialDataSources.allOMBUsers.filter(u => options?.includes(u.upn) == true)
  }

  checkIfSelectedUsersActive(selectedusers: any[], usersoptions, currentAssignedUsers: string){
    selectedusers?.forEach( u => {
      if (!usersoptions.some(x => x == u)){
        var upn: string = u;
        var name = "(INACTIVE) "
        if (currentAssignedUsers == 'originators'){
          name +=  this.catsPackageData.collaboration.originators.find(x => x?.originatorUpn?.toLowerCase().trim().includes(upn?.toLowerCase().trim()))?.originatorName;
        }
        else if (currentAssignedUsers == 'reviewers'){
          name +=  this.catsPackageData.collaboration.reviewers.find(x => x?.reviewerUPN?.toLowerCase().trim().includes(upn?.toLowerCase().trim()))?.reviewerName;
        }
        else{
          name +=  this.catsPackageData.collaboration.fYIUsers.find(x => x?.fyiUpn?.toLowerCase().trim().includes(upn?.toLowerCase().trim()))?.fYIUserName;
        }
        let user = {upn: upn?.toLowerCase().trim(), displayName: name, officeId: 0, office: this.catsPackageData.correspondence.leadOfficeName, emailAddress:'', roleId: 0, role:'', id:0};
        //adding inactive user to the option list
        if(!this.initialDataSources.allOMBUsers.some(x => x.upn.toLowerCase() == user.upn.toLowerCase())){
          this.initialDataSources.allOMBUsers.push(user);
        }          
      }
    });
  }

  setStepperDefautSettings(){

    let stepperStartIndex = this.stepper?.steps.length - 1;

    //mark the stepper as touched to activate the completed icon
    let index = 0;
    this.stepper?.steps?.forEach(step =>{

      step.interacted = true;
      //step.editable = true;
      //step.completed = true;
      if (this.routePath?.toLowerCase() == "manage"){
        if (step.label.indexOf('Members') != -1){
          stepperStartIndex = index;
        }
        else{
          this.isEditable = false;
        }
      }
      else if (this.routePath?.toLowerCase() == "olaunch"){
        //reset the two forms
        this.resetRoundsSelectionForm();
        this.resetMembersForm();
        //this.selectedOriginatorsObs = of([]);
        this.selectedReviewersObs = of([]);
        this.selectedFyiUsersObs = of([]);

        if (step.label.toLowerCase().indexOf('members') != -1){
          this.isEditable = true;
          step.interacted = true;
          step.stepControl.markAllAsTouched();
        }
        else if (step.label.toLowerCase().indexOf('rounds selection') != -1){
          stepperStartIndex = index;
          step.interacted = true;
          this.isEditable = true;
          step.stepControl.markAllAsTouched();
        }
        else if (step.label.toLowerCase().indexOf('confirm') != -1){
          step.interacted = false;
        }
        else{
          this.isEditable = false;
          step.completed = true;
        }
      }
      else if (this.routePath?.toLowerCase() == "ocollaborationdetails"){
        this.isEditable = false;
        step.interacted = true;
        step.completed = true;
        this.isLinear = true;

      }
      else if (this.routePath?.toLowerCase() == "opending"){
        
        stepperStartIndex = this.stepper?.steps.length - 1;
        if (step.label.toLowerCase().indexOf('documents') != -1){
          this.isEditable = false;
          step.interacted = true;
        }
        else{
          this.isEditable = true;
          step.completed = false;
        }

      }  
      else if (this.parentRoute?.toLowerCase() == "reviewer"){ 
        if (this.routePath == 'rmanage' && step.label.toLowerCase().indexOf('documents') != -1 && this.catsPackageData.correspondence.currentReview.toLowerCase()  == "final packaging"){
          stepperStartIndex = index;          
          this.instructionsAttachDocFormGroup.controls['finalDocuments'].setValidators([Validators.required]);
          this.instructionsAttachDocFormGroup.controls["finalDocuments"].updateValueAndValidity(); 
        } 
        step.interacted = true;        
        step.completed = true;       
        this.isEditable = false;
      }        
      else if (this.parentRoute == undefined){ 
        this.isEditable = false;
        stepperStartIndex = this.stepper.steps.length - 1;
      } 

      index++;
    });

    //move to the desired step  
    this.onsetSelecetedStep(stepperStartIndex);
  }

  setEdit_Icon_Summary_Page(){
    if (this.routeTitle?.toLowerCase() == "manage existing"){
    }
    else if (this.routeTitle?.toLowerCase() == "next round"){
      this.isSummaryAttachmentIconON = false;
      this.isSummaryDocDetailsIconON = false;
      this.isSummaryRoundSelectIconON = true;
      this.isSummaryMembersIconON = true;
    }
    else if (this.routeTitle?.toLowerCase() == "collaboration details"){
      
      this.isSummaryAttachmentIconON = false;
      this.isSummaryDocDetailsIconON = false;
      this.isSummaryRoundSelectIconON = false;
      this.isSummaryMembersIconON = false;
    }
    else if (this.routeTitle?.toLowerCase() == "pending"){      
      this.isSummaryAttachmentIconON = true;
      this.isSummaryDocDetailsIconON = false;
      this.isSummaryRoundSelectIconON = false;
      this.isSummaryMembersIconON = false;
    }
    else if (this.parentRoute?.toLowerCase() == "reviewer"){
      this.isSummaryAttachmentIconON = false;
      this.isSummaryDocDetailsIconON = false;
      this.isSummaryRoundSelectIconON = false;
      this.isSummaryMembersIconON = false;
    }  
    else if (this.parentRoute == undefined || this.parentRoute?.toLowerCase() == ""){
      this.isSummaryAttachmentIconON = false;
      this.isSummaryDocDetailsIconON = false;
      this.isSummaryRoundSelectIconON = false;
      this.isSummaryMembersIconON = false;
    } 
    else{        
      this.isSummaryAttachmentIconON = true;
      this.isSummaryDocDetailsIconON = true;
      this.isSummaryRoundSelectIconON = true;
      this.isSummaryMembersIconON = true;
    }    
  }

  resetMembersForm(){
    //this.membersAssignmentFormGroup.controls['originators'].reset();
    this.membersAssignmentFormGroup.controls['reviewers'].reset();
    this.membersAssignmentFormGroup.controls['fyiUsers'].reset();
  }

  resetRoundsSelectionForm(){
    this.roundsSelectionFormGroup.controls['currentRoundEndDate'].reset();
    this.roundsSelectionFormGroup.controls['currentReview'].reset();
  }

  setControls_Editable(){    
    
    

    if (this.itemId == 0){
      this.areControlsEditable = true;
    }
    else if (this.isResponseRequired || this.isLeadPending){
      this.areControlsEditable = false;
    }
    else{
      this.areControlsEditable = true;
    }
  }

  // help to auto resizing the textarea when the content expands
  triggerResize() {
    // Wait for changes to be applied, then trigger textarea resize.
    this.ngZone.onStable.pipe(take(1))
        .subscribe(() => this.autosize.resizeToFitContent(true));
  }

  private _filter(value: string): string[] {
    const filterValue = value.toLowerCase();

    return this.initialDataSources.allOMBUsers.map(u => u.displayName).sort().filter(option => option.toLowerCase().includes(filterValue));
  }

  setConfirmDetails(frmValue: any, frm: HTMLFormElement, errors: string[] = []){

    let arFrm : any[]= [];

    if (frm == undefined) return;       
    
    const isHint = frm.nativeElement.id.indexOf('Hint') != -1;

    let isShowEditIcon: boolean = true;
    
    if (frm.nativeElement.id == 'documentDetailsFormGroup'){
      isShowEditIcon = this.isSummaryDocDetailsIconON;
      Object.keys(frmValue)?.forEach(key => {
        //console.log('key=' + key + '  value=' + frmValue[key]);
        let ctr = { 
          label: '', 
          text: '',
          title:'',
          isValid: this.checkIfControlInvalid(this.documentDetailsFormGroup,key)
        };

        ctr.label = this.columnsConfigurations.getColumnsLabel(key,'correspondence') + ":";
        
        if (key == 'boilerplate'){
          ctr.label = "Boilerplate?:";
        } 

        ctr.text = frmValue[key];
        ctr.title = frmValue[key];
        
        if (key == 'letterSubject' && ctr.text?.length > 30){
          ctr.text = '<textarea style=\'width:100%;background-color: transparent;border: none\'>' + frmValue[key] + '</textarea>';
        }

        arFrm.push(ctr);

      })      
    }
    else if (frm.nativeElement.id == 'instructionsAttachDocFormGroup'){
        isShowEditIcon = this.isSummaryAttachmentIconON;
        Object.keys(frmValue)?.forEach(key => {
          //console.log('key=' + key + '  value=' + frmValue[key]);
          let ctr = { 
            label: '', 
            text: '',
            title:'',
            isValid: this.checkIfControlInvalid(this.instructionsAttachDocFormGroup,key)
          };

          if (key == 'referenceDocuments' || key == 'reviewDocuments' || key == 'finalDocuments'){
            ctr.label = this.columnsConfigurations.getColumnsLabel(key,'correspondence') + ":";
          } 
          else if (key == 'summaryMaterialBackground' || key == 'reviewInstructions'){
            ctr.label = this.columnsConfigurations.getColumnsLabel(key,'originator') + ":";
          }
          
          if (Array.isArray(frmValue[key])){
            ctr.text = frmValue[key].map(file => file.name).join('<br\>');
            ctr.title = frmValue[key].map(file => file.name).join('\n');
          }
          else{
            ctr.text = frmValue[key];            
            ctr.title = frmValue[key];
            if ((key == 'summaryMaterialBackground' || key == 'reviewInstructions' && ctr.text?.length > 30)){
              ctr.text = '<textarea style=\'width:100%;background-color: transparent;border: none\'>' + frmValue[key] + '</textarea>';
            }
          }

          arFrm.push(ctr);
      });
    }
    else if (frm.nativeElement.id.indexOf('membersAssignmentFormGroup') != -1){
      isShowEditIcon = this.isSummaryMembersIconON; 
      Object.keys(frmValue)?.forEach(key => {
        //console.log('key=' + key + '  value=' + frmValue[key]);
        let ctr = { 
          label: '', 
          text: '',
          title:'',
          isValid: this.checkIfControlInvalid(isHint == true ? this.membersAssignmentFormGroupHint : this.membersAssignmentFormGroup,key)
        };

        ctr.label = ctr.label = this.columnsConfigurations.getColumnsLabel(key,'correspondence') + ":"; 

        ctr.text = frmValue[key] ? getMembersFullnames(frmValue[key], this.initialDataSources.allOMBUsers).join('<br\>') : '';
        ctr.title = frmValue[key] ? getMembersFullnames(frmValue[key], this.initialDataSources.allOMBUsers).join('\n') : '';

        if (key == "completedUsers"){
          ctr.text = "<span class='completedUsers'>" + ctr.text + "</span>"
        }

        if (ctr.text?.trim() != '')
          if (getStringFromHtml(ctr.text?.trim()) != '')
            arFrm.push(ctr);
      });
    }
    else if (frm.nativeElement.id.indexOf('roundsSelectionFormGroup') != -1){
      isShowEditIcon = this.isSummaryRoundSelectIconON;
      Object.keys(frmValue)?.forEach(key => {
        //console.log('key=' + key + '  value=' + frmValue[key]);
        let ctr = { 
          label: '', 
          text: '',
          title:'',
          isValid: this.checkIfControlInvalid(isHint == true ? this.roundsSelectionFormGroupHint : this.roundsSelectionFormGroup, key)
        };

        ctr.label = ctr.label = this.columnsConfigurations.getColumnsLabel(key,'originator') + ":";

        if (key == 'currentRoundEndDate'){
          ctr.label = ctr.label = this.columnsConfigurations.getColumnsLabel(key,'originator') + ":";
          if (frmValue[key] == '' || frmValue[key] == null){
            ctr.isValid = false;
          }
          else{
            frmValue[key] = this.datePipe.transform(frmValue[key] ,'MM/dd/yyyy');
          }
        }      
        
        if (Array.isArray(frmValue[key])){

          if (key == "completedRounds"){
            let res : string[] = [];
            frmValue[key]?.forEach(element => {
              res.push("<span round='" + element + "' catsId='" + this.catsPackageData.correspondence?.catsid + "' title='" + element + "' class='completedRounds'>" + element + "</span>");
            });
            ctr.text = res.join('<br\>');
          }
          else{
            ctr.text = frmValue[key].join('<br\>');
            ctr.title = frmValue[key].join('\n');
          }
          
        }
        else{
          ctr.text = frmValue[key] ?  frmValue[key].split(',').join('<br\>') : '';
          ctr.title = frmValue[key] ?  frmValue[key].split(',').join('\n') : '';
        }

        if (ctr.text?.trim() != '')
          arFrm.push(ctr);
      });
    }   

    //get the list of from with errors
    this.setErrorListFormInvalid(frm.nativeElement.id);

    let x = { form: frm.nativeElement.title, controls: arFrm, isInValid: arFrm.some(c => c.isValid == false), isShowEditIconON: isShowEditIcon };

    let index = this.summaryForms.findIndex(f => f.form.indexOf(x.form.replace('Hint','').trim()) != -1);
    
    if (index == -1){
      this.summaryForms.push(x);
    }
    else{
      if (!isHint){
        this.summaryForms.splice(index, 1, x);
      }
      else{
        //merging two formgroups
        let mainform = this.summaryForms.filter(f => f.form?.indexOf(x.form.replace('Hint','').trim()) != -1);
        if(mainform != undefined){
          let d = mainform[0];
          d.controls = d.controls.concat(x.controls);
          this.summaryForms.splice(index, 1, d);
        }
        
      }
      
    }

  }

  onTransactionCancel(stepControl: any = null){
    if (this.routeTitle == "Pending"){
      this.onsetSelecetedStep(this.stepper.steps.length - 1);
    }
    else{
      this.router.navigateByUrl('/' + this.parentRoute);
    }    
  }
    
  onChangeSlideToggle(event){
    let d = event.checked;
    if (event.checked){
      this.boilerplateSelectedValue = "Yes";
    }
    else{
      this.boilerplateSelectedValue = "No";
    }    
    this.documentDetailsFormGroup.controls['boilerplate'].setValue(this.boilerplateSelectedValue);
  }

  onChildOptionValueChange(outPutOptionsSelected: OutPutOptionsSelected){
    var emitedObject = outPutOptionsSelected;
    var selectedValues =  emitedObject.selectedOptions as string[];
    this.membersAssignmentFormGroup.controls[emitedObject.source].setValue(selectedValues);
  }

  onsetSelecetedStep(index: number){
    this.stepper.linear = false; 
       
    //this.stepper.selectedIndex = index;      
    this.stepperSelectedStepIndex = index;
    setTimeout(() => {
       this.stepper.linear = this.isLinear;
    });
  }

  onStepReset(){
    this.summaryForms = [];
    this.stepper.reset();
    this.boiler.checked = false;
    this.documentDetailsFormGroup.controls['boilerplate'].setValue('No');
  }

  onManageExistingItemUpdate(){
    //call the service TO INSERT OR UPDATE with files attachments 
    this.collaborationService.updateCATSPackage(this.catsPackageData)
    .subscribe(async event => {
      
      switch (event.type) {
        case HttpEventType.Sent:
          //console.log('Request sent!');
          break;
        case HttpEventType.ResponseHeader:
          //console.log('Response header received!');
          break;
        case  HttpEventType.UploadProgress:
          //this.progress = Math.round(100 * event.loaded / event.total);
          //console.log('uploading!' + Math.round(100 * event.loaded / event.total));

          break;
        case HttpEventType.DownloadProgress:
          const kbLoaded = Math.round(event.loaded / 1024);
          //console.log(`Download in progress! ${ kbLoaded }Kb loaded`);
          break;
        case HttpEventType.Response:
          console.log('😺 Done!', event.body);
          this.correspodenceData =  event.body as Correspondence;
          
          //this.ngOnInit();
          if (this.routePath.toLowerCase().includes('manage') && this.parentRoute.toLowerCase().includes('originator')){
            this.router.navigateByUrl('/originator/manage?id=' + this.itemId);
          }
          else if (this.parentRoute.toLowerCase().includes('originator')){
            this.router.navigateByUrl('/originator');
          }
          else if (this.parentRoute.toLowerCase().includes('reviewer')){
            
            //send final files to the Archived emailbox as attachments
            if (this.catsPackageData.finalFiles?.length > 0 && this.catsPackageData.correspondence.letterStatus?.toLowerCase().includes('closed')){
              //this.correspondenceService.sendAttachedEmails(this.catsPackageData.correspondence, this.catsPackageData.finalFiles);
            }
            this.router.navigateByUrl('/reviewer');
          }
          else{
            this.router.navigateByUrl('/originator');
          }
          
      }
    }) ;
  }

  onPackageReturned(){
    this.updateCorrespondenceForReturn();
  }

  onPackageLaunchReview(){    
    
    this.setCorrespondence();

    if (this.catsPackageData.collaboration == null){
      this.setNewCollabotation();
    }
    else{
      this.setExistingCollabotation();
    }

    this.setMembersAssignment_Originators(this.membersAssignmentFormGroup.controls['originators'].value,[]);
    this.setMembersAssignment_Reviewers(this.membersAssignmentFormGroup.controls['reviewers'].value,[]);
    this.setMembersAssignment_FyiUsers(this.membersAssignmentFormGroup.controls['fyiUsers'].value,[]);
    
    this.onManageExistingItemUpdate();
  } 

  onReviewerSendsReviewDraft(){
    this.catsPackageData.correspondence.catsNotificationId = 6;//return draft is like adding a reviewer
    this.catsPackageData.correspondence.isEmailElligible = true;
    this.setCorrespondence();
    this.setExistingCollabotation();
    this.setMembersAssignment_Originators(this.catsPackageData.collaboration.currentOriginatorsIds.split(';'), []);
    this.setMembersAssignment_Reviewers([this.initialDataSources.currentBrowserUser.MainActingUserUPN], []);
    this.onManageExistingItemUpdate();
  } 

  onPackageReviewFinalizedOrCompleted(){
    this.catsPackageData.correspondence.catsNotificationId = 8;//Review completed/finalized
    this.catsPackageData.correspondence.isEmailElligible = true;
    this.setCorrespondence();
    this.setExistingCollabotation();
    this.setMembersAssignment_Originators(this.catsPackageData.collaboration.currentOriginatorsIds.split(';'), []);
    this.setMembersAssignment_Reviewers([this.initialDataSources.currentBrowserUser.MainActingUserUPN], []);
    this.onManageExistingItemUpdate();
  }

  onOriginatorSendsReviewReminder(user){
    this.catsPackageData.correspondence.catsNotificationId = 14;//send a reminder email notification to the assigned reviewer
    this.catsPackageData.correspondence.isEmailElligible = true;
    this.setMembersAssignment_Reviewers([user], []);
    this.onManageExistingItemUpdate();

    if (this.catsPackageData.correspondence.catsNotificationId == 14){
      let u: any = this.initialDataSources.allOMBUsers?.filter(x => x.upn == user);
      let msg = u[0] ? 'A reminder notification has been sent successfully to \"' + u[0]?.displayName  + '\"': 'A reminder notification has been sent successfully' ;
      Swal.fire('Hey!', msg, 'info');
    }
  }

  onOriginatorSendBackReviewDraft(user){
    let myUser = this.getMembersUpnAndFullnames(user);
    this.setMembersAssignment_Originators(this.membersAssignmentFormGroup.controls['originators'].value,[]);
    this.catsPackageData.correspondence.catsNotificationId = 6;//return draft is like adding a reviewer
    this.catsPackageData.correspondence.isEmailElligible = true;
    this.catsPackageData.collaboration.draftReviewers = this.catsPackageData.collaboration.draftReviewers.split(';').filter(u => u != myUser?.displayName).join(';');
    this.catsPackageData.collaboration.draftReviewersIds = this.catsPackageData.collaboration.draftReviewersIds.split(';').filter(u => u != myUser?.upn).join(';');
    this.catsPackageData.collaboration.draftReviewers;
    this.catsPackageData.collaboration.draftReviewersIds;

    this.setCorrespondence();
    this.setExistingCollabotation();

    this.setMembersAssignment_Reviewers([user], []);

    this.onManageExistingItemUpdate();

    let u: any = this.initialDataSources.allOMBUsers?.filter(x => x.upn == user);
    let msg = u[0] ? 'The draft has been sent back to \"' + u[0]?.displayName  + '\"': 'The draft has been sent back' ;
    Swal.fire('Hey!', msg, 'info');

  }

  updateCorrespondenceForReturn(){

    let message : string = "You are about to RETURN this item. This process cannot be undone. Please provide the reasons and click OK to proceed or Cancel.";
   
    const dialogRef = this.dialog.open(DialogPromptComponent, {
      width: '500px',
      data: {
        name: this.initialDataSources.currentBrowserUser.PreferredName + ",",
        label: "Reasons for Returning The item: " + this.catsPackageData.correspondence.catsid,
        title: message,
        isConfirmOnly: false,
        isReopening: false,
        noThanksLabel: "Cancel"
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      //console.log('The dialog was closed');
      if (result != undefined && result?.trim() != "No"){
        let emptyFiles: File[] = [];
        
        this.catsPackageData.correspondence.rejected = true;
        this.catsPackageData.correspondence.isLetterRequired = true;    
        this.catsPackageData.correspondence.isUnderReview = false;
        this.catsPackageData.correspondence.isEmailElligible = true;
        this.catsPackageData.correspondence.isPendingLeadOffice = false;
        this.catsPackageData.correspondence.catsNotificationId = 2; // Pending item Rejected by the Lead Office
        this.catsPackageData.correspondence.rejectionReason = result?.trim();
        this.catsPackageData.correspondence.rejectedLeadOffices = this.catsPackageData.correspondence.rejectedLeadOffices + ',' + this.catsPackageData.correspondence.letterTypeName;

        //call the service TO INSERT OR UPDATE with files attachments 
        this.correspondenceService.updateMyCorrespondence(this.catsPackageData.correspondence, emptyFiles)
        .subscribe(async event => {
          this.router.navigateByUrl('/originator');
        }) ;
      } 
    });   
    
  }

  setCorrespondence(){
    var emptyFiles: File[] = [];
    this.catsPackageData.reviewFiles = this.instructionsAttachDocFormGroup.get("reviewDocuments").value && Object.prototype.toString.call(this.instructionsAttachDocFormGroup.get("reviewDocuments").value) == "[object Array]" ?  this.instructionsAttachDocFormGroup.get("reviewDocuments").value : emptyFiles; 
    this.catsPackageData.referenceFiles = this.instructionsAttachDocFormGroup.get("referenceDocuments").value  && Object.prototype.toString.call(this.instructionsAttachDocFormGroup.get("referenceDocuments").value) == "[object Array]" ?  this.instructionsAttachDocFormGroup.get("referenceDocuments").value : emptyFiles;     
    this.catsPackageData.finalFiles = this.instructionsAttachDocFormGroup.get("finalDocuments").value && Object.prototype.toString.call(this.instructionsAttachDocFormGroup.get("finalDocuments").value) == "[object Array]" ?  this.instructionsAttachDocFormGroup.get("finalDocuments").value : emptyFiles; 
    
    this.catsPackageData.correspondence.currentUserUPN = this.initialDataSources.currentBrowserUser.MainActingUserUPN;
    this.catsPackageData.correspondence.currentUserEmail = this.initialDataSources.currentBrowserUser.MainActingUserEmail;
    this.catsPackageData.correspondence.currentUserFullName = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;
    this.catsPackageData.correspondence.isDeleted = false;

    if(this.itemId > 0){
      this.catsPackageData.correspondence.catsNotificationId = 4;//Launch next round
      this.catsPackageData.correspondence.reviewStatus = 'In Progress';
      this.catsPackageData.correspondence.isPendingLeadOffice = false;
      this.catsPackageData.correspondence.isUnderReview = true;
      this.catsPackageData.correspondence.isEmailElligible = true;
      this.catsPackageData.correspondence.currentReview = this.roundsSelectionFormGroup.controls['currentReview'].value.join(',');

      //Closing the package by the reviewer
      if(this.parentRoute == "reviewer" && this.routePath == 'rmanage'){
        this.catsPackageData.correspondence.catsNotificationId = this.catsPackageData.correspondence.currentReview.toLowerCase() == 'final packaging' ? 10 : 8;// Reviewer Completes the round or closes the package        
        this.catsPackageData.correspondence.reviewStatus = this.catsPackageData.correspondence.currentReview.toLowerCase() == 'final packaging' ? 'Finalized' : 'Completed';
        if(this.catsPackageData.correspondence.currentReview.toLowerCase() == 'final packaging'){
          this.catsPackageData.correspondence.letterStatus = 'Closed';
          this.catsPackageData.correspondence.isUnderReview = false;
        }          
      } 
      else if(this.parentRoute == "reviewer" && this.routePath == 'rdraft'){
        this.catsPackageData.correspondence.catsNotificationId = 9;//Reviewer sends draft to the originators        
        this.catsPackageData.correspondence.reviewStatus = 'Draft';
      }
    }
    else{
      let newCorrespondence: Correspondence = new Correspondence();
      newCorrespondence.catsNotificationId = 4;
      newCorrespondence.correspondentName = this.documentDetailsFormGroup.controls['correspondentName'].value;      
      newCorrespondence.letterSubject = this.documentDetailsFormGroup.controls['letterSubject'].value;
      newCorrespondence.letterTypeName =  this.documentDetailsFormGroup.controls['letterTypeName'].value;
      newCorrespondence.currentReview = this.roundsSelectionFormGroup.controls['currentReview'].value.join(',');
      newCorrespondence.letterStatus = 'Open';
      newCorrespondence.reviewStatus = 'In progress';
      newCorrespondence.fiscalYear = new Date().getFullYear().toString();
      newCorrespondence.isEmailElligible  = true;
      newCorrespondence.isFinalDocument = false;
      newCorrespondence.isLetterRequired = false;
      newCorrespondence.isPendingLeadOffice = false;
      newCorrespondence.isDeleted = false;
      newCorrespondence.isSaved = false;
      newCorrespondence.isUnderReview = true;
      newCorrespondence.copiedOfficeName = '';
      newCorrespondence.notRequiredReason = 'New Collaboration created by the Lead Office\'s Originator: ' + this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;
      newCorrespondence.leadOfficeName = this.documentDetailsFormGroup.controls['leadOfficeName'].value;
      newCorrespondence.leadOfficeUsersDisplayNames = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;
      newCorrespondence.modifiedBy = this.initialDataSources.currentBrowserUser.LoginName;
      newCorrespondence.modifiedTime = new Date();
      newCorrespondence.leadOfficeUsersIds = this.initialDataSources.currentBrowserUser.LoginName;
      newCorrespondence.createdBy = this.initialDataSources.currentBrowserUser.LoginName;
      newCorrespondence.createdTime = new Date();
      newCorrespondence.letterReceiptDate = newCorrespondence.letterDate =  new Date();

      this.catsPackageData.correspondence = newCorrespondence;
    }    

  }

  

  setNewCollabotation(){
    let newCollaboration: Collaboration = new Collaboration();
    
    newCollaboration.boilerPlate = this.documentDetailsFormGroup.controls['boilerplate'].value;
    newCollaboration.catsid = this.documentDetailsFormGroup.controls['catsid'].value;//this.catsPackageData.correspondence.catsid;
    newCollaboration.correspondenceId = this.catsPackageData.correspondence.id.toString();
    newCollaboration.currentRoundEndDate = new Date(this.roundsSelectionFormGroup.controls['currentRoundEndDate'].value);
    newCollaboration.currentRoundStartDate = new Date();
    newCollaboration.currentFYIUsers = getMembersFullnames(this.membersAssignmentFormGroup.controls['fyiUsers'].value, this.initialDataSources.allOMBUsers).join(';');
    newCollaboration.currentFYIUsersIds = this.membersAssignmentFormGroup.controls['fyiUsers'].value.join(';');
    newCollaboration.modifiedBy = this.initialDataSources.currentBrowserUser.LoginName;
    newCollaboration.modifiedTime = new Date();
    newCollaboration.currentOriginators = getMembersFullnames(this.membersAssignmentFormGroup.controls['originators'].value, this.initialDataSources.allOMBUsers).join(';');
    newCollaboration.currentOriginatorsIds = this.membersAssignmentFormGroup.controls['originators'].value.join(';');
    newCollaboration.currentReviewers = getMembersFullnames(this.membersAssignmentFormGroup.controls['reviewers'].value, this.initialDataSources.allOMBUsers).join(';');
    newCollaboration.currentReviewersIds = this.membersAssignmentFormGroup.controls['reviewers'].value.join(';');
    newCollaboration.reviewInstructions = this.instructionsAttachDocFormGroup.controls['reviewInstructions'].value;
    newCollaboration.summaryMaterialBackground = this.instructionsAttachDocFormGroup.controls['summaryMaterialBackground'].value;//Closing the package by the reviewer 
      
    newCollaboration.completedRounds += this.catsPackageData.collaboration.completedRounds == '' || this.catsPackageData.collaboration.completedRounds == undefined ? this.catsPackageData.correspondence.currentReview : "," + this.catsPackageData.correspondence.currentReview ;      
    newCollaboration.currentRoundEndDate = new Date(this.roundsSelectionFormGroup.controls['currentRoundEndDate'].value);
    newCollaboration.currentRoundStartDate = new Date();

    this.catsPackageData.collaboration = newCollaboration;
  }

  setExistingCollabotation(){
    let updatedCollaboration: Collaboration = this.catsPackageData.collaboration;
    updatedCollaboration.catsid = updatedCollaboration.catsid == '' ? this.documentDetailsFormGroup.controls['catsid'].value : updatedCollaboration.catsid;
    updatedCollaboration.modifiedBy = this.initialDataSources.currentBrowserUser.LoginName;
    updatedCollaboration.modifiedTime = new Date();
    updatedCollaboration.currentOriginators = getMembersFullnames(this.membersAssignmentFormGroup.controls['originators'].value, this.initialDataSources.allOMBUsers).join(';');
    updatedCollaboration.currentOriginatorsIds = this.membersAssignmentFormGroup.controls['originators'].value.join(';');
    updatedCollaboration.currentReviewers = getMembersFullnames(this.membersAssignmentFormGroup.controls['reviewers'].value, this.initialDataSources.allOMBUsers).join(';');
    updatedCollaboration.currentReviewersIds = this.membersAssignmentFormGroup.controls['reviewers'].value.join(';');
    updatedCollaboration.reviewInstructions = this.instructionsAttachDocFormGroup.controls['reviewInstructions'].value;
    updatedCollaboration.summaryMaterialBackground = this.instructionsAttachDocFormGroup.controls['summaryMaterialBackground'].value;//Closing the package by the reviewer
    
    //round review drafted by the reviewer
    if(this.parentRoute == "reviewer" && this.routePath == 'rdraft'){
      updatedCollaboration.draftReviewers = this.catsPackageData.collaboration.draftReviewers == '' ? this.initialDataSources.currentBrowserUser.MainActingUserPreferedName : this.catsPackageData.collaboration.draftReviewers + ";" + this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;
      updatedCollaboration.draftReviewersIds = this.catsPackageData.collaboration.draftReviewersIds == '' ? this.initialDataSources.currentBrowserUser.MainActingUserUPN : this.catsPackageData.collaboration.draftReviewersIds + ";" + this.initialDataSources.currentBrowserUser.MainActingUserUPN;
    }
    //round cleared/completed by the reviewer
    else if (this.parentRoute == "reviewer"){      
      updatedCollaboration.completedReviewers = this.catsPackageData.collaboration.completedReviewers == '' ? this.initialDataSources.currentBrowserUser.MainActingUserPreferedName :  this.catsPackageData.collaboration.completedReviewers + ";" + this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;
      updatedCollaboration.completedReviewersIds = this.catsPackageData.collaboration.completedReviewersIds == '' ? this.initialDataSources.currentBrowserUser.MainActingUserUPN : this.catsPackageData.collaboration.completedReviewersIds + ";" + this.initialDataSources.currentBrowserUser.MainActingUserUPN;
      updatedCollaboration.completedRounds = this.catsPackageData.collaboration.completedRounds == '' ? this.catsPackageData.collaboration.completedRounds : this.catsPackageData.collaboration.completedRounds + "," + this.catsPackageData.correspondence.currentReview;
    }

    this.catsPackageData.collaboration = updatedCollaboration;
  }
  

  setMembersAssignment_Reviewers(newusers: string[], removedusers: string[] = []){
      let catsid : string = this.documentDetailsFormGroup.controls['catsid'].value;
      let currentReview : string = this.roundsSelectionFormGroup.controls['currentReview'].value?.join(',');
      

              
      this.catsPackageData.reviewers = []; 
      this.catsPackageData.collaboration.currentReviewers = getMembersFullnames(this.membersAssignmentFormGroup.controls['reviewers'].value, this.initialDataSources.allOMBUsers)?.join(';');
      this.catsPackageData.collaboration.currentReviewersIds = this.membersAssignmentFormGroup.controls['reviewers'].value?.join(';');
      newusers?.forEach(x => {
        let reviewer : Reviewer = new Reviewer();
        reviewer.id = 0;
        reviewer.reviewerName = getMembersFullnames(x, this.initialDataSources.allOMBUsers);
        reviewer.reviewerUPN = x;
        reviewer.isCurrentRound = true;
        reviewer.collaborationId = this.catsPackageData.collaboration.id;
        reviewer.roundName = currentReview;
        reviewer.roundStartDate = this.routeTitle.toLowerCase().includes('manage') == true && this.catsPackageData.collaboration.currentRoundStartDate != undefined ? new Date(this.catsPackageData.collaboration.currentRoundStartDate.toString()).toLocaleString() : new Date().toLocaleString() ;
        reviewer.roundEndDate = this.routeTitle.toLowerCase().includes('manage') == true && this.catsPackageData.collaboration.currentRoundEndDate != undefined ? new Date(this.catsPackageData.collaboration.currentRoundEndDate.toString()).toLocaleString() :this.roundsSelectionFormGroup.controls['currentRoundEndDate'].value != '' ? new Date(this.roundsSelectionFormGroup.controls['currentRoundEndDate'].value).toLocaleString() : new Date().toLocaleString();
        reviewer.isEmailSent = false;
        reviewer.modifiedBy = this.initialDataSources.currentBrowserUser.LoginName;
        reviewer.ModifiedTime = new Date().toLocaleString();
        reviewer.isDeleted = false;
        reviewer.DeletedBy =  '';
        reviewer.deletedTime = null;
        reviewer.roundActivity = "Not Completed";
        
        if (this.catsPackageData.reviewers.indexOf(reviewer) == -1){
          this.catsPackageData.reviewers.push(reviewer);
        }  

        if(this.parentRoute == "reviewer"){

          if(this.parentRoute == "reviewer" && this.routePath == 'rdraft'){
            reviewer.draftBy = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;
            reviewer.draftByUpn = this.initialDataSources.currentBrowserUser.MainActingUserUPN;
            reviewer.draftDate = new Date().toLocaleString();
            reviewer.roundActivity = "Draft"
            reviewer.draftReason = "Test Draft reason...";
          }
          else{
            reviewer.roundActivity = "Completed"
            reviewer.roundCompletedBy  = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;
            reviewer.roundCompletedByUpn = this.initialDataSources.currentBrowserUser.MainActingUserUPN;
            reviewer.roundCompletedDate = new Date().toLocaleString();
            reviewer.isRoundCompletedBySurrogate = this.initialDataSources.currentBrowserUser.MainActingUserUPN != this.initialDataSources.currentBrowserUser.LoginName;
          }
        
        }
      });        

      removedusers?.forEach(x => {
        let reviewer : Reviewer = new Reviewer();
        reviewer.isCurrentRound = true;
        reviewer.id = 0;
        reviewer.reviewerName = getMembersFullnames(x, this.initialDataSources.allOMBUsers);
        reviewer.reviewerUPN = x;
        reviewer.collaborationId = this.catsPackageData.collaboration.id;
        reviewer.roundName = currentReview;
        reviewer.roundStartDate = this.routeTitle.toLowerCase().includes('manage') == true && this.catsPackageData.collaboration.currentRoundStartDate != undefined ? new Date(this.catsPackageData.collaboration.currentRoundStartDate.toString()).toLocaleString() : new Date().toLocaleString() ;
        reviewer.roundEndDate = this.routeTitle.toLowerCase().includes('manage') == true && this.catsPackageData.collaboration.currentRoundEndDate != undefined ? new Date(this.catsPackageData.collaboration.currentRoundEndDate.toString()).toLocaleString() :this.roundsSelectionFormGroup.controls['currentRoundEndDate'].value != '' ? new Date(this.roundsSelectionFormGroup.controls['currentRoundEndDate'].value).toLocaleString() : new Date().toLocaleString();
        reviewer.isEmailSent = false;
        reviewer.modifiedBy = this.initialDataSources.currentBrowserUser.LoginName;
        reviewer.ModifiedTime = new Date().toLocaleString();
        reviewer.isDeleted = true;
        reviewer.DeletedBy =  this.initialDataSources.currentBrowserUser.LoginName;
        reviewer.deletedTime = new Date().toLocaleString();//this.datePipe.transform(new Date() ,'MM/dd/yyyyThh:mm:ss');
        
        if (this.catsPackageData.reviewers.indexOf(reviewer) == -1){
          this.catsPackageData.reviewers.push(reviewer);
        }  
      });
  }  

  setMembersAssignment_Originators(newusers: string[], removedusers: string[] = []){ 
    let catsid : string = this.documentDetailsFormGroup.controls['catsid'].value;
    let currentReview : string = this.roundsSelectionFormGroup.controls['currentReview'].value?.join(',');

    
    this.catsPackageData.originators = [];
    this.catsPackageData.collaboration.currentOriginators = getMembersFullnames(this.membersAssignmentFormGroup.controls['originators'].value, this.initialDataSources.allOMBUsers)?.join(';');
    this.catsPackageData.collaboration.currentOriginatorsIds = this.membersAssignmentFormGroup.controls['originators'].value?.join(';');
    newusers?.forEach(x => {
      let originator : Originator = new Originator();          
      originator.id = 0;
      originator.originatorName = getMembersFullnames(x, this.initialDataSources.allOMBUsers);
      originator.originatorUpn = x;
      originator.collaborationId = this.catsPackageData.collaboration.id;
      originator.roundName = currentReview;
      originator.isEmailSent = false;
      originator.modifiedBy = this.initialDataSources.currentBrowserUser.LoginName;
      originator.ModifiedTime = new Date().toLocaleString();
      originator.IsDeleted = false;
      originator.DeletedBy =  '';
      originator.DeletedTime = null;

      if (this.catsPackageData.originators.indexOf(originator) == -1){
        this.catsPackageData.originators.push(originator);
      }      
    });    

    removedusers?.forEach(x => {
      let originator : Originator = new Originator();        
      originator.id = 0;
      originator.originatorName = getMembersFullnames(x, this.initialDataSources.allOMBUsers);
      originator.originatorUpn = x;
      originator.collaborationId = this.catsPackageData.collaboration.id;
      originator.roundName = currentReview;
      originator.isEmailSent = false;
      originator.modifiedBy = this.initialDataSources.currentBrowserUser.LoginName;
      originator.ModifiedTime = new Date().toLocaleString();
      originator.IsDeleted = true;
      originator.DeletedBy =  this.initialDataSources.currentBrowserUser.LoginName;
      originator.DeletedTime = new Date().toLocaleString();//this.datePipe.transform(new Date() ,'MM/dd/yyyy HH:mm:ss');

      if (this.catsPackageData.originators.indexOf(originator) == -1){
        this.catsPackageData.originators.push(originator);
      }  
    });

    
  }   

  setMembersAssignment_FyiUsers(newusers: string[], removedusers: string[] = []){
    let catsid : string = this.documentDetailsFormGroup.controls['catsid'].value;
    let currentReview : string = this.roundsSelectionFormGroup.controls['currentReview'].value?.join(',');

    

    this.catsPackageData.fyiusers = [];
    this.catsPackageData.collaboration.currentFYIUsers = getMembersFullnames(this.membersAssignmentFormGroup.controls['fyiUsers'].value, this.initialDataSources.allOMBUsers).join(';');
    this.catsPackageData.collaboration.currentFYIUsersIds = this.membersAssignmentFormGroup.controls['fyiUsers'].value?.join(';');
    newusers?.forEach(x => {
      let fyiUsers : FYIUser = new FYIUser();        
      fyiUsers.id =  0;
      fyiUsers.fYIUserName = getMembersFullnames(x, this.initialDataSources.allOMBUsers);
      fyiUsers.fyiUpn = x;
      fyiUsers.collaborationId = this.catsPackageData.collaboration.id;
      fyiUsers.roundName = currentReview;
      fyiUsers.isEmailSent = false;
      fyiUsers.modifiedBy = this.initialDataSources.currentBrowserUser.LoginName;
      fyiUsers.ModifiedTime = new Date().toLocaleString();
      fyiUsers.IsDeleted = false;
      fyiUsers.DeletedBy =  '';
      fyiUsers.DeletedTime = null;      
      
      if (this.catsPackageData.fyiusers.indexOf(fyiUsers) == -1){
        this.catsPackageData.fyiusers.push(fyiUsers);
      }  
    });   

    removedusers?.forEach(x => {
      let fyiUsers : FYIUser = new FYIUser();      
      fyiUsers.id = 0;
      fyiUsers.fYIUserName = getMembersFullnames(x, this.initialDataSources.allOMBUsers);
      fyiUsers.fyiUpn = x;
      fyiUsers.collaborationId = this.catsPackageData.collaboration.id;
      fyiUsers.roundName = currentReview;
      fyiUsers.modifiedBy = this.initialDataSources.currentBrowserUser.LoginName;
      fyiUsers.ModifiedTime = new Date().toLocaleString();
      fyiUsers.isEmailSent = false;
      fyiUsers.IsDeleted = true;
      fyiUsers.DeletedBy =  this.initialDataSources.currentBrowserUser.LoginName;
      fyiUsers.DeletedTime = new Date().toLocaleString();//this.datePipe.transform(new Date() ,'MM/dd/yyyy HH:mm:ss');     
      
      if (this.catsPackageData.fyiusers.indexOf(fyiUsers) == -1){
        this.catsPackageData.fyiusers.push(fyiUsers);
      }  
    });

      
  }  

  getMembersUpnAndFullnames(value: any): any{    
    return getMembersUpnAndFullnames(value, this.initialDataSources.allOMBUsers);
  }

  getMembersFullnames(value: any): any{
    return getMembersFullnames(value, this.initialDataSources.allOMBUsers);
  }

  getMembersUpns(value: any): any{
    return getMembersUpns(value, this.initialDataSources.allOMBUsers );
  }

  onActivate_ManageUpdate(form: any = ''){

    //Currently bound data in controls
    let inControlRev = form.reviewers != null ? form.reviewers : [];
    let inControlFyi = form.fyiUsers != null ? form.fyiUsers : [];
    let inControlOri = form.originators != null ? form.originators : []; 

    //Previous members
    this.initialOriginators = this.catsPackageData.collaboration.originators?.map(o => o.originatorUpn);
    this.initialReviewers = this.catsPackageData.collaboration.reviewers?.map(o => o.reviewerUPN);
    this.initialFyiUsers = this.catsPackageData.collaboration.fYIUsers?.map(o => o.fyiUpn);

    //Addtional data from the controls
    let newReviewers =  inControlRev?.filter(x => this.initialReviewers?.includes(x) == false);
    let newFyis =  inControlFyi?.filter(x =>   this.initialFyiUsers ?.includes(x) == false);      
    let newOriginators =  inControlOri?.filter(x => this.initialOriginators?.includes(x) == false);

    // Data marked for the deletion from the controls
    let toRemoveReviewers = this.initialReviewers?.filter( x => inControlRev?.includes(x) == false);
    let toRemoveFyis = this.initialFyiUsers ?.filter( x => inControlFyi?.includes(x) == false);
    let toRemoveOriginators = this.initialOriginators?.filter( x => inControlOri?.includes(x) == false);

    if (newOriginators?.length > 0 || toRemoveOriginators?.length > 0){
      this.setMembersAssignment_Originators(newOriginators, toRemoveOriginators);
    }
        
    if (newReviewers?.length > 0 || toRemoveReviewers?.length > 0){
      this.setMembersAssignment_Reviewers(newReviewers, toRemoveReviewers);
    }

    if (newFyis?.length > 0 || toRemoveFyis?.length > 0){
      this.setMembersAssignment_FyiUsers(newFyis, toRemoveFyis);
    }

    if (newReviewers?.length > 0 || newFyis?.length > 0 || newOriginators?.length > 0){
      this.isManageExistingUpdatable = true;     
      this.catsPackageData.correspondence.isEmailElligible = true;

      if (newReviewers?.length > 0 && newOriginators?.length > 0){
        this.catsPackageData.correspondence.catsNotificationId = 4; // Launch Review/Moved to Next Round or adding both originator and reviewers at the same time
      }
      else if (newReviewers?.length > 0){
        this.catsPackageData.correspondence.catsNotificationId = 6; // add reviewers only
      }
      else if (newOriginators?.length > 0){
        this.catsPackageData.correspondence.catsNotificationId = 5; // add originator only
      }
      else if (newFyis?.length > 0){
        this.catsPackageData.correspondence.catsNotificationId = 7; // add FYI only
      }
      else{
        this.catsPackageData.correspondence.catsNotificationId = 0;
      }
    }
    else{      
      this.isManageExistingUpdatable = false;      
      this.catsPackageData.correspondence.isEmailElligible = false;
    }

    if (toRemoveReviewers?.length > 0 || toRemoveFyis?.length > 0 || toRemoveOriginators?.length > 0){
      this.isManageExistingUpdatable = true;    
    }

  }

  openDialogConfirm(): void {

    let message : string = "You are about to Return this item. This process cannot be undone. Please click NO to Cancel or click OK to proceed.";
   
    const dialogRef = this.dialog.open(DialogPromptComponent, {
      width: '500px',
      data: {
        name: this.initialDataSources.currentBrowserUser.PreferredName + ",",
        label: "Return The item: " + this.catsPackageData.correspondence.catsid,
        title: message,
        isConfirmOnly: true,
        isReopening: false,
        noThanksLabel: "Cancel"
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      //console.log('The dialog was closed');
      if (result != undefined && result?.trim() != "No"){
        
      } 
    });
  }

  getSummary(){
  }

  onSummary(form: any){
  }

}
