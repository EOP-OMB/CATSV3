import { Component, OnInit, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { FormGroup, FormBuilder, Validators, FormControl } from '@angular/forms';

@Component({
  selector: 'app-select-autocomplete',
  templateUrl: './select-autocomplete.component.html',
  styleUrls: ['./select-autocomplete.component.scss']
})
export class SelectAutocompleteComponent implements OnInit {

  details: FormGroup;  
  
  @Input() sourceOptions: string[];
  @Input() selectedOptions: string[] = [];
  @Input() sourceName: string  = "";
  @Input() label: string  = "";
  @Input() placeholder: string  = "";
  @Input() isMulipleSelect: string = "false";
  @Input() isAutoCompleted: string = "false";
  @Input() isRequired: string = "false";
  @Input() isAdding: string = "true";


  localSourceOptions: string[] = [];
  localselectedOptions: string[] = [];
  title: string;

  @ViewChild('mySelect') mySelect:any; 
  @Output() valueChangeEmitter = new EventEmitter<OutPutOptionsSelected>();
  @Output() clickEmitter: EventEmitter<OutPutOptionsSelected> = new EventEmitter<OutPutOptionsSelected>();

  constructor(fb: FormBuilder) { 

    this.details = fb.group({
      //selectControl: ['', this.isRequired == "true" ?  Validators.required : ''],
      selectControlText:['']
    }); 
  }  

  ngOnInit(): void {
    
    var ctr = new FormControl('',Validators.required);
    this.details.addControl(this.sourceName,ctr); 

    this.localSourceOptions = this.selectedOptions.length > 0 ? this.selectedOptions : this.sourceOptions;
    this.localselectedOptions = this.selectedOptions;
    this.title = this.localselectedOptions.sort().join('\n');
    if (this.isMulipleSelect == "false"){
      this.details.controls[this.sourceName].setValue(this.localselectedOptions[0]);
    }
    else{
      this.details.controls[this.sourceName].setValue(this.localselectedOptions);
    }
    
    if (this.isRequired == "true") {
      //alert(target.innerText)
      this.details.controls[this.sourceName].setValidators([Validators.required]);
    }
    else{
      this.details.controls[this.sourceName].clearValidators();
    }
    this.details.controls[this.sourceName].updateValueAndValidity();
    
  }

  ngAfterViewInit() {
    //this.mySelect.open();  //to open the list  
    //close the select panel on mouse out
    this.mySelect.openedChange.subscribe(opened => {
      if (opened) {
        this.mySelect.panel.nativeElement.addEventListener('mouseleave', () => {
          this.mySelect.close();
        })
      }
    })
  } 

  onInputTextChange(event){
  
    event.stopPropagation();
    
    var searchValue: string = this.details.get("selectControlText").value;
    if (searchValue.trim() == ""){
      this.localSourceOptions = this.sourceOptions.sort();
    }
    else{
      this.localSourceOptions = this.sourceOptions.filter(x => x.toUpperCase().indexOf(searchValue.toUpperCase().trim()) != -1).sort();
    }
    
  }

  onOptionValueChange(event){
    this.localselectedOptions = typeof(event.value) == "string" || this.isMulipleSelect =="false" ?  [event.value] : event.value;
    this.emittChanges();
  }

  emittChanges(){
    const output: OutPutOptionsSelected = { 
      selectedOptions: this.localselectedOptions, 
      source: this.sourceName ,
      isValid : this.details.valid
    };    
    this.title = this.localselectedOptions.sort().join('\n');
    this.valueChangeEmitter .emit(output);
  }

  ngOnChanges(changes) {
    this.localSourceOptions = this.sourceOptions;
    console.log(this); // new value updated
  }

}

export interface OutPutOptionsSelected{
  selectedOptions: string |string[];
  source: string;
  isValid: boolean;
}
