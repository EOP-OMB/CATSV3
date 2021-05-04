import { DtoBase } from 'mod-framework';

export class ExternalUser extends DtoBase {
    id: number
    createdBy: string
    deletedBy: string
    deletedTime: string
    title: string
    name: string
}