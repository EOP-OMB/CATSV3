import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ClearancesheetComponent } from './clearancesheet.component';

describe('ClearancesheetComponent', () => {
  let component: ClearancesheetComponent;
  let fixture: ComponentFixture<ClearancesheetComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ClearancesheetComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ClearancesheetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
