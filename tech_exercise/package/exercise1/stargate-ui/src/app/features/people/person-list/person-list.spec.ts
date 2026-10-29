import { TestBed } from '@angular/core/testing';
import { PersonSummary } from '@abstractions/models/person';
import { PersonList } from './person-list';

const people: PersonSummary[] = [
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
];

describe('PersonList', () => {
  function render(selectedName: string | null = null) {
    const fixture = TestBed.createComponent(PersonList);
    fixture.componentRef.setInput('people', people);
    fixture.componentRef.setInput('selectedName', selectedName);
    fixture.detectChanges();
    const buttons = Array.from(
      (fixture.nativeElement as HTMLElement).querySelectorAll<HTMLButtonElement>('button'),
    );
    return { fixture, buttons };
  }

  it('shows each person with their current duty or lack of a record', () => {
    const { buttons } = render();

    const text = (b: HTMLButtonElement, selector: string) =>
      b.querySelector(selector)?.textContent?.trim() ?? null;

    expect(buttons.map((b) => [text(b, '.name'), text(b, '.sub'), text(b, '.chip')])).toEqual([
      ['Jane Doe', 'MAJ · Commander', 'Active'],
      ['Daniel Jackson', 'No astronaut record', null],
    ]);
  });

  it('emits the name of the clicked person', () => {
    const { fixture, buttons } = render();
    const emitted: string[] = [];
    fixture.componentInstance.selected.subscribe((name) => emitted.push(name));

    buttons[1].click();

    expect(emitted).toEqual(['Daniel Jackson']);
  });

  it('marks the selected person', () => {
    const { buttons } = render('Jane Doe');

    expect(buttons[0].getAttribute('aria-current')).toBe('true');
    expect(buttons[1].hasAttribute('aria-current')).toBe(false);
  });
});
