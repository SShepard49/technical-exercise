import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { PeopleService } from './people.service';
import { API_BASE_URL } from '../tokens';
import { PersonAstronaut } from '../models';

const baseUrl = 'http://localhost:9999';

const johnDoe: PersonAstronaut = {
  personId: 1,
  name: 'John Doe',
  currentRank: '1LT',
  currentDutyTitle: 'Commander',
  careerStartDate: '2022-01-01',
  careerEndDate: null,
  isRetired: false,
};

describe('PeopleService', () => {
  let service: PeopleService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        PeopleService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_BASE_URL, useValue: baseUrl },
      ],
    });
    service = TestBed.inject(PeopleService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('GETs /Person and unwraps the people array', () => {
    let received: PersonAstronaut[] | undefined;
    service.getAll().subscribe((p) => {
      received = p;
    });

    const req = httpMock.expectOne(`${baseUrl}/Person`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, message: 'ok', responseCode: 200, people: [johnDoe] });

    expect(received).toEqual([johnDoe]);
  });

  it('returns an empty array when the API omits the people field', () => {
    let received: PersonAstronaut[] | undefined;
    service.getAll().subscribe((p) => {
      received = p;
    });

    httpMock
      .expectOne(`${baseUrl}/Person`)
      .flush({ success: true, message: 'ok', responseCode: 200 });

    expect(received).toEqual([]);
  });

  it('GETs /Person/{name} with a URL-encoded name', () => {
    let received: PersonAstronaut | null | undefined;
    service.getByName('Jane Doe').subscribe((p) => {
      received = p;
    });

    const req = httpMock.expectOne(`${baseUrl}/Person/Jane%20Doe`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, message: 'ok', responseCode: 200, person: johnDoe });

    expect(received).toEqual(johnDoe);
  });

  it('returns null when the API responds with no person', () => {
    let received: PersonAstronaut | null | undefined;
    service.getByName('Unknown').subscribe((p) => {
      received = p;
    });

    httpMock
      .expectOne(`${baseUrl}/Person/Unknown`)
      .flush({ success: true, message: 'ok', responseCode: 200, person: null });

    expect(received).toBeNull();
  });
});
