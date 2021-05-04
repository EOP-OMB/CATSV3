import { Component, OnInit, EventEmitter, Output , ViewChild, ChangeDetectorRef, Input, ElementRef, ViewChildren, QueryList} from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort, Sort } from '@angular/material/sort';
import { Observable, of } from 'rxjs';
import { animate, state, style, transition, trigger } from '@angular/animations';
import {  MainLoaderService } from 'src/app/services/main-loader-service';
import { FormGroup, FormControl, FormBuilder,FormArray,Validators } from '@angular/forms';
import { Correspondence } from 'src/app/modules/correspondence/Models/correspondence.model';
import { DatePipe } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { DataSources } from '../../interfaces/data-source';
import { startWith, map, tap, mergeMap } from 'rxjs/operators';
import { Router, ActivatedRoute, NavigationStart, NavigationEnd } from '@angular/router';
import { DashboardsConfigurations } from '../../Data/dashboards.configurations';
import { Surrogate } from 'src/app/models/surrogate.model';
import { MatAutocompleteTrigger } from '@angular/material/autocomplete';
import { setItemUserRoles, getMembersUpnAndFullnames, getMembersFullnames, getMembersUpns, getStringFromHtml, CATSSelectedDashboardOptions, createLocalStorage, clearLocalStorage, getLocalStorageItem, createLocalSessionStorage, clearSessionStorage, getSessionStorageItem, setIncomingDashboardInRootComponent, generateClearancesheet, detectSopportedBrowser, checkSupportedBrowser, UserQuery } from '../../utilities/utility-functions';
import { DialogAlertComponent } from '../dialog-alert/dialog-alert.component';
import { Reviewer } from 'src/app/modules/reviewer/models/reviewer.model';
import { SurrogateReviewerService } from 'src/app/services/surrogateReviewer.service';
import { SurrogateOriginatorService } from 'src/app/services/surrogateOriginator.service';
import { AppConfigService } from 'src/app/services/AppConfigService';
import { RoleGuardService } from 'src/app/security/role-guard.service';
import { CorrespondenceService } from 'src/app/modules/correspondence/services/correspondence-service';
import { CollaborationService } from '../../services/collaboration-service';
import { Collaboration } from 'src/app/modules/originator/models/collaboration.model';
import { MatTabChangeEvent, MatTabGroup } from '@angular/material/tabs';
import { filter } from 'rxjs/operators';
import 'rxjs/add/operator/filter'
import { EventEmitterService } from 'src/app/services/event-emitter.service';
import { CorrespondenceCopiedArchivedService } from 'src/app/modules/correspondence/services/CorrespondenceCopiedArchived-service';
import { CorrespondenceCopiedArchived } from 'src/app/modules/correspondence/Models/correspondenceCopiedArchived.model';
import { PaginationDataSource } from 'ngx-pagination-data-source';
import { CdkColumnDef } from '@angular/cdk/table';
import { CdkDragEnd, CdkDragStart, CdkDropList, moveItemInArray } from '@angular/cdk/drag-drop';
import { THIS_EXPR } from '@angular/compiler/src/output/output_ast';

@Component({
  selector: 'app-dashboard-container',
  templateUrl: './dashboard-container.component.html',
  styleUrls: ['./dashboard-container.component.scss'],
  animations: [
    trigger('detailExpand', [
      state('void', style({ height: '0px', minHeight: '0', visibility: 'hidden' })),
      state('*', style({ height: '*', visibility: 'visible' })),
      transition('void <=> *', animate('525ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ],
  providers: [DatePipe],
})
export class DashboardContainerComponent implements OnInit {

  
  parentRoute: string;
  routePath : string = "";
  routeTitle: string = "";
  routingCurrent: string = "";
  routingDestination: string = "";
  
  pageSizeOptions: string[] = ['5', '10', '20', '50', '100'];
  selectedPageSize : number = 10;

   //radio buttons
  selectedStatus:  string ;  
  searchValue: string = "";
  globalSearch: string = "";
  originatorFilterBy: string = "collaboration"
  eventEditForm: FormGroup;
  formSetColumns:FormGroup;
  formImpersonation:FormGroup;
  formMySurrogate: FormGroup;
  formSurrogate:FormGroup;
  pageSizeForm: FormGroup;
  toggleForm:boolean;
  radioOptions : string[];  
  onOriginatorTabSelectedIndex: number = 0;
  globalInstance: any;
  isPending_Copied: boolean = false; //Applied to Originator Dashboard: can be either Collaboration or Pending/Copied  
  routerLink: string; 


  // value retured from the dialg prompt
  dialogReturnedValue: string = "";

  //making the dashboard data tables responsive on mobile
  isMobile:boolean = true;

  //in put variable receiving the dashboard call. Can be correspondence, originator or reviewer
  @Input() inComingDashboard: string;
  newLetterLabel = "";
  
  //dashboards columnns source and styles
  w_75 = [];
  w_85 = [];
  w_125 = [];
  w_150 =[];
  previousIndex: number;
  currentIndex: number;
  columns = [];
  hiddenColumns = [];
  correspodenceHiddenColumns = [];
  displayedColumns = [];
  correspodenceData : Correspondence[] = []; 
  collaborationData : Collaboration[] = [];
  reviewData : Reviewer[] = [];
  columnsConfigurations :DashboardsConfigurations;
  dataSource : MatTableDataSource < any >;
  dataSource2 : any;
  collapsedColumnsPanel: boolean = false;
  collapsedImpersonationPanel: boolean = false;
  toggleImpersonateText: string = '';
  isImpersonationActive: boolean = false;
  isSurrogateActive: boolean = false; //indicate that the current acting user is a surrogate
  isSurrogateAvailable: boolean = false; // indicates that the current active user has been granted surrogate role
  surrogateAvailableCount: number = 0;
  isReadOnlyActive: boolean = false;
  OMBUsersfilteredOptions: Observable<any[]>;
  SurrogatefilteredOptions: Observable<any[]>;
  MySurrogatefilteredOptions: Observable<any[]>;
  mySurrogateOriginator:any[] = [];
  mySurrogateReviewer:any[] = [];

  impersonationMode: string = '';

  isFieldSearchActivated: boolean = false;

  //activate the detail row expansion
  isExpansionDetailRow = (index, row) => row.hasOwnProperty('detailRow');

  @ViewChildren('in') inputfieldfilters: QueryList<HTMLInputElement>;
  @ViewChild('pageginator') paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort; 
  @ViewChild('CorrespondenceRadioGroup') CorrespondenceRadioGroup: ElementRef;
  @ViewChild(MatAutocompleteTrigger) autocomplete: MatAutocompleteTrigger;

  constructor(
    private fb: FormBuilder,
    private eventEmitterService: EventEmitterService,
    private mainLoaderService: MainLoaderService,
    private correspondenceService: CorrespondenceService,
    private collaborationService : CollaborationService,
    private appConfigService: AppConfigService,
    private roleGuardService: RoleGuardService,
    private surrogateOriginatorService: SurrogateOriginatorService,
    private surrogateReviewerService: SurrogateReviewerService,
    private copiedArchiveService: CorrespondenceCopiedArchivedService,
    public initialDataSources: DataSources,
    private datePipe:DatePipe,
    private dialog: MatDialog,
    private elementRef:ElementRef,    
    private router: Router,
    private activatedRoute: ActivatedRoute,
    ) { 

      
    

      this.formSetColumns = this.fb.group({
        checkArray: this.fb.array([])
      });

      this.formImpersonation = this.fb.group({
        impersonationInput: ['', Validators.required]
      });

      this.formSurrogate = this.fb.group({
        surrogateInput: ['', Validators.required]
      })

      this.formMySurrogate = this.fb.group({
        mySurrogateInput: ['', Validators.required]
      });

      this.pageSizeForm = this.fb.group({
        pageSizeSelect: [this.selectedPageSize.toString()]
      }); 

      activatedRoute.parent.url.subscribe((urlPath) => {
        this.parentRoute = urlPath[urlPath.length - 1].path;

        // //Dashboards RouteGuard checking if the current user is allowed
        // if (!this.roleGuardService.getAuthorization(this.parentRoute)){
        //   router.navigateByUrl('/forbidden');
        // }
      })

      activatedRoute.url.subscribe(x => {
        this.routePath = x.length > 0 ?  x[0]?.path : "";
      });

      activatedRoute.data.subscribe(x => {
        this.routeTitle = x ? x.title : "";
      });

      this.router.events.pipe(
        filter(event => event instanceof NavigationEnd)).subscribe((e: NavigationEnd) => {
          this.routingCurrent = e.url;
        });

      this.router.events
        .filter(event=> event instanceof NavigationStart)
        .subscribe((event:NavigationStart)=>{
          this.routingDestination = event.url          
          
          //this keeps track of the dashboard options to be used when the page is reused after nevigated back from the child route
          //From the parent to the child route
          if (this.routingDestination.includes(this.routingCurrent)){
            this.setDashboardOptionSession();
          }
          //From the child to the Parent route
          else if (this.routingDestination.includes(this.routingCurrent)){
            
          }
          //leaving to a new parent route
          else if (this.routingDestination.includes(this.routingCurrent) == false && this.routingCurrent.includes(this.routingDestination) == false){
            clearSessionStorage('cATSSelectedDashboardOptions');
          }
          
        });
      
  }

  ngOnInit() {

    // let isUnsupportedBrowser : boolean = detectSopportedBrowser();
    // if (isUnsupportedBrowser == false){
    //   const dialogRef = this.dialog.open(DialogAlertComponent, {
    //     width: '500px',
    //     data: 'Unsupported browser detected! Please use either CHROME or EDGE for CATS'
    //   });
      
    //   this.router.navigateByUrl('/home');
    //   return;
    // }

    this.inComingDashboard = this.parentRoute;

    //intialize the radio button group
    this.eventEditForm = new FormGroup({          
      'costatus': new FormControl(),
      'revstatus': new FormControl(),
      'matTabGroupOriginator': new FormControl()
      }); 

      //Create new letter  
      if (this.eventEmitterService.subsCreateNewLetter != undefined) {    
        this.eventEmitterService.subsCreateNewLetter = null;          
      }  
      this.eventEmitterService.subsCreateNewLetter = this.eventEmitterService.invokeCreateNewLetterFunction.subscribe((name:string) => {    
        this.onNewLetterCreation();    
      });

    //Open/close the Columns configuration panel  
    if (this.eventEmitterService.subsVarColumnsSettings != undefined) {    
      this.eventEmitterService.subsVarColumnsSettings = null;          
    }  
    this.eventEmitterService.subsVarColumnsSettings = this.eventEmitterService.invokeOpenColumnsSettingsFunction.subscribe((name:string) => {    
      this.onConfigulationPanelGearClick();    
    });

    //Open/close the ImpersonationSettings configuration pane
    if (this.eventEmitterService.subsVarImpersonationSettings == undefined) { 
      this.eventEmitterService.subsVarImpersonationSettings = null;
    };
    this.eventEmitterService.subsVarImpersonationSettings = this.eventEmitterService.invokeOpenImpersonationSettingsFunction.subscribe((name:string) => {    
      this.onImpersonationPanelOpenClick();    
    });  
    
    //check if the browser is supported
    if (!checkSupportedBrowser(this.dialog, this.router)) return;
    
    //intialize the acting User My Surragate as Originator
    this.mySurrogateOriginator = this.initialDataSources.currentBrowserUser.MySurrogateOriginator;

    //intialize the acting User My Surragate as Reviewer
    this.mySurrogateReviewer = this.initialDataSources.currentBrowserUser.MySurrogateReviewer;

    //intialize the surrogate matselect
    this.MySurrogatefilteredOptions =  this.formMySurrogate.controls['mySurrogateInput'].valueChanges
      .pipe(
        startWith(''),
        map(value =>           
          this._filterMySurrogateusers(value)
          )
      );

    //intialize the surrogate matselect
    this.SurrogatefilteredOptions =  this.formSurrogate.controls['surrogateInput'].valueChanges
      .pipe(
        startWith(''),
        map(value =>           
          this._filterSurrogateusers(value)
          )
      );

    //intialize the impersonation matselect
    this.OMBUsersfilteredOptions =  this.formImpersonation.controls['impersonationInput'].valueChanges
      .pipe(
        startWith(''),
        map(value =>           
          this._filterImpersonationusers(value)
          )
      );
      
      //Set the Current user acting role options
      if (this.initialDataSources.currentBrowserUser.CurrentActingRole == "Surrogate User"){
        this.isSurrogateActive = true;
        this.isImpersonationActive = false;
      }
      if (this.initialDataSources.currentBrowserUser.CurrentActingRole == "Impersonated User"){
        this.isSurrogateActive = false;
        this.isImpersonationActive = true;
      }
      else{
        this.isSurrogateActive = false;
        this.isImpersonationActive = false;
      }

      if (this.initialDataSources.currentBrowserUser.SurrogateOriginator.length > 0 || this.initialDataSources.currentBrowserUser.SurrogateReviewer.length > 0){
        if (this.parentRoute == 'originator'){
          this.surrogateAvailableCount = this.initialDataSources.currentBrowserUser.SurrogateOriginator.length;
        }
        else if (this.parentRoute == 'reviewer'){
          this.surrogateAvailableCount = this.initialDataSources.currentBrowserUser.SurrogateReviewer.length;
        }        
      }
      else{
        this.surrogateAvailableCount = 0;
      }

      //Disable the New Letter button if acting user is member of has role MemberOfCATSCorrespondenceReadOnly
      if (this.initialDataSources.currentBrowserUser.MemberOfCATSCorrespondenceReadOnly == true){
        this.isReadOnlyActive = true;
      }
      else{
        this.isReadOnlyActive = false;
      }           
    
    //set dashboard options    
    this.setDashboardOptions();

    // send the dahsboard title to the root component (appComponent)
    setIncomingDashboardInRootComponent(this.parentRoute, this.eventEmitterService);

    //load the dashboards
    //get data suscribed
    this.getDashboardsData();   

      
  }

  ngAfterViewInit() {

    if (this.inComingDashboard != "correspondence" && this.inComingDashboard != "originator" && this.inComingDashboard != "reviewer"){
      this.dataSource.paginator = this.paginator;    
      this.dataSource.sort = this.sort; 
    }                       
  }

  ngOnDestroy() {
    //this.globalInstance();
  }

  

  onItemStatusChanged(event: any){ 
    
    this.selectedStatus = event.value;
    this.setDataSource();
  }

  setDataSource(){
    const cATSSelectedDashboardOptions: CATSSelectedDashboardOptions = getSessionStorageItem('cATSSelectedDashboardOptions');
    this.dataSource2 = new PaginationDataSource<Correspondence, UserQuery>(
      (request, query) => 
        this.inComingDashboard =='correspondence' ? 
        this.correspondenceService.page(request, this.selectedStatus, query, cATSSelectedDashboardOptions) : 
        this.inComingDashboard =='originator' ? 
        this.collaborationService.page(request, this.selectedStatus, query, this.originatorFilterBy):
        this.collaborationService.page(request, this.selectedStatus, query, this.inComingDashboard),
      {property: 'modifiedTime', order: 'desc'},
      {search: '', registration: undefined}, 
      this.selectedPageSize
    );
    this.clearSessionStorage('cATSSelectedDashboardOptions');
  }  

  setDisplayedColumns() {
    this.columns.forEach(( colunm, index) => {
      colunm.index = index;
      this.displayedColumns[index] = colunm.columnDef;
    });
  } 

  onHeaderClick($event, index: number){
    this.currentIndex = index;
  }

  dragStarted(event: CdkDragStart, index: number ) {
    if (index == 0){
      event.source._dragRef.reset();
    }
    else{
      this.previousIndex = index;
    }
  }

  onDragEnded(event: CdkDragEnd, index: number): void {
      // if (this.currentIndex == 0) {
      //     event.source._dragRef.reset();
      // }
  }

  dropListDropped(event: CdkDropList, index: number = 0) {
    if (event) {
      this.currentIndex = this.currentIndex == 0 ? this.previousIndex : this.currentIndex;
      moveItemInArray(this.columns, this.previousIndex, this.currentIndex);
      this.setDisplayedColumns();
    }
  }

  onSearchInput(event: any, column: any, isglobalSearch?: boolean, columnheader?: string){
    this.clearSessionStorage('cATSSelectedDashboardOptions');
    if (columnheader?.toLowerCase().includes('date') && event.trim() != ''){
      var date_regex = /^\d{1,2}[./-]\d{1,2}[./-]\d{4}$/;
      if (!(date_regex.test(event))) {
          event = '';
          //return false;
      }
    }
    this.inputfieldfilters.forEach((input:any) => {
      if (input.nativeElement.id != column){
        input.nativeElement.value = '';
      }
    });

    if (isglobalSearch == true){      
      this.isFieldSearchActivated = false;
      this.globalSearch = event;
      this.dataSource2.queryBy({search: event.trim()});
    }
    else{
      
      this.globalSearch = '';
      this.isFieldSearchActivated = event.trim() == '' ? false : true;
      this.dataSource2.queryBy({search: event.trim() == '' ? '' : 'filterByColumn|' + column + '|' + event});
    }
    
  }

  onchangePageSize(event){
    this.selectedPageSize = event.value;
    this.setDataSource();
  }

  sortBy({active, direction}: Sort) {
    this.dataSource2.sortBy({
      property: active as keyof Correspondence,
      order: direction || 'asc'
    })
  }

  applyFilter(filterValue: string) {
    filterValue = filterValue.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase(); // Datasource defaults to lowercase matches
    this.dataSource2.filter = filterValue;
  }  

  onOriginatorTabChanged(tabChangeEvent: MatTabChangeEvent): void {
    
    //clear all the search/filter fields
    this.inputfieldfilters.forEach((input:any) => {
      input.nativeElement.value = '';
    });

    this.dataSource = null;
    this.onOriginatorTabSelectedIndex = tabChangeEvent.index;
    this.originatorFilterBy = tabChangeEvent.tab.textLabel.toLowerCase();
    //console.log('tabChangeEvent => ', tabChangeEvent);
    //console.log('index => ', tabChangeEvent.index);
    this.selectedStatus = 'Open'; 
    this.searchValue = ''; 
    this.globalSearch = '';

    //Save the current radio group selection
    this.setDashboardOptionSession();

    //Set the table columns sources
    this.setDashboardOptions(true);

    //load the data
    this.getDashboardsData();
  }

  radioChange(event) {  
    
    //clear all the search/filter fields
    this.inputfieldfilters.forEach((input:any) => {
      input.nativeElement.value = '';
    });  
    this.searchValue = "";   
    this.globalSearch = ''; 
    this.selectedStatus = event.value;// event.value;

    //Save the current radio group selection
    this.setDashboardOptionSession();
    //load the data
    this.getDashboardsData();
  }

  async getDashboardsData(){

    if (this.initialDataSources.currentBrowserUser.LoginName == ''){
      //Set the browser User as the impersonated and reload the page            
      await this.appConfigService.load();
    }    
    
    this.setDataSource(); 
  } 

  changedoriginatorToggle(event){
    this.isPending_Copied = event.checked;
    this.selectedStatus = this.radioOptions[0];
    this.getDashboardsData();
  }

  setFilterByOfficeOrUser(){    
    this.correspodenceData = this.correspodenceData.filter(x => this.initialDataSources.currentBrowserUser.MemberGroupsCollection.indexOf(x.leadOfficeName) != -1);
  }
  
  setDashboardOptions(isInitialLoad: boolean = true){

    this.columnsConfigurations = new DashboardsConfigurations(this.datePipe);
    
    //current or previous radio group button selection
    const cATSSelectedDashboardOptions: CATSSelectedDashboardOptions = getSessionStorageItem('cATSSelectedDashboardOptions');

    if (this.inComingDashboard === "correspondence"){

      this.radioOptions = this.columnsConfigurations.correspondenceRadioOptions;
      this.selectedStatus = this.radioOptions[0];
      
      //set table columns 
      this.w_75= this.columnsConfigurations.w_75;
      this.w_85= this.columnsConfigurations.w_85;
      this.w_125= this.columnsConfigurations.w_125;
      this.w_150= this.columnsConfigurations.w_150;
      
      var savedHiddenColumns = this.getLocalStorageItem("correspondenceHiddenColumns");// check first if local storage configuration exists
      this.hiddenColumns = savedHiddenColumns ? savedHiddenColumns as string[] : DashboardsConfigurations.correspondenceHiddenColumns.slice(0);   
      this.columns = this.columnsConfigurations.getColumns("correspondence");
      //set the New Collaboration button caption
      this.newLetterLabel = "Create New Letter";
    }
    else if (this.inComingDashboard === "originator"){  
      
      this.originatorFilterBy = cATSSelectedDashboardOptions != null ? cATSSelectedDashboardOptions.area : this.originatorFilterBy;
      var savedHiddenColumns = this.getLocalStorageItem("originatorHiddenColumns");// check first if local storage configuration exists
      this.hiddenColumns = savedHiddenColumns ? savedHiddenColumns as string[] : DashboardsConfigurations.originatorHiddenColumns.slice(0);   
      //set table columns 
      this.radioOptions = [];
      if (this.originatorFilterBy == 'collaboration'){
        this.radioOptions = this.columnsConfigurations.collaborationRadioOptions;
        this.columns = this.columnsConfigurations.getColumns("originator");
        this.onOriginatorTabSelectedIndex = 0;
      }
      else if (this.originatorFilterBy == 'office data'){
        this.radioOptions = this.columnsConfigurations.officedataRadioOptions;
        this.columns = this.columnsConfigurations.getColumns("originator");
        this.onOriginatorTabSelectedIndex = 1;
      }
      else if (this.originatorFilterBy == 'pending'){
          this.radioOptions = this.columnsConfigurations.pendingRadioOptions;
          this.columns = this.columnsConfigurations.getColumns("originator-pending");
          this.onOriginatorTabSelectedIndex = 2;
      }
      else if (this.originatorFilterBy == 'copied'){
        this.radioOptions = this.columnsConfigurations.copiedRadioOptions;
        this.columns = this.columnsConfigurations.getColumns("originator-pending");
        this.onOriginatorTabSelectedIndex = 3;
      }
      else{
        this.radioOptions = this.columnsConfigurations.collaborationRadioOptions;
        this.columns = this.columnsConfigurations.getColumns("originator");
        this.onOriginatorTabSelectedIndex = 0;
        this.correspodenceData[0].collaboration.fYIUsers.some(x => x.fyiUpn.toLowerCase().trim().includes(this.initialDataSources.currentBrowserUser.MainActingUserUPN.toLowerCase().trim()))
      }

      if(isInitialLoad){
        this.selectedStatus = this.radioOptions[0];
      } 
      
      //set the New Collaboration button caption
      this.newLetterLabel = "New Collaboration";

    }
    else if (this.inComingDashboard === "reviewer"){
      //set table columns 
      var savedHiddenColumns = this.getLocalStorageItem("reviewereHiddenColumns");// check first if local storage configuration exists
      this.hiddenColumns = savedHiddenColumns ? savedHiddenColumns as string[] : DashboardsConfigurations.reviewerHiddenColumns.slice(0);  
      this.radioOptions = this.columnsConfigurations.reviewRadioOptions; 
      this.columns = this.columnsConfigurations.getColumns("reviewer");

      if(isInitialLoad){
        this.selectedStatus = this.radioOptions[0];
      } 
      
      this.routerLink = "/reviewer/cdetails";

    }

    this.displayedColumns = this.columns.map(c => c.columnDef);//to be removed later. for originator and review dashboard
    this.setDisplayedColumns();       

    //when coming from the child route then override the selected option from the previous parent selections
    if (cATSSelectedDashboardOptions != null){
      this.selectedStatus = cATSSelectedDashboardOptions.filter;
      this.originatorFilterBy = cATSSelectedDashboardOptions.area;
      this.searchValue = this.globalSearch = cATSSelectedDashboardOptions.searchCriteria != undefined ? cATSSelectedDashboardOptions.searchCriteria : '';
    }

  }

  setOriginatorRouterLink(isPending: boolean){

    if(this.inComingDashboard === "originator"){
      if (isPending == true){
        this.routerLink = "/originator/opending";
      }
      else{
        this.routerLink = "/originator/manage";
      }
    } 
    else {
      this.routerLink = "";
    }

    return this.routerLink;

  }

  setCheckboxColumnsConfig(value){
    for (const field in this.hiddenColumns) { // 'field' is a string  
      if (this.hiddenColumns.indexOf(value) != -1) {
        return false;
      }
      else{
        return true;
      }
    }
  }

  //Collapse the Columns configuration panel by clicking the  gear icon
  onConfigulationPanelGearClick(){
    if (this.collapsedColumnsPanel){
      this.collapsedColumnsPanel = false;
    }
    else{
      this.collapsedColumnsPanel = true;
    }
  }
  

  onNewLetterCreation(){
    if (this.parentRoute == 'correspondence'){
      this.router.navigateByUrl('/correspondence/cform')
    }
    else if (this.parentRoute == 'originator'){
      this.router.navigateByUrl('/originator/onewcollaboration')
    }
  }

  //Show the Impersonation Panel
  onImpersonationPanelOpenClick(){
    if (this.collapsedImpersonationPanel){
      this.collapsedImpersonationPanel = false;
    }
    else{
      this.collapsedImpersonationPanel = true;
    }
  }

  onRemoveMySurrogate(userUpn: string, userId: number, selectedUser: string){
    let surrogate: Surrogate = new Surrogate();
    surrogate.id = userId;
    surrogate.catsUser = this.initialDataSources.currentBrowserUser.PreferredName;
    surrogate.catsUserUPN = this.initialDataSources.currentBrowserUser.LoginName;
    surrogate.modifiedBy = this.initialDataSources.currentBrowserUser.PreferredName;
    surrogate.surrogate = selectedUser;
    surrogate.surrogateUPN = userUpn;
    surrogate.modifiedTime = new Date();
    surrogate.deletedBy = this.initialDataSources.currentBrowserUser.PreferredName;
    surrogate.deletedTime = new Date();
    surrogate.isDeleted = true;

    if (this.parentRoute == 'originator'){
      this.surrogateOriginatorService.updateSurrogate(surrogate).subscribe(async response =>{
        this.formMySurrogate.controls["mySurrogateInput"].setValue('');
        // this.appConfigService.load().then(res => {
        //   this.ngOnInit();
        // });
        await this.mainLoaderService.setCurrentUserMetadata();
        this.ngOnInit(); 
      });
    }
    else{
      this.surrogateReviewerService.updateSurrogate(surrogate).subscribe(async response =>{
        this.formMySurrogate.controls["mySurrogateInput"].setValue('');
        // this.appConfigService.load().then(res => {
        //   this.ngOnInit();
        // }); 
        await this.mainLoaderService.setCurrentUserMetadata();
        this.ngOnInit();
      });
    }  
               
  }

  onAddMySurrogate(){

    const selectedUserUpn = this.formMySurrogate.controls["mySurrogateInput"].value; 
    let surrogate: Surrogate = new Surrogate();
    surrogate.catsUser = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;
    surrogate.catsUserUPN = this.initialDataSources.currentBrowserUser.MainActingUserUPN;
    surrogate.createdBy = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;
    surrogate.modifiedBy = this.initialDataSources.currentBrowserUser.MainActingUserUPN;
    surrogate.surrogate = getMembersFullnames(selectedUserUpn,this.initialDataSources.allOMBUsers);
    surrogate.surrogateUPN = selectedUserUpn;
    surrogate.createdTime = new Date();// this.datePipe.transform(new Date(), 'yyyy-MM-dd')
    surrogate.modifiedTime = new Date();//this.datePipe.transform(new Date(), 'yyyy-MM-dd')

    if (this.parentRoute == 'originator'){     
      surrogate.type = 'originator';
      if(this.mySurrogateOriginator.some(x => this.mySurrogateOriginator.some(x => x.upn == selectedUserUpn))){
        this.formMySurrogate.controls['mySurrogateInput'].setValue('');
        //alert('The selected User (' + selectedUser + ') is already a surrogate!' );
        const dialogRef = this.dialog.open(DialogAlertComponent, {
          width: '500px',
          data: 'The selected User (' + selectedUserUpn + ') is already a surrogate!'
        });
        return;
      }
      
  
      this.surrogateOriginatorService.addSurrogate(surrogate).subscribe(async response =>{
        // this.appConfigService.load().then(res => {
        //   this.ngOnInit();
        //   this.autocomplete.closePanel();
        // }); 
        
        await this.mainLoaderService.setCurrentUserMetadata();
        this.ngOnInit(); 
        this.autocomplete.closePanel();
      });           

    }
    else{     
      surrogate.type = 'reviewer';    
      if(this.mySurrogateReviewer.some(x => this.mySurrogateReviewer.some(x => x.upn == selectedUserUpn))){
        this.formMySurrogate.controls['mySurrogateInput'].setValue('');
        //alert('The selected User (' + selectedUser + ') is already a surrogate!' );
        const dialogRef = this.dialog.open(DialogAlertComponent, {
          width: '500px',
          data: 'The selected User (' + selectedUserUpn + ') is already a surrogate!'
        });
        return;
      }
  
      this.surrogateReviewerService.addSurrogate(surrogate).subscribe(async response =>{
        // this.appConfigService.load().then(res => {
        //   this.ngOnInit();
        //   this.autocomplete.closePanel();
        // }); 
        
        await this.mainLoaderService.setCurrentUserMetadata();
        this.ngOnInit(); 
        this.autocomplete.closePanel();
      });           
    }
    
    this.autocomplete.closePanel();
  }

  //Load the selected Impersonated User
  async onLoadImpersonation(mode: string){

    //clear all the sessions
    if (mode == 'surrogate'){
      this.clearSessionStorage('surrogateUser');
    }
    else{
      this.clearSessionStorage('impersonationUser');
      this.clearSessionStorage('surrogateUser');
    }

    const selectedValue = mode == 'impersonation' ? this.formImpersonation.controls['impersonationInput'].value : this.formSurrogate.controls['surrogateInput'].value;
    if (selectedValue != ''){
      const selectUser = mode == 'impersonation'? 
                                    this.initialDataSources.allOMBUsers.filter(option => selectedValue.toLowerCase().includes(option.upn.toLowerCase()))[0] || null : //selected  impersonation
                                    this.parentRoute == 'originator' ?
                                    this.initialDataSources.currentBrowserUser.SurrogateOriginator.filter(option => selectedValue.toLowerCase().includes(option.upn.toLowerCase()))[0] || null :
                                    this.initialDataSources.currentBrowserUser.SurrogateReviewer.filter(option => selectedValue.toLowerCase().includes(option.upn.toLowerCase()))[0] || null ;//selected  impersonation
      if (selectUser != null){
          const selectedUserUpn = selectUser.upn;
          const impersonationUser = {
            upn:selectUser.upn, 
            displayName:selectUser.displayName, 
            emailAddress:selectUser.emailAddress,
            id:'',
            currentActingRole: this.inComingDashboard.toUpperCase() == "ORIGINATOR" ? "SURROGATE ORIGINATOR" : "SURROGATE REVIEWER"
          };
          //set the impersonated user
          this.createLocalSessionStorage(impersonationUser,mode == 'impersonation' ? 'impersonationUser' : 'surrogateUser');
          
          //Set the browser User as the impersonated and reload the page  
          // this.appConfigService.load().then(res => {
          //   this.router.navigateByUrl(this.router.url);
          // }); 
          await this.mainLoaderService.setCurrentUserMetadata();
          this.router.navigateByUrl(this.router.url);
      }  
    }
  }

  onGenerateClearanceSheet(CATSID : string){
    generateClearancesheet(CATSID, this.dialog);
  }

  private _filterMySurrogateusers(value: string): string[] {
    const filterValue = value.toLowerCase();
    return this.initialDataSources.currentBrowserUser.Mycolleagues.filter(option => option.userUPN.toLowerCase().includes(filterValue.trim()) || option.userFullName.toLowerCase().includes(filterValue.trim()))
    //this.initialDataSources.allOMBUsers.filter(option => option.displayName.toLowerCase().includes(filterValue.trim()));  

  }

  private _filterSurrogateusers(value: string): string[] {
    const filterValue = value == undefined ? '' : value.toLowerCase();
    if (this.parentRoute == 'originator'){
      return this.initialDataSources.currentBrowserUser.SurrogateOriginator.filter(option => option.upn.toLowerCase().includes(filterValue.trim()) ||  option.displayName.toLowerCase().includes(filterValue.trim()));
    }
    else{
      return this.initialDataSources.currentBrowserUser.SurrogateReviewer.filter(option => option.upn.toLowerCase().includes(filterValue.trim()) ||  option.displayName.toLowerCase().includes(filterValue.trim()));
    }    
  }

  private _filterImpersonationusers(value: string): string[] {
    const filterValue =  value == undefined ? '' : value.toLowerCase();
    
    return this.initialDataSources.allOMBUsers.filter(option => option.upn.toLowerCase().includes(filterValue.trim()) ||  option.displayName.toLowerCase().includes(filterValue.trim()));
  }

  removeDuplicateValue(myArray){ 
    var newArray = [];
 
    this.initialDataSources.allOMBUsers.forEach(myArray, function(value, key) {
      var exists = false;
      this.initialDataSources.allOMBUsers.forEach(newArray, function(val2, key) {
        if(this.initialDataSources.allOMBUsers.equals(value?.id, val2?.id)){ exists = true }; 
      });
      if(exists == false && value?.id != "") { newArray.push(value); }
    });
  
    return newArray;
  }

  onArchive(itemId, option){
    //set the archive flag for the selected item   
    
    this.correspondenceService.loadCorrespondenceById(itemId).subscribe(response =>{

      
      let correspondenceCopiedArchived : CorrespondenceCopiedArchived = response.correspondenceCopiedArchiveds.find(x => x.archivedUserUpn == this.initialDataSources.currentBrowserUser.MainActingUserUPN);
      if (correspondenceCopiedArchived == undefined){
        correspondenceCopiedArchived = new CorrespondenceCopiedArchived();
        correspondenceCopiedArchived.correspondenceId = response.id;
        correspondenceCopiedArchived.archivedUserFullName = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;          
        correspondenceCopiedArchived.archivedUserUpn = this.initialDataSources.currentBrowserUser.MainActingUserUPN;
        correspondenceCopiedArchived.createdBy = this.initialDataSources.currentBrowserUser.MainActingUserUPN;
        correspondenceCopiedArchived.createdTime = new Date();
      }
     
      correspondenceCopiedArchived.modifiedBy = this.initialDataSources.currentBrowserUser.MainActingUserUPN;      
      correspondenceCopiedArchived.modifiedTime = new Date();

      if (option == "Archived"){

        correspondenceCopiedArchived.isDeleted = false;
        correspondenceCopiedArchived.deletedTime = null;
        correspondenceCopiedArchived.deletedBy = null;
      }
      else{
        correspondenceCopiedArchived.isDeleted = true;
        correspondenceCopiedArchived.deletedTime = new Date();
        correspondenceCopiedArchived.deletedBy = this.initialDataSources.currentBrowserUser.MainActingUserUPN;
      }     
      
      this.copiedArchiveService.updateCorrespondenceCopiedArchived(correspondenceCopiedArchived).subscribe(response => {
        this.setDataSource();
      }); 

    });
  }

  //Hide/show columns when checkbox is checked or unchecked
  onColumnCheckboxChange(e) {
    const checkArray: FormArray = this.formSetColumns.get('checkArray') as FormArray;
  
    if (e.target.checked) {
      if(this.hiddenColumns.indexOf(e.target.value) != -1){
        var index = this.hiddenColumns.indexOf(e.target.value);
        this.hiddenColumns.splice(index, 1);
      }      
    } 
    else {     
      if(this.hiddenColumns.indexOf(e.target.value) == -1){
        this.hiddenColumns.push(e.target.value);
      }      
    }
    //create local storage  to persist users configuration
    if (this.inComingDashboard == 'correspondence'){
      this.clearLocalStorage("correspondenceHiddenColumns");
      this.createLocalStorage(this.hiddenColumns,"correspondenceHiddenColumns");
    }
    else if (this.inComingDashboard == 'originator'){
      this.clearLocalStorage("originatorHiddenColumns");
      this.createLocalStorage(this.hiddenColumns,"originatorHiddenColumns");
    }
    else if (this.inComingDashboard == 'reviewer'){
      this.clearLocalStorage("reviewerHiddenColumns");
      this.createLocalStorage(this.hiddenColumns,"reviewerHiddenColumns");
    }  
  }

  onColumnsConfigReset(){
    this.hiddenColumns = DashboardsConfigurations.correspondenceHiddenColumns.slice(0);
    //clear local storage  to persist users configuration
    if (this.inComingDashboard == 'correspondence'){
      this.clearLocalStorage("correspondenceHiddenColumns");
    }
    else if (this.inComingDashboard == 'originator'){
      this.clearLocalStorage("originatorHiddenColumns");
    }
    else if (this.inComingDashboard == 'reviewer'){
      this.clearLocalStorage("reviewerHiddenColumns");
    }
  }  

  displayFn = value => {
    return getMembersFullnames(value, this.initialDataSources.allOMBUsers);
  }
  
  setMembersFullnames(value: any): any{    
    return getMembersFullnames(value, this.initialDataSources.allOMBUsers);
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

  getStringFromHtml(text){
      const html = text;
      const div = document.createElement('div');
      div.innerHTML = html;
      return div.textContent || div.innerText || '';
  }

  setDashboardOptionSession(){
    const cATSSelectedDashboardOptions = new CATSSelectedDashboardOptions();
    cATSSelectedDashboardOptions.title = this.inComingDashboard;
    cATSSelectedDashboardOptions.filter = this.selectedStatus;
    cATSSelectedDashboardOptions.area = this.originatorFilterBy;
    cATSSelectedDashboardOptions.selectCATSID = '';
    cATSSelectedDashboardOptions.searchCriteria = this.globalSearch;//this.searchValue;

    this.clearSessionStorage('cATSSelectedDashboardOptions');
    this.createLocalSessionStorage(cATSSelectedDashboardOptions, 'cATSSelectedDashboardOptions');
  }

  createLocalStorage(object: any, key : string){
    localStorage.setItem(key,JSON.stringify(object));
  }

  clearLocalStorage(key: string){
    localStorage.removeItem(key);
  }

  getLocalStorageItem(key: string): any{
   return JSON.parse(localStorage.getItem(key))
  }

  createLocalSessionStorage(object: any, key : string){
    sessionStorage.setItem(key,JSON.stringify(object));
  }

  clearSessionStorage(key: string){
    sessionStorage.removeItem(key);
  }

  getSessionStorageItem(key: string): any{
   return JSON.parse(sessionStorage.getItem(key))
  }


}