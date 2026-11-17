import { Link, useParams } from 'react-router-dom'
import { ProductImage } from '../components/ProductImage'
import { formatPrice } from '../components/price'
import { useProduct } from '../hooks'

function ProductDetails({ id }: { id: string }) {
  const state = useProduct(id)

  if (state.status === 'loading') return <p role="status">Loading product...</p>
  if (state.status === 'error') return <p role="alert">{state.message}</p>
  const product = state.data
  return (
    <article className="product-detail">
      <ProductImage key={product.pictureUrl} src={product.pictureUrl} name={product.name} />
      <div>
        <p>{product.brand} / {product.type}</p>
        <h1>{product.name}</h1>
        <p className="product-price">{formatPrice(product.price)}</p>
        <p>{product.quantityInStock > 0 ? `${product.quantityInStock} in stock` : 'Out of stock'}</p>
        <p className="product-description">{product.description}</p>
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
