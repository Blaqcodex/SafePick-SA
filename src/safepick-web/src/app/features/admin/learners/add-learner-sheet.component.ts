import {
  Component,
  EventEmitter,
  Input,
  Output,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

// ============================================================
// AddLearnerSheetComponent
//
// A bottom sheet for registering a new learner.
//
// Inputs:
//   submitting — true while the parent is submitting
//
// Outputs:
//   submit — emits { fullName, grade, classLetter }
//   dismiss — emits when the user cancels
// ============================================================

export interface AddLearnerPayload {
  fullName: string;
  grade: number;
  classLetter: string;
}

@Component({
  selector: 'app-add-learner-sheet',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './add-learner-sheet.component.html',
  styleUrl: './add-learner-sheet.component.scss',
})
export class AddLearnerSheetComponent {
  private readonly fb = inject(FormBuilder);

  @Input() submitting = false;
  @Output() submit = new EventEmitter<AddLearnerPayload>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    grade: [1, [Validators.required, Validators.min(1), Validators.max(12)]],
    classLetter: ['', [
      Validators.required,
      Validators.pattern(/^[A-Za-z]$/),
    ]],
  });

  get fullName()    { return this.form.controls.fullName; }
  get grade()       { return this.form.controls.grade; }
  get classLetter() { return this.form.controls.classLetter; }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    this.submit.emit({
      fullName: this.fullName.value.trim(),
      grade: Number(this.grade.value),
      classLetter: this.classLetter.value.trim().toUpperCase(),
    });
  }

  onDismiss(): void {
    if (this.submitting) return;
    this.dismiss.emit();
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) this.onDismiss();
  }

  onClassLetterInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    input.value = input.value.toUpperCase();
    this.classLetter.setValue(input.value);
  }
}