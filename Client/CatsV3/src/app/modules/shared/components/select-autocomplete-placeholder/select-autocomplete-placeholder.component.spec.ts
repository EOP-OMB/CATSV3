import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { SelectAutocompletePlaceholderComponent } from './select-autocomplete-placeholder.component';

describe('SelectAutocompletePlaceholderComponent', () => {
  let component: SelectAutocompletePlaceholderComponent;
  let fixture: ComponentFixture<SelectAutocompletePlaceholderComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SelectAutocompletePlaceholderComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SelectAutocompletePlaceholderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
