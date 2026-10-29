import {
  Component,
  ElementRef,
  afterNextRender,
  computed,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { NewAstronautDuty, RETIRED_DUTY_TITLE } from '@abstractions/models/person';
import { notBlank, showModal } from '../dialog-form';

/** Modal form for adding a duty. Opens when created; the parent removes it to close it. */
@Component({
  selector: 'app-add-duty-dialog',
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './add-duty-dialog.html',
})
export class AddDutyDialog {
  readonly personName = input.required<string>();
  /** Start date of the person's latest duty. The API requires a new duty to start after it. */
  readonly latestStartDate = input<string | null>(null);
  readonly saving = input(false);
  readonly error = input<string | null>(null);

  readonly save = output<NewAstronautDuty>();
  readonly closed = output<void>();

  protected readonly retiredTitle = RETIRED_DUTY_TITLE;
  protected readonly submitted = signal(false);
  protected readonly minStartDate = computed(() => {
    const latest = this.latestStartDate();
    return latest ? nextDay(latest) : null;
  });

  protected readonly form = inject(FormBuilder).nonNullable.group({
    rank: ['', notBlank],
    title: ['', notBlank],
    startDate: ['', (control: AbstractControl<string>) => this.validateStartDate(control)],
  });

  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  constructor() {
    afterNextRender(() => showModal(this.dialog().nativeElement));
  }

  protected invalid(name: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[name];
    return control.invalid && (control.touched || this.submitted());
  }

  protected submit(): void {
    this.submitted.set(true);
    if (this.form.invalid || this.saving()) return;

    const { rank, title, startDate } = this.form.getRawValue();
    this.save.emit({ rank: rank.trim(), title: title.trim(), startDate });
  }

  protected cancel(): void {
    this.dialog().nativeElement.close();
  }

  /** Esc closes a modal dialog; don't let it close while a save is in flight. */
  protected onCancelKey(event: Event): void {
    if (this.saving()) event.preventDefault();
  }

  private validateStartDate(control: AbstractControl<string>): ValidationErrors | null {
    if (!control.value) return { required: true };
    const min = this.minStartDate();
    return min && control.value < min ? { notAfterLatest: true } : null;
  }
}

/** "2022-01-10T00:00:00" -> "2022-01-11" */
function nextDay(isoDate: string): string {
  const date = new Date(`${isoDate.slice(0, 10)}T00:00:00Z`);
  date.setUTCDate(date.getUTCDate() + 1);
  return date.toISOString().slice(0, 10);
}
