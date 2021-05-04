import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ClearanceFormComponent } from './clearance-form.component';

describe('ClearanceFormComponent', () => {
  let component: ClearanceFormComponent;
  let fixture: ComponentFixture<ClearanceFormComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ClearanceFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ClearanceFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
