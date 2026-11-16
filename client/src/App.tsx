import { themeOptions, useTheme } from './theme'
import './App.css'

function App() {
  const { theme, setTheme } = useTheme()

  return (
    <>
      <header className="app-header">
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
      <section id="center">
        
      </section>

      <div className="ticks"></div>

      <section id="next-steps">

      </section>

      <div className="ticks"></div>
      <section id="spacer"></section>
    </>
  )
}

export default App
