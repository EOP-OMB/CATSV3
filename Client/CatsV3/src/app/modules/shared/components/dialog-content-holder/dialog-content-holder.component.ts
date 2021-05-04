import { Component, OnInit, Inject } from '@angular/core';
import {MatDialog, MAT_DIALOG_DATA} from '@angular/material/dialog';
import { DialogData } from '../dialog-prompt/dialog-prompt.component';

@Component({
  selector: 'app-dialog-content-holder',
  templateUrl: './dialog-content-holder.component.html',
  styleUrls: ['./dialog-content-holder.component.scss']
})
export class DialogContentHolderComponent implements OnInit {

  constructor(@Inject(MAT_DIALOG_DATA) public data: any) {}

  ngOnInit(): void {
  }

}


