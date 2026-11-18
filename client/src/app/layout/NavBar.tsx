import { useRef, useState } from 'react'
import { Link, NavLink } from 'react-router-dom'
import { themeOptions, useTheme } from '../../theme'

export default function NavBar() {
  const { theme, setTheme } = useTheme()
  const themeSettingsRef = useRef<HTMLDetailsElement>(null)
  const menuButtonRef = useRef<HTMLButtonElement>(null)
  const [menuOpen, setMenuOpen] = useState(false)

  return (
    <header className="app-header">
      <div className="navigation" onKeyDown={(event) => {
        if (event.key === 'Escape' && menuOpen) {
          setMenuOpen(false)
          menuButtonRef.current?.focus()
        }
      }}>
        <button
          ref={menuButtonRef}
          className="menu-toggle"
          type="button"
          aria-label="Main menu"
          aria-expanded={menuOpen}
          aria-controls="main-navigation"
          onClick={() => setMenuOpen(!menuOpen)}
        >
          <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round">
            <path d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>
        <nav id="main-navigation" className={menuOpen ? 'menu-open' : ''} aria-label="Main navigation">
          <NavLink to="/products" onClick={() => setMenuOpen(false)}>Products</NavLink>
          <NavLink to="/browsed-products" onClick={() => setMenuOpen(false)}>Browsed Products</NavLink>
          <NavLink to="/products-rtk" onClick={() => setMenuOpen(false)}>Products (RTK Query)</NavLink>
          <NavLink to="/about" onClick={() => setMenuOpen(false)}>About</NavLink>
          <NavLink to="/contact" onClick={() => setMenuOpen(false)}>Contact Us</NavLink>
        </nav>
      </div>
      <Link className="brand" to="/" onClick={() => setMenuOpen(false)}>ReStore</Link>
      <details className="theme-settings" ref={themeSettingsRef}>
        <summary aria-label="Theme settings" title="Theme settings">
          <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
            <circle cx="12" cy="12" r="4" />
            <path d="M12 2v2M12 20v2M4.93 4.93l1.42 1.42m11.3 11.3 1.42 1.42M2 12h2m16 0h2M4.93 19.07l1.42-1.42m11.3-11.3 1.42-1.42" />
          </svg>
        </summary>
        <div className="theme-panel">
          <fieldset>
            <legend>Choose a theme</legend>
            {themeOptions.map((option) => (
              <label key={option.name} className={theme === option.name ? 'selected' : ''}>
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
        </div>
      </details>
    </header>
  )
}
