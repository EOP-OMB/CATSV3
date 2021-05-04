import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { DocumentOperationsComponent } from './document-operations.component';

describe('DocumentOperationsComponent', () => {
  let component: DocumentOperationsComponent;
  let fixture: ComponentFixture<DocumentOperationsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DocumentOperationsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DocumentOperationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
