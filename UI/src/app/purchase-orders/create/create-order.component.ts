import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { PurchaseOrderService } from '../../services/purchase-orders/purchase-order.service';
import { purchaseOrderErrorMessage } from '../utilities/purchase-order-error';

const nonBlank: ValidatorFn = control => control.value?.trim() ? null : { required: true };

@Component({
  selector: 'app-create-order',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './create-order.component.html',
  styleUrl: './create-order.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateOrderComponent {
  private readonly purchaseOrders = inject(PurchaseOrderService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly saving = signal(false);
  protected readonly saveError = signal<string | null>(null);
  protected readonly form = this.fb.group({
    clientProvidedExternalReference: ['', [nonBlank, Validators.maxLength(128)]],
    currency: ['ZAR', [Validators.required, Validators.pattern(/^[A-Za-z]{3}$/)]],
    notes: [''],
    lineItems: this.fb.array([this.createLineItem()])
  });

  protected addLineItem(): void {
    this.form.controls.lineItems.push(this.createLineItem());
  }

  protected removeLineItem(index: number): void {
    if (this.form.controls.lineItems.length > 1) {
      this.form.controls.lineItems.removeAt(index);
    }
  }

  protected saveOrder(): void {
    if (this.saving()) {
      return;
    }

    this.saveError.set(null);
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }

    this.saving.set(true);
    this.purchaseOrders.create(this.form.getRawValue())
      .pipe(takeUntilDestroyed(this.destroyRef), finalize(() => this.saving.set(false)))
      .subscribe({
        next: () => {
          void this.router.navigate(['..'], { relativeTo: this.route, replaceUrl: true });
        },
        error: error => this.saveError.set(
          purchaseOrderErrorMessage(error, 'The order could not be saved. Please try again.'))
      });
  }

  private createLineItem() {
    return this.fb.group({
      sku: ['', [nonBlank, Validators.maxLength(64)]],
      name: ['', [nonBlank, Validators.maxLength(200)]],
      quantity: [1, [Validators.required, Validators.pattern(/^[1-9][0-9]*$/), Validators.max(2147483647)]],
      unitPrice: [0, [Validators.required, Validators.min(Number.MIN_VALUE)]]
    });
  }
}
