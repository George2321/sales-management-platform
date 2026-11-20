import { Routes } from '@angular/router';
import { CreateOrderComponent } from './purchase-orders/create/create-order.component';
import { PurchaseOrderListComponent } from './purchase-orders/list/purchase-order-list.component';

export const routes: Routes = [
  { path: '', redirectTo: 'sales-management', pathMatch: 'full' },
  {
    path: 'sales-management',
    children: [
      { path: '', pathMatch: 'full', component: PurchaseOrderListComponent, title: 'Sales Management' },
      { path: 'new', component: CreateOrderComponent, title: 'New purchase order' }
    ]
  }
];
