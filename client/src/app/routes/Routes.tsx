import { lazy } from 'react'
import { BrowserRouter, Link, Route, Routes as RouterRoutes } from 'react-router-dom'
import App from '../layout/App'

const ProductsPage = lazy(() => import('../features/products/pages/ProductsPage'))
const ProductDetailPage = lazy(() => import('../features/products/pages/ProductDetailPage'))
const AboutPage = lazy(() => import('../features/about/pages/AboutPage'))

export default function AppRoutes() {
  return (
    <BrowserRouter>
      <RouterRoutes>
        <Route element={<App />}>
          <Route
            index
            element={
              <section>
                <h1>Welcome to Restore</h1>
                <p>Explore our collection of products.</p>
                <Link className="button-link" to="/products">Browse products</Link>
              </section>
            }
          />
          <Route path="products" element={<ProductsPage />} />
          <Route path="products/:id" element={<ProductDetailPage />} />
          <Route path="about" element={<AboutPage />} />
          <Route
            path="*"
            element={
              <section>
                <h1>Page not found</h1>
                <Link to="/products">Browse products</Link>
              </section>
            }
          />
        </Route>
      </RouterRoutes>
    </BrowserRouter>
  )
}
