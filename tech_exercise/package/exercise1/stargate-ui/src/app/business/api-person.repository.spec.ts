import { TestBed } from '@angular/core/testing';
import { Observable, firstValueFrom, of, throwError } from 'rxjs';
import {
  ApiException,
  AstronautDutyClient,
  CreateAstronautDuty,
  GetAstronautDutiesByNameResult,
  GetPeopleResult,
  PersonClient,
  UpdatePerson,
} from '@api/stargate-api';
import { ApiPersonRepository, toErrorMessage } from './api-person.repository';

describe('ApiPersonRepository', () => {
  let people: Observable<GetPeopleResult>;
  let duties: GetAstronautDutiesByNameResult;
  let saveResponse: Observable<unknown>;
  let createdDuties: (CreateAstronautDuty | undefined)[];
  let updates: [string, UpdatePerson | undefined][];
  let repository: ApiPersonRepository;

  beforeEach(() => {
    saveResponse = of({ success: true });
    createdDuties = [];
    updates = [];

    TestBed.configureTestingModule({
      providers: [
        ApiPersonRepository,
        {
          provide: PersonClient,
          useValue: {
            getPeople: () => people,
            updatePerson: (name: string, body?: UpdatePerson) => {
              updates.push([name, body]);
              return saveResponse;
            },
          },
        },
        {
          provide: AstronautDutyClient,
          useValue: {
            getAstronautDutiesByName: () => of(duties),
            createAstronautDuty: (body?: CreateAstronautDuty) => {
              createdDuties.push(body);
              return saveResponse;
            },
          },
        },
      ],
    });
    repository = TestBed.inject(ApiPersonRepository);
  });

  it('sends a new duty with the API field names', async () => {
    await firstValueFrom(
      repository.addDuty('Jane Doe', { rank: 'LTC', title: 'Commander', startDate: '2025-01-01' }),
      { defaultValue: undefined },
    );

    expect(createdDuties).toEqual([
      { name: 'Jane Doe', rank: 'LTC', dutyTitle: 'Commander', dutyStartDate: '2025-01-01' },
    ]);
  });

  it('sends a rename for the person', async () => {
    await firstValueFrom(repository.rename('Jane Doe', 'Jane Smith'), { defaultValue: undefined });

    expect(updates).toEqual([['Jane Doe', { newName: 'Jane Smith' }]]);
  });

  it('turns a rejected save into an Error with the API message', async () => {
    saveResponse = throwError(() => ({
      success: false,
      message: 'Bad Request: Person with this name already exists',
    }));

    await expect(firstValueFrom(repository.rename('Jane Doe', 'Sam Carter'))).rejects.toThrow(
      /^Person with this name already exists$/,
    );
  });

  it('maps people, using null for missing astronaut details', async () => {
    people = of({
      people: [
        {
          personId: 1,
          name: 'Jane Doe',
          currentRank: 'MAJ',
          currentDutyTitle: 'Commander',
          careerStartDate: '2015-03-01T00:00:00',
        },
        { personId: 4, name: 'Daniel Jackson', currentRank: undefined, currentDutyTitle: '' },
      ],
    });

    const result = await firstValueFrom(repository.getAll());

    expect(result).toEqual([
      {
        id: 1,
        name: 'Jane Doe',
        currentRank: 'MAJ',
        currentDutyTitle: 'Commander',
        careerStartDate: '2015-03-01T00:00:00',
        careerEndDate: null,
      },
      {
        id: 4,
        name: 'Daniel Jackson',
        currentRank: null,
        currentDutyTitle: null,
        careerStartDate: null,
        careerEndDate: null,
      },
    ]);
  });

  it('maps a profile with its duties', async () => {
    duties = {
      person: { personId: 1, name: 'Jane Doe', careerStartDate: '2015-03-01T00:00:00' },
      astronautDuties: [
        {
          id: 3,
          rank: 'MAJ',
          dutyTitle: 'Commander',
          dutyStartDate: '2022-01-10T00:00:00',
          dutyEndDate: undefined,
        },
        {
          id: 2,
          rank: 'CPT',
          dutyTitle: 'Pilot',
          dutyStartDate: '2018-06-15T00:00:00',
          dutyEndDate: '2022-01-09T00:00:00',
        },
      ],
    };

    const result = await firstValueFrom(repository.getProfile('Jane Doe'));

    expect(result.person.name).toBe('Jane Doe');
    expect(result.duties).toEqual([
      { id: 3, rank: 'MAJ', title: 'Commander', startDate: '2022-01-10T00:00:00', endDate: null },
      {
        id: 2,
        rank: 'CPT',
        title: 'Pilot',
        startDate: '2018-06-15T00:00:00',
        endDate: '2022-01-09T00:00:00',
      },
    ]);
  });

  it('turns a client error into an Error with a readable message', async () => {
    // For documented error responses the generated client throws the BaseResponse body itself.
    people = throwError(() => ({ success: false, message: 'Bad Request' }));

    await expect(firstValueFrom(repository.getAll())).rejects.toThrow('Bad Request');
  });
});

describe('toErrorMessage', () => {
  it('uses the message from an error response body the client threw', () => {
    expect(toErrorMessage({ success: false, message: 'Name cannot be empty' })).toBe(
      'Name cannot be empty',
    );
  });

  it('explains when the API cannot be reached', () => {
    expect(toErrorMessage(new ApiException('error', 0, '', {}, null))).toBe(
      'Could not reach the Stargate API. Is it running?',
    );
  });

  it('reads the message from an undocumented error response', () => {
    const body = JSON.stringify({ success: false, message: 'Database unavailable' });

    expect(toErrorMessage(new ApiException('error', 500, body, {}, null))).toBe(
      'Database unavailable',
    );
  });

  it('falls back to the status code when the body has no message', () => {
    expect(toErrorMessage(new ApiException('error', 502, '<html>', {}, null))).toBe(
      'The server returned an error (502).',
    );
  });

  it('drops the API\'s "Bad Request:" prefix', () => {
    expect(
      toErrorMessage({ success: false, message: 'Bad Request: Name cannot be empty' }),
    ).toBe('Name cannot be empty');
  });

  it('uses the title of an ASP.NET validation problem', () => {
    expect(
      toErrorMessage({ title: 'One or more validation errors occurred.', status: 400 }),
    ).toBe('One or more validation errors occurred.');
  });

  it('handles values that are not errors', () => {
    expect(toErrorMessage(null)).toBe('Something went wrong.');
  });
});
