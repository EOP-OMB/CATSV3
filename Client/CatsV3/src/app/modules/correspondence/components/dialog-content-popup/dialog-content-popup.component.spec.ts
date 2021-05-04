import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { DialogContentPopupComponent } from './dialog-content-popup.component';

describe('DialogContentPopupComponent', () => {
  let component: DialogContentPopupComponent;
  let fixture: ComponentFixture<DialogContentPopupComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DialogContentPopupComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DialogContentPopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
