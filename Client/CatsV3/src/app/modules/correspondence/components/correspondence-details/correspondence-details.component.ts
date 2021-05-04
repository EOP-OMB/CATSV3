import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-correspondence-details',
  templateUrl: './correspondence-details.component.html',
  styleUrls: ['./correspondence-details.component.scss']
})
export class CorrespondenceDetailsComponent implements OnInit {

  itemIdValue : string = "0";
  constructor(private router: ActivatedRoute) { 
      this.itemIdValue = this.router.snapshot.queryParamMap.get('id');
  }

  ngOnInit(): void {

  }

}
