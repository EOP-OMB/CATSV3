import { DtoBase } from 'mod-framework';

export class Document extends DtoBase {
    id: number
    folderId: number
    createdBy: string
    createdTime: Date
    modifiedBy: string
    modifiedTime: Date
    deletedBy: string
    deletedTime: Date
    isDeleted: boolean
    documents: string
    name: string
    path: string
    fullPath: string
    type: string
    isModified: boolean
}
