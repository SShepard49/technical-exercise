import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { API_BASE_URL } from '../tokens';
import { BaseResponse, PersonAstronaut } from '../models';

interface GetPeopleResponse extends BaseResponse {
  people: PersonAstronaut[];
}

interface GetPersonByNameResponse extends BaseResponse {
  person: PersonAstronaut | null;
}

@Injectable({ providedIn: 'root' })
export class PeopleService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getAll(): Observable<PersonAstronaut[]> {
    return this.http
      .get<GetPeopleResponse>(`${this.baseUrl}/Person`)
      .pipe(map((res) => res.people ?? []));
  }

  getByName(name: string): Observable<PersonAstronaut | null> {
    const encoded = encodeURIComponent(name);
    return this.http
      .get<GetPersonByNameResponse>(`${this.baseUrl}/Person/${encoded}`)
      .pipe(map((res) => res.person ?? null));
  }
}
