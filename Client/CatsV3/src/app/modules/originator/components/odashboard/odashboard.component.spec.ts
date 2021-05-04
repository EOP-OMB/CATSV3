import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { OdashboardComponent } from './odashboard.component';

describe('OdashboardComponent', () => {
  let component: OdashboardComponent;
  let fixture: ComponentFixture<OdashboardComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ OdashboardComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OdashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
