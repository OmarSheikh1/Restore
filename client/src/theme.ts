import { useSyncExternalStore } from 'react'

type ThemeName = 'system' | 'light' | 'dark' | 'exotic' | 'nature' | 'liquid'
type ColorTheme = Exclude<ThemeName, 'system'>
type ThemeColors = Record<
  | '--text'
  | '--text-h'
  | '--bg'
  | '--border'
  | '--code-bg'
  | '--accent'
  | '--accent-bg'
  | '--accent-border'
  | '--social-bg'
  | '--shadow',
  string
>

const storageKey = 'app:theme-preference'

export const themeOptions: { name: ThemeName; label: string }[] = [
  { name: 'system', label: 'System' },
  { name: 'light', label: 'Light' },
  { name: 'dark', label: 'Dark' },
  { name: 'exotic', label: 'Exotic' },
  { name: 'nature', label: 'Nature' },
  { name: 'liquid', label: 'Liquid' },
]

const themes: Record<ColorTheme, ThemeColors> = {
  light: {
    '--text': '#6b6375', '--text-h': '#08060d', '--bg': '#fff',
    '--border': '#e5e4e7', '--code-bg': '#f4f3ec', '--accent': '#aa3bff',
    '--accent-bg': 'rgba(170, 59, 255, 0.1)',
    '--accent-border': 'rgba(170, 59, 255, 0.5)',
    '--social-bg': 'rgba(244, 243, 236, 0.5)',
    '--shadow': 'rgba(0, 0, 0, 0.1) 0 10px 15px -3px, rgba(0, 0, 0, 0.05) 0 4px 6px -2px',
  },
  dark: {
    '--text': '#9ca3af', '--text-h': '#f3f4f6', '--bg': '#16171d',
    '--border': '#2e303a', '--code-bg': '#1f2028', '--accent': '#c084fc',
    '--accent-bg': 'rgba(192, 132, 252, 0.15)',
    '--accent-border': 'rgba(192, 132, 252, 0.5)',
    '--social-bg': 'rgba(47, 48, 58, 0.5)',
    '--shadow': 'rgba(0, 0, 0, 0.4) 0 10px 15px -3px, rgba(0, 0, 0, 0.25) 0 4px 6px -2px',
  },
  exotic: {
    '--text': '#ecd3e8', '--text-h': '#fff0fa', '--bg': '#35204a',
    '--border': '#74508a', '--code-bg': '#43295c', '--accent': '#ffa6cf',
    '--accent-bg': 'rgba(233, 30, 99, 0.22)',
    '--accent-border': '#ec7fb3', '--social-bg': '#43295c',
    '--shadow': 'rgba(0, 0, 0, 0.3) 0 10px 15px -3px, rgba(0, 0, 0, 0.18) 0 4px 6px -2px',
  },
  nature: {
    '--text': '#53665b', '--text-h': '#1e3327', '--bg': '#f4f8f2',
    '--border': '#c8d8ca', '--code-bg': '#e6efe5', '--accent': '#28734e',
    '--accent-bg': 'rgba(40, 115, 78, 0.1)',
    '--accent-border': 'rgba(40, 115, 78, 0.5)', '--social-bg': '#e8f0e6',
    '--shadow': 'rgba(30, 51, 39, 0.12) 0 10px 15px -3px, rgba(30, 51, 39, 0.06) 0 4px 6px -2px',
  },
      liquid: {
        '--text': '#3f4f6b', '--text-h': '#10203d', '--bg': '#dbe9fb',
        '--border': 'rgba(255, 255, 255, 0.65)', '--code-bg': 'rgba(255, 255, 255, 0.45)',
        '--accent': '#1f6feb', '--accent-bg': 'rgba(31, 111, 235, 0.14)',
        '--accent-border': 'rgba(31, 111, 235, 0.45)', '--social-bg': 'rgba(255, 255, 255, 0.4)',
        '--shadow': 'rgba(31, 60, 120, 0.2) 0 8px 32px 0, rgba(255, 255, 255, 0.6) 0 1px 0 inset',
      },
}

function loadTheme(): ThemeName {
  try {
    const saved = localStorage.getItem(storageKey)
    const option = themeOptions.find((item) => item.name === saved)
    if (option) return option.name
    if (saved !== null) console.warn('Unknown saved theme preference:', saved)
  } catch (error) {
    console.warn('Unable to read the saved theme preference.', error)
  }
  return 'system'
}

class ThemeService {
  private theme = loadTheme()
  private listeners = new Set<() => void>()
  private systemTheme = window.matchMedia('(prefers-color-scheme: dark)')

  constructor() {
    this.applyTheme()
    this.systemTheme.addEventListener('change', () => {
      if (this.theme === 'system') this.applyTheme()
    })
  }

  getTheme = () => this.theme

  subscribe = (listener: () => void) => {
    this.listeners.add(listener)
    return () => this.listeners.delete(listener)
  }

  setTheme = (theme: ThemeName) => {
    this.theme = theme
    this.applyTheme()
    try {
      localStorage.setItem(storageKey, theme)
    } catch (error) {
      console.warn('Unable to save the theme preference.', error)
    }
    this.listeners.forEach((listener) => listener())
  }

  private applyTheme() {
    const name = this.theme === 'system'
      ? this.systemTheme.matches ? 'dark' : 'light'
      : this.theme
    const root = document.documentElement
    for (const [property, value] of Object.entries(themes[name])) {
      root.style.setProperty(property, value)
    }
    root.dataset.theme = name
    root.style.colorScheme = name === 'dark' || name === 'exotic' ? 'dark' : 'light'
  }
}

const themeService = new ThemeService()

export function useTheme() {
  const theme = useSyncExternalStore(themeService.subscribe, themeService.getTheme)
  return { theme, setTheme: themeService.setTheme }
}
