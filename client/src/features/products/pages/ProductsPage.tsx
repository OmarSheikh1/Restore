import type { FormEvent } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { ProductImage } from '../components/ProductImage'
import { formatPrice } from '../components/price'
import { useProducts } from '../hooks'

export default function ProductsPage() {
  const [params, setParams] = useSearchParams()
  const search = params.get('search')?.trim() ?? ''
  const pageParam = params.get('page') ?? '1'
  const parsedPage = Number(pageParam)
  const page = Number.isSafeInteger(parsedPage) && parsedPage > 0 && parsedPage <= 214748364 ? parsedPage : 1
  const state = useProducts(page, search)

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
      <h1>Products</h1>
      <form className="search-form" onSubmit={searchProducts} role="search">
        <label htmlFor="product-search">Search products</label>
        <div>
          <input key={search} id="product-search" name="search" type="search" defaultValue={search} placeholder="Search by name or description" />
          <button type="submit">Search</button>
        </div>
      </form>
      {state.status === 'loading' && <p role="status">Loading products...</p>}
      {state.status === 'error' && <p role="alert">{state.message} Please try again.</p>}
      {state.status === 'loaded' && (
        <>
          <p className="results-count">{state.data.totalCount} products found</p>
          {state.data.items.length === 0
            ? <p>No products found{search ? ' for this search' : ''}.{page > 1 && <> <Link to={pageLink(1)}>Go to the first page</Link>.</>}</p>
            : <div className="product-grid">
                {state.data.items.map(product => (
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
          {state.data.totalPages > 1 && (
            <nav className="pagination" aria-label="Product pages">
              {state.data.hasPreviousPage && <Link to={pageLink(page - 1)}>Previous</Link>}
              <span>Page {state.data.page} of {state.data.totalPages}</span>
              {state.data.hasNextPage && <Link to={pageLink(page + 1)}>Next</Link>}
            </nav>
          )}
        </>
      )}
    </section>
  )
}
