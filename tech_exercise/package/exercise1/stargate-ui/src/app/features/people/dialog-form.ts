import { AbstractControl, ValidationErrors } from '@angular/forms';

/** Like Validators.required, but also rejects values that are only whitespace. */
export function notBlank(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim() ? null : { required: true };
}

/** Opens a <dialog> as a modal if the environment supports it (jsdom in tests may not). */
export function showModal(dialog: HTMLDialogElement): void {
  if (typeof dialog.showModal === 'function' && !dialog.open) dialog.showModal();
}
