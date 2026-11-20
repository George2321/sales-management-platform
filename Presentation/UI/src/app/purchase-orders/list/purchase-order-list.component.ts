import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { OrderListItem } from '../../services/purchase-orders/models/order-list-item';
import { OrderStatus } from '../../services/purchase-orders/models/order-status';
import { PurchaseOrderService } from '../../services/purchase-orders/purchase-order.service';
import { orderStatusDetails } from '../utilities/order-status-details';
import { OrderDetailsModalComponent } from '../view/order-details-modal.component';

type OrdersState =
  | { kind: 'loading' }
  | { kind: 'ready'; orders: readonly OrderListItem[] }
  | { kind: 'error' };

@Component({
  selector: 'app-purchase-order-list',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, RouterLink, OrderDetailsModalComponent],
  templateUrl: './purchase-order-list.component.html',
  styleUrl: './purchase-order-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PurchaseOrderListComponent implements OnInit {
  private readonly purchaseOrders = inject(PurchaseOrderService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly state = signal<OrdersState>({ kind: 'loading' });
  protected readonly selectedOrderId = signal<string | null>(null);
  protected readonly statusDetails = orderStatusDetails;

  ngOnInit(): void {
    this.loadOrders();
  }

  protected loadOrders(): void {
    this.state.set({ kind: 'loading' });
    this.purchaseOrders.getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: orders => this.state.set({ kind: 'ready', orders }),
        error: () => this.state.set({ kind: 'error' })
      });
  }

  protected updateListStatus(orderId: string, status: OrderStatus): void {
    this.state.update(view => view.kind === 'ready'
      ? { kind: 'ready', orders: view.orders.map(order => order.id === orderId ? { ...order, status } : order) }
      : view);
  }
}
