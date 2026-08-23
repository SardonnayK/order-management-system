import { Component, computed, inject, signal, viewChild } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BrnDialogContent } from '@spartan-ng/brain/dialog';
import { HlmButton } from '@spartan-ng/helm/button';
import {
  HlmCard,
  HlmCardContent,
  HlmCardDescription,
  HlmCardHeader,
  HlmCardTitle,
} from '@spartan-ng/helm/card';
import {
  HlmDialog,
  HlmDialogClose,
  HlmDialogContent,
  HlmDialogDescription,
  HlmDialogFooter,
  HlmDialogHeader,
  HlmDialogTitle,
  HlmDialogTrigger,
} from '@spartan-ng/helm/dialog';
import { HlmInput } from '@spartan-ng/helm/input';
import { HlmLabel } from '@spartan-ng/helm/label';
import {
  HlmTable,
  HlmTableContainer,
  HlmTBody,
  HlmTd,
  HlmTh,
  HlmTHead,
  HlmTr,
} from '@spartan-ng/helm/table';
import { Customer, CustomerService } from '../customer.service';
import { LineItemInput, Order, OrderService, OrderStatus } from '../order.service';

@Component({
  imports: [
    CurrencyPipe,
    DatePipe,
    FormsModule,
    BrnDialogContent,
    HlmButton,
    HlmCard,
    HlmCardContent,
    HlmCardDescription,
    HlmCardHeader,
    HlmCardTitle,
    HlmDialog,
    HlmDialogClose,
    HlmDialogContent,
    HlmDialogDescription,
    HlmDialogFooter,
    HlmDialogHeader,
    HlmDialogTitle,
    HlmDialogTrigger,
    HlmInput,
    HlmLabel,
    HlmTable,
    HlmTableContainer,
    HlmTBody,
    HlmTd,
    HlmTh,
    HlmTHead,
    HlmTr,
  ],
  selector: 'app-orders-page',
  templateUrl: './orders-page.html',
})
export class OrdersPage {
  private readonly orderService = inject(OrderService);
  private readonly customerService = inject(CustomerService);

  private readonly createDialog = viewChild<HlmDialog>('createDialog');
  private readonly viewDialog = viewChild<HlmDialog>('viewDialog');

  protected readonly orders = signal<Order[]>([]);
  protected readonly customers = signal<Customer[]>([]);
  protected readonly error = signal<string | null>(null);

  // "New order" dialog state
  protected draft = { clientReference: '', currency: 'ZAR', notes: '' };
  protected readonly draftItems = signal<LineItemInput[]>([this.emptyItem()]);
  protected readonly customerSearch = signal('');
  protected readonly selectedCustomer = signal<Customer | null>(null);
  protected readonly formError = signal<string | null>(null);
  protected readonly saving = signal(false);

  // Row whose status change is in flight
  protected readonly updatingId = signal<string | null>(null);

  // Order shown in the view dialog
  protected readonly viewOrder = signal<Order | null>(null);

  protected readonly filteredCustomers = computed(() => {
    const query = this.customerSearch().trim().toLowerCase();
    const all = this.customers();
    if (!query) return all;
    return all.filter(
      (c) =>
        c.name.toLowerCase().includes(query) ||
        c.email.toLowerCase().includes(query) ||
        c.id.toLowerCase().includes(query),
    );
  });

  constructor() {
    this.load();
    this.loadCustomers();
  }

  protected load(): void {
    this.orderService.getOrders().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.error.set(null);
      },
      error: () => this.error.set('Could not reach the API. Is the backend running?'),
    });
  }

  protected loadCustomers(): void {
    this.customerService.getCustomers().subscribe({
      next: (customers) => this.customers.set(customers),
      error: () => undefined,
    });
  }

  // --- status transitions -------------------------------------------------

  protected transitionsFor(order: Order): OrderStatus[] {
    switch (order.status) {
      case 'Pending':
        return ['Confirmed', 'Cancelled'];
      case 'Confirmed':
        return ['Fulfilled', 'Cancelled'];
      default:
        return [];
    }
  }

  protected transitionLabel(status: OrderStatus): string {
    switch (status) {
      case 'Confirmed':
        return 'Confirm';
      case 'Fulfilled':
        return 'Fulfil';
      case 'Cancelled':
        return 'Cancel';
      default:
        return status;
    }
  }

  protected changeStatus(order: Order, status: OrderStatus): void {
    this.updatingId.set(order.id);
    this.orderService.updateStatus(order.id, status).subscribe({
      next: () => {
        this.updatingId.set(null);
        this.load();
      },
      error: (err) => {
        this.updatingId.set(null);
        this.error.set(err?.error?.message ?? 'Could not update the order status.');
      },
    });
  }

  // --- view dialog ----------------------------------------------------------

  protected openView(order: Order): void {
    this.viewOrder.set(order);
    this.viewDialog()?.open();
  }

  // --- new order dialog ----------------------------------------------------

  protected resetCreateForm(): void {
    this.draft = { clientReference: '', currency: 'ZAR', notes: '' };
    this.draftItems.set([this.emptyItem()]);
    this.customerSearch.set('');
    this.selectedCustomer.set(null);
    this.formError.set(null);
    this.loadCustomers();
  }

  protected selectCustomer(customer: Customer): void {
    this.selectedCustomer.set(customer);
  }

  protected addItem(): void {
    this.draftItems.update((items) => [...items, this.emptyItem()]);
  }

  protected removeItem(index: number): void {
    this.draftItems.update((items) => items.filter((_, i) => i !== index));
  }

  protected createOrder(): void {
    const customer = this.selectedCustomer();
    if (!customer) {
      this.formError.set('Select a customer for the order.');
      return;
    }
    if (!this.draft.clientReference.trim()) {
      this.formError.set('A client reference is required.');
      return;
    }

    const items = this.draftItems()
      .filter((i) => i.sku.trim() || i.name.trim())
      .map((i) => ({
        sku: i.sku.trim(),
        name: i.name.trim(),
        quantity: Number(i.quantity),
        unitPrice: Number(i.unitPrice),
      }));

    if (items.length === 0) {
      this.formError.set('Add at least one line item.');
      return;
    }
    if (
      items.some(
        (i) =>
          !i.sku ||
          !i.name ||
          !Number.isInteger(i.quantity) ||
          i.quantity < 1 ||
          !(i.unitPrice > 0),
      )
    ) {
      this.formError.set(
        'Every line item needs a SKU, a name, a whole quantity of at least 1 and a positive unit price.',
      );
      return;
    }

    this.saving.set(true);
    this.orderService
      .createOrder({
        clientReference: this.draft.clientReference.trim(),
        currency: this.draft.currency.trim() || 'ZAR',
        notes: this.draft.notes.trim() || null,
        customer,
        items,
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.createDialog()?.close(undefined);
          this.load();
        },
        error: (err) => {
          this.saving.set(false);
          this.formError.set(err?.error?.message ?? 'Could not create the order.');
        },
      });
  }

  private emptyItem(): LineItemInput {
    return { sku: '', name: '', quantity: 1, unitPrice: 0 };
  }
}
