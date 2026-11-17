export interface Product {
  id: number
  name: string
  description: string
  price: number
  pictureUrl: string
  type: string
  brand: string
  quantityInStock: number
}

export interface ProductPage {
  items: Product[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}
