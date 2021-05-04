import { Component, OnInit, Input, ViewChild, ElementRef, QueryList } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormControl, AbstractControl, FormArray } from '@angular/forms';
import { CorrespondenceService } from '../../services/correspondence-service';
import { Correspondence } from '../../Models/correspondence.model';
import { DatePipe, formatDate } from '@angular/common';
import { MainLoaderService } from 'src/app/services/main-loader-service';
import { DataSources } from 'src/app/modules/shared/interfaces/data-source';
import { Observable, Subscription,of, from } from 'rxjs';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { DialogContentHolderComponent } from 'src/app/modules/shared/components/dialog-content-holder/dialog-content-holder.component';
import { OutPutOptionsSelected, SelectAutocompleteComponent } from 'src/app/modules/shared/components/select-autocomplete/select-autocomplete.component';
import { Router, ActivatedRoute } from '@angular/router';
import { DialogPromptComponent } from 'src/app/modules/shared/components/dialog-prompt/dialog-prompt.component';
import { MatCheckboxChange } from '@angular/material/checkbox';
import { DashboardsConfigurations } from 'src/app/modules/shared/Data/dashboards.configurations';
import { AppConfigService } from 'src/app/services/AppConfigService';
import {Location} from '@angular/common';
import { HttpEventType, HttpEvent } from '@angular/common/http';
import { FileUploadService } from 'src/app/services/FileUpload-service';
import { ExternalUser } from 'src/app/models/externauUser.model';
import { getMembersFullnames, getStringFromHtml, setIncomingDashboardInRootComponent, generateClearancesheet } from 'src/app/modules/shared/utilities/utility-functions';
import { DialogContentPopupComponent } from '../dialog-content-popup/dialog-content-popup.component';
import { CollaborationService } from 'src/app/modules/shared/services/collaboration-service';
import { Collaboration } from 'src/app/modules/originator/models/collaboration.model';
import { EventEmitterService } from 'src/app/services/event-emitter.service';
import { map, startWith } from 'rxjs/operators';
import { MatAutocompleteTrigger } from '@angular/material/autocomplete';
import { ExternauUsersServiceService } from 'src/app/modules/shared/services/externau-users-service.service';
import Swal from 'sweetalert2/dist/sweetalert2.all.js';

@Component({
  selector: 'app-correspondence-form',
  templateUrl: './correspondence-form.component.html',
  styleUrls: ['./correspondence-form.component.scss'],
  providers: [DatePipe]
})
export class CorrespondenceFormComponent implements OnInit {

  selectedUsers: any[] = new Array<any>();

  filteredUsers: Observable<any[]>;
  lastFilter: string = '';
  lastFilterCorrespondentName: string = '';
  lastFilterOtherUsers: string = '';
  lastFilterLeadOffice: string = '';
  lastFilterLeadOfficeUsers: string = '';
  lastFilterCopiedOffice: string = '';
  lastFilterCopiedOfficeusers: string = '';

  showError = false;
  errorMessage = '';

  correspondentNameObs: Observable<any[]>;
  otherSignersObs: Observable<any[]>;

  columnsConfigurations :DashboardsConfigurations;
  
  progress: number = 0; //progress bar

  parentRoute: string = '';

  referenceDocuments: string[] = [];
  reviewDocuments: string[] = [];
  finalDocuments: string[] = [];

  validationOnLetterModification : string[] = ["browsefile",];

  /* #region Main */
  details: FormGroup;
  correspodenceData : Correspondence = new Correspondence();
  collaborationData : Collaboration;
  catsId: string = "";
  referenceDocumentCount : number = 0;
  todayDate : string = "";  
  /* #endregion */

  //Sources for dropdowns, auto fill....
  initialLocalDataSources: DataSources = {} as DataSources;
  
  /* #region Filter options for auto complete and select controls */
  filteredOptionsCorrepondents: Observable<any[]>;  

  externalUserOptions: string[] = [];
  externalUserselectedOptions: string[] = [];  

  correspondentSelectedOptionsObs: Observable<any[]>;  
  correspondentOptionsObs : Observable<string[] | string>; 
  correspondentOptions : string[] = []; 
  correspondentSelectedOptions : string = ''; 

  otherSignersSelectedOptionsObs: Observable<any[]>;  
  otherSignersOptionsObs : Observable<any[] | string>; 
  otherSignersOptions : string[] = []; 
  otherSignersSelectedOptions : string[] = []; 
  
  leadOfficeSelectedOptionsObs: Observable<any[]>;  
  leadOfficeOptionsObs : Observable<any[] | string>; 
  leadOfficeOptions : string[] = []; 
  leadOfficeSelectedOptions : string[] = []; 
  
  leadOfficeUsersSelectedOptionsObs: Observable<any[]>;  
  leadOfficeUsersOptionsObs : Observable<any[] | string>; 
  leadOfficeUsersOptions : string[] = []; 
  leadOfficeUsersSelectedOptions : string[] = []; 
  
  copiedOfficeSelectedOptionsObs: Observable<any[]>;  
  copiedOfficeOptionsObs : Observable<any[] | string>; 
  copiedOfficeOptions : string[] = []; 
  copiedOfficeSelectedOptions : string[] = []; 
  
  copiedOfficeUsersSelectedOptionsObs: Observable<any[]>;  
  copiedOfficeUsersOptionsObs : Observable<any[] | string>; 
  copiedOfficeUsersOptions : string[] = []; 
  copiedOfficeUsersSelectedOptions : string[] = [];

  isCorrespondentNameValid: boolean = false;  
  isotherSignersValid: boolean = false;
  isleadOfficeUsersValid: boolean = false;
  isleadOfficeforActionValid: boolean = false;
  isCcopiedOfficesValid: boolean = false;
  iscopiedOfficeUsersValid: boolean = false;
  isFinalDocument: boolean = false;

  IsRequired: string = "Yes";
  leadOfficeMembers: string ="";
  IsReopening = false;
  IsClosing = false;
  IsConfirmedOnSaved = false;
  
  //Submit buttons, but different purposes. Save (save only), Submit (send to review pending state), Reopen (opens the closed WHFyi)
  isSavedClicked: boolean = false;
  isPendingClicked: boolean = false;
  /* #endregion */

  
  // This variable indicates that we are entering in Manage state. 0 means new letter being processed
  @Input() itemId: number = 0; 

  @ViewChild('mySelectDocType') mySelectDocType:any; 
  @ViewChild('mySelectLetterStatus') mySelectLetterStatus:any;
  @ViewChild('mySelectDocumentsAttached') mySelectDocumentsAttached:any;
  @ViewChild('docChoiceCheckbox') docChoiceCheckbox: ElementRef;
  @ViewChild('fileInput') fileInput;
  @ViewChild(SelectAutocompleteComponent) multiSelect: SelectAutocompleteComponent;


  file: File | null = null;
  selectedFiles: any[] = [];
  filenames: string[] = [];
  fileInputTooltip: string = '';

  allInvalidControls: string[]= [];

  constructor(
      fb: FormBuilder, 
      public initialDataSources: DataSources,
      private svc: CorrespondenceService,
      private externauUsersService: ExternauUsersServiceService,
      private eventEmitterService: EventEmitterService,
      private collaborationService : CollaborationService,
      private datePipe:DatePipe = new DatePipe("en-US"),
      public dialog: MatDialog,
      private router: Router,
      private activeRoute: ActivatedRoute,)
    {
    
    console.log("Constructor ran");

    this.catsId = "";
    this.correspodenceData.referenceDocuments = "";
    this.todayDate = this.datePipe.transform(new Date(), 'yyyy-MM-dd');

    this.details = fb.group({
      id:['0'],
      catsid:[''],
      letterCrossReference: [''],
      correspondentName: ['', this.itemId == 0 ? Validators.required : ''],

      otherSigners: [],
      letterSubject: ['', [Validators.maxLength(400),Validators.required]],
      letterTypeName: ['', Validators.required],
      letterDate: [this.todayDate, Validators.required],
      letterReceiptDate: [this.todayDate, Validators.required],
      currentReview: [''],
      fiscalYear:[this.datePipe.transform(new Date(this.todayDate) ,'yyyy-MM-dd')],
      //phoneNumber: ['', Validators.pattern(/^\d{3}-\d{3}-\d{4}$/)],
      //email: ['', [Validators.email, Validators.required]],
      rejected:[false],
      rejectedLeadOffices:[''],
      rejectionReason:[''],
      reviewStatus:[''],

      isLetterRequired: ['true'],
      IsArchived: [false],
      isPendingLeadOffice: [true],
      isSaved: [false],
      isUnderReview: [false],
      isAdminClosure: [false],
      isAdminReOpen: [false],
      isFinalDocument: [false],

      documentsAttached: [''],
      externalAgencies: [''],
      padDueDate: [this.todayDate, this.IsRequired == "Yes" ?  Validators.required : ''],
      dueforSignatureByDate: [this.todayDate, this.IsRequired == "Yes" ? Validators.required : ''],
      leadOfficeName: ['', this.IsRequired == "Yes" ?  Validators.required : ''],
      leadOfficeUsersDisplayNames:[''],
      leadOfficeUsersIds:['', this.IsRequired == "Yes" ?  Validators.required : ''],
      copiedUsersDisplayNames:[''],      
      copiedUsersIds:['', this.IsRequired == "No" ?  Validators.required : ''],
      copiedOfficeName: ['',this.IsRequired == "No" ?  Validators.required : ''],
      letterStatus: ['Open', Validators.required],
      notes: [''],
      notRequiredReason: ['',this.IsRequired == "No" ?   Validators.required: ''],
      otherSignersText:[''],
      filetext:[''],
      browsefile:[null, this.itemId == 0 ? Validators.required : ''],
      attachedFiles:[null],
      whFyi: ['']

    });
  }

  ngOnInit() { 

    //setting the incoming dashboard options  
    // send the dahsboard title to the root component (appComponent)
    setIncomingDashboardInRootComponent(this.parentRoute, this.eventEmitterService);
    
    //this hold all the fields names and their respective labels
    this.columnsConfigurations = new DashboardsConfigurations(this.datePipe);

    this.correspondentNameObs = this.details.controls['correspondentName'].valueChanges
      .pipe(
        startWith(''),
        map(value => this._filter(value, 'correspondentName'))
      );
    
    this.otherSignersObs = this.details.controls['otherSigners'].valueChanges.pipe(
      startWith<string | any[]>(''),
      map(value => typeof value === 'string' ? value : this.lastFilter),
      map(filter => this._filter(filter,'otherSigners'))
    );


    //initialize the sources for control binding
    this.initializeOnInit();

    //onFormValueChanges()
    //this.onFormValueChanges();
    
  }

  onFormValueChanges(): void {
    this.details.valueChanges.subscribe(val=>{
      console.log(val);
      //show the invalid controls validators 
      this.findInvalidControls();
    })
  }

  initializeOnInit(){

    this.initialLocalDataSources = this.initialDataSources;
    let externalUserOptions = this.initialDataSources.externalUsers.map(x => x.name?.trim()).sort();  
    let otherSignersOptions  = this.initialDataSources.externalUsers.map(x => x.name?.trim()).sort();
    let leadOffices = this.initialDataSources.leadOffices.map(x => x.name?.trim()).sort(); 
    let copiedOffices = this.initialDataSources.leadOffices.map(x => x.name?.trim()).sort(); 

    //remove duplicate if any
    this.externalUserOptions = this.externalUserOptions.filter((el, i, a) => i === a.indexOf(el));    

    this.correspondentOptions = externalUserOptions;
    this.correspondentOptionsObs = of(this.correspondentOptions);

    this.otherSignersOptions = otherSignersOptions;
    this.otherSignersOptionsObs = of(this.otherSignersOptions);

    this.leadOfficeOptions = leadOffices;
    this.leadOfficeOptionsObs = of( this.leadOfficeOptions);  

    this.copiedOfficeOptionsObs = of(copiedOffices);  
    this.leadOfficeUsersSelectedOptionsObs = of(copiedOffices)  

    if (this.itemId > 0 ){
      this.svc
          .loadCorrespondenceById(this.itemId)
          .subscribe(response => {
              this.correspodenceData = response;
              if(this.correspodenceData.isUnderReview == true || this.correspodenceData.currentReview?.toLocaleLowerCase() != 'none'){
                this.collaborationService.loadCollaborationByCorrespondenceId(this.itemId).subscribe(res =>{
                  this.collaborationData = res;
                  this.loadDetailsOnInit();
                });
              }
              else{
                this.loadDetailsOnInit();
              }

              //Refresh the form validator After binding the controls 
              //Based on the letter response status (Yes or No), some controls are not required and others hidden
              this.updateValidatorsIsResponseRequired();              
          });
    }
    else{
      //initialize the observable as they are serving as input to the 
      //child component SelectAutocompleteComponent
      this.correspondentSelectedOptionsObs  = of([]);
      this.otherSignersSelectedOptionsObs = of([]);
      this.leadOfficeSelectedOptionsObs = of([]);
      this.leadOfficeUsersSelectedOptionsObs = of([]);
      this.copiedOfficeSelectedOptionsObs = of([]);
      this.copiedOfficeUsersSelectedOptionsObs = of([]);

      //Refresh the form validator After binding the controls 
      //Based on the letter response status (Yes or No), some controls are not required and others hidden
      this.updateValidatorsIsResponseRequired();
    }

  }

  ngAfterViewInit() {

    //close the select control panel on mouse out
    this.mySelectDocType.openedChange.subscribe(opened => {
      if (opened) {
        this.mySelectDocType.panel.nativeElement.addEventListener('mouseleave', () => {
          this.mySelectDocType.close();
        })
      }
    });

    if (this.mySelectLetterStatus){
      this.mySelectLetterStatus.openedChange.subscribe(opened => {
        if (opened) {
          this.mySelectLetterStatus.panel.nativeElement.addEventListener('mouseleave', () => {
            this.mySelectLetterStatus.close();
          })
        }
      });
    }

    if (this.itemId > 0){      
      this.mySelectDocumentsAttached.openedChange.subscribe(opened => {
        if (opened) {
            this.mySelectDocumentsAttached.panel.nativeElement.addEventListener('mouseleave', () => {
              this.mySelectDocumentsAttached.close();
            })
          }
        });
    }
  } 

  _filter(filter: any, control: string): any[] {
    this.lastFilter = filter;
    if (filter) {
      return this.initialDataSources.externalUsers.filter(option => {
        return option.name.toLowerCase().indexOf(filter.toLowerCase()) >= 0
         // || option.lastname.toLowerCase().indexOf(filter.toLowerCase()) >= 0;
      })
    } else {
      return this.initialDataSources.externalUsers.slice();
    }
  }  

  displayFn(value: any[] | string): string | undefined {
    let displayValue: string;
    if (Array.isArray(value)) {
      value.forEach((user, index) => {
        if (index === 0 || displayValue == '') {
          displayValue = user;
        } else {
          displayValue += ', ' + user;
        }
      });
    } else {
      displayValue = value;
    }
    return displayValue;
  } 

  loadDetailsOnInit(){

    //Disable if the Browser User has not the edit permission
    if (this.initialDataSources.currentBrowserUser.MemberOfCATSCorrespondenceReadOnly == true){
      this.details.disable();
    }
    else{
      this.details.enable();
    }
    
    //Data bind
    this.catsId = this.correspodenceData.catsid;
    this.referenceDocumentCount = this.correspodenceData.referenceDocuments ? this.correspodenceData.referenceDocuments?.split('</br>').length : 0;
    this.IsRequired = this.correspodenceData.isLetterRequired == false ? "No" : "Yes";
    
    //set Lead Office Users & Copied Office Users dropdowns sources
    let leadOffice = this.correspodenceData.leadOfficeName ? this.correspodenceData.leadOfficeName?.toUpperCase().trim() : '';
    let copiedOffices =  this.copiedOfficeSelectedOptions =  this.correspodenceData.copiedOfficeName ? this.correspodenceData.copiedOfficeName?.split(';').map(x => x.trim())  : [];   
    
    this.correspondentSelectedOptionsObs = of([this.correspodenceData.correspondentName]);
    this.otherSignersSelectedOptions = this.correspodenceData.otherSigners ? this.correspodenceData.otherSigners?.replace(/;/g, '\n').replace(/,/g, '\n')?.split("\n") : [];
    this.otherSignersSelectedOptionsObs = of(this.otherSignersSelectedOptions); 

    this.leadOfficeSelectedOptions = [leadOffice];
    this.leadOfficeSelectedOptionsObs = of(this.leadOfficeSelectedOptions);
    this.leadOfficeUsersOptions =  this.setAssignLeadOfficeUsersSource(leadOffice, false);
    this.leadOfficeUsersOptionsObs = of(this.getMemberOptionsObjects(this.leadOfficeUsersOptions)); 
    this.leadOfficeUsersSelectedOptions = this.correspodenceData.leadOfficeUsersIds ?  this.correspodenceData.leadOfficeUsersIds?.split(';').sort().map(x => x.trim()) : [];
    this.checkIfSelectedUsersActive(false);
    this.leadOfficeUsersSelectedOptionsObs = of(this.getMemberOptionsObjects(this.leadOfficeUsersSelectedOptions));  
    
    this.copiedOfficeSelectedOptions =  copiedOffices; 
    this.copiedOfficeSelectedOptionsObs = of(this.copiedOfficeSelectedOptions)  ;
    this.copiedOfficeUsersOptions =  this.setAssignLeadOfficeUsersSource(copiedOffices, true);
    this.copiedOfficeUsersOptionsObs = of(this.getMemberOptionsObjects(this.copiedOfficeUsersOptions)); 
    this.copiedOfficeUsersSelectedOptions = this.correspodenceData.copiedUsersIds ? this.correspodenceData.copiedUsersIds?.split(';').map(x => x.trim())  : [];
    this.checkIfSelectedUsersActive(true);
    this.copiedOfficeUsersSelectedOptionsObs = of(this.getMemberOptionsObjects(this.copiedOfficeUsersSelectedOptions));  
     

    this.referenceDocuments = !this.correspodenceData.referenceDocuments ? [] : this.correspodenceData.referenceDocuments?.indexOf("<a") > 0 ? this.correspodenceData.referenceDocuments
                        .substring(this.correspodenceData.referenceDocuments?.indexOf('<a'), 
                                  this.correspodenceData.referenceDocuments?.lastIndexOf('</a')+ 4)?.split('</br>') : [];

    this.reviewDocuments = !this.correspodenceData.reviewDocuments ? [] :  this.correspodenceData.reviewDocuments?.indexOf("<a") > 0 ?  this.correspodenceData.reviewDocuments
                        .substring(this.correspodenceData.reviewDocuments?.indexOf('<a'), 
                                  this.correspodenceData.reviewDocuments?.lastIndexOf('</a')+ 4)?.split('</br>') : [];

    this.finalDocuments = !this.correspodenceData.finalDocuments ? [] :  this.correspodenceData.finalDocuments?.indexOf("<a") > 0 ?  this.correspodenceData.finalDocuments
                        .substring(this.correspodenceData.finalDocuments?.indexOf('<a'), 
                                  this.correspodenceData.finalDocuments?.lastIndexOf('</a')+ 4)?.split('</br>') : [];

    this.IsClosing = this.correspodenceData.letterStatus == "Open" ? false : true;

    //this.details.disable();//Disable the form for all the userss except LA and Correspondence Team
    //bind the control
    this.details.patchValue({
      id:this.correspodenceData.id,
      catsid:this.correspodenceData.catsid,
      letterCrossReference: this.correspodenceData.letterCrossReference,
      correspondentName: this.correspodenceData.correspondentName,
      otherSigners: this.correspodenceData.otherSigners,
      letterSubject: this.correspodenceData.letterSubject ? this.correspodenceData.letterSubject : '',
      letterTypeName: this.correspodenceData.letterTypeName?.trim() ?  this.correspodenceData.letterTypeName?.trim() : '',
      letterDate: this.datePipe.transform(new Date(this.correspodenceData.letterDate) ,'yyyy-MM-dd'),
      letterReceiptDate: this.datePipe.transform(new Date(this.correspodenceData.letterReceiptDate) ,'yyyy-MM-dd'),
      currentReview: this.correspodenceData.currentReview ? this.correspodenceData.currentReview : '',
      isLetterRequired: this.correspodenceData.isLetterRequired == false ? 'false' : 'true',
      fiscalYear: this.correspodenceData.fiscalYear,
      
      rejected:this.correspodenceData.rejected,
      rejectedLeadOffices:this.correspodenceData.rejectedLeadOffices,
      rejectionReason:this.correspodenceData.rejectionReason,
      reviewStatus:this.correspodenceData.reviewStatus,
      attachedLetters: "",
      browsefile:null,
      padDueDate: this.datePipe.transform(new Date(this.correspodenceData.padDueDate) ,'yyyy-MM-dd'),
      dueforSignatureByDate: this.datePipe.transform(new Date(this.correspodenceData.dueforSignatureByDate) ,'yyyy-MM-dd'),
      leadOfficeName: this.correspodenceData.leadOfficeName ,
      leadOfficeUsersDisplayNames: this.correspodenceData.leadOfficeUsersDisplayNames == undefined ? '' : this.correspodenceData.leadOfficeUsersDisplayNames ,
      leadOfficeUsersIds: this.correspodenceData.leadOfficeUsersIds == undefined ? '' : this.correspodenceData.leadOfficeUsersIds ,
      copiedOfficeName: this.correspodenceData.copiedOfficeName ,
      copiedUsersDisplayNames: this.correspodenceData.copiedUsersDisplayNames == undefined ? '' : this.correspodenceData.copiedUsersDisplayNames ,
      copiedUsersIds: this.correspodenceData.copiedUsersIds == undefined ? '' : this.correspodenceData.copiedUsersIds,
      letterStatus: this.correspodenceData.letterStatus ,
      notes: this.correspodenceData.rejected ? this.correspodenceData.rejectionReason : getStringFromHtml(this.correspodenceData.notes) ? getStringFromHtml(this.correspodenceData.notes)?.split('|').join('\n'): '',
      notRequiredReason: this.correspodenceData.notRequiredReason,
      externalAgencies : this.correspodenceData.externalAgencies,
      //whFyi: this.correspodenceData.whFyi == true ? true : false,
      
      isPendingLeadOffice: this.correspodenceData.isPendingLeadOffice,      
      isSaved  : this.correspodenceData.isSaved,
      IsArchived: this.correspodenceData.isArchived,
      isUnderReview: this.correspodenceData.isUnderReview,
      isFinalDocument: this.correspodenceData.isFinalDocument,

      //Not updatable at this stage      
      adminClosureReason:this.correspodenceData.adminClosureReason,
      adminReOpenDate: this.correspodenceData.adminReOpenDate,
      adminReOpenReason: this.correspodenceData.adminReOpenReason,
      //reasonsToReopen: this.correspodenceData.reasonsToReopen,
      isAdminClosure: this.correspodenceData.isAdminClosure,
      isAdminReOpen: this.correspodenceData.isAdminReOpen,    
      adminClosureDate: this.correspodenceData.adminClosureDate,
      isDeleted : this.correspodenceData.isDeleted,
      deletedBy : this.correspodenceData.deletedBy,
      deletedTime : this.correspodenceData.deletedTime,
      modifiedBy  : this.correspodenceData.modifiedBy,
      createdTime  : this.correspodenceData.createdTime,
      filetext: ''
    });     

    //Refresh the form validator After binding the controls 
    //Based on the letter response status (Yes or No), some controls are not required and others hidden
    this.updateValidatorsIsResponseRequired();

    this.triggerValidation(this.details);

    //if item still Open then show the invalid controls    
    if(this.correspodenceData.letterStatus == "Open"){
      //this.findInvalidControls();
    }
       

  } 

  checkIfSelectedUsersActive(isCopied: boolean = false){

    if (!isCopied){
      this.leadOfficeUsersSelectedOptions.forEach( u => {
        if (!this.leadOfficeUsersOptions.some(x => x == u)){
          var upn = u;
          var index = this.correspodenceData.leadOfficeUsersIds?.split(';').indexOf(u);
          var name = "(INACTIVE) " +  this.correspodenceData.leadOfficeUsersDisplayNames?.split(';')[index];
          let user = {upn: upn?.toLowerCase().trim(), displayName: name, officeId: 0, office: this.correspodenceData.leadOfficeName, emailAddress:'', roleId: 0, role:'', id:0};
          //adding inactive user to the option list
          if(!this.initialDataSources.allOMBUsers.some(x => x.upn.toLowerCase() == user.upn.toLowerCase())){
            this.initialDataSources.allOMBUsers.push(user);
          }          
        }
      });
    }
    else{

      this.copiedOfficeUsersSelectedOptions.forEach( u => {
        if (!this.copiedOfficeOptions.some(x => x == u)){
          var upn = u;
          var index = this.correspodenceData.copiedUsersIds?.split(';').indexOf(u);
          var name = "(INACTIVE) " +  this.correspodenceData.copiedUsersDisplayNames?.split(';')[index];
          let user = {upn: upn?.toLowerCase().trim(), displayName: name, officeId: 0, office: this.correspodenceData.leadOfficeUsersIds, emailAddress:'', roleId: 0, role:'', id:0};
          //adding inactive user to the option list
          if(!this.initialDataSources.allOMBUsers.some(x => x.upn.toLowerCase() == user.upn.toLowerCase())){
            this.initialDataSources.allOMBUsers.push(user);
          }          
        }

      });
    }

  }

  private triggerValidation(control: AbstractControl) {
    if (control instanceof FormGroup) {
        const group = (control as FormGroup);

        for (const field in group.controls) {
            const c = group.controls[field];

            this.triggerValidation(c);
            if (this.itemId > 0 && this.validationOnLetterModification.indexOf(field) != -1){
              c.clearValidators();
            }
            c.updateValueAndValidity();
        }
    }
    else if (control instanceof FormArray) {
        const group = (control as FormArray);

        for (const field in group.controls) {
            const c = group.controls[field];

            this.triggerValidation(c);
            c.updateValueAndValidity();
        }
    }

    //refersh the formgroup validation status
    control.updateValueAndValidity({ onlySelf: false });
}

  //this method is listening from the child select controls output emit events
   onChildOptionValueChange(outPutOptionsSelected: OutPutOptionsSelected){
    var emitedObject = outPutOptionsSelected;
    if (emitedObject.source == "correspondentName"){
      this.correspondentSelectedOptions = emitedObject.selectedOptions.toString() ;
      this.details.controls["correspondentName"].setValue(this.correspondentSelectedOptions);
    }
    else if (emitedObject.source == "otherSigners"){
      this.otherSignersSelectedOptions = emitedObject.selectedOptions as string[] ;
      this.details.controls["otherSigners"].setValue(this.otherSignersSelectedOptions.join(";"));
    }
    else if (emitedObject.source == "leadOfficeName"){
      this.leadOfficeSelectedOptions = emitedObject.selectedOptions as string[] ;
      this.leadOfficeUsersOptions =  this.setAssignLeadOfficeUsersSource(this.leadOfficeSelectedOptions[0], false);
      this.leadOfficeUsersOptionsObs = of(this.getMemberOptionsObjects(this.leadOfficeUsersOptions));
      //reset the previous selected options
      this.leadOfficeUsersSelectedOptions = [] ;
      this.leadOfficeUsersSelectedOptionsObs = of(this.getMemberOptionsObjects(this.leadOfficeUsersSelectedOptions));

      this.details.controls["leadOfficeName"].setValue(this.leadOfficeSelectedOptions[0]);
      this.details.controls["leadOfficeUsersDisplayNames"].setValue('');
      this.details.controls["leadOfficeUsersIds"].setValue('')
    }
    else if (emitedObject.source == "leadOfficeUsers"){
      this.leadOfficeUsersSelectedOptions = emitedObject.selectedOptions as string[] ;
      this.leadOfficeUsersSelectedOptionsObs = of(this.getMemberOptionsObjects(this.leadOfficeUsersSelectedOptions));
      this.details.controls["leadOfficeUsersDisplayNames"].setValue(getMembersFullnames(this.leadOfficeUsersSelectedOptions, this.initialDataSources.allOMBUsers).join(';'));
      this.details.controls["leadOfficeUsersIds"].setValue(this.leadOfficeUsersSelectedOptions.join(";"));
    }
    else if (emitedObject.source == "copiedOfficeName"){
      this.copiedOfficeSelectedOptions = emitedObject.selectedOptions as string[] ;

      this.copiedOfficeUsersOptions =  this.setAssignLeadOfficeUsersSource(this.copiedOfficeSelectedOptions, true);
      this.copiedOfficeUsersOptionsObs = of(this.getMemberOptionsObjects(this.copiedOfficeUsersOptions));
      //reset the previous selected options
      this.copiedOfficeUsersSelectedOptions = [] ;
      this.copiedOfficeUsersSelectedOptionsObs = of(this.getMemberOptionsObjects(this.copiedOfficeUsersSelectedOptions));

      this.details.controls["copiedOfficeName"].setValue(this.copiedOfficeSelectedOptions.join(";"));
      this.details.controls["copiedUsersDisplayNames"].setValue('');
      this.details.controls["copiedUsersIds"].setValue('');
    }
    else if (emitedObject.source == "copiedUsersDisplayNames"){
      
      this.copiedOfficeUsersOptions =  this.setAssignLeadOfficeUsersSource(this.copiedOfficeSelectedOptions, true);
      this.copiedOfficeUsersOptionsObs = of(this.getMemberOptionsObjects(this.copiedOfficeUsersOptions));

      this.copiedOfficeUsersSelectedOptions = emitedObject.selectedOptions as string[] ;
      this.copiedOfficeUsersSelectedOptionsObs = of(this.getMemberOptionsObjects(this.copiedOfficeUsersSelectedOptions));
      this.details.controls["copiedUsersDisplayNames"].setValue(getMembersFullnames(this.copiedOfficeUsersSelectedOptions, this.initialDataSources.allOMBUsers).join(';'));
      this.details.controls["copiedUsersIds"].setValue(this.copiedOfficeUsersSelectedOptions.join(";"));
    }

    console.log('Emit: ', emitedObject);
  }

  setAssignLeadOfficeUsersSource(office: string | string[], IsCopied: boolean): string[]{
    if (!IsCopied){
      var members = this.initialDataSources.leadOffices.filter(x => x.name.trim() == office).map(x => x.leadOfficeMembers);
      if (members.length > 0){
        return members[0].sort((a,b) => a.userFullName?.localeCompare(b.userFullName)).map(x => x.userUPN).sort();
      }
      else{
        return [];
      }
    }
    else{
      this.copiedOfficeOptions  = [];
      var members = this.initialDataSources.leadOffices.filter(x => office.indexOf(x.name.trim()) != -1).map(x => x.leadOfficeMembers);
      if (members?.length > 0){
        members.forEach(x => x.forEach(d => this.copiedOfficeOptions.push(d.userUPN)));    
        //sort  
        return this.copiedOfficeOptions.sort();
      }
      else{
        return [];
      }
    }
  }
  
  //Radio change event: Is Response Required control
  onIsRequiredChangeYes(value: any) {
      this.IsRequired = value            
      this.details.controls["isLetterRequired"].setValue('true');
      this.updateValidatorsIsResponseRequired();
      this.findInvalidControls()
  }

  onIsRequiredChangeNo(value: any) {
    this.IsRequired = value      
    this.details.controls["isLetterRequired"].setValue('false');
    this.updateValidatorsIsResponseRequired();
    this.findInvalidControls()
  }

  //Letter Status change event: Can be either Closed or Open
  onStatusChange(event){
    if (event.value == "Open" && this.correspodenceData.letterStatus == "Closed"){
      this.IsReopening = true;
    }
    else{
      this.IsReopening = false;
    }    

    if (event.value == "Open"){
      if (this.docChoiceCheckbox)
        this.docChoiceCheckbox['checked'] = false;
      this.IsClosing = false;
    }
    else{
      this.IsClosing = true;
    }

    this.updateValidatorsIsResponseRequired();
  }

  //To determine if the uploaded file is a final or Reference document
  onDocumentTypeChange(event:MatCheckboxChange){
    if(event.checked){
      this.isFinalDocument = true;
    }
    else{
      this.isFinalDocument = false;
    }
  }

  updateValidatorsIsResponseRequired(){

    this.maskAllControlAsUnTouched();

    if (this.IsRequired == "Yes") {
      //alert(target.innerText)
      this.details.controls["padDueDate"].setValidators([Validators.required]);
      this.details.controls["dueforSignatureByDate"].setValidators([Validators.required]);
      this.details.controls["leadOfficeName"].setValidators([Validators.required]);
      this.details.controls["leadOfficeUsersIds"].setValidators([Validators.required]);
      this.details.controls["copiedUsersIds"].clearValidators();
      this.details.controls["copiedOfficeName"].clearValidators();
      this.details.controls["notRequiredReason"].clearValidators();
    }
    else {
      //alert(target.innerText)
        this.details.controls["padDueDate"].clearValidators();
        this.details.controls["dueforSignatureByDate"].clearValidators();
        this.details.controls["leadOfficeName"].clearValidators();
        this.details.controls["leadOfficeUsersIds"].clearValidators();
        this.details.controls["copiedUsersIds"].setValidators([Validators.required]);
        this.details.controls["copiedOfficeName"].setValidators([Validators.required]);
        this.details.controls["notRequiredReason"].setValidators([Validators.required]);

        if (this.correspodenceData.currentReview?.trim() != '' && this.correspodenceData.currentReview?.trim().toLowerCase() != 'none' && this.correspodenceData.currentReview != undefined){
          this.details.controls["copiedUsersIds"].clearValidators();
          this.details.controls["copiedOfficeName"].clearValidators();
        }
    }

    //Corresponedent's Name is required only when creating new Letter
    if (this.itemId > 0){
      this.details.controls["correspondentName"].clearValidators();
    }
    //update validators
    this.details.controls["padDueDate"].updateValueAndValidity();
    this.details.controls["dueforSignatureByDate"].updateValueAndValidity();
    this.details.controls["leadOfficeName"].updateValueAndValidity();
    this.details.controls["leadOfficeUsersIds"].updateValueAndValidity();
    this.details.controls["copiedUsersIds"].updateValueAndValidity();
    this.details.controls["copiedOfficeName"].updateValueAndValidity();
    this.details.controls["notRequiredReason"].updateValueAndValidity();  
    this.details.controls["correspondentName"].updateValueAndValidity(); 
    //this.details.controls["reasonsToReopen"].updateValueAndValidity();
  }

  maskAllControlAsUnTouched(){
    for (const field in this.details.controls) {
        const c = this.details.controls[field];
        c.markAsUntouched();        
    }
  }
  
  //method to show the invalid controls validators
  findInvalidControls(){
    this.allInvalidControls = [];
    for (const field in this.details.controls) {
        const c = this.details.get(field);//this.details.controls[field];        
        if (c.invalid == true){
          this.allInvalidControls.push(this.columnsConfigurations.getColumnsLabel(field,"correspondence"));
        }
    }    
    //this.details.updateValueAndValidity();
  }

   //Dialog to open the Review details
  openDialogReviewDetails(){
    const dialogConfig = new MatDialogConfig();
    dialogConfig.disableClose = false;
    dialogConfig.hasBackdrop = true;
    dialogConfig.autoFocus = true;
    //dialogConfig.width = '350px';
    dialogConfig.data= {
      catsId: this.correspodenceData.catsid,
      currentReviewRound: this.correspodenceData.currentReview,
      correspondenceId: this.correspodenceData.id,
      collaborationId: this.correspodenceData.id//this.collaborationData?.id
    }
    const dialogRef = this.dialog.open(DialogContentPopupComponent, dialogConfig);

    dialogRef.afterClosed().subscribe(result => {
      console.log(`Dialog result: ${result}`);
    });

    
  }

  //Confirmation dialog
  openDialogConfirm(event): void {

    let message : string = "You are about to submit this form. <br>This process is irreversible<br>Please click \" No, dismiss\" to Cancel, or <br> \" Yes, go ahead\" to proceed.";
    if (this.isSavedClicked){
      message = "Are you sure you just want to save this item?<br>Please click \" No, dismiss\" to Cancel or<br> \" Yes, go ahead\" to proceed.";
    }
    else if(this.IsReopening){
      //message = "Are you sure you just want to re-open this item? Please click NO to Cancel or to proceed, type in the REASON for reopening and then OK .";
      message = "Are you sure you just want to re-open this item?<br>Please click \" No, dismiss\" to Cancel, or <br>type in the REASON for reopening, and then <br> \"Yes, go ahead\" to proceed.";
    }
    // const dialogRef = this.dialog.open(DialogPromptComponent, {
    //   width: '500px',
    //   data: {
    //     name: this.initialDataSources.currentBrowserUser.PreferredName + ",",
    //     label: this.IsReopening ? "Reason to re-open" : "Confirmation",
    //     title: message,
    //     isConfirmOnly: this.IsReopening ? false : true,
    //     isReopening: this.IsReopening,
    //     noThanksLabel: "No Thanks"
    //   }
    // });

    // dialogRef.afterClosed().subscribe(result => {
    //   console.log('The dialog was closed');
    //   if (result != undefined && result.trim() != "No" && result.trim() != "Cancel"){
    //     if (this.IsReopening == true){ 
    //       this.correspodenceData.reasonsToReopen = result.trim(); 

    //       //Append the reopen reason to the notes
    //       if (this.IsReopening == true){
    //         this.correspodenceData.notes += "|" +  formatDate(new Date(), 'MM/dd/yyyy HH:mm', 'en') + ": " +  result.trim() + ";";
    //       }          
    //     }
    //     this.saveCorrespondenceDetails(event);
    //   } 
    // });
    
    
    Swal.fire({
      title: 'Hi ' + this.initialDataSources.currentBrowserUser.PreferredName + '!',
      //text: message,
      html:this.IsReopening == false ?  message + '<br>' : message + '<input class=\'prompt-text\' type=\'text\' style=\'width:100%\' placeholder=\'Reason for reopening...\'/>',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Yes, go ahead.',
      cancelButtonText: 'No, dismiss'
    }).then((result) => {
      if (result.value) {   
        if (this.IsReopening == true){ 
          let promptTextElement = document.getElementsByClassName('prompt-text')[0] as HTMLInputElement;
          if (promptTextElement.value.trim() == ''){
            Swal.fire('', 'Reason for Reopening cannot be empty', 'error');
            return;
          }
          this.correspodenceData.reasonsToReopen = promptTextElement.value.trim(); 

          //Append the reopen reason to the notes
          if (this.IsReopening == true){
            this.correspodenceData.notes += "|" +  formatDate(new Date(), 'MM/dd/yyyy HH:mm', 'en') + ": " +  promptTextElement.value.trim() + ";";
          }          
        }        
        
        this.saveCorrespondenceDetails(event);
      } else if (result.dismiss === Swal.DismissReason.cancel) {
        Swal.fire(
          'Cancelled',
          'Process Cancelled',
          'error'
        )
      }
    })
  }

  //Select and attched the files
  onChangeFileInput(event: any): void {
    this.filenames = []; 
    
    const files = this.fileInput.nativeElement.files;    
    this.file =  files[0]; 

    for (let index = 0; index <  files.length; index++)  
    {  
      const file =  files[index] as File;  
      this.filenames.push(file.name);  
    } 

    this.fileInputTooltip = this.filenames.join('\n')   ;
    //set the text in the browse contrl text box
    const label = this.filenames.length > 1 ? this.filenames[0] + ' (+' + (this.filenames.length - 1) + ' others)' : this.filenames[0];
    this.details.controls["filetext"].setValue(label); 
    
    if(files.length > 0){
      this.details.controls["attachedFiles"].setValue(files);
    }
    else{
      this.details.controls["attachedFiles"].setValue(null);
    }
  }

  clearInputFile(){
    this.fileInput.nativeElement.value = null;
    this.details.controls['filetext'].setValue('');
  }

  async saveCorrespondenceDetails(event): Promise<void> {

    console.log('Form Submitted', this.details.value);    
    var emptyFiles: File[] = [];
    const files = this.details.get("attachedFiles").value ?  this.details.get("attachedFiles").value : emptyFiles; 
    
    //extract the formgroup data
    var correspondent  = new Correspondence();
    correspondent = Object.assign(correspondent, this.details.getRawValue());
    
    //Record the user acting on behalf of the record assigned user. Can be Surrogate or impersonation
    if (this.initialDataSources.currentBrowserUser.SurrogateActive == true || this.initialDataSources.currentBrowserUser.ImpersonationActive == true){
      this.correspodenceData.SurrogateFullName = this.svc.user.displayName;
      this.correspodenceData.SurrogateUpn = this.svc.user.upn;
    }

    if (this.itemId == 0)    {
      //set the create/modified and created BY/modify by   
      correspondent.catsNotificationId = correspondent.letterStatus?.toLowerCase() == 'closed' ? 10 : 1;//10 means closing the package; 1 means new letter is created or lead/copied offices assigned   
      correspondent.createdBy = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;// this.svc.user.displayName;
      correspondent.modifiedBy = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;//this.svc.user.displayName;
      correspondent.currentUserEmail = this.initialDataSources.currentBrowserUser.MainActingUserEmail;
      correspondent.currentUserFullName = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;
      correspondent.currentUserUPN = this.initialDataSources.currentBrowserUser.MainActingUserUPN;
      correspondent.createdTime = new Date;
      correspondent.modifiedTime = new Date;
      correspondent.isFinalDocument = this.isFinalDocument;    
      correspondent.isUnderReview = false;
      correspondent.reviewStatus = 'NONE';
      correspondent.currentReview = 'NONE';
      this.correspodenceData = correspondent;

      //set the 3 submit modes (Save, pending, Reopen)
      if (this.isSavedClicked){        
        this.correspodenceData.isSaved = true;
        this.correspodenceData.isEmailElligible = false;
        this.correspodenceData.isPendingLeadOffice = false;      
      }
      else{      
        this.correspodenceData.isSaved = false;
        this.correspodenceData.isEmailElligible = true;
        this.correspodenceData.isPendingLeadOffice = true;
      }

    }
    else{

      this.updateThePreviousData(correspondent);
    }    

    let progress: number;
    
    //call the service TO INSERT OR UPDATE with files attachments 
    this.svc.updateMyCorrespondence(this.correspodenceData, files)
    .subscribe(async event => {
      
      switch (event.type) {
        case HttpEventType.Sent:
          console.log('Request sent!');
          break;
        case HttpEventType.ResponseHeader:
          console.log('Response header received!');
          break;
        case  HttpEventType.UploadProgress:
          this.progress = Math.round(100 * event.loaded / event.total);
          console.log('uploading!' + Math.round(100 * event.loaded / event.total));

          break;
        case HttpEventType.DownloadProgress:
          const kbLoaded = Math.round(event.loaded / 1024);
          console.log(`Download in progress! ${ kbLoaded }Kb loaded`);
          break;
        case HttpEventType.Response:
          console.log('😺 Done!', event.body);
          this.correspodenceData =  event.body as Correspondence;
          
          this.selectedFiles = [];       
          
          if (this.itemId == 0){
            console.log('Form inserted',event.body);
            if (this.isSavedClicked){
              this.correspodenceData = event.body;
              this.router.navigate(['correspondence/cdetails'], { queryParams: {id: this.correspodenceData.id} }); // redirect to the details page
            }
            else{
              this.router.navigate(['.'], { relativeTo: this.activeRoute.parent });//redirect to the parent route for new letter
            }
            
          }
          else{            
            if (files.length > 0 && this.correspodenceData.letterStatus.toLowerCase().includes('closed')){
              //this.svc.sendAttachedEmails(this.correspodenceData, files);
            }
            console.log('Form Updated',event.body);
            //refresh the initial setting data after the update. This will refresh all the dropdown source data
            //await this.appConfigService.load();
            this.onUpdateInitialDatasourceExternalUsers();
            this.router.navigateByUrl('correspondence/cdetails?id=' + this.correspodenceData.id);
            //this.initializeOnInit();         
            
          }
          
      }
    }) ;
  }

  onUpdateInitialDatasourceExternalUsers(){
    
    let d : ExternalUser = new ExternalUser();
    if (this.initialDataSources.externalUsers.some(x => x.name.includes(this.details.controls["correspondentName"].value)) == false){
      d.name = this.details.controls["correspondentName"].value;
      d.title = this.details.controls["correspondentName"].value;
      d.id = 0;
      this.initialDataSources.externalUsers.push(d);
    }

    let y = this.details.controls["otherSigners"].value?.split(";").filter(x => this.initialDataSources.externalUsers.filter(o => o.name?.indexOf(x) == -1 && o.title?.indexOf(x) == -1)?.length == 0);

    if (y?.length > 0){
      y.forEach(z => {
        d.name = z;
        d.title = z;
        d.id = 0;
        this.initialDataSources.externalUsers.push(d);
      });      
    }
    this.initialDataSources.externalUsers.sort((a,b) => a.name?.localeCompare(b.name)).map(x => x.name).sort();

  }     

  setIncomingDashboardInRootComponent(){
    this.eventEmitterService.onIncomingDashboard(this.parentRoute);
  }

  onCancelClick(event){
    this.router.navigate(['.'], { relativeTo: this.activeRoute.parent });//redirect to the parent route for new letter
  }

  onSubmitButtonClick(event): void {
    if (event.currentTarget.innerText == "SAVE"){      
      this.isSavedClicked = true;
      this.isPendingClicked = false;
    }
    else{
      this.isSavedClicked = false;
      this.isPendingClicked = true;
    }

    this.isPendingClicked =  this.correspodenceData.isUnderReview ? false : this.isPendingClicked;
  } 

  onGenerateClearancesheet(event){
    generateClearancesheet(this.correspodenceData.catsid, this.dialog);
  }

  updateThePreviousData(correspondent: Correspondence){
    
    let isLeadOfficeChanged : boolean = correspondent.leadOfficeName != this.correspodenceData.leadOfficeName;
    let isLeadOfficeUsersChanged: boolean = correspondent.leadOfficeUsersIds?.split(';').filter(x => this.correspodenceData.leadOfficeUsersIds?.split(';').indexOf(x) == -1)?.length > 0;
    let isCopiedOfficeChanged : boolean = correspondent.copiedOfficeName?.split(';').filter(x => this.correspodenceData.copiedOfficeName?.split(';').indexOf(x) == -1)?.length > 0;      
    let isCopiedOfficeUsersChanged: boolean = correspondent.copiedUsersIds?.split(';').filter(x => this.correspodenceData.copiedUsersIds?.split(';').indexOf(x) == -1)?.length > 0;
    let isNotesChanged: boolean = correspondent.notes?.replace(/\n/g, '|')?.trim().split('|').filter(x => this.correspodenceData.notes?.replace(/\n/g, '|')?.split('|').indexOf(x) == -1)?.length > 0;

    let isAnyChange : boolean = isLeadOfficeChanged == true || isLeadOfficeUsersChanged == true || isCopiedOfficeChanged == true || isCopiedOfficeUsersChanged == true || correspondent.letterStatus?.toLowerCase() == 'closed';

    // if (this.isSavedClicked == true){          
    //   this.correspodenceData.isSaved = true;
    //   this.correspodenceData.isEmailElligible = false;
    //   this.correspodenceData.isPendingLeadOffice = false;
    // }    
    
    //If previously the item was just saved then send it to the pending lead office and If previously rejected then send it to pending lead office. Only if required response and not closed
    if (
        (
          this.correspodenceData.rejected == true || 
          this.correspodenceData.isPendingLeadOffice == true || 
          this.correspodenceData.isUnderReview == false
        ) 
        && correspondent.letterStatus?.toLowerCase() != 'closed'
        && (this.correspodenceData.isLetterRequired == true)
        && (this.correspodenceData.collaboration == null)
      ){  

        this.correspodenceData.reviewStatus = this.correspodenceData.isUnderReview ? this.correspodenceData.reviewStatus:  'NONE';
        this.correspodenceData.currentReview = this.correspodenceData.isUnderReview ? this.correspodenceData.reviewStatus: 'NONE';  
        this.correspodenceData.catsNotificationId = 1;

        //never send email when saved
        if (this.isSavedClicked != false){          
          this.correspodenceData.isSaved = true;
          this.correspodenceData.isEmailElligible = false;
        }    
        //saved previously and now sent to the lead office
        else if (this.correspodenceData.isSaved == true && this.isSavedClicked == false){
          this.correspodenceData.isEmailElligible = true;  
          this.correspodenceData.isSaved = false;
          this.correspodenceData.rejected = false; 
          this.correspodenceData.isPendingLeadOffice = true;
        }
        //always send email when previously rejected
        else if (this.correspodenceData.rejected){        
          this.correspodenceData.isEmailElligible = true; 
          this.correspodenceData.isPendingLeadOffice = true;
          this.correspodenceData.isSaved = false;
          this.correspodenceData.rejected = false; 
        }
        //send email when previous pending but if only there any change
        else if ((this.correspodenceData.isPendingLeadOffice == true || this.correspodenceData.isUnderReview == false) && isAnyChange == true){        
          this.correspodenceData.isEmailElligible = true; 
          this.correspodenceData.isSaved = false;
          this.correspodenceData.rejected = false; 
          this.correspodenceData.isPendingLeadOffice = true;
        }
        else{
          this.correspodenceData.isPendingLeadOffice = true;
          this.correspodenceData.isEmailElligible = false;
          this.correspodenceData.rejected = false; 
          this.correspodenceData.isSaved = false;
        } 
        //this.correspodenceData.isSaved = false;
        //this.correspodenceData.rejected = false; 
    }
    else{

      let isClosed : boolean = correspondent.letterStatus?.toLowerCase() == 'closed';
      
      this.correspodenceData.isPendingLeadOffice = false;      
      this.correspodenceData.isEmailElligible = false;
      
      //never send email when saved
      if (this.isSavedClicked != false){          
        this.correspodenceData.isSaved = true;
        this.correspodenceData.isEmailElligible = false;
        this.correspodenceData.rejected = false; 
      }   
      //always send email when closed
      else if (isClosed == true){
        this.correspodenceData.isEmailElligible = true;
        this.correspodenceData.isUnderReview = false;
        this.correspodenceData.catsNotificationId = 10;
        this.correspodenceData.isSaved = false;
        this.correspodenceData.rejected = false; 
        this.correspodenceData.isPendingLeadOffice = false;
      }
      //never send email if under review
      else if(this.correspodenceData.isUnderReview == true){
        this.correspodenceData.isEmailElligible = false;
        this.correspodenceData.isSaved = false;
        this.correspodenceData.rejected = false; 
        this.correspodenceData.isPendingLeadOffice = false;
      }
      //Response not required always pending no
      else if (this.correspodenceData.isLetterRequired == false){ 
        this.correspodenceData.isPendingLeadOffice = false;
        this.correspodenceData.isSaved = false;
        this.correspodenceData.rejected = false; 
        if (isAnyChange)
          this.correspodenceData.isEmailElligible = true;
      }
      else if (this.correspodenceData.isLetterRequired == true){ 
        this.correspodenceData.isPendingLeadOffice = false;
        this.correspodenceData.isSaved = false;
        this.correspodenceData.rejected = false; 
        if (isAnyChange)
          this.correspodenceData.isEmailElligible = true;
      }
  
      if (this.IsReopening){    
        this.correspodenceData.isReopen = true;
        this.correspodenceData.catsNotificationId = 11; // reopen closed package
        this.correspodenceData.isEmailElligible = true;
        this.correspodenceData.isUnderReview = false;
        this.correspodenceData.letterStatus = 'Open'
        this.correspodenceData.reviewStatus = null;
        if (this.correspodenceData.collaboration != null){          
          this.correspodenceData.isUnderReview = true;          
          this.correspodenceData.reviewStatus = 'In Progress';
        }
      }
      else{
        if (isNotesChanged ){
          const notes = (this.correspodenceData.notes?.trim() != '' ? "|" : '') + (this.correspodenceData.rejected ? this.correspodenceData.notes?.trim() : formatDate(new Date(), 'MM/dd/yyyy HH:mm', 'en') + ": " + correspondent.notes?.trim());
          this.correspodenceData.notes += (notes == undefined ? '' :  notes);
        }          
        this.correspodenceData.isReopen = false;
      }

      
    }

   

    
    this.correspodenceData.currentUserEmail = this.initialDataSources.currentBrowserUser.MainActingUserEmail;
    this.correspodenceData.currentUserFullName = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;
    this.correspodenceData.currentUserUPN = this.initialDataSources.currentBrowserUser.MainActingUserUPN;
    this.correspodenceData.modifiedBy = correspondent.modifiedBy = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;//this.svc.user ? this.svc.user.displayName : '';
    this.correspodenceData.modifiedTime = correspondent.modifiedTime = new Date;

    this.correspodenceData.correspondentName = correspondent.correspondentName;
    this.correspodenceData.letterSubject = correspondent.letterSubject;
    this.correspodenceData.otherSigners = correspondent.otherSigners;    
    this.correspodenceData.letterCrossReference = correspondent.letterCrossReference;
    this.correspodenceData.whFyi = correspondent.whFyi;
    this.correspodenceData.letterTypeName = correspondent.letterTypeName;    
    this.correspodenceData.externalAgencies = correspondent.externalAgencies;
    this.correspodenceData.letterDate = correspondent.letterDate;    
    this.correspodenceData.letterReceiptDate = correspondent.letterReceiptDate;
    this.correspodenceData.currentReview = correspondent.currentReview;
    this.correspodenceData.isLetterRequired = correspondent.isLetterRequired;
    this.correspodenceData.dueforSignatureByDate = correspondent.dueforSignatureByDate;
    this.correspodenceData.padDueDate = correspondent.padDueDate;
    this.correspodenceData.leadOfficeName = correspondent.leadOfficeName;    
    this.correspodenceData.leadOfficeUsersIds = correspondent.leadOfficeUsersIds;    
    this.correspodenceData.leadOfficeUsersDisplayNames = correspondent.leadOfficeUsersDisplayNames;
    this.correspodenceData.copiedOfficeName = correspondent.copiedOfficeName;
    this.correspodenceData.copiedUsersIds = correspondent.copiedUsersIds;    
    this.correspodenceData.copiedUsersDisplayNames = correspondent.copiedUsersDisplayNames;
    this.correspodenceData.letterStatus = correspondent.letterStatus;    
    this.correspodenceData.notRequiredReason = correspondent.notRequiredReason; 
    this.correspodenceData.isFinalDocument = this.isFinalDocument;
    
    this.correspodenceData.rejected = false;
    this.correspodenceData.rejectionReason = "";
    //this.correspodenceData.rejectedLeadOffices = "";

  }  

   

getStringFromHtml(value: any){
    return getStringFromHtml(value);
}

  getMemberOptionsObjects(options: any[]): any{
    return this.initialDataSources.allOMBUsers.filter(u => options?.includes(u.upn) == true);
  }

  public IsInvalidControlsExist() : boolean {
    return this.allInvalidControls?.length > 0 && this.allInvalidControls?.length <= 5 ? true:false;
  }

  logTheForm(): void {
    console.log('Invalid',  this.allInvalidControls)
    console.log('form: ', this.details);
  }

  toApiDate(bDate) {
    const apiDate: string = new Date(bDate).toUTCString();
    return apiDate;
  }

  ngOnDestroy() {

  }
  

}
