export interface CreateOrderLineItem {
  readonly sku: string;
  readonly name: string;
  readonly quantity: number;
  readonly unitPrice: number;
}

export interface CreateOrderRequest {
  readonly clientProvidedExternalReference: string;
  readonly customerId: string;
  readonly currency: string;
  readonly notes: string;
  readonly lineItems: readonly CreateOrderLineItem[];
}
