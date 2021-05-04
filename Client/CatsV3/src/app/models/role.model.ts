import { DtoBase } from 'mod-framework';

export class Role extends DtoBase {
    
    id: number
    name: string
    description: string
    officeId: number
    office: string
    createdBy: string
    createdTime: Date
    deletedBy: string
    deletedTime: Date
    isDeleted: boolean
    modifiedBy: string
    modifiedTime: Date
}