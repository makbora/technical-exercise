import {
  Component,
  ElementRef,
  OnInit,
  afterNextRender,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { notBlank, showModal } from '../dialog-form';

export type PersonNameDialogMode = 'add' | 'edit';

/**
 * Modal form for a person's name: adding a new person, or renaming the current one.
 * Opens when created; the parent removes it to close it.
 */
@Component({
  selector: 'app-person-name-dialog',
  imports: [ReactiveFormsModule],
  templateUrl: './person-name-dialog.html',
})
export class PersonNameDialog implements OnInit {
  readonly mode = input.required<PersonNameDialogMode>();
  /** The name being edited. Only used in edit mode. */
  readonly currentName = input('');
  readonly saving = input(false);
  readonly error = input<string | null>(null);

  readonly save = output<string>();
  readonly closed = output<void>();

  protected readonly submitted = signal(false);
  protected readonly form = inject(FormBuilder).nonNullable.group({ name: ['', notBlank] });

  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  constructor() {
    afterNextRender(() => showModal(this.dialog().nativeElement));
  }

  ngOnInit(): void {
    if (this.mode() !== 'edit') return;

    // Inputs are only readable from here on, so the edit-only validator is added now.
    this.form.controls.name.addValidators((control: AbstractControl<string>) =>
      this.validateChanged(control),
    );
    this.form.setValue({ name: this.currentName() });
  }

  protected invalid(): boolean {
    const control = this.form.controls.name;
    return control.invalid && (control.touched || this.submitted());
  }

  protected submit(): void {
    this.submitted.set(true);
    if (this.form.invalid || this.saving()) return;
    this.save.emit(this.form.controls.name.value.trim());
  }

  protected cancel(): void {
    this.dialog().nativeElement.close();
  }

  /** Esc closes a modal dialog; don't let it close while a save is in flight. */
  protected onCancelKey(event: Event): void {
    if (this.saving()) event.preventDefault();
  }

  private validateChanged(control: AbstractControl<string>): ValidationErrors | null {
    return control.value.trim() === this.currentName() ? { unchanged: true } : null;
  }
}
