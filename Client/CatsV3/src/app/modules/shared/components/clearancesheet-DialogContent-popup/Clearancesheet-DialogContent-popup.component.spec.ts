import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ClearancesheetDialogContentPopupComponent } from './Clearancesheet-DialogContent-popup';

describe('DialogContentPopupComponent', () => {
  let component: ClearancesheetDialogContentPopupComponent;
  let fixture: ComponentFixture<ClearancesheetDialogContentPopupComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ClearancesheetDialogContentPopupComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ClearancesheetDialogContentPopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
