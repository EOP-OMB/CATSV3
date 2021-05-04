import { DtoBase } from 'mod-framework';

export class DLGroupMembers extends DtoBase {
    id: number
    createdBy: string
    deletedBy: string
    deletedTime: string
    description: string
    name: string
    dlGroupId: number
}