import type { ReactNode } from 'react'
import { ProductServiceContext } from './context'
import { ApiProductService } from './service'
import type { ProductService } from './types'

const productService: ProductService = new ApiProductService()

export function ProductServiceProvider({ children, service = productService }: { children: ReactNode; service?: ProductService }) {
  return <ProductServiceContext.Provider value={service}>{children}</ProductServiceContext.Provider>
}
