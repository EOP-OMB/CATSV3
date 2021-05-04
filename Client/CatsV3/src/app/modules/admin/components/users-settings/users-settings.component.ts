import { SelectionModel } from '@angular/cdk/collections';
import { QueryList, ViewChild, ViewChildren } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MatCheckbox } from '@angular/material/checkbox';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MatTabChangeEvent, MatTabGroup } from '@angular/material/tabs';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { startWith, map } from 'rxjs/operators';
import { CATSUser, LeadOffice, UserEntity } from 'src/app/models/LeadOffice.model';
import { LeadOfficeMember } from 'src/app/models/leadOfficeMember.model';
import { LeadOfficeOfficeManager } from 'src/app/models/LeadOfficeOfficeManager.model';
import { HelpDocumentService } from 'src/app/modules/help/services/helpDocument.service';
import { DataSources } from 'src/app/modules/shared/interfaces/data-source';
import { DlGroupsService } from 'src/app/modules/shared/services/dl-groups.service';
import { LeadOfficeServiceService } from 'src/app/modules/shared/services/lead-office-service.service';
import { checkSupportedBrowser, getMembersFullnames } from 'src/app/modules/shared/utilities/utility-functions';
import { AppConfigService } from 'src/app/services/AppConfigService';
import { MainLoaderService } from 'src/app/services/main-loader-service';
import { AdminUserSettingService } from '../../services/admin-usersettings-services';
import Swal from 'sweetalert2/dist/sweetalert2.all.js';

@Component({
  selector: 'app-users-settings',
  templateUrl: './users-settings.component.html',
  styleUrls: ['./users-settings.component.scss']
})
export class UsersSettingsComponent implements OnInit {

  displayedColumns: string[] = ['select', 'userFullName', 'EntityName', 'delete'];
  dataSource: any ;
  selection = new SelectionModel<CATSUser>(true, []);

  catsEntities : UserEntity[] = [];
  selected: number = 0;
  isSelected: boolean = false;
  isLeadOfficeActivated: boolean = true;
  isSupportTeamActivated: boolean = false;
  selectedOption: string = 'lead';
  selectedOptionTitle: string = 'Lead Offices';
  leadOffices: any[] = []; 
  dlGroups: any[] = [];
  selectedMembers: any[] = [];
  checkedOffices:  UserEntity[] = [];
  entitiesForm: FormGroup;
  selectUserform: FormGroup;
  selectOfficeFilterform: FormGroup;
  userfilteredOptions: Observable<any[]>;
  userOfficefilteredOptions: Observable<any[]>;
  selectedUsersToAdd: any[] = [];
  selectedManagerToAdd: LeadOfficeOfficeManager[] = [];
  filterUserOfficeAcivated: boolean = false;

  @ViewChildren('leadOffice') ckboxes:QueryList<MatCheckbox>;
  @ViewChildren('leadOfficeUsers') membersckboxes:QueryList<MatCheckbox>;
  @ViewChild('leadOfficesUserFilter') leadOfficesUserFilter:MatCheckbox;
  @ViewChild('userTabGroup') userTabGroup:MatTabGroup ;
  @ViewChild(MatPaginator) paginator: MatPaginator;
  @ViewChild(MatSort) sort: MatSort;

  constructor(
    private fb: FormBuilder, 
    private appConfigService: AppConfigService, 
    private mainLoaderService : MainLoaderService, 
    private adminService: AdminUserSettingService, 
    private leadOfficeService: LeadOfficeServiceService,
    private dlService: DlGroupsService,
    public initialDataSources: DataSources, 
    private dialog: MatDialog, 
    private router: Router) {
    this.selectUserform = this.fb.group({
      userInput: ['']
    });
    this.selectOfficeFilterform = this.fb.group({
      userFilterInput: ['']
    });
    this.entitiesForm = this.fb.group({
      lead_Offices: [''],
      dL_Groups: ['']
    });
   }
  ngOnInit(): void {
    
    //check if the browser is supported
    if (!checkSupportedBrowser(this.dialog, this.router)) return; 
    
    this.seCATEntities();
    
    this.userOfficefilteredOptions =  this.selectOfficeFilterform.controls['userFilterInput'].valueChanges
      .pipe(
        startWith(''),
        map(value =>           
          this._filterusers(value)
          )
      );
    
      this.userfilteredOptions =  this.selectUserform.controls['userInput'].valueChanges
        .pipe(
          startWith(''),
          map(value =>           
            this._filterusers(value)
            )
        );

        if (this.initialDataSources.currentBrowserUser.MemberOfAdmins == true){
          this.isSupportTeamActivated = true;
        }
  }  

  ngAfterViewInit() {
    if (this.initialDataSources.currentBrowserUser.MemberOfCATSOfficeManagers && 
      !this.initialDataSources.currentBrowserUser.MemberOfAdmins && 
      !this.initialDataSources.currentBrowserUser.MemberOfCATSSupport){  
        
        this.onSetCheckedForManager();

        this.seCATEntities();
        this.resetLeadOfficeCheckBoxes();
        this.onLoadCheckedEntitiesMembers();  
        this.onLoadCheckedEntitiesMembers();
        this.extractSelectedMembers();
    }
  }

  seCATEntities(){    
    
    if (this.selectedOption == 'lead'){
      this.catsEntities = [];
      this.leadOffices = this.initialDataSources.leadOffices?.filter(o => o.isHidden != true).sort((a, b) => (a.name > b.name) ? 1 : -1);
      this.catsEntities = []
      this.leadOffices.forEach(office => {
      var catsEntity : UserEntity = new UserEntity();
      catsEntity.id = office.id;
      catsEntity.name = office.name;
      catsEntity.description = office.description;
      catsEntity.createdBy = office.createdBy;
      catsEntity.createdTime = office.createdBy;
      catsEntity.modifiedBy = office.modifiedBy;
      catsEntity.modifiedTime = office.modifiedTime;
      catsEntity.isDl = false;
      office.leadOfficeMembers = office.leadOfficeMembers?.filter(m => m.isDeleted != true);
      office.leadOfficeMembers?.forEach(m => {
        var catUser: CATSUser = new CATSUser();
        catUser.id = m.id;
        catUser.roleId = m.roleId;
        catUser.isManager = false;
        catUser.userFullName = m.userFullName;
        catUser.userUPN = m.userUPN;
        catUser.EntityId = office.id;
        catUser.EntityName = office.name;
        catsEntity.Members.push(catUser);
      });
      
      office.leadOfficeOfficeManagers = office.leadOfficeOfficeManagers?.filter(m => m.isDeleted != true);
      office.leadOfficeOfficeManagers?.forEach(m => {
        var catUser: CATSUser = new CATSUser();
        catUser.id = m.id;
        catUser.roleId = m.roleId;
        catUser.isManager = true;
        catUser.userFullName = m.userFullName;
        catUser.userUPN = m.userUPN;
        catUser.EntityId = office.id;
        catUser.EntityName = office.name + ' Office Manager';
        catsEntity.Members.push(catUser);
      });
      this.catsEntities.push(catsEntity);
      });

    }
    else  if (this.selectedOption == 'dl'){  
      this.catsEntities = [];
      this.dlGroups = this.initialDataSources.dlGroups?.sort((a, b) => (a.name > b.name) ? 1 : -1);
      this.dlGroups?.forEach(group => {
        var catsEntity : UserEntity = new UserEntity();
        catsEntity.id = group.id;
        catsEntity.name = group.name;
        catsEntity.description = group.name;
        catsEntity.isDl = true;
        
        group.dlGroupMembers = group.dlGroupMembers?.filter(m => m.isDeleted != true);
        group.dlGroupMembers?.forEach(m => {
         var catUser: CATSUser = new CATSUser();
         catUser.id = m.id;
         catUser.roleId = m.roleId;
         catUser.isManager = false;
         catUser.userFullName = m.userFullName;
         catUser.userUPN = m.userUPN;
         catUser.EntityId = m.dlGroupId;
         catUser.EntityName = group.name;
         catsEntity.Members.push(catUser);
        });
        this.catsEntities.push(catsEntity);
       });
    }
    else{
      this.catsEntities = [];
      var supportRoles: string[] = ['Administrator', 'Support', 'Report Manager']
      var roles = this.initialDataSources.roles?.filter(r => supportRoles.includes(r.name));
      roles?.forEach(r => {
        var catsEntity : UserEntity = new UserEntity();
        catsEntity.id = r.id;
        catsEntity.name = r.name;
        catsEntity.description = r.name + ' (' + r.description + ')';
        catsEntity.isDl = false;

        var offices = this.initialDataSources.leadOffices?.filter(x => x.leadOfficeMembers.some(l => r?.id == l.role?.id) || x.leadOfficeOfficeManagers.some(l => r?.id == l.role?.id));
        
        let supportmembers = [];
        offices?.forEach(office => {
          office.leadOfficeMembers.forEach(m => {
            if (m.role?.id == r.id){
              supportmembers.push(m);
            }
          });
          
          office.leadOfficeOfficeManagers.forEach(m => {
            if(m.role?.id == r.id){
              supportmembers.push(m);
            }
          });
          
        });   
        // supportmembers = [...supportmembers, ...leadOfficeOfficeManagers.filter(m => m.some(x => x.role.id == r.id))];

        supportmembers?.forEach(m => {
          var catUser: CATSUser = new CATSUser();
          catUser.id = m.id;
          catUser.roleId = r.id;
          catUser.isManager = false;
          catUser.userFullName = m.userFullName;
          catUser.userUPN = m.userUPN;
          catUser.EntityId = r.id;
          catUser.EntityName = r.name;
          catsEntity.Members.push(catUser);
        });

        this.catsEntities.push(catsEntity);
      }); 
      
    }

  } 

  private _filterusers(value: string): string[] {
    const filterValue = value.toLowerCase();
    if (value == ''){
      this.filterUserOfficeAcivated = false;     
    }
    return this.initialDataSources.allOMBUsers.filter(option => option.upn?.toLowerCase().includes(filterValue.trim()) || option.displayName?.toLowerCase().includes(filterValue.trim()));
  }   

  applyFilterForEntityMembers(filterValue: string) {
    filterValue = filterValue.trim(); // Remove whitespace
    filterValue = filterValue.toLowerCase(); // Datasource defaults to lowercase matches
    this.dataSource.filter = filterValue;
  }  

  displayFn = value => {
    return getMembersFullnames(value, this.initialDataSources.allOMBUsers);
  }

  onShowAvailableEntity(catsEntity : UserEntity): boolean{
    if(this.initialDataSources.currentBrowserUser.MemberOfAdmins){
      return true;
    }
    if(this.initialDataSources.currentBrowserUser.MemberOfCATSSupport){
      if(this.selectedOption == 'admin' && catsEntity.name.toLowerCase().includes('administrator')){
        return false;
      }
      else{
        return true;
      }
    }
    else{
      if (this.initialDataSources.currentBrowserUser.MemberCATSOfficeManagerCollection.includes(catsEntity.name)){//MemberCATSOfficeManagerCollection
        return true
      }
      else{
        return false;
      }
    }
  }

  onSetCheckedForManager(){
    this.ckboxes.forEach(ckOffice => {
      ckOffice.checked = false;
      if (this.initialDataSources.currentBrowserUser.MemberGroupsCollection.some(office => office.toLowerCase().includes(ckOffice.value.toLowerCase()))){
        ckOffice.checked = true;
      }
    }); 
  }

  onUserAdded(){
    const selectedUserUpn = this.selectUserform.controls['userInput'].value; 
    const selectOfficeIds = this.checkedOffices.map(o => o.id);
    
    if (this.selectedOption == 'lead'){
      this.adminService.addCatsLeadOfficeUsers(this.selectedUsersToAdd, this.selectedManagerToAdd, selectOfficeIds).subscribe( response => {
        this.leadOfficeService.getAll().then(data =>{
          this.initialDataSources.leadOffices = data;
          this.seCATEntities();
          this.resetLeadOfficeCheckBoxes();    
          this.onLoadCheckedEntitiesMembers(); 
          this.onClearUserSelectionInput();
        });
       });
    }
    else if (this.selectedOption == 'dl'){
      this.adminService.addCatsDLUsers(this.selectedUsersToAdd, selectOfficeIds).subscribe( response => {
        this.dlService.getAll().then(data =>{            
          this.initialDataSources.dlGroups = data;
          this.seCATEntities();
          this.resetLeadOfficeCheckBoxes();
          this.onLoadCheckedEntitiesMembers();  
          this.onLoadCheckedEntitiesMembers();
        });
       });
    }
    else if (this.selectedOption == 'admin'){
      this.adminService.addCatsSupports(this.selectedUsersToAdd, selectOfficeIds).subscribe( response => {
        this.leadOfficeService.getAll().then(data =>{
          this.initialDataSources.leadOffices = data;
          this.seCATEntities();
          this.resetLeadOfficeCheckBoxes();
          this.onLoadCheckedEntitiesMembers();      
          this.onClearUserSelectionInput();
        });//response;
       });
    }
  }

  onUserRemoved(user: string, upn: string, entityId: number){
    var memberId = this.catsEntities.find(o => o.id == entityId)?.Members.find(m => m.userUPN?.trim().toLowerCase() == upn?.trim().toLowerCase() && m.isManager == false)?.id;
    var memberManagerId = this.catsEntities.find(o => o.id == entityId)?.Members.find(m => m.userUPN?.trim().toLowerCase() == upn?.trim().toLowerCase() && m.isManager == true)?.id;;
    const isManager = memberId != undefined ? false: true;

    if (this.selectedOption == 'lead'){  
      if (memberId == undefined)  {
        Swal.fire('Hey!', 'Member does not exist!', 'info');
        return;
      }
      this.adminService.deleteCatsLeadOfficeUsers(isManager ? memberManagerId :  memberId, isManager, entityId).subscribe( response => {
        this.leadOfficeService.getAll().then(data =>{
          this.initialDataSources.leadOffices = data;
          this.seCATEntities();
          this.resetLeadOfficeCheckBoxes(); 
          this.onLoadCheckedEntitiesMembers();     
          this.onClearUserSelectionInput();
        });
       });
    }
    else  if (this.selectedOption == 'dl'){   
      if (memberId == undefined)  {
        Swal.fire('Hey!', 'Member does not exist!', 'info');
        return;
      }
      this.adminService.deleteCatsDLUsers(isManager ? memberManagerId :  memberId, entityId).subscribe( response => {
        this.dlService.getAll().then(data =>{            
          this.initialDataSources.dlGroups = data;
          this.seCATEntities();
          this.resetLeadOfficeCheckBoxes();
          this.onLoadCheckedEntitiesMembers();  
          this.onLoadCheckedEntitiesMembers();
        });
       });
    }
    else  if (this.selectedOption == 'admin'){   
      if (upn == undefined)  {
        Swal.fire('Hey!', 'Member does not exist!', 'info');
        return;
      }
      this.adminService.deleteCatsSupportUsers(upn, entityId).subscribe( response => {
        this.leadOfficeService.getAll().then(data =>{
          this.initialDataSources.leadOffices = data;
          this.seCATEntities();
          this.resetLeadOfficeCheckBoxes();
          this.onLoadCheckedEntitiesMembers();      
          this.onClearUserSelectionInput();
        });
       });
    }
  }   

  resetLeadOfficeCheckBoxes(){
    var checkedItems: any = this.ckboxes.filter(ck => ck.checked).map(ck => ck.value);
    this.checkedOffices = this.catsEntities.filter(o => checkedItems.includes(o.name));
    this.ckboxes.forEach(ckOffice => {
      ckOffice.checked = false;
      if (this.checkedOffices.some(office => office.name == ckOffice.value)){
        ckOffice.checked = true;
      }
    }); 
  }

  onIsOfficeManagerSelect(event: any, userUPN: string){
    if (event.checked == true){
      var member: any  = this.selectedUsersToAdd.find(u => u.userUPN.trim().toLowerCase() == userUPN.trim().toLowerCase());
      if (member != undefined){
        var manager: LeadOfficeOfficeManager = new LeadOfficeOfficeManager();
        Object.assign(manager, member);
        this.selectedManagerToAdd.push(manager);
      }
    }
    else{      
      var manager  = this.selectedManagerToAdd.find(u => u.userUPN.trim().toLowerCase() == userUPN.trim().toLowerCase());
      if (manager != undefined){
        var member: any = new LeadOfficeMember();
        this.selectedManagerToAdd = this.selectedManagerToAdd.filter(m => m != manager);
      }
    }

  }

  onOptionUserToAddDetetionSelected(event: any){
    this.selectedUsersToAdd = this.selectedUsersToAdd.filter(u => u != event);
    this.selectedManagerToAdd = this.selectedManagerToAdd.filter(u => u != event);
    this.onClearUserSelectionInput();
  }

  onOptionUserSelected(event: any){
    this.filterUserOfficeAcivated = true;  
    
    if (this.checkedOffices.length == 0){     
      return;
    }
    var leadmember : LeadOfficeMember = new LeadOfficeMember();
    leadmember.id = 0;
    leadmember.userFullName = event.source.viewValue;
    leadmember.userUPN = event.source.value;
    leadmember.leadOfficeId = this.checkedOffices[0].id;
    leadmember.isDeleted = false;
    leadmember.modifiedBy = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;
    leadmember.createdBy = this.initialDataSources.currentBrowserUser.MainActingUserPreferedName;
    leadmember.createdTime = new Date();
    leadmember.modifiedTime = new Date();

    if (!this.selectedUsersToAdd.some(x => x.userUPN?.trim().toLowerCase() == event.source.value?.trim().toLowerCase())){
      this.selectedUsersToAdd.push(leadmember);
    }
  }

  entityRadioChange(event: any){
    this.checkedOffices = [];
    this.catsEntities = [];
    this.onClearUserSelectionInput();
    this.onClearSelectedUsers();
    
    this.selectedOption = event.value;
    this.selectedOptionTitle = event.source._elementRef.nativeElement.innerText;
    this.userTabGroup.selectedIndex = 0;
    this.ngOnInit();
    
  }

  onTabChanged(tabChangeEvent: MatTabChangeEvent){
    console.log('tabChangeEvent => ', tabChangeEvent); 
    console.log('index => ', tabChangeEvent.index); 
  }

  onClearUserSelectionInput(){
    this.selectUserform.controls['userInput'].setValue('');
    this.selectOfficeFilterform.controls['userFilterInput'].setValue('') ; 
  }

  onClearSelectedUsers(){
    this.selectedManagerToAdd.length = 0;
    this.selectedUsersToAdd.length = 0;
  }

  onToggleAll(isselected: any){
    this.membersckboxes.forEach(ck => {
      ck.checked = isselected;
    });
  }

  onFilterUserLeadOffice(upn: string){
    this.onClearSelectedUsers();
    var entities = this.catsEntities.filter(o => o.Members.some(m => m.userUPN?.toLowerCase().trim().includes(upn?.toLowerCase().trim())));
    this.ckboxes.forEach(ckOffice => {
      ckOffice.checked = false;
      if (entities.some(office => office.name == ckOffice.value)){
        ckOffice.checked = true;
      }
    });  
    this.onLoadCheckedEntitiesMembers();
  }

  onLeadOfficeChecked(office: any, event: any){
    this.ckboxes.forEach(ck => {
      if (ck.value == office.name && ck.checked == false){
        ck.checked = true;
      }
      else if (ck.value == office.name && ck.checked == true){
        ck.checked = false;
      }
    });

    this.onLoadCheckedEntitiesMembers();
  }

  onLoadCheckedEntitiesMembers(){   
    var checkedItems: any = this.ckboxes.filter(ck => ck.checked).map(ck => ck.value);
    this.checkedOffices = this.catsEntities.filter(o => checkedItems.includes(o.name));
    this.extractSelectedMembers();    
    //this.onClearUserSelectionInput();
  }

  setLeadOfficeChecked(entity: string): boolean{   
    return this.checkedOffices.some(x => x.name.includes(entity));
  }

  extractSelectedMembers(){    
    this.selectedMembers = [];
    this.checkedOffices.forEach(o => {
      o.Members.sort((a, b) => (a.userFullName > b.userFullName) ? 1 : -1).forEach(u => { 
        this.selectedMembers.push(u);
      });
    });

    this.selectedMembers = this.selectedMembers.sort((a, b) => (a.userFullName > b.userFullName) ? 1 : -1);
    this.dataSource = new MatTableDataSource<CATSUser>(this.selectedMembers);
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }
  /** Whether the number of selected elements matches the total number of rows. */
  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  /** Selects all rows if they are not all selected; otherwise clear selection. */
  masterToggle() {
    this.isAllSelected() ?
        this.selection.clear() :
        this.dataSource.data.forEach(row => this.selection.select(row));
  }

 

}
