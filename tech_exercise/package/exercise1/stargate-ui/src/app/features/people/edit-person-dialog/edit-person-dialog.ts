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

/** Modal form for renaming a person. Opens when created; the parent removes it to close it. */
@Component({
  selector: 'app-edit-person-dialog',
  imports: [ReactiveFormsModule],
  templateUrl: './edit-person-dialog.html',
})
export class EditPersonDialog implements OnInit {
  readonly currentName = input.required<string>();
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
    // currentName is only readable once inputs are set, so this validator is added here.
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

  protected onCancelKey(event: Event): void {
    if (this.saving()) event.preventDefault();
  }

  private validateChanged(control: AbstractControl<string>): ValidationErrors | null {
    return control.value.trim() === this.currentName() ? { unchanged: true } : null;
  }
}
