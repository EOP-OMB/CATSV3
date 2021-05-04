import { DtoBase } from 'mod-framework';
import { CollaborationService } from '../../shared/services/collaboration-service';
import { Collaboration } from './collaboration.model';

export class Originator extends DtoBase {
    roundName: string
    collaborationId: number
    emailControlId: number
    originatorUpn: string
    sharepointId: number
    originatorName: string
    office: string
    ModifiedTime: string
    DeletedTime: string
    CreatedBy: string
    ModifiedBy: string
    DeletedBy: string
    IsDeleted: boolean
    isEmailSent:boolean
    collaboration: Collaboration
}