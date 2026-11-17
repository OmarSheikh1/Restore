import type { Product, ProductPage } from '../../models/product'
import type { ProductService } from './types'

export class ProductApiError extends Error {
  readonly status: number

  constructor(status: number) {
    super(status === 404 ? 'Product not found.' : `Products request failed (HTTP ${status}).`)
    this.status = status
  }
}

export class ApiProductService implements ProductService {
  private async getJson<T>(path: string, signal: AbortSignal): Promise<T> {
    const response = await fetch(`/api/Products${path}`, { signal })
    if (!response.ok) throw new ProductApiError(response.status)
    return response.json() as Promise<T>
  }

  list(page: number, searchTerm: string, signal: AbortSignal) {
    const params = new URLSearchParams({ page: String(page), pageSize: '10' })
    if (searchTerm) params.set('searchTerm', searchTerm)
    return this.getJson<ProductPage>(`?${params}`, signal)
  }

  get(id: string, signal: AbortSignal) {
    return this.getJson<Product>(`/${encodeURIComponent(id)}`, signal)
  }
}
