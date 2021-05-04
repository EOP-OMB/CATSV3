import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ReportLeadofficePerformanceMetricsComponent } from './report-leadoffice-performance-metrics.component';

describe('ReportLeadofficePerformanceMetricsComponent', () => {
  let component: ReportLeadofficePerformanceMetricsComponent;
  let fixture: ComponentFixture<ReportLeadofficePerformanceMetricsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ReportLeadofficePerformanceMetricsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ReportLeadofficePerformanceMetricsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
