import { useEffect } from 'react'
import type { FormEvent } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { ProductImage } from '../../products/components/ProductImage'
import { formatPrice } from '../../products/components/price'
import { useGetProductsQuery, usePrefetch } from '../api'

export default function RtkProductsPage() {
  const [params, setParams] = useSearchParams()
  const search = params.get('search')?.trim() ?? ''
  const parsedPage = Number(params.get('page') ?? '1')
  const page = Number.isSafeInteger(parsedPage) && parsedPage > 0 && parsedPage <= 214748364 ? parsedPage : 1

  // setupListeners in the store enables these refetch triggers.
  const { data, error, isLoading, isFetching, fulfilledTimeStamp, refetch } = useGetProductsQuery(
    { page, searchTerm: search },
    { refetchOnFocus: true, refetchOnReconnect: true },
  )
  const prefetchProducts = usePrefetch('getProducts')

  // Warm the cache for the next page so clicking "Next" is instant.
  useEffect(() => {
    if (data?.hasNextPage) prefetchProducts({ page: page + 1, searchTerm: search })
  }, [data?.hasNextPage, page, search, prefetchProducts])

  function searchProducts(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const value = new FormData(event.currentTarget).get('search')?.toString().trim() ?? ''
    setParams(value ? { search: value } : {})
  }

  function pageLink(nextPage: number) {
    const next = new URLSearchParams()
    if (search) next.set('search', search)
    next.set('page', String(nextPage))
    return `?${next}`
  }

  return (
    <section>
      <h1>Products (RTK Query)</h1>
      <aside className="rtk-info">
        <h2>Why RTK Query?</h2>
        <ul>
          <li><strong>Caching:</strong> revisit a page or search and it appears instantly; the "last fetched" time won't change.</li>
          <li><strong>Request deduplication:</strong> identical requests share a single network call.</li>
          <li><strong>Prefetching:</strong> the next page is fetched in the background, so "Next" is instant.</li>
          <li><strong>Auto refetch:</strong> data refreshes when you return to this tab or reconnect.</li>
          <li><strong>Built-in states:</strong> loading, fetching and error flags with no manual useEffect/useState.</li>
        </ul>
        <p>Open the Network tab and the Redux DevTools (<code>productsApi</code>) to watch it work.</p>
      </aside>
      <form className="search-form" onSubmit={searchProducts} role="search">
        <label htmlFor="rtk-product-search">Search products</label>
        <div>
          <input key={search} id="rtk-product-search" name="search" type="search" defaultValue={search} placeholder="Search by name or description" />
          <button type="submit">Search</button>
          <button type="button" onClick={() => refetch()} disabled={isFetching}>Refresh</button>
        </div>
      </form>
      {isLoading && <p role="status">Loading products...</p>}
      {error && <p role="alert">Unable to load products. Please try again.</p>}
      {data && (
        <>
          <p className="results-count">
            {data.totalCount} products found
            {fulfilledTimeStamp && <> · last fetched at {new Date(fulfilledTimeStamp).toLocaleTimeString()}</>}
            {isFetching && <> · refreshing in background...</>}
          </p>
          {data.items.length === 0
            ? <p>No products found{search ? ' for this search' : ''}.{page > 1 && <> <Link to={pageLink(1)}>Go to the first page</Link>.</>}</p>
            : <div className={isFetching ? 'product-grid is-refreshing' : 'product-grid'}>
                {data.items.map(product => (
                  <article className="product-card" key={product.id}>
                    <Link to={`/products/${product.id}`}>
                      <ProductImage src={product.pictureUrl} name={product.name} />
                      <div className="product-card-content">
                        <p>{product.brand} / {product.type}</p>
                        <h2>{product.name}</h2>
                        <strong>{formatPrice(product.price)}</strong>
                        <p>{product.quantityInStock > 0 ? 'In stock' : 'Out of stock'}</p>
                      </div>
                    </Link>
                  </article>
                ))}
              </div>}
          {data.totalPages > 1 && (
            <nav className="pagination" aria-label="Product pages">
              {data.hasPreviousPage && <Link to={pageLink(page - 1)}>Previous</Link>}
              <span>Page {data.page} of {data.totalPages}</span>
              {data.hasNextPage && <Link to={pageLink(page + 1)}>Next</Link>}
            </nav>
          )}
        </>
      )}
    </section>
  )
}
