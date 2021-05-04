import { DtoBase } from 'mod-framework';

export class CorrespondenceCopiedArchived extends DtoBase {

    id: number
    correspondenceId: number
    archivedUserUpn: string
    archivedUserFullName: string
    createdBy: string
    modifiedBy: string
    deletedBy: string
    deleteTime: Date
    createdTime: Date
    modifiedTime: Date
    isDeleted: boolean
  deletedTime: Date;
}