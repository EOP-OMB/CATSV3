import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { DeletedPackagesComponent } from './deleted-packages.component';

describe('DeletedPackagesComponent', () => {
  let component: DeletedPackagesComponent;
  let fixture: ComponentFixture<DeletedPackagesComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ DeletedPackagesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DeletedPackagesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
