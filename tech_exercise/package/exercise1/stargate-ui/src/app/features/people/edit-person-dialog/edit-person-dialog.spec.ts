import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EditPersonDialog } from './edit-person-dialog';

describe('EditPersonDialog', () => {
  let fixture: ComponentFixture<EditPersonDialog>;
  let host: HTMLElement;
  let input: HTMLInputElement;
  let saved: string[];

  beforeEach(async () => {
    fixture = TestBed.createComponent(EditPersonDialog);
    fixture.componentRef.setInput('currentName', 'Jane Doe');
    saved = [];
    fixture.componentInstance.save.subscribe((name) => saved.push(name));
    await fixture.whenStable();
    host = fixture.nativeElement as HTMLElement;
    input = host.querySelector<HTMLInputElement>('#person-name')!;
  });

  async function rename(value: string) {
    input.value = value;
    input.dispatchEvent(new Event('input'));
    host.querySelector('form')!.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  }

  const error = () => host.querySelector('.field-error')?.textContent?.trim() ?? null;

  it('starts with the current name filled in', () => {
    expect(input.value).toBe('Jane Doe');
  });

  it('emits the trimmed new name', async () => {
    await rename('  Jane Smith ');

    expect(saved).toEqual(['Jane Smith']);
  });

  it('asks for a different name when it is unchanged', async () => {
    await rename('Jane Doe ');

    expect(saved).toEqual([]);
    expect(error()).toBe('Enter a different name.');
  });

  it('rejects a blank name', async () => {
    await rename('   ');

    expect(saved).toEqual([]);
    expect(error()).toBe('Enter a name.');
  });
});
