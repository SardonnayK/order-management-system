import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'orders' },
  {
    path: 'orders',
    loadComponent: () => import('./pages/orders-page').then((m) => m.OrdersPage),
  },
  {
    path: 'customers',
    loadComponent: () => import('./pages/customers-page').then((m) => m.CustomersPage),
  },
];
