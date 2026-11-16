import { useSyncExternalStore } from 'react'

type ThemeName = 'system' | 'light' | 'dark' | 'exotic' | 'nature'
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
    '--text': '#ffb8d7', '--text-h': '#ffe8f2', '--bg': '#1a0033',
    '--border': '#9b326e', '--code-bg': '#2d1b4e', '--accent': '#ff9bc6',
    '--accent-bg': 'rgba(233, 30, 99, 0.2)',
    '--accent-border': '#e65b99', '--social-bg': '#2d1b4e',
    '--shadow': 'rgba(0, 0, 0, 0.4) 0 10px 15px -3px, rgba(0, 0, 0, 0.25) 0 4px 6px -2px',
  },
  nature: {
    '--text': '#59564f', '--text-h': '#2d2924', '--bg': '#faf8f3',
    '--border': '#d4cec0', '--code-bg': '#ede9dc', '--accent': '#047857',
    '--accent-bg': 'rgba(5, 150, 105, 0.1)',
    '--accent-border': 'rgba(5, 150, 105, 0.5)', '--social-bg': '#ede9dc',
    '--shadow': 'rgba(45, 41, 36, 0.12) 0 10px 15px -3px, rgba(45, 41, 36, 0.06) 0 4px 6px -2px',
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
