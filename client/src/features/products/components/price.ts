const formatter = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })

export function formatPrice(cents: number) {
  return formatter.format(cents / 100)
}
