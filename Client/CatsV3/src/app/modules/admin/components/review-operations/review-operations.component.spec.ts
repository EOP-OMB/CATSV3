import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ReviewOperationsComponent } from './review-operations.component';

describe('ReviewOperationsComponent', () => {
  let component: ReviewOperationsComponent;
  let fixture: ComponentFixture<ReviewOperationsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ReviewOperationsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ReviewOperationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
