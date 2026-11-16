import { Component, lazy, Suspense } from 'react'
import type { ErrorInfo, ReactNode } from 'react'
import { BrowserRouter, Link, NavLink, Route, Routes } from 'react-router-dom'
import { themeOptions, useTheme } from './theme'
import { ProductServiceProvider } from './features/products'
import './App.css'

const ProductsPage = lazy(() => import('./features/products/pages/ProductsPage'))
const ProductDetailPage = lazy(() => import('./features/products/pages/ProductDetailPage'))

class RouteErrorBoundary extends Component<{ children: ReactNode }, { failed: boolean }> {
  state = { failed: false }

  static getDerivedStateFromError() {
    return { failed: true }
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Unable to render the page.', error, info)
  }

  render() {
    return this.state.failed
      ? <p role="alert">Unable to load this page. Please reload and try again.</p>
      : this.props.children
  }
}

function App() {
  const { theme, setTheme } = useTheme()

  return (
    <BrowserRouter>
      <header className="app-header">
        <Link className="brand" to="/">Restore</Link>
        <nav aria-label="Main navigation">
          <NavLink to="/products">Products</NavLink>
        </nav>
        <details className="theme-settings">
          <summary>Theme settings</summary>
          <fieldset>
            <legend>Choose a theme</legend>
            {themeOptions.map((option) => (
              <label key={option.name}>
                <input
                  type="radio"
                  name="theme"
                  value={option.name}
                  checked={theme === option.name}
                  onChange={() => setTheme(option.name)}
                />
                {option.label}
              </label>
            ))}
          </fieldset>
        </details>
      </header>
      <main className="main-content">
        <ProductServiceProvider>
          <RouteErrorBoundary>
            <Suspense fallback={<p role="status">Loading page...</p>}>
              <Routes>
                <Route path="/" element={<section><h1>Welcome to Restore</h1><p>Explore our collection of products.</p><Link className="button-link" to="/products">Browse products</Link></section>} />
                <Route path="/products" element={<ProductsPage />} />
                <Route path="/products/:id" element={<ProductDetailPage />} />
                <Route path="*" element={<section><h1>Page not found</h1><Link to="/products">Browse products</Link></section>} />
              </Routes>
            </Suspense>
          </RouteErrorBoundary>
        </ProductServiceProvider>
      </main>
    </BrowserRouter>
  )
}

export default App
