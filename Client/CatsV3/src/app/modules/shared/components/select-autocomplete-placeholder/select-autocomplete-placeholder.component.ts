import { Component, OnInit, Input } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';

@Component({
  selector: 'app-select-autocomplete-placeholder',
  templateUrl: './select-autocomplete-placeholder.component.html',
  styleUrls: ['./select-autocomplete-placeholder.component.scss']
})
export class SelectAutocompletePlaceholderComponent implements OnInit {

  @Input() label: string  = "";
  details: FormGroup;

  constructor(fb: FormBuilder) {
    this.details = fb.group({
      dumbControl: ['']
    });

   }

  ngOnInit(): void {
  }

}
