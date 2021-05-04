import { Component, OnInit, ViewChild } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { MainLoaderService } from 'src/app/services/main-loader-service';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';

@Component({
  selector: 'app-odashboard',
  templateUrl: './odashboard.component.html',
  styleUrls: ['./odashboard.component.scss'],
  animations: [
    trigger('detailExpand', [
      state('void', style({ height: '0px', minHeight: '0', visibility: 'hidden' })),
      state('*', style({ height: '*', visibility: 'visible' })),
      transition('void <=> *', animate('525ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ]
})
export class OdashboardComponent implements OnInit {

  outGoingDashboard: string = "originator"

    constructor(private svc: MainLoaderService) {
  }

  ngOnInit() {    

  }
}
