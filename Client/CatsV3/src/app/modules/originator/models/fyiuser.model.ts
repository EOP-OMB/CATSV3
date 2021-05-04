import { DtoBase } from 'mod-framework';
import { CollaborationService } from '../../shared/services/collaboration-service';

export class FYIUser extends DtoBase {
    roundName: string
    collaborationId: number
    collaboration: CollaborationService
    fyiUpn: string
    sharepointId: number
    fYIUserName: string
    cffice: string
    CreatedTime: string
    ModifiedTime: string
    DeletedTime: string
    CreatedBy: string
    ModifiedBy: string
    DeletedBy: string
    IsDeleted: boolean
    isEmailSent:boolean
}