import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { Customer } from './customer.service';

export type OrderStatus = 'Pending' | 'Confirmed' | 'Fulfilled' | 'Cancelled';

export interface LineItem {
  sku: string;
  name: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface LineItemInput {
  sku: string;
  name: string;
  quantity: number;
  unitPrice: number;
}

export interface Order {
  id: string;
  clientReference: string;
  customer: Customer;
  items: LineItem[];
  status: OrderStatus;
  currency: string;
  notes: string | null;
  createdAtUtc: string;
  subtotal: number;
  total: number;
}

export interface CreateOrderPayload {
  clientReference: string;
  customer: Customer;
  items: LineItemInput[];
  currency?: string;
  notes?: string | null;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/orders`;

  getOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(this.baseUrl);
  }

  getOrder(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.baseUrl}/${id}`);
  }

  createOrder(order: CreateOrderPayload): Observable<Order> {
    return this.http.post<Order>(this.baseUrl, order);
  }

  updateOrder(id: string, notes: string | null, items: LineItemInput[]): Observable<Order> {
    return this.http.put<Order>(`${this.baseUrl}/${id}`, { notes, items });
  }

  updateStatus(id: string, status: OrderStatus): Observable<Order> {
    return this.http.patch<Order>(`${this.baseUrl}/${id}/status`, { status });
  }
}
