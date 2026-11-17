import { Component, Suspense } from 'react'
import type { ErrorInfo, ReactNode } from 'react'
import { Outlet } from 'react-router-dom'
import { ProductServiceProvider } from '../features/products'
import NavBar from './NavBar'

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
  return (
    <>
      <NavBar />
      <main className="main-content">
        <ProductServiceProvider>
          <RouteErrorBoundary>
            <Suspense fallback={<p role="status">Loading page...</p>}>
              <Outlet />
            </Suspense>
          </RouteErrorBoundary>
        </ProductServiceProvider>
      </main>
    </>
  )
}

export default App
