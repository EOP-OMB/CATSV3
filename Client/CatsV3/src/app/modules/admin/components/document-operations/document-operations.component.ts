import { SelectionModel } from '@angular/cdk/collections';
import { Component, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { ThemePalette } from '@angular/material/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSelect } from '@angular/material/select';
import { MatSort, Sort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { map, startWith } from 'rxjs/operators';
import { Folder } from 'src/app/models/folder.model';
import { Document } from 'src/app/models/document.model';
import { Correspondence } from 'src/app/modules/correspondence/Models/correspondence.model';
import { CorrespondenceService } from 'src/app/modules/correspondence/services/correspondence-service';
import { FolderService } from 'src/app/services/folder.service';
import { DataSources } from 'src/app/modules/shared/interfaces/data-source';
import { MatCheckbox } from '@angular/material/checkbox';
import { CATSPackage, decodeUrl, UserQuery } from 'src/app/modules/shared/utilities/utility-functions';
import { HttpEventType } from '@angular/common/http';
import { PaginationDataSource } from 'ngx-pagination-data-source';
import { DialogPromptComponent } from 'src/app/modules/shared/components/dialog-prompt/dialog-prompt.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-document-operations',
  templateUrl: './document-operations.component.html',
  styleUrls: ['./document-operations.component.scss']
})
export class DocumentOperationsComponent implements OnInit {

  itemDisplayedColumns: string[] = ['icon', 'select', 'leadOfficeName', 'reviewround', 'status'];
  displayedColumns: string[] = ['icon', 'catsid', 'leadOfficeName', 'currentReview', 'letterStatus', 'action'];
  data : any[] = [];
  dataSource: any;
  itemsDataSource: any ;
  itemDocDisplayedColumns: string[] = ['selectall','document', 'type', 'swap', 'delete'];
  itemsDocDataSource: any ;
  selection = new SelectionModel<Correspondence>(false, []);
  selectedItem : any;
  status: string[] =  ['Open', 'Closed', 'Deleted'];
  letterStatus: string = 'Open';
  pageSizeOptions: string[] = ['5', '10', '20', '50', '100'];
  selectedPageSize : number = 10;

  folder: Folder;
  pageEvent: PageEvent;
  currentPage: number = 0;
  totalLength: number = 1004;
  isFiltering: boolean = false;

  @ViewChild(MatSort, {static: true}) sort: MatSort;
  @ViewChild(MatPaginator, {static: true}) paginator: MatPaginator;
  @ViewChild('itemStatus') itemStatus: MatSelect;
  @ViewChildren('matCheckItem') listmatCheckItem: QueryList<MatCheckbox>
  @ViewChildren('matCheckSwap') listMatCheckSwap: QueryList<MatCheckbox>

  documentsToSwap: Document[] = [];
  documentToRemove: Document;

  documentOperationFormGroup: FormGroup;  
  itemsForm: FormGroup;
  pageSizeForm: FormGroup;
  multiple: boolean = true;
  isAddButtonEnabled = false;
  color: ThemePalette = 'primary';

  constructor(
    private fb: FormBuilder, 
    private correspondenceService: CorrespondenceService, 
    private folderService : FolderService,
    private initialDataSources: DataSources,
    private dialog: MatDialog,) 
    {
    this.documentOperationFormGroup = this.fb.group({
      reviewDocuments: [''],
      referenceDocuments: [''],
      finalDocuments: [''],
    }); 

    this.pageSizeForm = this.fb.group({
      pageSizeSelect: [this.selectedPageSize.toString()]
    }); 

    this.itemsForm = this.fb.group({
      itemStatus: [''],
      globalsearchfilter: ['']
    }); 
  }

  ngOnInit(): void {
      this.itemsForm.controls['itemStatus'].setValue('Open');
      this.setDataSource();
  }
  
  ngAfterViewInit() {            
  }

  onGetRecord(row: any, checked: boolean = null){
    this.selectedItem = row;
    if (checked == undefined){
      checked = this.checkItemCheckbox(row.catsid);
    }

    if (checked){
      this.folderService.loadFolderById(row.folderId).subscribe(response => {
        this.folder = response;
        this.setDocumentsDataSource();   
      });
    }
    else{
      this.documentOperationFormGroup.get("reviewDocuments").setValue('');
      this.documentOperationFormGroup.get("referenceDocuments").setValue('');
      this.documentOperationFormGroup.get("finalDocuments").setValue('');

      this. itemsDocDataSource = null;
      this.selectedItem = null;
    }
  }

  onItemStatusChanged(event: any){ 
    
    this.letterStatus = event.value;
    this.setDataSource();
  }

  setDataSource(){
    this.dataSource = new PaginationDataSource<Correspondence, UserQuery>(
      (request, query) => this.correspondenceService.page(request, this.letterStatus, query),
      {property: 'catsid', order: 'desc'},
      {search: '', registration: undefined}, this.selectedPageSize
    );

  }  

  onchangePageSize(event){
    this.selectedPageSize = event.value;
    this.setDataSource();
  }

  sortBy({active, direction}: Sort) {
    this.dataSource.sortBy({
      property: active as keyof Correspondence,
      order: direction || 'asc'
    })
  }

  setDocumentsDataSource(){
    this.folder.documents = this.folder.documents?.filter(doc => doc.isDeleted == false);
    this.itemsDocDataSource = new MatTableDataSource<Document>(this.folder.documents); 
  }
  
  onFileChange(doc: any){
    this.enableAddUpdateButton();
  }

  onDocumentSwapped(document: Document, checked: boolean){

    if(checked){
      this.folder.documents.forEach(doc => {
        if (doc.id == document.id){          
          doc.deletedBy = this.initialDataSources.currentBrowserUser.LoginName;
          doc.deletedTime = new Date();  
          doc.modifiedBy = this.initialDataSources.currentBrowserUser.LoginName;;
          doc.modifiedTime = new Date();
          doc.isDeleted = true;
          doc.isModified = true;
        }
      });
    }
    else{
      this.folder.documents.forEach(doc => {
        if (doc.id == document.id){          
          doc.deletedBy = null;
          doc.deletedTime = null;
          doc.isDeleted = false;
          doc.isModified = null;
        }
      });
    };

    this.enableAddUpdateButton();
  }

  onAddNewDocuments(){
    var emptyFiles: File[] = [];
    var reviewFiles : File[] = this.documentOperationFormGroup.get("reviewDocuments").value  ?  this.documentOperationFormGroup.get("reviewDocuments").value : emptyFiles; 
    var referenceFiles : File[] = this.documentOperationFormGroup.get("referenceDocuments").value   ?  this.documentOperationFormGroup.get("referenceDocuments").value : emptyFiles;     
    var finalFiles : File[] = this.documentOperationFormGroup.get("finalDocuments").value ?  this.documentOperationFormGroup.get("finalDocuments").value : emptyFiles; 

    this.setFiles(reviewFiles, 'Reference Document');
    this.setFiles(referenceFiles, 'Review Document');
    this.setFiles(finalFiles, 'Final Document');

    if (reviewFiles.length > 0 || referenceFiles.length > 0 || finalFiles.length > 0){

      var cATSPackage: CATSPackage = new CATSPackage();
      cATSPackage.reviewFiles = reviewFiles;
      cATSPackage.referenceFiles = referenceFiles;
      cATSPackage.finalFiles = finalFiles;

      this.folderService.addDocumentToFolder(this.folder, cATSPackage).subscribe(async event => {
      
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
            this.folder = event.body;
            this.setDocumentsDataSource() ;
            this.documentOperationFormGroup.get("reviewDocuments").setValue('');
            this.documentOperationFormGroup.get("referenceDocuments").setValue('');
            this.documentOperationFormGroup.get("finalDocuments").setValue('');
      
            this.resetItemCheckboxes();
            
        }
      }) ;

    }
    else{

      this.folderService.update(this.folder).then(res =>{
        this.folder = res;
        this.setDocumentsDataSource() ;
        this.documentOperationFormGroup.get("reviewDocuments").setValue('');
        this.documentOperationFormGroup.get("referenceDocuments").setValue('');
        this.documentOperationFormGroup.get("finalDocuments").setValue('');
  
        this.resetItemCheckboxes();
      });

    }
  }

  onDocumentDeleted(id: number){
    this.folder.documents.forEach(doc => {
      if (doc.id == id){          
        doc.deletedBy = this.initialDataSources.currentBrowserUser.LoginName;;
        doc.deletedTime = new Date();      
        doc.modifiedBy = this.initialDataSources.currentBrowserUser.LoginName;;
        doc.modifiedTime = new Date();
        doc.isDeleted = true;
        doc.isModified = true;
      }
    });

    this.folderService.update(this.folder).then(res =>{
      this.folder = res;
      this.setDocumentsDataSource() ;
      this.resetItemCheckboxes();
    });
  }  

  onItemDeleted(id: number, catsid : string){

    this.onRestoreOrRemove (id, catsid, 'delete');
    
  }

  onItemRestored(id: number, catsid : string){

    this.onRestoreOrRemove (id, catsid, 'restore');

  }

  setFiles(files :File[], fileType: string){
    files.forEach(file => {
      var doc : Document = new Document();
      doc.id = 0;
      doc.folderId = this.folder.id;
      doc.name = file.name;
      doc.path = file.name;
      doc.type = fileType;
      doc.createdBy = this.initialDataSources.currentBrowserUser.LoginName;
      doc.createdTime = new Date();
      doc.modifiedBy = this.initialDataSources.currentBrowserUser.LoginName;
      doc.modifiedTime = new Date();
      this.folder.documents.push(doc);
    });
  }

  enableAddUpdateButton(){
    var emptyFiles: File[] = [];
    var reviewFiles : File[] = this.documentOperationFormGroup.get("reviewDocuments").value  ?  this.documentOperationFormGroup.get("reviewDocuments").value : emptyFiles; 
    var referenceFiles : File[] = this.documentOperationFormGroup.get("referenceDocuments").value   ?  this.documentOperationFormGroup.get("referenceDocuments").value : emptyFiles;     
    var finalFiles : File[] = this.documentOperationFormGroup.get("finalDocuments").value ?  this.documentOperationFormGroup.get("finalDocuments").value : emptyFiles;

    if (reviewFiles.length > 0 || referenceFiles.length > 0 || finalFiles.length > 0){
      this.isAddButtonEnabled = true;
    }
    else if (this.listMatCheckSwap.some(chk => chk.checked == true)){
      this.isAddButtonEnabled = true;
    }
    else{
      this.isAddButtonEnabled = false;
    }
  }

  resetItemCheckboxes(){
    this.listmatCheckItem.forEach(item => {
      item.checked = false;
      if(item.value == this.selectedItem.catsid){
        item.checked = true;
      }
    });
  }  

  checkItemCheckbox(catsid: string): boolean{
    var isChecked : boolean = false;
    this.listmatCheckItem.forEach(item => {
      if(item.value == catsid && item.checked == true){
        isChecked = false;
      }
      else if(item.value == catsid && item.checked != true){
        isChecked = true;
      }
    });

    return isChecked;
  }
  /** Whether the number of selected elements matches the total number of rows. */
  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.itemsDocDataSource.data.length;
    return numSelected === numRows;
  }

  /** Selects all rows if they are not all selected; otherwise clear selection. */
  masterToggle() {
    this.isAllSelected() ?
        this.selection.clear() :
        this.itemsDocDataSource.data.forEach(row => this.selection.select(row));
  }

  goToLink(url: string){
    window.open(url, "_blank");
  }

  decodeUrl(url: string): string{
    return decodeUrl(url);
  }

  onRestoreOrRemove(id: number,  catsid?: string , action?: string){

    let message : string = "You are about to " + action + " this item. Please provide the reasons and click OK to proceed or Cancel.";
   
    const dialogRef = this.dialog.open(DialogPromptComponent, {
      width: '500px',
      data: {
        name: this.initialDataSources.currentBrowserUser.PreferredName + ",",
        label: action == "delete" ? "Reasons for Deleting The item: " : "Reasons for Restoring The item: " + catsid,
        title: message,
        isConfirmOnly: false,
        isReopening: false,
        noThanksLabel: "Cancel"
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      //console.log('The dialog was closed');
      if (result != undefined && result?.trim() != "Cancel"){
        this.correspondenceService.get(id).then(res => {
          var correspondence = res;
          correspondence.adminClosureDate = action == 'delete' ?  new Date() : null;
          correspondence.adminClosureReason =  action == 'delete' ? result : '';
          correspondence.adminReOpenReason =   action != 'delete' ? result : '';
          correspondence.adminReOpenDate =  action != 'delete' ?  new Date() : null;
          correspondence.isDeleted = action == 'delete' ? true : false;
          correspondence.deletedBy = action == 'delete' ? this.initialDataSources.currentBrowserUser.MainActingUserUPN : null;
          correspondence.deletedTime = action == 'delete' ? new Date() : null;
          correspondence.modifiedBy = this.initialDataSources.currentBrowserUser.MainActingUserUPN;
          correspondence.modifiedTime = new Date();  
          this.correspondenceService.restoreDeletedCorrespondence(correspondence).subscribe(res => {      
            this.setDataSource();
            this.selectedItem = null;
          });  
        });
      } 
    });   
    
  }

}
