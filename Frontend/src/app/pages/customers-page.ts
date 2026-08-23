import { Component, inject, signal, viewChild } from '@angular/core';
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

@Component({
  imports: [
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
  selector: 'app-customers-page',
  templateUrl: './customers-page.html',
})
export class CustomersPage {
  private readonly customerService = inject(CustomerService);

  private readonly addDialog = viewChild<HlmDialog>('addDialog');

  protected readonly customers = signal<Customer[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly formError = signal<string | null>(null);
  protected readonly saving = signal(false);

  protected draft: Customer = { id: '', name: '', email: '' };

  constructor() {
    this.load();
  }

  protected load(): void {
    this.customerService.getCustomers().subscribe({
      next: (customers) => {
        this.customers.set(customers);
        this.error.set(null);
      },
      error: () => this.error.set('Could not reach the API. Is the backend running?'),
    });
  }

  protected resetForm(): void {
    this.draft = { id: '', name: '', email: '' };
    this.formError.set(null);
  }

  protected saveCustomer(): void {
    const { id, name, email } = this.draft;
    if (!id.trim() || !name.trim() || !email.trim()) {
      this.formError.set('Id, name and email are all required.');
      return;
    }

    this.saving.set(true);
    this.customerService.createCustomer({ id: id.trim(), name: name.trim(), email: email.trim() }).subscribe({
      next: () => {
        this.saving.set(false);
        this.addDialog()?.close(undefined);
        this.load();
      },
      error: (err) => {
        this.saving.set(false);
        this.formError.set(err?.error?.message ?? 'Could not save the customer.');
      },
    });
  }
}
