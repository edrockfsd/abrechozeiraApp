/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { AbrechozeiraApiService } from './abrechozeira-api.service';

describe('Service: AbrechozeiraApi', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AbrechozeiraApiService]
    });
  });

  it('should ...', inject([AbrechozeiraApiService], (service: AbrechozeiraApiService) => {
    expect(service).toBeTruthy();
  }));
});
