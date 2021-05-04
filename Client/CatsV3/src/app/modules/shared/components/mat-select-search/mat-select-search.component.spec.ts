import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { MatSelectSearchComponent } from './mat-select-search.component';

describe('MatSelectSearchComponent', () => {
  let component: MatSelectSearchComponent;
  let fixture: ComponentFixture<MatSelectSearchComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ MatSelectSearchComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MatSelectSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
