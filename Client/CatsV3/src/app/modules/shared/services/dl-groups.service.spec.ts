import { TestBed } from '@angular/core/testing';

import { DlGroupsService } from './dl-groups.service';

describe('DlGroupsService', () => {
  let service: DlGroupsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DlGroupsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
