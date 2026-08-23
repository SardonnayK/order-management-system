import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Order, OrderService } from './order.service';

@Component({
  imports: [DatePipe],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {
  private readonly orderService = inject(OrderService);

  protected readonly title = signal('Order Management');
  protected readonly orders = signal<Order[]>([]);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.loadOrders();
  }

  protected loadOrders(): void {
    this.orderService.getOrders().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.error.set(null);
      },
      error: () => this.error.set('Could not reach the API. Is the backend running?'),
    });
  }
}
