import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateOrderRequest } from './models/create-order-request';
import { OrderCustomerOption } from './models/order-customer-option';
import { OrderDetails } from './models/order-details';
import { OrderListItem } from './models/order-list-item';
import { OrderStatus } from './models/order-status';

@Injectable({ providedIn: 'root' })
export class PurchaseOrderService {
  private readonly http = inject(HttpClient);
  private readonly ordersUrl = `${environment.apiUrl}/PurchaseOrder`;

  getAll(): Observable<readonly OrderListItem[]> {
    return this.http.get<readonly OrderListItem[]>(this.ordersUrl);
  }

  getById(id: string): Observable<OrderDetails> {
    return this.http.get<OrderDetails>(`${this.ordersUrl}/${encodeURIComponent(id)}`);
  }

  getCustomers(): Observable<readonly OrderCustomerOption[]> {
    return this.http.get<readonly OrderCustomerOption[]>(`${this.ordersUrl}/customers`);
  }

  create(request: CreateOrderRequest): Observable<OrderDetails> {
    return this.http.post<OrderDetails>(this.ordersUrl, request);
  }

  changeStatus(id: string, expectedStatus: OrderStatus, status: OrderStatus): Observable<OrderDetails> {
    return this.http.patch<OrderDetails>(`${this.ordersUrl}/${encodeURIComponent(id)}/status`, {
      expectedStatus,
      status
    });
  }
}
