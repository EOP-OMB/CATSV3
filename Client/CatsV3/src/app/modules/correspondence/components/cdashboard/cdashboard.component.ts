import { Component, OnInit, EventEmitter, Output , ViewChild, ChangeDetectorRef} from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { DataSource } from '@angular/cdk/table';
import { Observable } from 'rxjs';
import { of } from 'rxjs';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { MainLoaderService } from 'src/app/services/main-loader-service';
import { FormGroup, FormControl } from '@angular/forms';
import { CorrespondenceService } from '../../services/correspondence-service';


@Component({
  selector: 'app-cdashboard',
  templateUrl: './cdashboard.component.html',
  styleUrls: ['./cdashboard.component.scss'],
  animations: [
    trigger('detailExpand', [
      state('void', style({ height: '0px', minHeight: '0', visibility: 'hidden' })),
      state('*', style({ height: '*', visibility: 'visible' })),
      transition('void <=> *', animate('525ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ],
})
export class CdashboardComponent implements OnInit {

  outGoingDashboard: string = "correspondence"

  constructor(
      private svc: MainLoaderService,
    private svc2: CorrespondenceService) {
  }

  ngOnInit() {   
  }
}
