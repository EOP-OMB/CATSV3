import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { CdashboardComponent } from './cdashboard.component';

describe('CdashboardComponent', () => {
  let component: CdashboardComponent;
  let fixture: ComponentFixture<CdashboardComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CdashboardComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CdashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
