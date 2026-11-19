import { Routes } from '@angular/router';
import { SalesManagementScreenComponent } from './sales-management/sales-management-screen.component';

export const routes: Routes = [
  { path: '', redirectTo: 'sales-management', pathMatch: 'full' },
  {
    path: 'sales-management',
    component: SalesManagementScreenComponent,
    title: 'Sales Management'
  }
];
