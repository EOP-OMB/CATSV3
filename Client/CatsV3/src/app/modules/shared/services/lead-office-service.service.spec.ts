import { TestBed } from '@angular/core/testing';

import { LeadOfficeServiceService } from './lead-office-service.service';

describe('LeadOfficeServiceService', () => {
  let service: LeadOfficeServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LeadOfficeServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
