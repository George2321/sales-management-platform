import { HttpErrorResponse } from '@angular/common/http';

export const orderFieldHints = {
  clientProvidedExternalReference: 'Enter an order reference of up to 128 characters.',
  currency: 'Enter a three-letter currency code, such as ZAR.',
  customerId: 'Choose a customer from the list.',
  notes: 'Enter the notes as plain text.',
  sku: 'Enter a SKU of up to 64 characters.',
  name: 'Enter an item name of up to 200 characters.',
  quantity: 'Enter a whole quantity between 1 and 2,147,483,647.',
  unitPrice: 'Enter zero or a positive unit price within the supported decimal range.',
  status: 'Choose a valid order status.',
  expectedStatus: 'Choose a valid expected order status.'
};

export function purchaseOrderFieldErrors(error: unknown): { path: string; message: string }[] {
  const errors = error instanceof HttpErrorResponse && error.status === 400 && error.error?.errors;
  if (!errors || typeof errors !== 'object' || Array.isArray(errors)) {
    return [];
  }

  const fields = [];
  for (const key of Object.keys(errors)) {
    // MVC binding errors use JSON paths such as $.lineItems[0].unitPrice.
    const match = /^(?:\$\.)?(?:request\.)?(?:lineItems\[(\d+)\]\.)?(\w+)$/i.exec(key);
    if (!match) continue;

    const [, index, property] = match;
    const entry = Object.entries(orderFieldHints).find(([name]) => name.toLowerCase() === property.toLowerCase());
    if (entry) {
      const [field, message] = entry;
      fields.push({ path: index === undefined ? field : `lineItems.${index}.${field}`, message });
    }
  }
  return fields;
}

export function purchaseOrderErrorMessage(error: unknown, fallback: string): string {
  if (error instanceof HttpErrorResponse && typeof error.error?.detail === 'string') {
    return error.error.detail;
  }
  return purchaseOrderFieldErrors(error)[0]?.message ?? fallback;
}
