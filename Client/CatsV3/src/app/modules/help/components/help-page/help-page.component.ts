import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router } from '@angular/router';
import { DataSources } from 'src/app/modules/shared/interfaces/data-source';
import { checkSupportedBrowser } from 'src/app/modules/shared/utilities/utility-functions';
import { HelpDocument } from '../../Models/helpDocument.model';
import { HelpDocumentService } from '../../services/helpDocument.service';

@Component({
  selector: 'app-help-page',
  templateUrl: './help-page.component.html',
  styleUrls: ['./help-page.component.scss']
})
export class HelpPageComponent implements OnInit {

  helpDocuments: HelpDocument[] = [];
  documentUrl: string = 'assets/helpdocuments/Introduction to CATS 8-30-18.pdf';
  selectedHelpItem: string;
  selected: number = 0;

  constructor(private helpDocumentService: HelpDocumentService, public initialDataSources: DataSources, private dialog: MatDialog, private router: Router, private route: ActivatedRoute) { }

  ngOnInit(): void {
    
    //check if the browser is supported
    if (!checkSupportedBrowser(this.dialog, this.router)) return;

    if (this.initialDataSources.helpDocuments == undefined){
      this.helpDocumentService.getAll().then( response => { 
        this.helpDocuments = response;
         //return value ? true : false; 
      });
    }
    else{
      this.helpDocuments =  this.initialDataSources.helpDocuments;
    }

    this.route.queryParams.subscribe(params => {
        this.selectedHelpItem = params['selectedhelp'] ? params['selectedhelp'] : this.helpDocuments[0]?.title;
        let item = this.helpDocuments.find(x => x.title.includes(this.selectedHelpItem));
        if (item != undefined){
          this.selected = this.helpDocuments.findIndex(x => x.title.includes(this.selectedHelpItem));;
          this.documentUrl = 'assets/helpdocuments/' + item.url;
        }
    });
    
  }

  onshowDocument(docUrl: string, selectedIndex : number){
    this.selected = selectedIndex;
    this.documentUrl = 'assets/helpdocuments/' + docUrl;
  }

}
