import { DtoBase } from 'mod-framework';

export class CorrespondenceCopiedOffice extends DtoBase {

    id: number
    correspondenceId: number
    officeId: number
    officeName: string
    catsid: string
    createdBy: string
    modifiedBy: string
    deletedBy: string
    deleteTime: Date
    createdTime: Date
    modifiedTime: Date
    isDeleted: boolean
    deletedTime: Date;
}