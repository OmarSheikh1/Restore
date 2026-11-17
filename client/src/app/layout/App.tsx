import { Component, Suspense, useRef } from 'react'
import type { ErrorInfo, ReactNode } from 'react'
import { Link, NavLink, Outlet } from 'react-router-dom'
import { themeOptions, useTheme } from '../../theme'
import { ProductServiceProvider } from '../features/products'

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
  const themeSettingsRef = useRef<HTMLDetailsElement>(null)

  return (
    <>
      <header className="app-header">
        <Link className="brand" to="/">Restore</Link>
        <nav aria-label="Main navigation">
          <NavLink to="/products">Products</NavLink>
          <NavLink to="/about">About</NavLink>
        </nav>
        <details className="theme-settings" ref={themeSettingsRef}>
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
                  onChange={() => {
                    setTheme(option.name)
                    if (themeSettingsRef.current) themeSettingsRef.current.open = false
                  }}
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
              <Outlet />
            </Suspense>
          </RouteErrorBoundary>
        </ProductServiceProvider>
      </main>
    </>
  )
}

export default App
