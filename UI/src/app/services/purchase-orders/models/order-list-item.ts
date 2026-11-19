import { OrderStatus } from './order-status';

export interface OrderListItem {
  readonly id: string;
  readonly clientProvidedExternalReference: string;
  readonly orderDateTimestamp: string;
  readonly status: OrderStatus;
  readonly lineItemCount: number;
  readonly currency: string;
  readonly subtotal: number;
  readonly total: number;
}
