import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PersonNameDialog, PersonNameDialogMode } from './person-name-dialog';

describe('PersonNameDialog', () => {
  let fixture: ComponentFixture<PersonNameDialog>;
  let host: HTMLElement;
  let input: HTMLInputElement;
  let saved: string[];

  async function render(mode: PersonNameDialogMode) {
    fixture = TestBed.createComponent(PersonNameDialog);
    fixture.componentRef.setInput('mode', mode);
    if (mode === 'edit') fixture.componentRef.setInput('currentName', 'Jane Doe');
    saved = [];
    fixture.componentInstance.save.subscribe((name) => saved.push(name));
    await fixture.whenStable();
    host = fixture.nativeElement as HTMLElement;
    input = host.querySelector<HTMLInputElement>('#person-name')!;
  }

  async function submit(value: string) {
    input.value = value;
    input.dispatchEvent(new Event('input'));
    host.querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  }

  const text = (selector: string) => host.querySelector(selector)?.textContent?.trim() ?? null;

  describe('adding a person', () => {
    beforeEach(() => render('add'));

    it('starts empty, with add wording', () => {
      expect(input.value).toBe('');
      expect(text('h2')).toBe('Add person');
      expect(text('button[type="submit"]')).toBe('Add person');
    });

    it('emits the trimmed name', async () => {
      await submit('  Jane Doe ');

      expect(saved).toEqual(['Jane Doe']);
    });

    it('rejects a blank name', async () => {
      await submit('   ');

      expect(saved).toEqual([]);
      expect(text('.field-error')).toBe('Enter a name.');
    });
  });

  describe('editing a person', () => {
    beforeEach(() => render('edit'));

    it('starts with the current name filled in, with edit wording', () => {
      expect(input.value).toBe('Jane Doe');
      expect(text('h2')).toBe('Edit person');
      expect(text('button[type="submit"]')).toBe('Save');
    });

    it('emits the trimmed new name', async () => {
      await submit('  Jane Smith ');

      expect(saved).toEqual(['Jane Smith']);
    });

    it('asks for a different name when it is unchanged', async () => {
      await submit('Jane Doe ');

      expect(saved).toEqual([]);
      expect(text('.field-error')).toBe('Enter a different name.');
    });
  });

  it('shows the error from a failed save', async () => {
    await render('add');
    fixture.componentRef.setInput('error', 'Person with this name already exists');
    await fixture.whenStable();

    expect(text('[role="alert"]')).toBe('Person with this name already exists');
  });
});
