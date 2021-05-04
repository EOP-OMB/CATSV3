import { DtoBase } from 'mod-framework';

export class LetterType extends DtoBase {
    id: number
    createdBy: string
    deletedBy: string
    deletedTime: string
    description: string
    name: string
    htmlContent: string
}