import { Component, OnInit, Input, IterableDiffers, ChangeDetectionStrategy, Output, EventEmitter, ElementRef } from '@angular/core';
import { ReviewerService } from 'src/app/modules/reviewer/services/reviewer.service';
import { Reviewer } from 'src/app/modules/reviewer/models/reviewer.model';
import { MatDialog } from '@angular/material/dialog';
import { DialogContentHolderComponent } from '../dialog-content-holder/dialog-content-holder.component';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-summary',
  templateUrl: './summary.component.html',
  styleUrls: ['./summary.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [DatePipe]
})
export class SummaryComponent implements OnInit {
  @Input() summaryForms: any[];
  @Input() changeDetectionValue: number;
  @Input() isPendingLetter : boolean;
  @Output() stepEditEmitter = new EventEmitter<number>();
  @Input() isEditable : boolean;

  summaryresult : any[];

  constructor( private elementRef: ElementRef, private reviewerService: ReviewerService, public dialog: MatDialog,private datePipe: DatePipe = new DatePipe('en-US')) { 
  }

  ngOnInit(): void {  
    //console.log('created!');
  }

  ngOnChanges() {
    //console.log('Changes detected! ngOnChanges');
    //if (this.summaryForms)
    //  console.log(JSON.stringify(this.summaryForms) );   
      
  }

  ngAfterViewChecked(){
    //listener to show completed users for a clicked round in the summary page "Completed Round" field
    var el = this.elementRef.nativeElement.querySelectorAll('.completedRounds');
    if(el.length > 0) {
      el.forEach(element => {
        const onclick = element.getAttribute('listener');
        //making sure that the handler is not registered again
        if (onclick != 'true'){
          const catsid = element.getAttribute('catsid');
          const round = element.getAttribute('round');
          element.setAttribute('listener', 'true');
          element.addEventListener('click', this.onShowCompletedUsers.bind(this, catsid, round));
        }        
      });
    } 
  }

  onShowCompletedUsers(catsid: string, round: string){
    let _this = this;
    let _round = round;
    let _catsId = catsid;
    this.reviewerService.getCompletedReviewersWithOptions(catsid, round, "Completed").subscribe(response => {
      let reviewers : Reviewer[] = response;
      let data: string = '<div class="completedTitle"><span>' + _round + '</span>' + '<span>' + _catsId + '</span></div>';
      data += '<table class="showCompletedTable">';
      data += '<tr><th>Completed By</th><th>Completed Date</th></tr>';
      reviewers = reviewers.sort((a,b) => a.roundCompletedDate?.localeCompare(b.roundCompletedDate));
      reviewers.forEach(rev => {
        if (rev.roundCompletedBy?.trim() != '' && rev.roundCompletedBy != undefined){
          data += "<tr><td class='showCompletedText'>" + rev.roundCompletedBy + "</td>" + "<td class='showCompletedText'>" + _this.datePipe.transform(rev.roundCompletedDate,"MM/dd/yyyy HH:mm:ss") + "</td></tr>";
        }        
      });
      data += '</table>';
      const dialogRef = this.dialog.open(DialogContentHolderComponent, {
        //width: '500px',
        data: data
      });
    });
  }

  onEditStep(stepIndex:number){
    this.stepEditEmitter.emit(stepIndex);
  }

  getStringFromHtml(text):any{
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

}
