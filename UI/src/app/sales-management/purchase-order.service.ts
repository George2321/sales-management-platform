import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { OrderListItem } from './order-list-item';

@Injectable({ providedIn: 'root' })
export class PurchaseOrderService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<readonly OrderListItem[]> {
    return this.http.get<readonly OrderListItem[]>(`${environment.apiUrl}/PurchaseOrder`);
  }
}
