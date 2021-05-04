import { FormGroup } from '@angular/forms';
import { startWith, map } from 'rxjs/operators';
import { Observable } from 'rxjs';

export class CorrespondenceFormUtilities {
    
    _lastFilter : string;
    public constructor( private details:FormGroup, private options: string[], private selectedOptions: string[]){

    }
    
    activateAutoCompleteControls( 
        filteredOptionsOtherSigners: Observable<string[]>,
        filteredOptionsCorrepondents: Observable<string[]>,
        lastFilter: string = ''){

            this._lastFilter = lastFilter;
    
        filteredOptionsOtherSigners = this.details.controls['otherSigners'].valueChanges.pipe(
          startWith<string | string[]>(''),
          map(value => typeof value === 'string' ? value : lastFilter),
          //map(filter => this.filterOtherSigners(filter))
          map(value => 
            { 
              let values : string[] = value.split('\n');
              return this.filterOtherSigners(values[values.length - 1])
            })
        );
  
        filteredOptionsCorrepondents = this.details.controls['correspondentName'].valueChanges
        .pipe(
          startWith(''),
          map(value => this._filter(value))
        );
        //
    }

    //filter as typing in the auto completed box
  private _filter(value: string): string[] {
    const filterValue = value.toLowerCase();
    return this.options.filter(option => option.toLowerCase().includes(filterValue));
  }

  filterOtherSigners(filter: string):string[] {
    this._lastFilter = filter;
    if (filter ) {
      return this.options.filter(option => {
        return option.toLowerCase().indexOf(filter.toLowerCase()) >= 0 || 
        this.selectedOptions.indexOf(option) != -1;
      })
    } else {
      return this.options.slice();
    }
  }

  displayFn(value: string[] | string): string | undefined {
    let displayValue: string;
    if (Array.isArray(value)) {
      value.forEach((option, index) => {
        if (index === 0) {
          displayValue = option;
        } else {
          displayValue += '\n' + option;
        }
      });
    } else {
      displayValue = value;
    }
    return displayValue;
  }

  toggleSelection(option: string) {
    if (this.selectedOptions.indexOf(option) == -1){
      this.selectedOptions.push(option);
    }
    else{
      const i = this.selectedOptions.findIndex(value => value === option);
      this.selectedOptions.splice(i, 1);
    }
    this.details.controls['otherSigners'].setValue(this.selectedOptions);
  }
}
