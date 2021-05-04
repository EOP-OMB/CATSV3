import { Component, OnInit, ViewChild, ElementRef, HostListener, Input } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { checkSupportedBrowser, setIframebackground } from 'src/app/modules/shared/utilities/utility-functions';
import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml, SafeStyle, SafeScript, SafeUrl, SafeResourceUrl } from '@angular/platform-browser';
import { ReportViewerComponent } from 'ngx-ssrs-reportviewer';
import { DatePipe } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { map, startWith } from 'rxjs/operators';
import { DataSources } from 'src/app/modules/shared/interfaces/data-source';

@Component({
  selector: 'app-report-viewer',
  templateUrl: './report-viewer-cats.component.html',
  styleUrls: ['./report-viewer-cats.component.scss'],
  providers: [DatePipe]
})
export class ReportViewerComponentCATS implements OnInit {

  step = 0;
  myDefaultValue = ' ';
  reportDatesBetween: FormGroup;
  isReportDetailsByItemOn: boolean = false;
  dateBetween : string = '';
  detailsSelectedItem = '';
  isUnderReviewOnly: string = '';
  isShowToolbar: boolean = true;
  reportType: string = '';
  hideEmbedded: boolean = true;
  safeUrl: any;

  //iframe sources
  reportStatisticsSrc: string;
  reportMasterReviewOpenItemsReportSrc: string;
  reportReviewOpenItemsDetailsReportSrc: string;

  // Reports settings
  //isLoading: boolean = true;
  reportResponseTitle: string = '';
  reportServer: string = 'https://reports.omb.gov/ReportServer';
  reportUrlStatistics: string = 'CATS/Reports/CATSV3StatisticsReport';
  reportUrlOpenItemsMasterReport: string = 'CATS/Reports/CATSV3OpenItemsMasterReport';
  showParameters: string = "false"; 
  parameters: any = {
    'StartDate': null,
    'EndDate' : null,
    'CorrespondentName': null
   };
  language: string = "en-us";
  width: number = 100;
  height: number = 100;
  toolbar: string = "false";

  @ViewChild('statisticReport') paginator: HTMLIFrameElement;

  constructor(public initialDataSources: DataSources,private elementRef: ElementRef,fb: FormBuilder, protected _sanitizer: DomSanitizer, private dialog: MatDialog, private router: Router, private datePipe:DatePipe = new DatePipe("en-US")) { 
    this.reportDatesBetween = fb.group({
      startDate:[this.datePipe.transform(new Date('01/01/2018'), 'yyyy-MM-dd'),  Validators.required],
      endDate:[this.datePipe.transform(new Date(), 'yyyy-MM-dd'),  Validators.required],
      correspondentName:[''],
      administrationId:['']
    });
    
    //add post message listener to get messages from ssrs report events 
    if (window.addEventListener) {
      window.addEventListener("message", this.receiveMessage.bind(this), false);
    } else {
        (<any>window).attachEvent("onmessage", this.receiveMessage.bind(this));
    }

  }

  ngOnInit(): void {
    
    this.reportUrlStatistics = this.getSafeUrl('');
    this.reportUrlOpenItemsMasterReport =  this.getSafeUrl('');
    this.reportMasterReviewOpenItemsReportSrc =  this.getSafeUrl('');
    this.reportReviewOpenItemsDetailsReportSrc =  this.getSafeUrl('');
    this.setDatesRange ();
    this.setReportStatisticSource();
    //this.setReportMasterReviewOpenItemsReport(); 
    
    this.reportDatesBetween .controls['correspondentName'].valueChanges
      .pipe(
        startWith(''),
        map(value => this.onCorrespondentNaneChange(value? value: ''))
      );
    
    //check if the browser is supported
    if (!checkSupportedBrowser(this.dialog, this.router)) return; 
    
  }

  onCorrespondentNaneChange(value: any): any {
    return '';
  }

  isLoading(){
    alert('loaded')
  }

  receiveMessage: any = (event: any) =>  {
    let origin = event.origin.toLowerCase(); 
    if (origin.indexOf('.omb.gov') === -1 || typeof event.data == 'object')
        return;
    if (event.data.split('|').length > 1){
      this.reportType = event.data.split('|')[0];
      this.detailsSelectedItem = event.data.split('|')[1];
      this.isReportDetailsByItemOn = true;
      this.setreportReviewOpenItemsDetailsReportSrc();
    }
    else{
      this.isReportDetailsByItemOn = false;
      this.reportType = event.data.split('|')[0];
    }
    
    this.setReportMasterReviewOpenItemsReport();
    //alert(this.reportType);
  }
  
  onReportRefresh(report: string){    
    
    this.setDatesRange ();
    this.reportUrlStatistics = this.getSafeUrl('');
    this.reportUrlOpenItemsMasterReport =  this.getSafeUrl('');
    this.reportMasterReviewOpenItemsReportSrc =  this.getSafeUrl('');
    this.reportReviewOpenItemsDetailsReportSrc =  this.getSafeUrl('');
    this.isReportDetailsByItemOn = false;
    //this.isShowToolbar = true;
    this.setReportStatisticSource();
    //this.setReportMasterReviewOpenItemsReport();
  }

  onGoBackToTheMainReport(){
    this.isReportDetailsByItemOn = false;
  }

  onShowRepotCommands(event){
    this.isShowToolbar = event;
    this.setReportMasterReviewOpenItemsReport();
  }

  setDatesRange ()
  { 
    this.parameters = {
      'StartDate': this.reportDatesBetween.controls['startDate'].value,
      'EndDate' : this.reportDatesBetween.controls['endDate'].value,
      'CorrespondentName' : this.reportDatesBetween.controls['correspondentName'].value == '' ? '' : this.reportDatesBetween.controls['correspondentName'].value,
      'AdministrationId' : this.reportDatesBetween.controls['administrationId'].value == '' ? '' : this.reportDatesBetween.controls['administrationId'].value
    }
    
    //this.dateBetween = "&StartDate=" + this.reportDatesBetween.controls['startDate'].value + "&EndDate=" + this.reportDatesBetween.controls['endDate'].value + "&"; 
    this.dateBetween = "&StartDate=" + this.reportDatesBetween.controls['startDate'].value + "&EndDate=" + this.reportDatesBetween.controls['endDate'].value + "&CorrespondentName=" + this.reportDatesBetween.controls['correspondentName'].value + "&" + "&AdministrationId=" + this.reportDatesBetween.controls['administrationId'].value + "&"; 
  }

  setReportStatisticSource(){
    this.reportStatisticsSrc = "https://reports.omb.gov/Reports/report/CATS/Reports/CATSV3StatisticsReport?" + this.dateBetween + "rc:Toolbar=false&rc:Parameters=Collapsed&rs:ClearSession=true&rs:embed=true";   
    this.reportStatisticsSrc = this.getSafeUrl(this.reportStatisticsSrc);
    //this.reportUrlStatistics = 'CATS/Reports/CATSV3StatisticsReport';

  }  

  setReportMasterReviewOpenItemsReport(){
    if (this.reportType == '1'){
      this.reportResponseTitle = 'CATS Open Items Report:';
      this.isUnderReviewOnly = "IsUnderReviewOnly=false&";
    }
    else  if (this.reportType == '2'){
      this.reportResponseTitle = 'CATS Open In Clearance Collaboration Items';
      this.isUnderReviewOnly = "IsUnderReviewOnly=true&";
    }
    
    this.reportMasterReviewOpenItemsReportSrc = "https://reports.omb.gov/Reports/report/CATS/Reports/CATSV3OpenItemsMasterReport?" + this.dateBetween + this.isUnderReviewOnly + "isShowToolbar=" + this.isShowToolbar + "&rc:Toolbar=" + this.isShowToolbar + "&rc:Parameters=Collapsed&rs:ClearSession=true&rs:embed=true";   
    this.reportMasterReviewOpenItemsReportSrc = this.getSafeUrl(this.reportMasterReviewOpenItemsReportSrc);
    //this.reportUrlOpenItemsMasterReport = 'CATS/Reports/CATSV3OpenItemsMasterReport';
  }

  setreportReviewOpenItemsDetailsReportSrc(){
    this.reportReviewOpenItemsDetailsReportSrc = "https://reports.omb.gov/Reports/report/CATS/Reports/CATSV3Reviews Status Report Details?&Param_CATSID=" + this.detailsSelectedItem + "&rc:Toolbar=true&rc:Parameters=Collapsed&rs:ClearSession=true&rs:embed=true";  
    this.reportReviewOpenItemsDetailsReportSrc = this.getSafeUrl(this.reportReviewOpenItemsDetailsReportSrc);
  }

  getSafeUrl(url) : any{
      return this._sanitizer.bypassSecurityTrustResourceUrl(url); 
  }

  iframeOnLoad(){
    setIframebackground();
  }
  

}