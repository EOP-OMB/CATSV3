import { DtoBase } from 'mod-framework';

export class ReviewRound extends DtoBase {
    
    id: number
    name: string
    description: string
    reviewRoundAcronym: string
    isCombinedRounds:boolean
    createdBy: string
    createdTime: Date
    deletedBy: string
    deletedTime: Date
    isDeleted: boolean
    modifiedBy: string
    modifiedTime: Date
}