import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { DialogContentHolderComponent } from './dialog-content-holder.component';

describe('DialogContentHolderComponent', () => {
  let component: DialogContentHolderComponent;
  let fixture: ComponentFixture<DialogContentHolderComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DialogContentHolderComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DialogContentHolderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
