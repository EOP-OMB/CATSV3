import { DtoBase } from 'mod-framework';
import { DLGroupMembers } from './dLGroupMembers.model';

export class DLGroup extends DtoBase {
    id: number
    createdBy: string
    deletedBy: string
    deletedTime: string
    description: string
    name: string
    dLGroupMembers: DLGroupMembers[]
}