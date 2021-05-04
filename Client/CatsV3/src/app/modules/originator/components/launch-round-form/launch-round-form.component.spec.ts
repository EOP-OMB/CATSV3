import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { LaunchRoundFormComponent } from './launch-round-form.component';

describe('LaunchRoundFormComponent', () => {
  let component: LaunchRoundFormComponent;
  let fixture: ComponentFixture<LaunchRoundFormComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ LaunchRoundFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LaunchRoundFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
