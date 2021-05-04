import { DtoBase } from 'mod-framework';
import { Administration } from 'src/app/models/administration.model';
import { Folder } from '../../../models/folder.model';
import { Collaboration } from '../../originator/models/collaboration.model';
import { Originator } from '../../originator/models/originator.model';
import { CorrespondenceCopiedArchived } from './correspondenceCopiedArchived.model';
import { CorrespondenceCopiedOffice } from './CorrespondenceCopiedOffice';

export class Correspondence extends DtoBase {

    id: number
    catsid: string
    adminClosureReason: string
    adminReOpenDate: Date
    adminReOpenReason: string
    reasonsToReopen: string
    copiedOfficeName: string
    copiedUsersDisplayNames: string
    copiedUsersIds: string
    correspondentName: string
    currentReview: string
    externalAgencies: string
    fiscalYear: string
    icon: string
    isReopen: boolean
    isAdminClosure: boolean
    isAdminReOpen: boolean
    isLetterRequired: boolean
    isPendingLeadOffice: boolean    
    isFinalDocument: boolean
    isUnderReview: boolean    
    isArchived: boolean
    isCurrentUserFYI: boolean
    leadOfficeName: string
    leadOfficeUsersDisplayNames: string
    leadOfficeUsersIds: string
    letterCrossReference: string
    letterStatus: string
    letterSubject: string
    letterTypeName: string
    notRequiredReason: string
    notes: string
    otherSigners: string
    otherSignersFormated: string
    padDueDate: Date
    letterDate: Date
    letterReceiptDate: Date
    dueforSignatureByDate: Date
    adminClosureDate: Date
    rejected: boolean
    rejectedLeadOffices: string
    rejectionReason: string
    reviewStatus: string
    folderId: number
    //folder: Folder
    referenceDocuments: string
    reviewDocuments: string
    finalDocuments: string
    documentsCount: string
    reviewers: string
    reviewersCount: number
    originators: string
    originatorsCount: number
    fyiUsers: string
    fyiUsersCount: number
    completedUsers : string
    completedCount: number
    completedRounds : string
    draftUsers : string  
    draftCount: number
    isDeleted : boolean
    deletedBy : string
    deletedTime : Date
    modifiedBy  : string
    modifiedTime: Date
    createdTime  : Date
    createdBy: string
    isSaved  : boolean
    isEmailElligible  : boolean
    whFyi : boolean    
    browsefile:FormData
    attachedFiles: FormData
    currentUserUPN : string
    currentUserEmail: string
    currentUserFullName: string
    SurrogateFullName: string
    SurrogateUpn: string
    officeRole: string
    catsNotificationId: number
    finalDocumentsCount: number
    referenceDocumentsCount: number
    reviewDocumentsCount: number
    administrationId: number
    administration : Administration
    collaboration: Collaboration
    correspondenceCopiedArchiveds : CorrespondenceCopiedArchived[]
    //correspondenceCopiedOffice: CorrespondenceCopiedOffice[]
}
