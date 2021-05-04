import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { PendingLettersComponent } from './pending-letters.component';

describe('PendingLettersComponent', () => {
  let component: PendingLettersComponent;
  let fixture: ComponentFixture<PendingLettersComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ PendingLettersComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PendingLettersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
