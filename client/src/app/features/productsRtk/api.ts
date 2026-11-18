import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import type { ProductPage } from '../../models/product'

export interface ProductsQueryArgs {
  page: number
  searchTerm: string
}

export const productsApi = createApi({
  reducerPath: 'productsApi',
  baseQuery: fetchBaseQuery({ baseUrl: '/api/' }),
  // Cached pages stay available for 5 minutes after the last component using them unmounts.
  keepUnusedDataFor: 300,
  endpoints: builder => ({
    getProducts: builder.query<ProductPage, ProductsQueryArgs>({
      query: ({ page, searchTerm }) => {
        const params = new URLSearchParams({ page: String(page), pageSize: '10' })
        if (searchTerm) params.set('searchTerm', searchTerm)
        return `Products?${params}`
      },
    }),
  }),
})

export const { useGetProductsQuery, usePrefetch } = productsApi
