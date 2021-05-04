import { Component, OnInit, Input } from '@angular/core';
import { ReviewerService } from 'src/app/modules/reviewer/services/reviewer.service';
import { Reviewer } from 'src/app/modules/reviewer/models/reviewer.model';
import { DatePipe } from '@angular/common';
import { CorrespondenceService } from 'src/app/modules/correspondence/services/correspondence-service';

@Component({
  selector: 'app-clearancesheet',
  templateUrl: './clearancesheet.component.html',
  styleUrls: ['./clearancesheet.component.scss'],
  providers: [DatePipe]
})
export class ClearancesheetComponent implements OnInit {

  @Input() inComingCATSID: any;
  reviewers: Reviewer[];
  catsId: string = '';
  leadOffice: string = '';
  subject: string = '';
  documentType: string = '';
  summary: string = '';

  constructor(private correspondenceService: CorrespondenceService, private reviewerService: ReviewerService, private datePipe: DatePipe = new DatePipe('en-US')) { }

  ngOnInit(): void {
    this.onShowCompletedUsers();
  }

  onShowCompletedUsers(){
    this.correspondenceService.loadCorrespondenceById(this.inComingCATSID).subscribe(response => {
    //this.reviewerService.getCompletedReviewersWithOptions(this.inComingCATSID, '', "Completed").subscribe(response => {
      this.reviewers = response.collaboration.reviewers.filter(x => x.roundCompletedDate != '' && x.roundCompletedDate != undefined);
      this.reviewers = this.reviewers.sort((a,b) => a.roundCompletedDate?.localeCompare(b.roundCompletedDate));
      this.reviewers.forEach(x => {
        x.roundCompletedDate = this.datePipe.transform(x.roundCompletedDate,"MM/dd/yyyy HH:mm:ss");
      });

      if (this.reviewers.length > 0){
        this.catsId = response?.catsid;
        this.leadOffice = response?.leadOfficeName;
        this.subject = response?.letterSubject;
        this.documentType = response?.letterTypeName;
        this.summary = response.collaboration?.summaryMaterialBackground;
      }
    });
  }
}
