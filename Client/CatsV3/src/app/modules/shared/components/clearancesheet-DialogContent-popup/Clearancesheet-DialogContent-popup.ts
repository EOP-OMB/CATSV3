import { Component, OnInit, Inject, ElementRef } from '@angular/core';
import {MatDialog, MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';

@Component({
  selector: 'app-clearancesheet-dialogcontent-popup',
  templateUrl: './Clearancesheet-DialogContent-popup.html',
  styleUrls: ['./Clearancesheet-DialogContent-popup.scss']
})
export class ClearancesheetDialogContentPopupComponent implements OnInit {

  printPageHtml: HTMLElement;
  constructor( private elementRef: ElementRef, private dialogRef: MatDialogRef<ClearancesheetDialogContentPopupComponent>, @Inject(MAT_DIALOG_DATA, ) public data: any) {}

  ngOnInit(): void {
  }

  ngAfterViewChecked(){
    //listener to show completed users for a clicked round in the summary page "Completed Round" field
    var el = this.elementRef.nativeElement.querySelectorAll('.printClearancesheetDiv');
    if(el.length > 0) {
      this.printPageHtml = el;
    } 
  }

  onDialogClose(){
    this.dialogRef.close();
  }

  printDiv(){
    //window.print();
    printDiv(this.data.CATSID, this.printPageHtml)
  } 

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


