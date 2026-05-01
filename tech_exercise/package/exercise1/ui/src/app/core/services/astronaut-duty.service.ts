import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { API_BASE_URL } from '../tokens';
import { AstronautDuty, BaseResponse, PersonAstronaut } from '../models';

interface GetAstronautDutiesByNameResponse extends BaseResponse {
  person: PersonAstronaut | null;
  astronautDuties: AstronautDuty[];
}

export interface AstronautDutiesResult {
  person: PersonAstronaut | null;
  duties: AstronautDuty[];
}

@Injectable({ providedIn: 'root' })
export class AstronautDutyService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getByName(name: string): Observable<AstronautDutiesResult> {
    const encoded = encodeURIComponent(name);
    return this.http
      .get<GetAstronautDutiesByNameResponse>(`${this.baseUrl}/AstronautDuty/${encoded}`)
      .pipe(
        map((res) => ({
          person: res.person ?? null,
          duties: res.astronautDuties ?? [],
        })),
      );
  }
}
