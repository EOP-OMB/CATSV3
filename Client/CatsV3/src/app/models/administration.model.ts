import { DtoBase } from 'mod-framework';
import { Document } from 'src/app/models/document.model';

export class Administration extends DtoBase {
    id: number
    name: string
    description: string
    isCurrent: boolean
    inaugurationDate: Date
    createdBy: string
    deletedBy: string
    deletedTime: string
}
