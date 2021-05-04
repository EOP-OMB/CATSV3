import { DtoBase } from 'mod-framework';
import { DatePipe } from '@angular/common';
import { DataSources } from '../modules/shared/interfaces/data-source';

export class Surrogate extends DtoBase {

    id: number = 0
    catsUserSPID: string = ''
    catsUser: string  =  ""
    catsUserUPN: string = ''
    surrogateSPUserID: string = ''
    surrogate: string = ''
    surrogateUPN: string = ''
    createdBy: string = ''
    modifiedBy:string = ''
    deletedBy: string 
    createdTime:Date = new Date();// this.datePipe.transform(new Date(), 'yyyy-MM-dd')
    modifiedTime:Date = new Date();//this.datePipe.transform(new Date(), 'yyyy-MM-dd')
    deletedTime:Date 
    isDeleted:boolean = false
    type: string = '';
}
