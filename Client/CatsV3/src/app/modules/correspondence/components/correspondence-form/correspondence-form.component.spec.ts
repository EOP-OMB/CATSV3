import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { CorrespondenceFormComponent } from './correspondence-form.component';

describe('CorrespondenceFormComponent', () => {
  let component: CorrespondenceFormComponent;
  let fixture: ComponentFixture<CorrespondenceFormComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CorrespondenceFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CorrespondenceFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
