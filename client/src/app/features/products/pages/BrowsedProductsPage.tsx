import { useSelector } from 'react-redux'
import { Link } from 'react-router-dom'
import { ProductImage } from '../components/ProductImage'
import { formatPrice } from '../components/price'
import type { RootState } from '../../../store/store'

export default function BrowsedProductsPage() {
  const products = useSelector((state: RootState) => state.browsedProducts)

  return (
    <section>
      <h1>Browsed Products</h1>
      {products.length === 0
        ? <p>You haven't browsed any products yet. <Link to="/products">Browse products</Link>.</p>
        : <div className="product-grid">
            {products.map(product => (
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
    </section>
  )
}
