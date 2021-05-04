import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ReportReviewroundPerformanceMetricsComponent } from './report-reviewround-performance-metrics.component';

describe('ReportReviewroundPerformanceMetricsComponent', () => {
  let component: ReportReviewroundPerformanceMetricsComponent;
  let fixture: ComponentFixture<ReportReviewroundPerformanceMetricsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ReportReviewroundPerformanceMetricsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ReportReviewroundPerformanceMetricsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
