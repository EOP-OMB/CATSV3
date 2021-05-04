import { Component, OnInit, Inject } from '@angular/core';
import {MatDialog, MAT_DIALOG_DATA, MatDialogRef} from '@angular/material/dialog';

@Component({
  selector: 'app-dialog-content-popup',
  templateUrl: './dialog-content-popup.component.html',
  styleUrls: ['./dialog-content-popup.component.scss']
})
export class DialogContentPopupComponent implements OnInit {

  constructor(private dialogRef: MatDialogRef<DialogContentPopupComponent>, @Inject(MAT_DIALOG_DATA, ) public data: any) {}

  ngOnInit(): void {
  }

  onDialogClose(){
    this.dialogRef.close();
  }

}


