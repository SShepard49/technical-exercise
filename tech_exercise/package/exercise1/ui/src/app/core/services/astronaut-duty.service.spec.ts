import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { AstronautDutiesResult, AstronautDutyService } from './astronaut-duty.service';
import { API_BASE_URL } from '../tokens';

const baseUrl = 'http://localhost:9999';

describe('AstronautDutyService', () => {
  let service: AstronautDutyService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AstronautDutyService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: baseUrl },
      ],
    });
    service = TestBed.inject(AstronautDutyService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('GETs /AstronautDuty/{name} and unwraps person + duties', () => {
    let received: AstronautDutiesResult | undefined;
    service.getByName('Jane Doe').subscribe((r) => {
      received = r;
    });

    const req = httpMock.expectOne(`${baseUrl}/AstronautDuty/Jane%20Doe`);
    expect(req.request.method).toBe('GET');
    req.flush({
      success: true,
      message: 'ok',
      responseCode: 200,
      person: {
        personId: 2,
        name: 'Jane Doe',
        currentRank: 'COL',
        currentDutyTitle: 'RETIRED',
        careerStartDate: '2020-01-01',
        careerEndDate: '2021-05-31',
        isRetired: true,
      },
      astronautDuties: [
        {
          id: 1,
          personId: 2,
          rank: 'COL',
          dutyTitle: 'RETIRED',
          dutyStartDate: '2021-06-01',
          dutyEndDate: null,
        },
      ],
    });

    expect(received?.person?.name).toBe('Jane Doe');
    expect(received?.duties.length).toBe(1);
    expect(received?.duties[0]?.dutyEndDate).toBeNull();
  });

  it('returns null person and empty duties when the API does not find the name', () => {
    let received: AstronautDutiesResult | undefined;
    service.getByName('Unknown').subscribe((r) => {
      received = r;
    });

    httpMock
      .expectOne(`${baseUrl}/AstronautDuty/Unknown`)
      .flush({ success: true, message: 'ok', responseCode: 200, person: null });

    expect(received?.person).toBeNull();
    expect(received?.duties).toEqual([]);
  });
});
