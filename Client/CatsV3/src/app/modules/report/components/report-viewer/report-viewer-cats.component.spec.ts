import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ReportViewerComponentCATS } from './report-viewer-cats.component';

describe('ReportViewerComponent', () => {
  let component: ReportViewerComponentCATS;
  let fixture: ComponentFixture<ReportViewerComponentCATS>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ReportViewerComponentCATS ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ReportViewerComponentCATS);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
