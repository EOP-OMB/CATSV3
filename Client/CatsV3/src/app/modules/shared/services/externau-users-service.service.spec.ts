import { TestBed } from '@angular/core/testing';

import { ExternauUsersServiceService } from './externau-users-service.service';

describe('ExternauUsersServiceService', () => {
  let service: ExternauUsersServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ExternauUsersServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
