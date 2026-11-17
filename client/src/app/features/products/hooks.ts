import { useEffect, useState } from 'react'
import type { Product, ProductPage } from '../../models/product'
import { useProductService } from './context'

type LoadState<T> = { status: 'loading' } | { status: 'loaded'; data: T } | { status: 'error'; message: string }

export function useProducts(page: number, searchTerm: string) {
  const service = useProductService()
  const key = JSON.stringify([page, searchTerm])
  const [result, setResult] = useState<{ key: string; state: LoadState<ProductPage> }>()

  useEffect(() => {
    const controller = new AbortController()
    service.list(page, searchTerm, controller.signal).then(
      data => {
        if (!controller.signal.aborted) setResult({ key, state: { status: 'loaded', data } })
      },
      error => {
        if (!controller.signal.aborted) {
          console.error('Unable to load products.', error)
          setResult({ key, state: { status: 'error', message: error instanceof Error ? error.message : 'Unable to load products.' } })
        }
      },
    )
    return () => controller.abort()
  }, [service, page, searchTerm, key])

  return result?.key === key ? result.state : { status: 'loading' as const }
}

export function useProduct(id: string) {
  const service = useProductService()
  const [result, setResult] = useState<{ id: string; state: LoadState<Product> }>()

  useEffect(() => {
    const controller = new AbortController()
    service.get(id, controller.signal).then(
      data => {
        if (!controller.signal.aborted) setResult({ id, state: { status: 'loaded', data } })
      },
      error => {
        if (!controller.signal.aborted) {
          console.error('Unable to load product.', error)
          setResult({ id, state: { status: 'error', message: error instanceof Error ? error.message : 'Unable to load product.' } })
        }
      },
    )
    return () => controller.abort()
  }, [service, id])

  return result?.id === id ? result.state : { status: 'loading' as const }
}
