import { DtoBase } from 'mod-framework'
import { Correspondence } from '../../correspondence/Models/correspondence.model'
import { Originator } from './originator.model'
import { Reviewer } from '../../reviewer/models/reviewer.model'
import { FYIUser } from './fyiuser.model'

export class Collaboration extends DtoBase {
    id: number
    correspondenceId : string
    correspondence : Correspondence
    catsid : string
    currentRoundStartDate : Date
    currentRoundEndDate : Date
    completedRounds : string
    boilerPlate : string
    currentOriginators : string
    currentOriginatorsIds : string
    originatorsHtml : string
    currentReviewers : string
    currentReviewersIds: string
    reviewersHtml : string
    currentFYIUsers : string
    currentFYIUsersIds: string
    fYIUsersHtml : string
    surrogateFullName : string
    surrogateUpn : string
    completedReviewers : string
    completedReviewersIds : string
    completedReviewersHtml : string
    draftReviewers : string
    draftReviewersIds : string
    draftReviewersHtml : string
    currentActivity : string
    summaryMaterialBackground : string
    reviewInstructions : string
    originators: Originator[]
    reviewers: Reviewer[]
    fYIUsers: FYIUser[]
    currentOriginatorsIdsCount: number
    currentReviewersCount: number
    currentFYIUsersIdsCount: number
    completedReviewersIdsCount: number
    draftReviewersIdsCount: number


}