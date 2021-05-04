import { DtoBase } from 'mod-framework';
import { LeadOfficeMember } from './leadOfficeMember.model';
import { LeadOfficeOfficeManager } from './LeadOfficeOfficeManager.model';

export class LeadOffice extends DtoBase {
    id: number
    createdBy: string
    deletedBy: string
    deletedTime: string
    description: string
    name: string
    isHidden: boolean
    leadOfficeMembers: LeadOfficeMember[]
    leadOfficeOfficeManagers: LeadOfficeOfficeManager[]
}

export class UserEntity extends DtoBase {
    id: number
    createdBy: string
    deletedBy: string
    deletedTime: string    
    isDl: boolean
    description: string
    name: string
    Members: CATSUser[] = []
}

export class CATSUser extends DtoBase {
    id: number
    createdBy: string
    deletedBy: string
    deletedTime: string
    description: string
    isManager: boolean
    EntityId: number
    EntityName: string
    roleId: number
    userFullName: string
    userUPN: string
}