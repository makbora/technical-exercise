import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NewAstronautDuty } from '@abstractions/models/person';
import { AddDutyDialog } from './add-duty-dialog';

describe('AddDutyDialog', () => {
  let fixture: ComponentFixture<AddDutyDialog>;
  let host: HTMLElement;
  let saved: NewAstronautDuty[];

  beforeEach(async () => {
    fixture = TestBed.createComponent(AddDutyDialog);
    fixture.componentRef.setInput('personName', 'Jane Doe');
    fixture.componentRef.setInput('latestStartDate', '2022-01-10T00:00:00');
    saved = [];
    fixture.componentInstance.save.subscribe((duty) => saved.push(duty));
    await fixture.whenStable();
    host = fixture.nativeElement as HTMLElement;
  });

  function fill(id: string, value: string) {
    const input = host.querySelector<HTMLInputElement>(`#${id}`)!;
    input.value = value;
    input.dispatchEvent(new Event('input'));
  }

  async function submit() {
    host.querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  }

  const errors = () =>
    Array.from(host.querySelectorAll('.field-error')).map((e) => e.textContent?.trim());

  it('emits the trimmed duty', async () => {
    fill('duty-rank', ' LTC ');
    fill('duty-title', ' Commander ');
    fill('duty-start', '2025-01-01');

    await submit();

    expect(saved).toEqual([{ rank: 'LTC', title: 'Commander', startDate: '2025-01-01' }]);
  });

  it('shows what is missing instead of submitting an empty form', async () => {
    fill('duty-rank', '   ');

    await submit();

    expect(saved).toEqual([]);
    expect(errors()).toEqual(['Enter a rank.', 'Enter a duty title.', 'Choose a start date.']);
  });

  it('rejects a start date that is not after the current duty', async () => {
    fill('duty-rank', 'LTC');
    fill('duty-title', 'Commander');
    fill('duty-start', '2022-01-10');

    await submit();

    expect(saved).toEqual([]);
    expect(errors()[0]).toMatch(/^Must be after Jan 10, 2022/);
    expect(host.querySelector('#duty-start')!.getAttribute('min')).toBe('2022-01-11');
  });

  it('shows the error from a failed save', async () => {
    fixture.componentRef.setInput('error', 'Person with this name already exists');
    await fixture.whenStable();

    expect(host.querySelector('[role="alert"]')?.textContent).toContain(
      'Person with this name already exists',
    );
  });
});
