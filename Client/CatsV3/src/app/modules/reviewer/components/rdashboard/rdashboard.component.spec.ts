import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { RdashboardComponent } from './rdashboard.component';

describe('RdashboardComponent', () => {
  let component: RdashboardComponent;
  let fixture: ComponentFixture<RdashboardComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ RdashboardComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RdashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
