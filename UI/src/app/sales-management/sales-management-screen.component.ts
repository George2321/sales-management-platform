import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OrderListItem, OrderStatus } from './order-list-item';
import { PurchaseOrderService } from './purchase-order.service';

type OrdersState =
  | { kind: 'loading' }
  | { kind: 'ready'; orders: readonly OrderListItem[] }
  | { kind: 'error' };

@Component({
  selector: 'app-sales-management-screen',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './sales-management-screen.component.html',
  styleUrl: './sales-management-screen.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SalesManagementScreenComponent implements OnInit {
  private readonly purchaseOrders = inject(PurchaseOrderService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly state = signal<OrdersState>({ kind: 'loading' });
  protected readonly statusDetails: Record<OrderStatus, { label: string; tone: string }> = {
    [OrderStatus.Pending]: { label: 'Pending', tone: 'status--pending' },
    [OrderStatus.Confirmed]: { label: 'Confirmed', tone: 'status--confirmed' },
    [OrderStatus.Fulfilled]: { label: 'Fulfilled', tone: 'status--fulfilled' },
    [OrderStatus.Cancelled]: { label: 'Cancelled', tone: 'status--cancelled' }
  };

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
}
