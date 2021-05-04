import { Correspondence } from '../../correspondence/Models/correspondence.model';
import { DatePipe } from '@angular/common';
import { isDate } from 'util';

export class DashboardsConfigurations{
    
    constructor(private datePipe:DatePipe) {

    }
    
    w_75= ['icon, boilerPlate'];
    w_85= ['leadOfficeName','letterStatus'];
    w_125= ['documentsCount'];
    w_150= [];

    correspondenceRadioOptions = ['Open','Closed'];

    originatorRadioOptions = ['Open','Closed','Open & Closed', 'Archived'];
    collaborationRadioOptions = ['Open','Closed'];
    officedataRadioOptions = ['Open','Closed'];
    pendingRadioOptions = ['Open'];
    copiedRadioOptions = ['Open', 'Archived'];


    originatorPendingRadioOptions = ['Open','Closed', 'Archived'];

    reviewRadioOptions = ['Not Completed','Draft','Completed', 'FYI'];

    //CORRESPONDENCE COLUMNS CONFIGURATIONS

    static readonly correspondenceHiddenColumns = ['id','padDueDate','reviewDocuments','referenceDocuments','finalDocuments','reviewers','originators',
      'fyiUsers','completedUsers','completedRounds','draftUsers', 'otherSigners','createdTime','createdBy','modifiedTime','modifiedBy',
      'reasonsToReopen', 'copiedUsersIds', 'leadOfficeUsersIds','copiedOfficeName','copiedUsersDisplayNames','fiscalYear','isLetterRequired','leadOfficeUsersDisplayNames',
      'letterCrossReference','notRequiredReason','notes','isSaved','whFyi', 'administration'];
      
    private correspondenceColumns = [
        { columnDef: 'icon', header: '',    cell: (element: Correspondence) => `${element.icon}`},        
        { columnDef: 'id', header: 'ID',    cell: (element: Correspondence) => `${element.id}`},
        { columnDef: 'catsid', header: 'CATS ID.',    cell: (element: Correspondence) => `${element.catsid}`},
        { columnDef: 'leadOfficeName', header: 'Lead Office',   cell: (element: Correspondence) => `${element.leadOfficeName}`},          
        { columnDef: 'leadOfficeUsersDisplayNames',   header: 'Lead Office Users', cell: (element: Correspondence) => `${element.leadOfficeUsersDisplayNames ? element.leadOfficeUsersDisplayNames.split(';').join('\n'):''}`},       
        { columnDef: 'leadOfficeUsersIds',   header: 'Lead Office Users', cell: (element: Correspondence) => `${element.leadOfficeUsersIds ? element.leadOfficeUsersIds.split(';').join('\n'):''}`},
        { columnDef: 'copiedOfficeName',   header: 'Copied Offices', cell: (element: Correspondence) => `${element.copiedOfficeName ? element.copiedOfficeName.split(';').join('\n'):''}`},
        { columnDef: 'copiedUsersDisplayNames',   header: 'Copied Office Users', cell: (element: Correspondence) => `${element.copiedUsersDisplayNames ? element.copiedUsersDisplayNames.split(';').join('\n'):''}`},
        { columnDef: 'copiedUsersIds',   header: 'Copied Office Users', cell: (element: Correspondence) => `${element.copiedUsersIds ? element.copiedUsersIds.split(';').join('\n'):''}`},
        { columnDef: 'correspondentName',   header: 'Correspondent\'s Name', cell: (element: Correspondence) => `${element.correspondentName}`},        
        { columnDef: 'otherSigners',   header: 'Other Signers', cell: (element: Correspondence) => `${element.otherSigners ? element.otherSigners.split(';').join('\n'): ''}`},
        { columnDef: 'letterSubject',   header: 'Subject', cell: (element: Correspondence) => `${element.letterSubject}`},
        { columnDef: 'letterTypeName',   header: 'Document Type', cell: (element: Correspondence) => `${element.letterTypeName}`},
        { columnDef: 'documentsCount',   header: 'Documents', cell: (element: Correspondence) => `${element.documentsCount}` },
        { columnDef: 'currentReview',   header: 'In Clearance', cell: (element: Correspondence) => `${element.currentReview}`},
        { columnDef: 'letterReceiptDate',   header: 'Received Date', cell: (element: Correspondence) => `${this.datePipe.transform(new Date(element.letterReceiptDate) ,'MM/dd/yyyy')}`},
        { columnDef: 'dueforSignatureByDate',   header: 'Signature Due Date', cell: (element: Correspondence) => `${this.datePipe.transform(new Date(element.dueforSignatureByDate) ,'MM/dd/yyyy')}`},        
        { columnDef: 'padDueDate',   header: 'PAD Due Date', cell: (element: Correspondence) => `${this.datePipe.transform(new Date(element.padDueDate) ,'MM/dd/yyyy')}`},
        { columnDef: 'letterStatus',   header: 'Status', cell: (element: Correspondence) => `${element.letterStatus}`},
        //{ columnDef: 'reviewStatus',   header: 'Round Status', cell: (element: Correspondence) => `${element.reviewStatus}`},
        { columnDef: 'reviewStatus',   header: 'Review Status', cell: (element: Correspondence) => `${
          element.reviewStatus?.trim() == 'Completed' && element.collaboration != undefined ? element.collaboration.completedReviewers?.split(';').length +  ' ' + element.reviewStatus : 
          element.reviewStatus?.trim() == 'Draft' && element.collaboration != undefined ? element.collaboration.draftReviewers?.split(';').length +  ' ' + element.reviewStatus : element.reviewStatus
        }`},
        
        { columnDef: 'reviewers',   header: 'Reviewers', cell: (element: Correspondence) => `${element.reviewers ? element.reviewers.split(';').join('\n'):''}`},
        { columnDef: 'originators',   header: 'Originators', cell: (element: Correspondence) => `${element.originators ? element.originators.split(';').join('\n'):''}`},
        { columnDef: 'fyiUsers',   header: 'FYI Users', cell: (element: Correspondence) => `${element.fyiUsers ? element.fyiUsers.split(';').join('\n'):''}`},
        { columnDef: 'completedUsers',   header: 'Cleared By', cell: (element: Correspondence) => `${element.completedUsers ? element.completedUsers.split(';').join('\n'):''}`},
        { columnDef: 'completedRounds',   header: 'Completed Rounds ', cell: (element: Correspondence) => `${element.completedRounds ? element.completedRounds.split(';').join('\n'):''}`},
        { columnDef: 'draftUsers',   header: 'Draft Users ', cell: (element: Correspondence) => `${element.draftUsers ? element.draftUsers.split(';').join('\n'):''}`},
        
        { columnDef: 'createdTime',   header: 'Created', cell: (element: Correspondence) => `${this.datePipe.transform(new Date(element.createdTime) ,'MM/dd/yyyy')}`},        
        { columnDef: 'createdBy',   header: 'Created By', cell: (element: Correspondence) => `${element.createdBy}`},        
        { columnDef: 'modifiedTime',   header: 'Modified', cell: (element: Correspondence) =>  `${this.datePipe.transform(new Date(element.modifiedTime) ,'MM/dd/yyyy')}`},
        { columnDef: 'modifiedBy',   header: 'Modified By', cell: (element: Correspondence) => `${element.modifiedBy}`},

              
        { columnDef: 'reasonsToReopen',   header: 'Reopen Reason', cell: (element: Correspondence) => `${element.reasonsToReopen}`},
        { columnDef: 'fiscalYear',   header: 'Year', cell: (element: Correspondence) => `${element.fiscalYear}`},
        { columnDef: 'isLetterRequired',   header: 'Is Response Required', cell: (element: Correspondence) => `${element.isLetterRequired}`},     
        { columnDef: 'letterCrossReference',   header: 'Cross Reference', cell: (element: Correspondence) => `${element.letterCrossReference}`},
        { columnDef: 'notRequiredReason',   header: 'Not Required Reason', cell: (element: Correspondence) => `${element.notRequiredReason}`},
        { columnDef: 'notes',   header: 'Notes', cell: (element: Correspondence) => `${element.notes}`},        
        { columnDef: 'isSaved',   header: 'SAVED', cell: (element: Correspondence) => `${element.isSaved}`},
        { columnDef: 'whFyi',   header: 'WH FYI', cell: (element: Correspondence) => `${element.whFyi}`},
        { columnDef: 'reviewDocuments',   header: 'Review Documents', cell: (element: Correspondence) => `${element.reviewDocuments}`},
        { columnDef: 'referenceDocuments',   header: 'Reference Documents', cell: (element: Correspondence) => `${element.referenceDocuments}`},
        { columnDef: 'finalDocuments',   header: 'Final Documents', cell: (element: Correspondence) => `${element.finalDocuments}`},
        { columnDef: 'administration',   header: 'Administration', cell: (element: Correspondence) => `${element.administration?.name}`},
      ];

      //This is the source of the columns configuration panel
      correspondenceColumnsConfigData: Array<any> = [
        
        { name: 'Lead Office', value: 'leadOfficeName' },
        { name: 'Lead Office Users', value: 'leadOfficeUsersDisplayNames' },
        { name: 'Copied Offices', value: 'copiedOfficeName' },
        { name: 'Copied Office Users', value: 'copiedUsersDisplayNames' },
        { name: 'Correspondent\'s Name', value: 'correspondentName' },
        { name: 'Subject', value: 'letterSubject' },
        { name: 'Other Signers', value: 'otherSigners' },
        { name: 'Document Type', value: 'letterTypeName' },
        { name: 'Documents', value: 'documentsCount' },
        { name: 'In Clearance', value: 'currentReview' },
        { name: 'Date of Receipt', value: 'letterReceiptDate' },
        { name: 'Due for Signature By', value: 'dueforSignatureByDate' },
        { name: 'PAD Due Date', value: 'padDueDate' },
        { name: 'Status', value: 'letterStatus' },
        { name: 'Review Status', value: 'reviewStatus' },
        { name: 'completed Rounds', value: 'completedRounds' },
        { name: 'Cleared By', value: 'completedUsers' },
        { name: 'Draft Users', value: 'draftUsers' },
        { name: 'Reviewers', value: 'reviewers' },
        { name: 'Originators', value: 'originators' },
        { name: 'FYI Users', value: 'fyiUsers' },
        { name: 'Created', value: 'createdTime' },
        { name: 'Created By', value: 'createdBy' },
        { name: 'Modified', value: 'modifiedTime' },
        { name: 'Modified By', value: 'modifiedBy' },
        { name: 'Administration', value: 'administration' }
      ];

      //ORIGINATOR COLUMNS CONFIGURATIONS
      static readonly originatorHiddenColumns = ['id','summary','instructions','completedRounds','letterReceiptDate','dueforSignatureByDate', 'administration'];

      private originatorCollaborationColumns = [
        { columnDef: 'icon', header: '',    cell: (element: Correspondence) => `${element.icon}`},        
        { columnDef: 'id', header: 'ID',    cell: (element: Correspondence) => `${element.collaboration?.id}`},
        { columnDef: 'catsid', header: 'CATS ID',    cell: (element: Correspondence) => `${element.catsid}`},
        { columnDef: 'leadOfficeName', header: 'Lead Office',   cell: (element: Correspondence) => `${element.leadOfficeName}`},
        { columnDef: 'correspondentName',   header: 'Correspondent\'s Name', cell: (element: Correspondence) => `${element.correspondentName}`},         
        { columnDef: 'letterSubject',   header: 'Subject', cell: (element: Correspondence) => `${element.letterSubject}`},
        { columnDef: 'letterTypeName',   header: 'Document Type', cell: (element: Correspondence) => `${element.letterTypeName}`},
        { columnDef: 'boilerPlate',   header: 'BR?', cell: (element: Correspondence) => `${element.collaboration?.boilerPlate}`},
        { columnDef: 'documentsCount',   header: 'Documents', cell: (element: Correspondence) => `${element.documentsCount}` },
        { columnDef: 'summaryMaterialBackground',   header: 'Summary Background', cell: (element: Correspondence) => `${element.collaboration?.summaryMaterialBackground}` }, 
        { columnDef: 'reviewInstructions',   header: 'Review Instructions', cell: (element: Correspondence) => `${element.collaboration?.reviewInstructions}` },
        { columnDef: 'reviewStatus',   header: 'Review Status', cell: (element: Correspondence) => `${
          element.reviewStatus?.trim() == 'Completed' ? element.collaboration?.completedReviewers?.split(';').length +  ' ' + element.reviewStatus : 
          element.reviewStatus?.trim() == 'Draft' ? element.collaboration?.draftReviewers?.split(';').length +  ' ' + element.reviewStatus : element.reviewStatus
        }`},
        { columnDef: 'currentReview',   header: 'Review Round', cell: (element: Correspondence) => `${element.currentReview}`},               
        { columnDef: 'currentRoundStartDate',   header: 'Start Date', cell: (element: Correspondence) => `${element.collaboration?.currentRoundStartDate != undefined ? this.datePipe.transform(new Date(element.collaboration?.currentRoundStartDate) ,'MM/dd/yyyy'): ''}`},                      
        { columnDef: 'currentRoundEndDate',   header: 'Due Date', cell: (element: Correspondence) => `${element.collaboration?.currentRoundEndDate != undefined ?  this.datePipe.transform(new Date(element.collaboration?.currentRoundEndDate) ,'MM/dd/yyyy'): ''}`},         
        { columnDef: 'letterReceiptDate',   header: 'Received Date', cell: (element: Correspondence) => `${element.letterReceiptDate != undefined ? this.datePipe.transform(new Date(element.letterReceiptDate) ,'MM/dd/yyyy'): ''}`},
        { columnDef: 'dueforSignatureByDate',   header: 'Signature Due Date', cell: (element: Correspondence) => `${element.dueforSignatureByDate != undefined ? this.datePipe.transform(new Date(element.dueforSignatureByDate) ,'MM/dd/yyyy'): ''}`},               
        { columnDef: 'completedRounds',   header: 'Completed Rounds', cell: (element: Correspondence) => `${element.collaboration?.completedRounds != undefined ?element.collaboration?.completedRounds.split(',').join('\n') : ''}`}, 
        { columnDef: 'administration',   header: 'Administration', cell: (element: Correspondence) => `${element.administration?.name}`}, 
      ];

      //This is the source of the columns configuration panel
      originatorColumnsConfigData: Array<any> = [
        
        { name: 'Lead Office', value: 'leadOfficeName' },
        { name: 'Correspondent\'s Name', value: 'correspondentName' },
        { name: 'Subject', value: 'letterSubject' },
        { name: 'BR?', value: 'boilerPlate' },
        { name: 'Documents', value: 'documentsCount' },
        { name: 'Summary Background', value: 'summaryMaterialBackground' },
        { name: 'Instructions', value: 'reviewInstructions' },
        { name: 'Document Type', value: 'letterTypeName' },
        { name: 'Review Round', value: 'currentReview' },
        { name: 'Start Date', value: 'currentRoundStartDate' },
        { name: 'Due Date', value: 'currentRoundEndDate' },
        { name: 'Administration', value: 'administration' },
        { name: 'Completed Rounds', value: 'completedRounds' },
      ];

      private originatorPendingColumns = [
        { columnDef: 'icon', header: '',    cell: (element: Correspondence) => `${element.icon}`},        
        { columnDef: 'id', header: 'ID',    cell: (element: Correspondence) => `${element.id}`},
        { columnDef: 'catsid', header: 'CATS ID.',    cell: (element: Correspondence) => `${element.catsid}`},
        { columnDef: 'officeRole',   header: 'My Role', cell: (element: Correspondence) => `${element.officeRole}`},
        { columnDef: 'leadOfficeName', header: 'Lead Office',   cell: (element: Correspondence) => `${element.leadOfficeName}`}, 
        { columnDef: 'correspondentName',   header: 'Correspondent\'s Name', cell: (element: Correspondence) => `${element.correspondentName}`}, 
        
        { columnDef: 'letterSubject',   header: 'Subject', cell: (element: Correspondence) => `${element.letterSubject}`},
        { columnDef: 'letterTypeName',   header: 'Document Type', cell: (element: Correspondence) => `${element.letterTypeName}`},
        { columnDef: 'documentsCount',   header: 'Documents', cell: (element: Correspondence) => `${element.documentsCount}` },                
        { columnDef: 'letterReceiptDate',   header: 'Received Date', cell: (element: Correspondence) => `${this.datePipe.transform(new Date(element.letterReceiptDate) ,'MM/dd/yyyy')}`},
        { columnDef: 'letterStatus',   header: 'Status', cell: (element: Correspondence) => `${element.letterStatus}`},               
        { columnDef: 'padDueDate',   header: 'PAD Due Date', cell: (element: Correspondence) => `${this.datePipe.transform(new Date(element.padDueDate) ,'MM/dd/yyyy')}`},
      ];


      //REVIEWER COLUMNS CONFIGURATIONS

      static readonly reviewerHiddenColumns = ['id','boilerPlate','summaryMaterialBackground','reviewInstructions','reviewStatus','currentRoundStartDate','currentRoundEndDate', 'administration'];
      
      private reviewerColumns = this.originatorCollaborationColumns;

      //This is the source of the columns configuration panel
      reviewerColumnsConfigData: Array<any> = [
        
        { name: 'Lead Office', value: 'leadOfficeName' },
        { name: 'Correspondent\'s Name', value: 'correspondentName' },
        { name: 'Subject', value: 'letterSubject' },
        { name: 'Documents', value: 'documentsCount' },
        { name: 'Document Type', value: 'letterTypeName' },
        { name: 'Review Round', value: 'currentReview' },
        { name: 'Administration', value: 'administration' },
        { name: 'Completed Rounds', value: 'completedRounds' }
      ];

      

      public getColumns(value: string): any {
        if (value == "correspondence"){
            return this.correspondenceColumns
        }
        else if (value == "originator"){
            return this.originatorCollaborationColumns
        }
        else if (value == "originator-pending"){
          return this.originatorPendingColumns
        }
        else if (value == "reviewer"){
          return this.reviewerColumns
        }
      } 

      public getColumnsLabel(value: string, dashboard: string): any {
        if (dashboard == "correspondence"){
          let labels = this.correspondenceColumns.filter(c => c.columnDef == value).map(c => c.header);
          if (labels.length > 0){
            return labels[0]
          }
          else if(value=="browsefile"){
            return "Attach Letter(s)";
          }
          else{
            return '';
          }
        }
        else if (dashboard == "originator"){
          let labels = this.originatorCollaborationColumns.filter(c => c.columnDef == value).map(c => c.header);
          if (labels.length > 0){
            return labels[0]
          }
          else{
            return '';
          }
        }
        else{
          let labels = this.reviewerColumns.filter(c => c.columnDef == value).map(c => c.header);
          if (labels.length > 0){
            return labels[0]
          }
          else{
            return '';
          }
        }
      } 

      public getHiddenColumns(value: string): any {
        if (value == "correspondence"){
            return DashboardsConfigurations.correspondenceHiddenColumns
        }
      } 
}