//import { Component, OnInit } from '@angular/core';
import {Component, OnInit, ViewChild, AfterViewInit, VERSION, Input, Output, EventEmitter, Renderer2} from '@angular/core';
//import {VERSION} from '@angular/material';
import { FormControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
//import {MatSelect} from '@angular/material';

import { ReplaySubject } from 'rxjs';
import { Subject } from "rxjs";
import { take, takeUntil } from 'rxjs/operators';
import { MatSelect } from '@angular/material/select';
import { of } from 'rxjs';

@Component({
  selector: 'app-select-search',
  templateUrl: './select-search.component.html',
  styleUrls: ['./select-search.component.scss']
})
export class SelectSearchComponent implements OnInit {
  version = VERSION;

  /** control for the selected bank */
  form: FormGroup;
  public selectCtrl: FormControl = new FormControl();
  
   /** control for the MatSelect filter keyword */
   public selectFilterCtrl: FormControl = new FormControl();

   
   title: string;
   multipleSelect: boolean;
   showAddOptionButton: boolean;
   isDelayCancelled: boolean = false;

   /** list of options */
   @Input() sourceOptions : any[] = []; 
   @Input() selectedOptions: any[] = [];
   @Input() sourceName: string  = "";
   @Input() label: string  = "";
   @Input() isMatLableActivated: boolean = false;
   @Input() placeholder: string  = "";
   @Input() isDisabled: boolean = false;
   @Input() isAutoCompleted: boolean = false;
   @Input() isRequired: string = "false";
   @Input() isAdding: string = "false";
   @Input() appearance: string = "";
   @Input() isSourceOptionsAllString : boolean = true;
   @ViewChild('singleSelect') singleSelect: MatSelect; 

   @Input()  set isMulipleSelect(value: string) {
      this.multipleSelect = value == 'true' ? true : false;
   }
   
  @Output() valueChangeEmitter = new EventEmitter<OutPutOptionsSelected>();
  @Output() clickEmitter: EventEmitter<OutPutOptionsSelected> = new EventEmitter<OutPutOptionsSelected>();

  

  @ViewChild('mySelect') mySelect:any;

   
   /** Subject that emits when the component has been destroyed. */
   private _onDestroy = new Subject<void>();


  constructor(fb: FormBuilder, private renderer: Renderer2,) { 
      this.form = fb.group({
        selectFilterCtrl:['']});
  }

  /** list of options filtered by search keyword */
  public filteredOptions: ReplaySubject<string[]> = new ReplaySubject<string[]>(1);

  ngOnInit(): void {

    //this.sourceOptions = this.sourceOptions == undefined ? [] : this.selectedOptions;
    //this.selectedOptions = this.selectedOptions == undefined ? [] : this.selectedOptions;

    var ctr = new FormControl('',Validators.required);
    this.form.addControl(this.sourceName,ctr); 

    this.isSourceOptionsAllString = this.sourceOptions?.length == 0  || this.sourceOptions == undefined ? true : Object.prototype.toString.call(this.sourceOptions[0]) == '[object String]';
    
    //set default selected option
    if (this.sourceOptions != null){

      this.sourceOptions = this.sourceOptions?.length == 0 || this.sourceOptions != undefined ? this.selectedOptions != undefined ? this.sourceOptions : [] : [];
      
      if(this.isSourceOptionsAllString == true){
      
        this.title = this.selectedOptions?.sort().join('\n');
        if (this.multipleSelect == false){
          this.form.controls[this.sourceName].setValue(this.selectedOptions[0]);
        }
        else{
          this.form.controls[this.sourceName].setValue(this.selectedOptions);
        }
  
        // load the initial option list
        this.filteredOptions.next(this.sourceOptions?.slice());
        
      }
      else{

        if (this.selectedOptions != undefined){
          this.title =  this.selectedOptions?.sort((a, b) => a.displayName < b.displayName ? -1 : a.displayName > b.displayName ? 1 : 0).map(x => x.displayName).join('\n');
          if (this.multipleSelect == false){
            this.form.controls[this.sourceName].setValue(this.selectedOptions[0].upn);
          }
          else{
            this.form.controls[this.sourceName].setValue(this.selectedOptions.map(x => x.upn));
          }
        }
      }      
        
      // load the initial option list
      this.filteredOptions.next(this.sourceOptions.slice());
    }
    else{
      this.sourceOptions = [];
    }
    
    

    //set validation
    this.setValidations();

    // listen for search field value changes
    this.form.controls['selectFilterCtrl'].valueChanges
      .pipe(takeUntil(this._onDestroy))
      .subscribe(() => {
        this.filterOptions();
      });

  }

  setValidations(){   

    //set validation
    if (this.isRequired == "true") {
      //alert(target.innerText)
      this.form.controls[this.sourceName]?.setValidators([Validators.required]);
    }
    else{
      this.form.controls[this.sourceName]?.clearValidators();
    }
    this.form.controls[this.sourceName]?.updateValueAndValidity();
  }

  ngAfterViewInit() {         
    // set initial selection
    this.setInitialValue();
    
    //close the select panel on mouse out
    this.mySelect.openedChange.subscribe(opened => {
      if (opened) {        
        const delay = ms => new Promise(resolve => setTimeout(resolve, ms));   
        this.mySelect.panel.nativeElement.addEventListener('mouseleave', async () => {
          this.isDelayCancelled = false; 
          await delay(1000);   
          if(!this.isDelayCancelled)  {
            this.mySelect.close();
          }          
        });
        this.mySelect.panel.nativeElement.addEventListener('mouseenter', async () => {
          this.isDelayCancelled = true;
          this.mySelect.open();
        })
      }
    });
  }

  ngOnDestroy() {
    this._onDestroy.next();
    this._onDestroy.complete();
  }

  ngOnChanges(changes) {

    //this.sourceOptions = this.sourceOptions == undefined ? [] : this.selectedOptions;
    //this.selectedOptions = this.selectedOptions == undefined ? [] : this.selectedOptions;
    
    this.isSourceOptionsAllString = this.sourceOptions?.length == 0 || this.sourceOptions == undefined ? true : Object.prototype.toString.call(this.sourceOptions[0]) == '[object String]';

    if (this.form.controls[this.sourceName]){
      if(this.isSourceOptionsAllString){
        this.title = this.selectedOptions?.sort().join('\n');
        if (this.multipleSelect == false){
          if (this.selectedOptions.length > 0)
            this.form.controls[this.sourceName].setValue(this.selectedOptions[0]);
          else
            this.form.controls[this.sourceName].setValue([]);
        }
        else{
          this.form.controls[this.sourceName].setValue(this.selectedOptions);
        }
      }
      else{
        this.title =  this.selectedOptions?.sort((a, b) => a.displayName < b.displayName ? -1 : a.displayName > b.displayName ? 1 : 0).map(x => x.displayName).join('\n');
        if (this.multipleSelect == false){
          if (this.selectedOptions.length > 0)
            this.form.controls[this.sourceName].setValue(this.selectedOptions[0].upn);
        }
        else{
          this.form.controls[this.sourceName].setValue(this.selectedOptions.map(x => x.upn));
        }
      }
    }
    // load the initial option list
    this.sourceOptions = this.sourceOptions ? this.sourceOptions : [];
    this.filteredOptions.next(this.sourceOptions.slice());
    //set validation
    this.setValidations();

    //console.log(this); // new value updated
  }  

  /**
   * Emit change to the parent component
   */

  onOptionValueChange(event){
    if(this.isSourceOptionsAllString){
      this.selectedOptions = typeof(event.value) == "string" || this.multipleSelect == false ?  [event.value] : event.value;
      this.selectedOptions = this.selectedOptions.filter((el, i, a) => i === a.indexOf(el))
      //this.selectedOptions = this.selectedOptions.filter((el, i, a) => i === a.indexOf(el))
    }
    else{
      let res = this.sourceOptions.filter(x => event.value.indexOf(x.upn) != -1)
      this.selectedOptions = typeof(event.value) == "string" || this.multipleSelect == false ?  (res?.length > 0 ?res[0] : '') : res;
    }
    
    this.emittChanges();
  }

  emittChanges(){
    if(this.isSourceOptionsAllString){      
      const output: OutPutOptionsSelected = { 
        selectedOptions: this.selectedOptions, 
        source: this.sourceName ,
        isValid : this.form.valid
      };    
      this.title = this.selectedOptions.sort().join('\n');
      this.valueChangeEmitter .emit(output);
    }
    else{     
      const output: OutPutOptionsSelected = { 
        selectedOptions: this.selectedOptions.map(x => x.upn), 
        source: this.sourceName ,
        isValid : this.form.valid
      };    
      this.title = this.selectedOptions?.sort((a, b) => a.displayName < b.displayName ? -1 : a.displayName > b.displayName ? 1 : 0).map(x => x.displayName).join('\n');
      this.valueChangeEmitter .emit(output);
    }
    
  }

  /**
   * Sets the initial value after the filteredValue are loaded initially
   */
  private setInitialValue() {
    this.filteredOptions
      .pipe(take(1), takeUntil(this._onDestroy))
      .subscribe(() => {          
        // setting the compareWith property to a comparison function 
        this.singleSelect.compareWith = (a: string, b: string) => a === b;
      });
  }

  toggleSelectAll(selectAllValue: boolean) {
    this.filteredOptions.pipe(take(1), takeUntil(this._onDestroy))
      .subscribe(val => {
        if (selectAllValue) {
          if(this.isSourceOptionsAllString){
            this.selectedOptions = val?.sort();
            this.form.controls[this.sourceName].patchValue(this.selectedOptions);
            this.mySelect.panel.nativeElement.querySelectorAll('mat-pseudo-checkbox').forEach(x => {
              let d = x;
              this.renderer.removeClass(x, 'mat-pseudo-checkbox-checked')
            })
          }
          else{
            this.selectedOptions = val;
            this.selectedOptions = this.selectedOptions?.sort((a, b) => a.displayName < b.displayName ? -1 : a.displayName > b.displayName ? 1 : 0);
            this.form.controls[this.sourceName].patchValue(this.selectedOptions.map(x => x.upn));
          }
        } else {          
          this.selectedOptions = [];
        }        
        
        this.emittChanges();
      });
  }

  private filterOptions() {
    if (!this.sourceOptions) {
      return;
    }
    // get the search keyword
    let search = this.form.controls['selectFilterCtrl'].value;
    if (!search) {
      this.showAddOptionButton = false;
      this.filteredOptions.next(this.sourceOptions.slice());
      return;
    } else {
      search = search.toLowerCase();
    }
    // filter the options
    if(this.isSourceOptionsAllString){
      this.filteredOptions.next(
        this.sourceOptions.filter(option => option.toLowerCase().indexOf(search) > -1)
      );
    }
    else{
      this.filteredOptions.next(
        this.sourceOptions.filter(option => option.displayName.toLowerCase().indexOf(search) > -1)
      );
    }
    
    
    //Show/hide the Add Button 
    let findings : any[] =  [];
    this.filteredOptions.subscribe(options => findings = options);
    if (findings?.length == 0){
      this.showAddOptionButton = true;
    }
    else{
      this.showAddOptionButton = false;
    }

  }

  onAddingOption(){
    let search = this.form.controls['selectFilterCtrl'].value;
    if(this.isSourceOptionsAllString){
      if (this.selectedOptions.indexOf(search) == -1 && this.sourceOptions.indexOf(search) == -1){
        this.sourceOptions.push(search);
        this.sourceOptions = this.sourceOptions.sort();
        this.filterOptions()
      }
    }
    else{
      if (this.selectedOptions.map(x => x.displayName).indexOf(search) == -1 && this.sourceOptions.map(x => x.displayName).indexOf(search) == -1){
        let d = {
          displayName:search,
          upn:search
        }
        this.sourceOptions.push(d);
        this.sourceOptions = this.sourceOptions?.sort((a, b) => a.displayName < b.displayName ? -1 : a.displayName > b.displayName ? 1 : 0);
        this.filterOptions()
      }
    }
  }

  addClass(className: string) {
      // make sure you declare classname in your main style.css
      this.renderer.addClass(this.mySelect.nativeElement, className);
  }

  removeClass(className: string) {
      this.renderer.removeClass(this.mySelect.nativeElement,className);
  }


}

export function showSSRSReport(reportType){
  alert(reportType);
}

export interface OutPutOptionsSelected{
  selectedOptions: string |string[];
  source: string;
  isValid: boolean;
}
