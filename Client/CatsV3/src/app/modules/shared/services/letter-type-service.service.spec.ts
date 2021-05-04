import { TestBed } from '@angular/core/testing';

import { LetterTypeServiceService } from './letter-type-service.service';

describe('LetterTypeServiceService', () => {
  let service: LetterTypeServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LetterTypeServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
