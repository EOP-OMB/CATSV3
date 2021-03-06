import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { UsersSettingsComponent } from './users-settings.component';

describe('UsersSettingsComponent', () => {
  let component: UsersSettingsComponent;
  let fixture: ComponentFixture<UsersSettingsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ UsersSettingsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UsersSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
