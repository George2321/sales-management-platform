import { OrderListItem } from './order-list-item';

export interface OrderLineItem {
  readonly id: string;
  readonly sku: string;
  readonly name: string;
  readonly quantity: number;
  readonly unitPrice: number;
}

export interface OrderDetails extends Omit<OrderListItem, 'lineItemCount' | 'subtotal' | 'total'> {
  readonly completedDateTimeStamp: string | null;
  readonly notes: string;
  readonly lineItems: readonly OrderLineItem[];
  readonly canChangeStatus: boolean;
}
