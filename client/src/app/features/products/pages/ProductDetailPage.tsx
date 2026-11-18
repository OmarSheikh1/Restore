import { useEffect } from 'react'
import { useDispatch } from 'react-redux'
import { Link, useParams } from 'react-router-dom'
import type { AppDispatch } from '../../../store/store'
import { addBrowsedProduct } from '../../../store/store'
import { ProductImage } from '../components/ProductImage'
import { formatPrice } from '../components/price'
import { useProduct } from '../hooks'

function ProductDetails({ id }: { id: string }) {
  const state = useProduct(id)
  const dispatch = useDispatch<AppDispatch>()
  const product = state.status === 'loaded' ? state.data : undefined

  useEffect(() => {
    if (product) dispatch(addBrowsedProduct(product))
  }, [dispatch, product])

  if (state.status === 'loading') return <p role="status">Loading product...</p>
  if (state.status === 'error') return <p role="alert">{state.message}</p>
  const loadedProduct = state.data
  return (
    <article className="product-detail">
      <ProductImage key={loadedProduct.pictureUrl} src={loadedProduct.pictureUrl} name={loadedProduct.name} />
      <div>
        <p>{loadedProduct.brand} / {loadedProduct.type}</p>
        <h1>{loadedProduct.name}</h1>
        <p className="product-price">{formatPrice(loadedProduct.price)}</p>
        <p>{loadedProduct.quantityInStock > 0 ? `${loadedProduct.quantityInStock} in stock` : 'Out of stock'}</p>
        <p className="product-description">{loadedProduct.description}</p>
      </div>
    </article>
  )
}

export default function ProductDetailPage() {
  const { id = '' } = useParams()
  const validId = /^[1-9]\d*$/.test(id) && Number.isSafeInteger(Number(id)) && Number(id) <= 2147483647
  return (
    <section>
      <Link to="/products">Back to products</Link>
      {validId ? <ProductDetails key={id} id={id} /> : <p role="alert">Invalid product ID.</p>}
    </section>
  )
}
