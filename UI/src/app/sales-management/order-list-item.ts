export enum OrderStatus {
  Pending = 0,
  Confirmed = 1,
  Fulfilled = 2,
  Cancelled = 3
}

export interface OrderListItem {
  readonly id: string;
  readonly clientProvidedExternalReference: string;
  readonly customerName: string;
  readonly orderDateTimestamp: string;
  readonly status: OrderStatus;
  readonly lineItemCount: number;
}
