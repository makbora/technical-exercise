import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, throwError } from 'rxjs';
import { PersonRepository } from '@abstractions/person.repository';
import {
  AstronautDuty,
  AstronautProfile,
  NewAstronautDuty,
  PersonSummary,
} from '@abstractions/models/person';
import {
  ApiException,
  AstronautDutyClient,
  AstronautDuty as ApiAstronautDuty,
  PersonAstronaut,
  PersonClient,
} from '@api/stargate-api';

/** PersonRepository backed by the Stargate API, through the NSwag-generated clients. */
@Injectable()
export class ApiPersonRepository extends PersonRepository {
  private readonly personClient = inject(PersonClient);
  private readonly dutyClient = inject(AstronautDutyClient);

  getAll(): Observable<PersonSummary[]> {
    return this.personClient.getPeople().pipe(
      map((r) => (r.people ?? []).map(toPersonSummary)),
      catchError((e) => throwError(() => new Error(toErrorMessage(e)))),
    );
  }

  getProfile(name: string): Observable<AstronautProfile> {
    return this.dutyClient.getAstronautDutiesByName(name).pipe(
      map((r) => ({
        person: r.person ? toPersonSummary(r.person) : emptyPerson(name),
        duties: (r.astronautDuties ?? []).map(toDuty),
      })),
      catchError((e) => throwError(() => new Error(toErrorMessage(e)))),
    );
  }

  addPerson(name: string): Observable<void> {
    return this.personClient.createPerson(name).pipe(
      map(() => undefined),
      catchError((e) => throwError(() => new Error(toErrorMessage(e)))),
    );
  }

  addDuty(name: string, duty: NewAstronautDuty): Observable<void> {
    return this.dutyClient
      .createAstronautDuty({
        name,
        rank: duty.rank,
        dutyTitle: duty.title,
        dutyStartDate: duty.startDate,
      })
      .pipe(
        map(() => undefined),
        catchError((e) => throwError(() => new Error(toErrorMessage(e)))),
      );
  }

  rename(name: string, newName: string): Observable<void> {
    return this.personClient.updatePerson(name, { newName }).pipe(
      map(() => undefined),
      catchError((e) => throwError(() => new Error(toErrorMessage(e)))),
    );
  }
}

function toPersonSummary(p: PersonAstronaut): PersonSummary {
  return {
    id: p.personId ?? 0,
    name: p.name ?? '',
    currentRank: p.currentRank || null,
    currentDutyTitle: p.currentDutyTitle || null,
    careerStartDate: p.careerStartDate || null,
    careerEndDate: p.careerEndDate || null,
  };
}

function toDuty(d: ApiAstronautDuty): AstronautDuty {
  return {
    id: d.id ?? 0,
    rank: d.rank ?? '',
    title: d.dutyTitle ?? '',
    startDate: d.dutyStartDate ?? '',
    endDate: d.dutyEndDate || null,
  };
}

function emptyPerson(name: string): PersonSummary {
  return {
    id: 0,
    name,
    currentRank: null,
    currentDutyTitle: null,
    careerStartDate: null,
    careerEndDate: null,
  };
}

/**
 * The generated client fails in two ways: for documented error responses (400) it throws the
 * response body itself; for anything else it throws an ApiException.
 */
export function toErrorMessage(e: unknown): string {
  if (e != null && ApiException.isApiException(e)) {
    if (e.status === 0) return 'Could not reach the Stargate API. Is it running?';
    return messageFromBody(e.response) ?? `The server returned an error (${e.status}).`;
  }
  return messageOf(e) ?? 'Something went wrong.';
}

function messageFromBody(body: string): string | null {
  try {
    return messageOf(JSON.parse(body));
  } catch {
    return null;
  }
}

/**
 * Reads the message from the API's BaseResponse, or the title from ASP.NET's ProblemDetails
 * (returned when the request body fails model binding), without the API's "Bad Request:" prefix.
 */
function messageOf(value: unknown): string | null {
  if (typeof value !== 'object' || value === null) return null;
  const { message, title } = value as { message?: unknown; title?: unknown };
  const text = typeof message === 'string' ? message : typeof title === 'string' ? title : null;
  return text?.replace(/^Bad Request:\s*/, '') || null;
}
