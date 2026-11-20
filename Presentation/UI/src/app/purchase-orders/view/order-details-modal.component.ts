import { CurrencyPipe, DatePipe, getNumberOfCurrencyDigits } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  AfterViewInit, ChangeDetectionStrategy, Component, DestroyRef, ElementRef, inject, input,
  OnInit, output, signal, ViewChild
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { OrderDetails } from '../../services/purchase-orders/models/order-details';
import { OrderStatus } from '../../services/purchase-orders/models/order-status';
import { PurchaseOrderService } from '../../services/purchase-orders/purchase-order.service';
import { orderStatusDetails } from '../utilities/order-status-details';
import { purchaseOrderErrorMessage } from '../utilities/purchase-order-error';

type DetailsState =
  | { kind: 'loading' }
  | { kind: 'ready'; order: OrderDetails }
  | { kind: 'error'; message: string };

type SaveFeedback =
  | { kind: 'saved' }
  | { kind: 'error' | 'conflict'; message: string };

@Component({
  selector: 'app-order-details-modal',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, ReactiveFormsModule],
  templateUrl: './order-details-modal.component.html',
  styleUrl: './order-details-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrderDetailsModalComponent implements OnInit, AfterViewInit {
  readonly orderId = input.required<string>();
  readonly closed = output<void>();
  readonly statusUpdated = output<OrderStatus>();

  @ViewChild('dialog', { static: true }) private dialog!: ElementRef<HTMLDialogElement>;

  private readonly purchaseOrders = inject(PurchaseOrderService);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly state = signal<DetailsState>({ kind: 'loading' });
  protected readonly saving = signal(false);
  protected readonly feedback = signal<SaveFeedback | null>(null);
  protected readonly statusControl = new FormControl<OrderStatus>(OrderStatus.Pending, { nonNullable: true });
  protected readonly statusDetails = orderStatusDetails;
  protected readonly currencyDigits = getNumberOfCurrencyDigits;
  protected readonly statuses = [OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Fulfilled, OrderStatus.Cancelled];

  ngOnInit(): void {
    this.loadDetails();
  }

  ngAfterViewInit(): void {
    this.dialog.nativeElement.showModal();
  }

  protected loadDetails(): void {
    this.state.set({ kind: 'loading' });
    this.feedback.set(null);
    this.purchaseOrders.getById(this.orderId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: order => this.showOrder(order),
        error: error => this.state.set({
          kind: 'error',
          message: purchaseOrderErrorMessage(error, 'Order details could not be loaded. Please try again.')
        })
      });
  }

  protected saveStatus(): void {
    const view = this.state();
    const status = this.statusControl.value;
    if (view.kind !== 'ready' || !view.order.canChangeStatus || this.saving() ||
        this.feedback()?.kind === 'conflict' || status === view.order.status) {
      return;
    }

    this.saving.set(true);
    this.feedback.set(null);
    this.purchaseOrders.changeStatus(view.order.id, view.order.status, status)
      .pipe(takeUntilDestroyed(this.destroyRef), finalize(() => this.saving.set(false)))
      .subscribe({
        next: order => {
          this.showOrder(order);
          this.feedback.set({ kind: 'saved' });
        },
        error: error => this.feedback.set({
          kind: error instanceof HttpErrorResponse && error.status === 409 ? 'conflict' : 'error',
          message: purchaseOrderErrorMessage(error, 'The order status could not be saved. Please try again.')
        })
      });
  }

  protected preventCloseWhileSaving(event: Event): void {
    if (this.saving()) {
      event.preventDefault();
    }
  }

  protected statusSelectionChanged(): void {
    if (this.feedback()?.kind === 'saved') {
      this.feedback.set(null);
    }
  }

  private showOrder(order: OrderDetails): void {
    this.statusControl.setValue(order.status);
    this.state.set({ kind: 'ready', order });
    this.statusUpdated.emit(order.status);
  }
}
