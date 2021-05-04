import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { CorrespondenceDetailsComponent } from './correspondence-details.component';

describe('CorrespondenceDetailsComponent', () => {
  let component: CorrespondenceDetailsComponent;
  let fixture: ComponentFixture<CorrespondenceDetailsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CorrespondenceDetailsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CorrespondenceDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
