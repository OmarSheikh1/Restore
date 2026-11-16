# Theme Management System Guidelines

## Overview
A flexible, extensible theme system that:
- Supports multiple themes (dark, light, exotic, nature + future themes)
- Applies themes via CSS variables (no re-renders)
- Allows users to switch themes instantly
- Persists user preference
- Easy to add new themes

---

## 1. Theme Architecture

### 1.1 Theme Type Definition
```typescript
// src/core/types/theme.types.ts
export interface ThemeConfig {
  name: string;
  displayName: string;
  description?: string;
  icon?: string;
  colors: {
    primary: string;
    secondary: string;
    background: string;
    surface: string;
    text: string;
    textSecondary: string;
    border: string;
    error: string;
    warning: string;
    success: string;
    info: string;
  };
  typography?: {
    fontFamily: string;
    fontSize: {
      xs: string;
      sm: string;
      base: string;
      lg: string;
      xl: string;
    };
  };
  spacing?: {
    xs: string;
    sm: string;
    md: string;
    lg: string;
    xl: string;
  };
  shadows?: {
    sm: string;
    md: string;
    lg: string;
  };
}

export interface IThemeService {
  getCurrentTheme(): ThemeConfig;
  setTheme(name: string): void;
  getAvailableThemes(): ThemeConfig[];
  registerTheme(theme: ThemeConfig): void;
  subscribe(callback: (theme: ThemeConfig) => void): () => void;
  getThemeByName(name: string): ThemeConfig | undefined;
}
```

---

## 2. Define Theme Configurations

### 2.1 Light Theme
```typescript
// src/ui/themes/lightTheme.ts
import { ThemeConfig } from '@/core/types/theme.types';

export const lightTheme: ThemeConfig = {
  name: 'light',
  displayName: 'Light',
  description: 'Clean, bright theme',
  icon: 'sun',
  colors: {
    primary: '#0066ff',
    secondary: '#00d4ff',
    background: '#ffffff',
    surface: '#f5f5f5',
    text: '#1a1a1a',
    textSecondary: '#666666',
    border: '#e0e0e0',
    error: '#dc2626',
    warning: '#f59e0b',
    success: '#10b981',
    info: '#3b82f6',
  },
  typography: {
    fontFamily: "'Inter', sans-serif",
    fontSize: {
      xs: '0.75rem',
      sm: '0.875rem',
      base: '1rem',
      lg: '1.125rem',
      xl: '1.25rem',
    },
  },
};
```

### 2.2 Dark Theme
```typescript
// src/ui/themes/darkTheme.ts
import { ThemeConfig } from '@/core/types/theme.types';

export const darkTheme: ThemeConfig = {
  name: 'dark',
  displayName: 'Dark',
  description: 'Eye-friendly, dark theme',
  icon: 'moon',
  colors: {
    primary: '#3b82f6',
    secondary: '#06b6d4',
    background: '#0f172a',
    surface: '#1e293b',
    text: '#f1f5f9',
    textSecondary: '#cbd5e1',
    border: '#334155',
    error: '#f87171',
    warning: '#fbbf24',
    success: '#34d399',
    info: '#60a5fa',
  },
  typography: {
    fontFamily: "'Inter', sans-serif",
    fontSize: {
      xs: '0.75rem',
      sm: '0.875rem',
      base: '1rem',
      lg: '1.125rem',
      xl: '1.25rem',
    },
  },
};
```

### 2.3 Exotic Theme
```typescript
// src/ui/themes/exoticTheme.ts
import { ThemeConfig } from '@/core/types/theme.types';

export const exoticTheme: ThemeConfig = {
  name: 'exotic',
  displayName: 'Exotic',
  description: 'Vibrant, gradient-rich theme',
  icon: 'sparkles',
  colors: {
    primary: '#e91e63',
    secondary: '#ff6b9d',
    background: '#1a0033',
    surface: '#2d1b4e',
    text: '#ffd6e8',
    textSecondary: '#ff99c8',
    border: '#c2185b',
    error: '#ff1744',
    warning: '#ffab40',
    success: '#1de9b6',
    info: '#00e5ff',
  },
  typography: {
    fontFamily: "'Poppins', sans-serif",
    fontSize: {
      xs: '0.75rem',
      sm: '0.875rem',
      base: '1rem',
      lg: '1.125rem',
      xl: '1.25rem',
    },
  },
};
```

### 2.4 Nature Theme
```typescript
// src/ui/themes/natureTheme.ts
import { ThemeConfig } from '@/core/types/theme.types';

export const natureTheme: ThemeConfig = {
  name: 'nature',
  displayName: 'Nature',
  description: 'Earthy, calming theme',
  icon: 'leaf',
  colors: {
    primary: '#059669',
    secondary: '#84cc16',
    background: '#faf8f3',
    surface: '#ede9dc',
    text: '#2d2924',
    textSecondary: '#6b6962',
    border: '#d4cec0',
    error: '#d32f2f',
    warning: '#f57c00',
    success: '#388e3c',
    info: '#0288d1',
  },
  typography: {
    fontFamily: "'Segoe UI', sans-serif",
    fontSize: {
      xs: '0.75rem',
      sm: '0.875rem',
      base: '1rem',
      lg: '1.125rem',
      xl: '1.25rem',
    },
  },
};
```

---

## 3. Theme Registry (Extensible)

### 3.1 Registry Implementation
```typescript
// src/ui/themes/ThemeRegistry.ts
import { ThemeConfig, IThemeService } from '@/core/types/theme.types';
import { lightTheme } from './lightTheme';
import { darkTheme } from './darkTheme';
import { exoticTheme } from './exoticTheme';
import { natureTheme } from './natureTheme';

export class ThemeRegistry {
  private static themes = new Map<string, ThemeConfig>();
  
  static {
    // Initialize with default themes
    this.register(lightTheme);
    this.register(darkTheme);
    this.register(exoticTheme);
    this.register(natureTheme);
  }

  static register(theme: ThemeConfig): void {
    if (this.themes.has(theme.name)) {
      console.warn(`Theme '${theme.name}' already registered, replacing...`);
    }
    this.themes.set(theme.name, theme);
  }

  static get(name: string): ThemeConfig {
    const theme = this.themes.get(name);
    if (!theme) {
      console.warn(`Theme '${name}' not found, using light theme`);
      return this.themes.get('light')!;
    }
    return theme;
  }

  static getAll(): ThemeConfig[] {
    return Array.from(this.themes.values());
  }

  static has(name: string): boolean {
    return this.themes.has(name);
  }
}
```

### 3.2 Adding New Themes at Runtime
```typescript
// In your app or plugin system
import { ThemeRegistry } from '@/ui/themes/ThemeRegistry';
import { customTheme } from './customTheme';

// Register new theme
ThemeRegistry.register(customTheme);

// Now users can switch to it
themeService.setTheme('customTheme');
```

---

## 4. Theme Service Implementation

### 4.1 Service with Persistence
```typescript
// src/services/theme/ThemeService.ts
import { ThemeConfig, IThemeService } from '@/core/types/theme.types';
import { ThemeRegistry } from '@/ui/themes/ThemeRegistry';
import { IStorageService } from '@abstractions/storage.abstraction';

const THEME_STORAGE_KEY = 'app:theme-preference';
const DEFAULT_THEME = 'light';

export class ThemeService implements IThemeService {
  private currentTheme: ThemeConfig;
  private subscribers: Array<(theme: ThemeConfig) => void> = [];

  constructor(private storage: IStorageService) {
    this.currentTheme = this.loadTheme();
    this.applyTheme(this.currentTheme);
  }

  private loadTheme(): ThemeConfig {
    const savedThemeName = this.storage.getItem(THEME_STORAGE_KEY);
    const themeName = savedThemeName || this.getSystemPreference() || DEFAULT_THEME;
    return ThemeRegistry.get(themeName);
  }

  private getSystemPreference(): string {
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
      return 'dark';
    }
    return 'light';
  }

  getCurrentTheme(): ThemeConfig {
    return this.currentTheme;
  }

  setTheme(name: string): void {
    const theme = ThemeRegistry.get(name);
    if (theme.name === this.currentTheme.name) {
      return; // No change
    }

    this.currentTheme = theme;
    this.storage.setItem(THEME_STORAGE_KEY, theme.name);
    this.applyTheme(theme);
    this.notifySubscribers();
  }

  private applyTheme(theme: ThemeConfig): void {
    const root = document.documentElement;

    // Apply color variables
    root.style.setProperty('--color-primary', theme.colors.primary);
    root.style.setProperty('--color-secondary', theme.colors.secondary);
    root.style.setProperty('--color-background', theme.colors.background);
    root.style.setProperty('--color-surface', theme.colors.surface);
    root.style.setProperty('--color-text', theme.colors.text);
    root.style.setProperty('--color-text-secondary', theme.colors.textSecondary);
    root.style.setProperty('--color-border', theme.colors.border);
    root.style.setProperty('--color-error', theme.colors.error);
    root.style.setProperty('--color-warning', theme.colors.warning);
    root.style.setProperty('--color-success', theme.colors.success);
    root.style.setProperty('--color-info', theme.colors.info);

    // Apply typography
    if (theme.typography) {
      root.style.setProperty('--font-family', theme.typography.fontFamily);
      root.style.setProperty('--font-size-xs', theme.typography.fontSize.xs);
      root.style.setProperty('--font-size-sm', theme.typography.fontSize.sm);
      root.style.setProperty('--font-size-base', theme.typography.fontSize.base);
      root.style.setProperty('--font-size-lg', theme.typography.fontSize.lg);
      root.style.setProperty('--font-size-xl', theme.typography.fontSize.xl);
    }

    // Update document attributes for CSS selectors
    document.documentElement.setAttribute('data-theme', theme.name);
  }

  getAvailableThemes(): ThemeConfig[] {
    return ThemeRegistry.getAll();
  }

  registerTheme(theme: ThemeConfig): void {
    ThemeRegistry.register(theme);
  }

  getThemeByName(name: string): ThemeConfig | undefined {
    return ThemeRegistry.has(name) ? ThemeRegistry.get(name) : undefined;
  }

  subscribe(callback: (theme: ThemeConfig) => void): () => void {
    this.subscribers.push(callback);
    return () => {
      this.subscribers = this.subscribers.filter(s => s !== callback);
    };
  }

  private notifySubscribers(): void {
    this.subscribers.forEach(callback => callback(this.currentTheme));
  }
}
```

---

## 5. Theme Provider Setup

### 5.1 Context and Hook
```typescript
// src/core/di/providers/ThemeProvider.tsx
import { createContext, PropsWithChildren, useEffect, useState } from 'react';
import { ThemeConfig, IThemeService } from '@/core/types/theme.types';
import { container } from '@/core/di/container';

export interface ThemeContextType {
  theme: ThemeConfig;
  changeTheme: (name: string) => void;
  availableThemes: ThemeConfig[];
  registerTheme: (theme: ThemeConfig) => void;
}

export const ThemeContext = createContext<ThemeContextType | null>(null);

export const ThemeProvider = ({ children }: PropsWithChildren) => {
  const themeService = container.get<IThemeService>('themeService');
  const [theme, setTheme] = useState<ThemeConfig>(themeService.getCurrentTheme());
  const [availableThemes, setAvailableThemes] = useState(themeService.getAvailableThemes());

  useEffect(() => {
    const unsubscribe = themeService.subscribe((newTheme) => {
      setTheme(newTheme);
    });
    return unsubscribe;
  }, [themeService]);

  const changeTheme = (name: string) => {
    themeService.setTheme(name);
  };

  const registerTheme = (newTheme: ThemeConfig) => {
    themeService.registerTheme(newTheme);
    setAvailableThemes(themeService.getAvailableThemes());
  };

  const value: ThemeContextType = {
    theme,
    changeTheme,
    availableThemes,
    registerTheme,
  };

  return (
    <ThemeContext.Provider value={value}>
      {children}
    </ThemeContext.Provider>
  );
};
```

### 5.2 Custom Hook
```typescript
// src/services/theme/useTheme.hook.ts
import { useContext } from 'react';
import { ThemeContext, ThemeContextType } from '@/core/di/providers/ThemeProvider';

export const useTheme = (): ThemeContextType => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within ThemeProvider');
  }
  return context;
};
```

---

## 6. CSS Variables Setup

### 6.1 Global CSS Variables
```css
/* src/ui/styles/variables.css */
:root {
  /* Colors - Set by ThemeService */
  --color-primary: #0066ff;
  --color-secondary: #00d4ff;
  --color-background: #ffffff;
  --color-surface: #f5f5f5;
  --color-text: #1a1a1a;
  --color-text-secondary: #666666;
  --color-border: #e0e0e0;
  --color-error: #dc2626;
  --color-warning: #f59e0b;
  --color-success: #10b981;
  --color-info: #3b82f6;

  /* Typography - Set by ThemeService */
  --font-family: 'Inter', sans-serif;
  --font-size-xs: 0.75rem;
  --font-size-sm: 0.875rem;
  --font-size-base: 1rem;
  --font-size-lg: 1.125rem;
  --font-size-xl: 1.25rem;

  /* Spacing */
  --spacing-xs: 0.25rem;
  --spacing-sm: 0.5rem;
  --spacing-md: 1rem;
  --spacing-lg: 1.5rem;
  --spacing-xl: 2rem;

  /* Shadows */
  --shadow-sm: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
  --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
}
```

### 6.2 Component Styling with Variables
```css
/* src/ui/components/Button.css */
.btn {
  font-family: var(--font-family);
  font-size: var(--font-size-base);
  background-color: var(--color-primary);
  color: var(--color-background);
  border: 1px solid var(--color-border);
  padding: var(--spacing-sm) var(--spacing-md);
  border-radius: 0.375rem;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: var(--shadow-sm);
}

.btn:hover {
  background-color: var(--color-secondary);
  box-shadow: var(--shadow-md);
}

.btn:active {
  box-shadow: var(--shadow-lg);
}

.btn.error {
  background-color: var(--color-error);
}

.btn.success {
  background-color: var(--color-success);
}

.btn.warning {
  background-color: var(--color-warning);
}
```

### 6.3 Tailwind Alternative (if using Tailwind)
```typescript
// tailwind.config.js
module.exports = {
  theme: {
    colors: {
      primary: 'var(--color-primary)',
      secondary: 'var(--color-secondary)',
      background: 'var(--color-background)',
      surface: 'var(--color-surface)',
      text: 'var(--color-text)',
      'text-secondary': 'var(--color-text-secondary)',
      border: 'var(--color-border)',
      error: 'var(--color-error)',
      warning: 'var(--color-warning)',
      success: 'var(--color-success)',
      info: 'var(--color-info)',
    },
    fontSize: {
      xs: 'var(--font-size-xs)',
      sm: 'var(--font-size-sm)',
      base: 'var(--font-size-base)',
      lg: 'var(--font-size-lg)',
      xl: 'var(--font-size-xl)',
    },
  },
};
```

---

## 7. Theme Switcher Component

### 7.1 Basic Theme Switcher
```typescript
// src/ui/components/ThemeSwitcher.tsx
import { useTheme } from '@/services/theme/useTheme.hook';

export const ThemeSwitcher = () => {
  const { theme, changeTheme, availableThemes } = useTheme();

  return (
    <div className="theme-switcher">
      <label>Theme:</label>
      <select 
        value={theme.name} 
        onChange={(e) => changeTheme(e.target.value)}
        className="theme-select"
      >
        {availableThemes.map((t) => (
          <option key={t.name} value={t.name}>
            {t.displayName}
          </option>
        ))}
      </select>
    </div>
  );
};
```

### 7.2 Advanced Theme Switcher with Icons
```typescript
// src/ui/components/ThemeSwitcherAdvanced.tsx
import { useTheme } from '@/services/theme/useTheme.hook';
import { Sun, Moon, Sparkles, Leaf } from 'lucide-react';

const themeIcons: Record<string, React.ReactNode> = {
  light: <Sun size={20} />,
  dark: <Moon size={20} />,
  exotic: <Sparkles size={20} />,
  nature: <Leaf size={20} />,
};

export const ThemeSwitcherAdvanced = () => {
  const { theme, changeTheme, availableThemes } = useTheme();

  return (
    <div className="theme-switcher-advanced">
      <div className="theme-options">
        {availableThemes.map((t) => (
          <button
            key={t.name}
            onClick={() => changeTheme(t.name)}
            className={`theme-option ${theme.name === t.name ? 'active' : ''}`}
            title={t.displayName}
            aria-label={`Switch to ${t.displayName} theme`}
          >
            {themeIcons[t.name] || t.icon}
            <span>{t.displayName}</span>
          </button>
        ))}
      </div>
    </div>
  );
};
```

---

## 8. Applying Themes in Components

### 8.1 Using Theme Variables in Components
```typescript
// src/ui/components/Card.tsx
import './Card.css';

export const Card = ({ children, variant = 'default' }) => {
  return (
    <div className={`card card-${variant}`}>
      {children}
    </div>
  );
};
```

```css
/* src/ui/components/Card.css */
.card {
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 0.5rem;
  padding: var(--spacing-md);
  box-shadow: var(--shadow-sm);
  color: var(--color-text);
  font-family: var(--font-family);
}

.card-elevated {
  box-shadow: var(--shadow-lg);
}

.card:hover {
  box-shadow: var(--shadow-md);
  transition: box-shadow 0.2s ease;
}
```

### 8.2 Responsive Theme Application
```typescript
// src/ui/components/Header.tsx
import { useTheme } from '@/services/theme/useTheme.hook';

export const Header = () => {
  const { theme } = useTheme();

  return (
    <header 
      style={{
        backgroundColor: theme.colors.surface,
        color: theme.colors.text,
        borderBottomColor: theme.colors.border,
      }}
    >
      {/* Header content */}
    </header>
  );
};
```

---

## 9. Adding Custom Themes

### 9.1 Creating Custom Theme
```typescript
// src/ui/themes/customTheme.ts
import { ThemeConfig } from '@/core/types/theme.types';

export const customTheme: ThemeConfig = {
  name: 'custom',
  displayName: 'Custom Theme',
  description: 'My custom theme',
  colors: {
    primary: '#your-color',
    secondary: '#your-color',
    // ... other colors
  },
};
```

### 9.2 Registering at Startup
```typescript
// src/main.tsx
import { ThemeRegistry } from '@/ui/themes/ThemeRegistry';
import { customTheme } from '@/ui/themes/customTheme';

ThemeRegistry.register(customTheme);

// Or dynamically at runtime
export const registerCustomTheme = (theme: ThemeConfig) => {
  ThemeRegistry.register(theme);
};
```

---

## 10. Best Practices Checklist

- [ ] All theme values defined in theme config (no hardcoded colors)
- [ ] CSS variables applied in root `:root`
- [ ] Theme service persists user preference
- [ ] Theme service respects system preference
- [ ] ThemeSwitcher component uses `useTheme()` hook
- [ ] New themes registered in ThemeRegistry
- [ ] CSS files use `var(--color-*)` not hardcoded colors
- [ ] Theme changes trigger re-render only for ThemeContext subscribers
- [ ] Themes accessible via theme name string
- [ ] Custom themes can be added at runtime

---

## 11. Testing Themes

```typescript
// src/services/theme/__tests__/ThemeService.test.ts
import { ThemeService } from '../ThemeService';
import { MockStorageService } from '@services/storage/adapters/MockStorageAdapter';
import { lightTheme } from '@/ui/themes/lightTheme';

describe('ThemeService', () => {
  it('should set theme and persist', () => {
    const storage = new MockStorageService();
    const service = new ThemeService(storage);

    service.setTheme(lightTheme.name);

    expect(service.getCurrentTheme().name).toBe('light');
    expect(storage.getItem('app:theme-preference')).toBe('light');
  });

  it('should notify subscribers on theme change', () => {
    const storage = new MockStorageService();
    const service = new ThemeService(storage);
    const callback = jest.fn();

    service.subscribe(callback);
    service.setTheme('dark');

    expect(callback).toHaveBeenCalledWith(expect.objectContaining({ name: 'dark' }));
  });
});
```

---

## References
- [CSS Variables (Custom Properties)](https://developer.mozilla.org/en-US/docs/Web/CSS/--*)
- [prefers-color-scheme](https://developer.mozilla.org/en-US/docs/Web/CSS/@media/prefers-color-scheme)
- [Theme Design System Best Practices](https://www.smashingmagazine.com/2022/04/complete-guide-dark-mode-css/)
