import { Correspondence } from '../../correspondence/Models/correspondence.model';
import { CurrentBrowserUser } from 'src/app/models/currentBrowserUser.model';
import { FormGroup } from '@angular/forms';
import { Collaboration } from '../../originator/models/collaboration.model';
import { Originator } from '../../originator/models/originator.model';
import { Reviewer } from '../../reviewer/models/reviewer.model';
import { FYIUser } from '../../originator/models/fyiuser.model';
import { EventEmitterService } from 'src/app/services/event-emitter.service';
import { MatDialog } from '@angular/material/dialog';
import { ClearancesheetDialogContentPopupComponent } from '../components/clearancesheet-DialogContent-popup/Clearancesheet-DialogContent-popup';
import { ElementRef } from '@angular/core';
import { DialogAlertComponent } from '../components/dialog-alert/dialog-alert.component';
import { Router } from '@angular/router';

export function sum(a: number, b: number): number {
    return a + b;
}

export interface UserQuery {
  search: string
  registration: Date
}

export function setItemUserRoles(correspodenceData: Correspondence[], currentBrowserUser : CurrentBrowserUser): Correspondence[]{

    correspodenceData.forEach(x => {
      const col = currentBrowserUser.MemberGroupsCollection;
      if (currentBrowserUser.MemberOfAdmins == true){
        x.officeRole = "Admin"
      }
      else if (col.indexOf(x?.leadOfficeName) != -1 || x.leadOfficeName == null){
        x.officeRole = "Lead"
      }
      else {
        x.officeRole = "Copied"
      }
    });  

    return correspodenceData;
}

export function getUserRoles(correspodenceData: Correspondence, currentBrowserUser : CurrentBrowserUser): Correspondence{
    
    const memberGroupsCollection = currentBrowserUser.MemberGroupsCollection;
    const memberExternalGroupsCollection = currentBrowserUser.MemberExternalGroupsCollection;
    if (currentBrowserUser.MemberOfAdmins == true){
      correspodenceData.officeRole = "Admin"
    }
    else if (memberGroupsCollection.indexOf(correspodenceData?.leadOfficeName) != -1 || correspodenceData?.leadOfficeName == null){
      correspodenceData.officeRole = "Lead"
    }
    else if (memberExternalGroupsCollection.indexOf(correspodenceData?.leadOfficeName) != -1 || correspodenceData?.leadOfficeName == null){
      correspodenceData.officeRole = "Extenal Lead"
    }
    else {
      correspodenceData.officeRole = "Copied"
    }

    return correspodenceData;
}

export function getStringFromHtml3(text):any{
  const html = text;
  const div = document.createElement('div');
  div.innerHTML = html;

  let atag = div.querySelectorAll('a');
  if (atag.length > 0){
    let anchorArr: string[] = [];
    atag.forEach(a => anchorArr.push(a.innerText));
    return anchorArr.join('\n');
  }
  else{
    return div.textContent || div.innerText || '';
  }
  
}



export function getStringFromHtml(text){
  const html = text;
  const div = document.createElement('div');
  div.innerHTML = html;
  return div.textContent || div.innerText || '';
}

export function maskAllControlsAsTouched(frm: FormGroup){
  frm.markAllAsTouched();
}

export function sortArrayOfObject(col, field){
  
}

export function getMembersFullnames(value: any, allOMBUsers: any): any{
  if (typeof(value) == 'string'){
    return allOMBUsers.find(u =>  value?.includes(u.upn) == true)?.displayName 
  }
  else{
    return allOMBUsers.filter(u =>  value?.includes(u.upn) == true).map(u => u.displayName);
  }
}

export function getMembersUpns(value: any, allOMBUsers: any): any{
  if (typeof(value) == 'string'){
    return allOMBUsers.find(u =>  value?.includes(u.upn) == true)?.upn
  }
  else{
    return allOMBUsers.filter(u =>  value?.includes(u.upn) == true).map(u => u.upn);
  }
}

export function getMembersUpnAndFullnames(value: any, allOMBUsers: any): any{
    
  if (typeof(value) == 'string'){
    let res : any = {upn: value, displayName: allOMBUsers.find(u =>  value?.includes(u.upn) == true)?.displayName}
    return  res;
  }
  else{
      var filtered: any[] = [];
      value = value.sort();
      
      allOMBUsers.filter(u =>  value?.includes(u.upn) == true)
      .forEach(u =>{
        if ( u.upn.includes('DL-')){
          const res = {upn: u.upn, displayName: u.upn}
          filtered.push(res);
        }
        else{
          const res = {upn: u.upn, displayName: u.displayName}
          filtered.push(res);
        }          
      });

      return filtered.sort((a,b) => a.displayName?.localeCompare(b.displayName));
  }
}

export class CATSPackage{
  correspondence: Correspondence = new Correspondence();
  collaboration: Collaboration = new Collaboration();
  originators: Originator[] = [];
  reviewers: Reviewer[] = [];
  fyiusers: FYIUser[] = [];
  reviewFiles: File[] = [];
  referenceFiles: File[] = [];
  finalFiles: File[] = [];
}

export class CATSSelectedDashboardOptions{
  title: string = ''; // title of the incoming dashboard : Correspondence, Originator, Reviewer
  area: string = '';  // Area within the selected dashboard: Collaboration, Office Data, pending, Cpoied
  filter: string = ''; // data filter used to retrieve data: Open, Closed, Open & Closed, Archive
  selectCATSID: string = '' ; // The selected item in the dashboard 
  searchCriteria: string = ''; // represent the search string when retreiving closed ot Closed&open from the dashboard
}


export function  createLocalStorage(object: any, key : string){
    localStorage.setItem(key,JSON.stringify(object));
  }

export function  clearLocalStorage(key: string){
    localStorage.removeItem(key);
  }

  export function  getLocalStorageItem(key: string): any{
   return JSON.parse(localStorage.getItem(key))
  }

  export function createLocalSessionStorage(object: any, key : string){
    sessionStorage.setItem(key,JSON.stringify(object));
  }

  export function clearSessionStorage(key: string){
    sessionStorage.removeItem(key);
  }

  export function getSessionStorageItem(key: string): any{
   return JSON.parse(sessionStorage.getItem(key))
  }

  export function setIncomingDashboardInRootComponent(dashboardTitle: string, eventEmitterService: EventEmitterService){
    eventEmitterService.onIncomingDashboard(dashboardTitle);
  }

  export function generateClearancesheet(catsID : string, dialog: MatDialog){
    const dialogRef =dialog.open(ClearancesheetDialogContentPopupComponent, {
      // width: '500px',
      data: {
        CATSID: catsID
      }
    });
  }

  export function decodeUrl(url : string): string{
     return decodeURI(url);
  }

  export function encodeUrl(url : string): string{
     return encodeURI(url);
  }

  export function printDiv(CATSID: string, elementToPrint: HTMLElement) {

    var newWin = window.open('', 'Print-Window');

    newWin.document.write(
            '<html>' +
            '<head>' +
                '<title>Clearance Sheet for ' + CATSID + '</title>' +
                '<link href="../../../../../assets/css/clearancesheet.component.scss" rel="stylesheet" />' +
                '<link href=../../../../../assets/css/jquery-ui.css" rel="stylesheet" />' +
                '<link href="../../../../../assets/css/bootstrap.min.css" rel="stylesheet" />' +
                '<link href="../../../../../assets/css/bootstrap-grid.min.css" rel="stylesheet" />' +
                '<link href="../../../../../assets/css/bootstrap-reboot.min.css" rel="stylesheet" />' +
                '<link href="../../../../../assets/css/fileinput.min.css" rel="stylesheet" />' +
                '<link href="../../../../../assets/css/fileinput-rtl.min.css" rel="stylesheet" />' +
                '<link href="../../../../../assets/css/datatables.css" rel="stylesheet" />' +
                '<link href="../../../../../assets/css/ombcats_master_bootstrapped.css" rel="stylesheet"  />' +
            '</head>' +
            //'<body id="rd-printClearanceSheet">' +
            '<body style="border:none"  id="rd-printClearanceSheet" onload="window.print();" ">' +
                '<div class="clearfix img-omb-seal" id="rd-cats-omb-seal-div-ID" style="margin-bottom: 20px; position:relative">' +
                    '<img src="../../../../../assets/images/OMB_Seal_4_mh.png" style="position:absolute; top:0px" />' +
                    '<div class="clearfix" style="margin-left: 105px; min-height: 29px; padding-top: 8px;">' +
                        '<span class="navbar-brand" style="font-family:\'Times New Roman\', Times, serif; font-weight: bold; font-size: 1.39rem; color: rgb(125, 104, 2);">' +
                            'Office of Management and Budget' +
                        '</span>' +
                    '</div>' +
                    '<div class="clearfix" style="margin-left: 105px; margin-top: -12px;">' +
                        '<div class="clearfix" style="font-family: Century Gothic; font-weight: normal; font-style: normal; font-size:1.16rem; color: rgb(41, 40, 61);">' +
                            '<div class="clearfix" style="float: left;">' +
                                'C o l l a b o r a t i o n' +
                            '</div>' +
                            '<div class="clearfix" style="float: left; margin-left: 15px;">' +
                                'a n d' +
                            '</div>' +
                            '<div class="clearfix" style="float: left; margin-left: 15px;">' +
                                'T r a c k i n g' +
                            '</div>' +
                            '<div class="clearfix" style="float: left; margin-left: 15px;">' +
                                'S y s t e m' +
                            '</div>' +
                            '<div class="clearfix" style="float: left; margin-left: 15px;">' +
                                '-' +
                            '</div>' +
                            '<div class="clearfix" style="float: left; margin-left: 15px; font-weight: bold;">' +
                                'C A T S' +
                            '</div>' +
                        '</div>' +
                    '</div>' +
                '</div>' +
                elementToPrint[0].outerHTML +
             '</body></html>'
        );

    newWin.document.close();

    setTimeout(function () { newWin.close(); }, 500);

}

export function setIframebackground(){
  var iframe1 = document.getElementsByTagName('iframe')[0];
  iframe1.style.background = 'yellow';
  iframe1.contentDocument.body.style.backgroundColor = 'blue';
  iframe1.contentWindow.document.body.style.backgroundColor = 'blue';

  
  //var iframe2 = document.getElementsByTagName('iframe')[0];
  //iframe2.style.background = 'yellow';
  //iframe2.contentDocument.body.style.backgroundColor = 'blue';
  //iframe2.contentWindow.document.body.style.backgroundColor = 'blue';
}

export function checkSupportedBrowser( dialog: MatDialog, router: Router): boolean{   
    let isUnsupportedBrowser : boolean = detectSopportedBrowser();
    if (isUnsupportedBrowser == false){
      const dialogRef = dialog.open(DialogAlertComponent, {
        width: '500px',
        data: 'Unsupported browser detected! <br/>Please use either "CHROME" or "EDGE" browser to access CATS'
      });
      
      router.navigateByUrl('/home');
      return false;
    }
    else{
      return true;
    }
}

export function detectSopportedBrowser() {
  var ua = window.navigator.userAgent;
  
  var msie = ua.indexOf('MSIE ');
  if (msie > 0) {
      // IE 10 or older => return version number
      return false;// parseInt(ua.substring(msie + 5, ua.indexOf('.', msie)), 10);
  }
  
  var trident = ua.indexOf('Trident/');
  if (trident > 0) {
      // IE 11 => return version number
      var rv = ua.indexOf('rv:');
      return false; //parseInt(ua.substring(rv + 3, ua.indexOf('.', rv)), 10);
  }
  
  var edge = ua.indexOf('Edge/');
  if (edge > 0) {
     // Edge (IE 12+) => return version number
     return true;  //parseInt(ua.substring(edge + 5, ua.indexOf('.', edge)), 10);
  }
  
  var firefox = ua.indexOf('Firefox/');
  if (firefox > 0) {
     // Edge (IE 12+) => return version number
     return true;  //parseInt(ua.substring(firefox + 5, ua.indexOf('.', firefox)), 10);
  }
  
  var opera = ua.indexOf('Opera/');
  if (opera > 0) {
     // Edge (IE 12+) => return version number
     return true; //parseInt(ua.substring(opera + 5, ua.indexOf('.', opera)), 10);
  }
  
  var chrome = ua.indexOf('Chrome/');
  if (chrome > 0) {
     // Edge (IE 12+) => return version number
     return true;  //parseInt(ua.substring(chrome + 5, ua.indexOf('.', chrome)), 10);
  }
  
  var safari = ua.indexOf('Safari/');
  if (safari > 0) {
     // Edge (IE 12+) => return version number
     return true;  //parseInt(ua.substring(safari + 5, ua.indexOf('.', safari)), 10);
  }
  
  // other browser
  return false;
  }
