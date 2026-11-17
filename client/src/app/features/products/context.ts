import { createContext, useContext } from 'react'
import type { ProductService } from './types'

export const ProductServiceContext = createContext<ProductService | null>(null)

export function useProductService() {
  const service = useContext(ProductServiceContext)
  if (!service) throw new Error('ProductServiceProvider is missing.')
  return service
}
