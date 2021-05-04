import { DtoBase } from 'mod-framework';
import { Collaboration } from '../../originator/models/collaboration.model';

export class Reviewer extends DtoBase {
    roundName: string
    isCurrentRound : boolean
    collaborationId: number
    collaboration: Collaboration
    emailControlId: number
    reviewerUPN: string
    sharepointId: number
    reviewerName: string
    office: string
    CreatedTime: string
    ModifiedTime: string
    deletedTime: string
    CreatedBy: string
    ModifiedBy: string
    DeletedBy: string
    isDeleted: boolean
    isEmailSent:boolean
    icon: string
    roundStartDate: string
    roundEndDate: string
    roundCompletedDate: string
    roundCompletedBy: string
    roundCompletedByUpn: string
    isRoundCompletedBySurrogate: boolean
    roundActivity: string
    draftBy: string
    draftByUpn: string
    draftReason: string
    draftDate: string
    surrogateFullName: string
    surrogateUpn: string
}

      