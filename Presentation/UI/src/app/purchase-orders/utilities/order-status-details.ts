import { OrderStatus } from '../../services/purchase-orders/models/order-status';

export const orderStatusDetails: Record<OrderStatus, { label: string; tone: string }> = {
  [OrderStatus.Pending]: { label: 'Pending', tone: 'status--pending' },
  [OrderStatus.Confirmed]: { label: 'Confirmed', tone: 'status--confirmed' },
  [OrderStatus.Fulfilled]: { label: 'Fulfilled', tone: 'status--fulfilled' },
  [OrderStatus.Cancelled]: { label: 'Cancelled', tone: 'status--cancelled' }
};
