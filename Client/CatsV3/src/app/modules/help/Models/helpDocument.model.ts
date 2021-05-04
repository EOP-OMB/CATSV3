import { DtoBase } from 'mod-framework';

export class HelpDocument extends DtoBase {

    id: number
    title: string
    url: string
    isExternal: boolean
    isDevider: boolean
}
