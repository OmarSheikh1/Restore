import type { Product, ProductPage } from '../../models/product'

export interface ProductService {
  list(page: number, searchTerm: string, signal: AbortSignal): Promise<ProductPage>
  get(id: string, signal: AbortSignal): Promise<Product>
}
