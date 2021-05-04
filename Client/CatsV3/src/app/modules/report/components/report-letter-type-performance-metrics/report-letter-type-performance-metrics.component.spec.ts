import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ReportLetterTypePerformanceMetricsComponent } from './report-letter-type-performance-metrics.component';

describe('ReportLetterTypePerformanceMetricsComponent', () => {
  let component: ReportLetterTypePerformanceMetricsComponent;
  let fixture: ComponentFixture<ReportLetterTypePerformanceMetricsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ReportLetterTypePerformanceMetricsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ReportLetterTypePerformanceMetricsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
