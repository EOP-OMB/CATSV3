import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { CollaborationDetailsComponent } from './collaboration-details.component';

describe('CollaborationDetailsComponent', () => {
  let component: CollaborationDetailsComponent;
  let fixture: ComponentFixture<CollaborationDetailsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CollaborationDetailsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CollaborationDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
