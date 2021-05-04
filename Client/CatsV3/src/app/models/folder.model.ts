import { DtoBase } from 'mod-framework';
import { Document } from 'src/app/models/document.model';

export class Folder extends DtoBase {
    id: number
    correspondenceId: number
    createdBy: string
    deletedBy: string
    deletedTime: string
    name: string
    path: string
    documents: Document[]
}
