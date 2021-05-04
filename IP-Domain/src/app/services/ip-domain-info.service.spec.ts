import { TestBed } from '@angular/core/testing';

import { IpDomainInfoService } from './ip-domain-info.service';

describe('IpDomainInfoService', () => {
  let service: IpDomainInfoService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(IpDomainInfoService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
