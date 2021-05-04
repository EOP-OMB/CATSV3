import { Component, OnInit, ViewChild } from '@angular/core';
import { MainLoaderService } from 'src/app/services/main-loader-service';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { trigger, state, style, transition, animate } from '@angular/animations';

@Component({
  selector: 'app-rdashboard',
  templateUrl: './rdashboard.component.html',
  styleUrls: ['./rdashboard.component.scss'],
  animations: [
    trigger('detailExpand', [
      state('void', style({ height: '0px', minHeight: '0', visibility: 'hidden' })),
      state('*', style({ height: '*', visibility: 'visible' })),
      transition('void <=> *', animate('525ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ]
})
export class RdashboardComponent implements OnInit {

  outGoingDashboard: string = "reviewer"

    constructor(private svc: MainLoaderService) {
  }

  ngOnInit() {    

  }
}
