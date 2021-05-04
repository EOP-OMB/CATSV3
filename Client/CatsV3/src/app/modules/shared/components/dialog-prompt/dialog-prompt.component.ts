import { Component, OnInit, Inject, ViewChild, ElementRef } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormBuilder, FormGroup, FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-dialog-prompt',
  templateUrl: './dialog-prompt.component.html',
  styleUrls: ['./dialog-prompt.component.scss']
})
export class DialogPromptComponent implements OnInit {

  form: FormGroup;
  noThanksLabel: string = "No Thanks";  
  @ViewChild('noThankBtn') noThankBtn: HTMLButtonElement;
  
  constructor(
    private elementRef:ElementRef,
    public dialogRef: MatDialogRef<DialogPromptComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData, private fb: FormBuilder) {
      this.form = this.fb.group({
        promptInput: ['', Validators.required]
      });
    }

  onNoClick(): void {
    this.dialogRef.close("No");    
  }

  onCancel(){
    this.dialogRef.close("Cancel"); 
  }

  onYesClick(): void {
    if (this.data.isConfirmOnly){
      this.data.valueReturned = "OK";      
      this.dialogRef.close(this.data.valueReturned);
    }  
  }


  ngOnInit(): void {        
    
    if (this.data.isReopening == true){
      //this.form.controls["promptInput"].setValidators([Validators.required]);
      //this.form.controls["promptInput"].updateValueAndValidity();
    }
    else{      
      //this.form.controls["promptInput"].clearValidators();
      //this.form.controls["promptInput"].updateValueAndValidity();
    }

    if (this.data.noThanksLabel != ""){
      this.noThanksLabel = this.data.noThanksLabel;
    }
    
    if(this.data.isConfirmOnly == true) {
      this.form.controls["promptInput"].disable();
    } 
    else {
      this.form.controls["promptInput"].enable();
    }

  }

  ngAfterViewInit() {
    this.form.controls['promptInput'].valueChanges.subscribe(selectedValue => {
      if (selectedValue.trim() != ''){
         this.noThankBtn.disabled = true;
      }
      else{
        this.noThankBtn.disabled = false;
      }      
    })
  }

}

export interface DialogData {
  title: string;
  label:string;
  valueReturned: string;
  name: string;
  isConfirmOnly: boolean;
  isReopening: boolean;
  noThanksLabel: string;
}
