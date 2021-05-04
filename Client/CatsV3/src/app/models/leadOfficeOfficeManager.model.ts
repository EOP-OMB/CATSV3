import { DtoBase } from 'mod-framework';
import { Role } from './role.model';

export class LeadOfficeOfficeManager extends DtoBase {
    
    id: number
    leadOfficeId: number  
    externalLeadOfficeIds: string
    createdBy: string
    createdTime: Date
    deletedBy: string
    deletedTime: Date
    isDeleted: boolean
    modifiedBy: string
    modifiedTime: Date
    userFullName: string
    userUPN: string
    role: Role
}