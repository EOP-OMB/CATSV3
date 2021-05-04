import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { NewCollaborationFormComponent } from './new-collaboration-form.component';

describe('NewCollaborationFormComponent', () => {
  let component: NewCollaborationFormComponent;
  let fixture: ComponentFixture<NewCollaborationFormComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ NewCollaborationFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(NewCollaborationFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
