import { Component, OnInit, ViewChild, NgZone, ElementRef } from '@angular/core';
import { DataSources } from 'src/app/modules/shared/interfaces/data-source';
import { STEPPER_GLOBAL_OPTIONS } from '@angular/cdk/stepper';
import { FormBuilder } from '@angular/forms';

@Component({
  selector: 'app-pending-letters',
  templateUrl: './pending-letters.component.html',
  styleUrls: ['./pending-letters.component.scss'],
  providers: [{
    provide: STEPPER_GLOBAL_OPTIONS, useValue: {displayDefaultIndicatorType: false}
  }]
})
export class PendingLettersComponent implements OnInit {


  constructor(private _formBuilder: FormBuilder, private ngZone: NgZone,private initialDataSources: DataSources) {
  }

  ngOnInit() {

    
  }

  ngAfterViewInit() {        
  } 
  

}
