# Secure Authentication, Authorization & Navigation Guidelines

## Overview
Comprehensive guide for implementing secure role-based access control (RBAC), dynamic user menus, auth guards, and protected routes.

---

## 1. Authentication Architecture

### 1.1 Auth Type Definitions
```typescript
// src/core/types/auth.types.ts
export interface User {
  id: string;
  email: string;
  name: string;
  avatar?: string;
  roles: string[]; // ['admin', 'user', 'editor']
  claims: Record<string, string | boolean | string[]>; // Custom claims
  permissions: string[]; // Computed from roles
  isEmailVerified: boolean;
}

export interface AuthState {
  token: string | null;
  refreshToken: string | null;
  expiresAt: number | null;
  user: User | null;
  isAuthenticated: boolean;
}

export interface LoginCredentials {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: User;
}

export interface AuthContextType extends AuthState {
  login(credentials: LoginCredentials): Promise<void>;
  logout(): Promise<void>;
  refreshAccessToken(): Promise<string>;
  hasRole(role: string): boolean;
  hasAnyRole(...roles: string[]): boolean;
  hasAllRoles(...roles: string[]): boolean;
  hasClaim(claim: string, value?: any): boolean;
  hasPermission(permission: string): boolean;
  hasAnyPermission(...permissions: string[]): boolean;
  isTokenExpired(): boolean;
  isLoading: boolean;
  error: string | null;
}
```

### 1.2 Abstract Auth Service
```typescript
// src/abstractions/auth.abstraction.ts
export interface IAuthService {
  // Authentication
  login(credentials: LoginCredentials): Promise<LoginResponse>;
  logout(): Promise<void>;
  refreshAccessToken(): Promise<string>;
  getCurrentUser(): Promise<User | null>;
  verifyToken(token: string): Promise<boolean>;

  // Authorization
  hasRole(user: User, role: string): boolean;
  hasAnyRole(user: User, ...roles: string[]): boolean;
  hasAllRoles(user: User, ...roles: string[]): boolean;
  hasClaim(user: User, claim: string, value?: any): boolean;
  hasPermission(user: User, permission: string): boolean;
  hasAnyPermission(user: User, ...permissions: string[]): boolean;

  // Token Management
  setAuthToken(token: string): void;
  getAuthToken(): string | null;
  clearAuthToken(): void;
  isTokenExpired(token: string): boolean;
}
```

---

## 2. Auth Service Implementation

### 2.1 Complete Auth Service
```typescript
// src/services/auth/AuthService.ts
import { IAuthService } from '@abstractions/auth.abstraction';
import { IApiClient } from '@abstractions/api.abstraction';
import { IStorageService } from '@abstractions/storage.abstraction';
import {
  User,
  LoginCredentials,
  LoginResponse,
} from '@/core/types/auth.types';

const AUTH_TOKEN_KEY = 'app:auth-token';
const REFRESH_TOKEN_KEY = 'app:refresh-token';
const TOKEN_EXPIRY_KEY = 'app:token-expiry';

export class AuthService implements IAuthService {
  constructor(
    private apiClient: IApiClient,
    private storage: IStorageService
  ) {
    this.restoreTokens();
  }

  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    try {
      const response = await this.apiClient.post<LoginResponse>(
        '/auth/login',
        {
          email: credentials.email,
          password: credentials.password,
        }
      );

      this.setAuthToken(response.accessToken);
      this.storage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
      this.storage.setItem(
        TOKEN_EXPIRY_KEY,
        String(Date.now() + response.expiresIn * 1000)
      );

      return response;
    } catch (error) {
      throw new Error('Login failed');
    }
  }

  async logout(): Promise<void> {
    try {
      await this.apiClient.post('/auth/logout', {});
    } catch (error) {
      // Logout on backend failed, but still clear local tokens
      console.error('Logout failed on server', error);
    }

    this.clearAuthToken();
    this.storage.removeItem(REFRESH_TOKEN_KEY);
    this.storage.removeItem(TOKEN_EXPIRY_KEY);
  }

  async refreshAccessToken(): Promise<string> {
    const refreshToken = this.storage.getItem(REFRESH_TOKEN_KEY);
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    try {
      const response = await this.apiClient.post<{ accessToken: string }>(
        '/auth/refresh',
        { refreshToken }
      );

      this.setAuthToken(response.accessToken);
      return response.accessToken;
    } catch (error) {
      this.clearAuthToken();
      throw new Error('Token refresh failed');
    }
  }

  async getCurrentUser(): Promise<User | null> {
    if (!this.getAuthToken()) {
      return null;
    }

    try {
      return await this.apiClient.get<User>('/auth/me');
    } catch (error) {
      return null;
    }
  }

  async verifyToken(token: string): Promise<boolean> {
    try {
      const response = await this.apiClient.post<{ valid: boolean }>(
        '/auth/verify',
        { token }
      );
      return response.valid;
    } catch {
      return false;
    }
  }

  hasRole(user: User | null, role: string): boolean {
    return user?.roles.includes(role) ?? false;
  }

  hasAnyRole(user: User | null, ...roles: string[]): boolean {
    return user?.roles.some(r => roles.includes(r)) ?? false;
  }

  hasAllRoles(user: User | null, ...roles: string[]): boolean {
    return roles.every(r => user?.roles.includes(r)) ?? false;
  }

  hasClaim(user: User | null, claim: string, value?: any): boolean {
    if (!user) return false;
    if (!(claim in user.claims)) return false;
    if (value === undefined) return true;
    return user.claims[claim] === value;
  }

  hasPermission(user: User | null, permission: string): boolean {
    return user?.permissions.includes(permission) ?? false;
  }

  hasAnyPermission(user: User | null, ...permissions: string[]): boolean {
    return permissions.some(p => user?.permissions.includes(p)) ?? false;
  }

  setAuthToken(token: string): void {
    this.storage.setItem(AUTH_TOKEN_KEY, token);
    this.apiClient.setAuthToken(token);
  }

  getAuthToken(): string | null {
    return this.storage.getItem(AUTH_TOKEN_KEY);
  }

  clearAuthToken(): void {
    this.storage.removeItem(AUTH_TOKEN_KEY);
    this.apiClient.clearAuthToken();
  }

  isTokenExpired(token: string): boolean {
    try {
      const expiryTime = parseInt(
        this.storage.getItem(TOKEN_EXPIRY_KEY) || '0',
        10
      );
      return Date.now() >= expiryTime;
    } catch {
      return true;
    }
  }

  private restoreTokens(): void {
    const token = this.getAuthToken();
    if (token) {
      this.apiClient.setAuthToken(token);
    }
  }
}
```

---

## 3. Auth Provider & Context

### 3.1 Auth Context Setup
```typescript
// src/core/di/providers/AuthProvider.tsx
import {
  createContext,
  PropsWithChildren,
  useEffect,
  useState,
  useCallback,
} from 'react';
import { container } from '@/core/di/container';
import { IAuthService } from '@abstractions/auth.abstraction';
import {
  User,
  LoginCredentials,
  AuthContextType,
  AuthState,
} from '@/core/types/auth.types';

export const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider = ({ children }: PropsWithChildren) => {
  const authService = container.get<IAuthService>('authService');
  const [authState, setAuthState] = useState<AuthState>({
    token: authService.getAuthToken(),
    refreshToken: null,
    expiresAt: null,
    user: null,
    isAuthenticated: false,
  });
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Initialize auth on mount
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        const token = authService.getAuthToken();
        if (token && !authService.isTokenExpired(token)) {
          const user = await authService.getCurrentUser();
          setAuthState(prev => ({
            ...prev,
            user,
            isAuthenticated: !!user,
            token,
          }));
        } else if (token) {
          // Token expired, try refresh
          try {
            const newToken = await authService.refreshAccessToken();
            const user = await authService.getCurrentUser();
            setAuthState(prev => ({
              ...prev,
              token: newToken,
              user,
              isAuthenticated: !!user,
            }));
          } catch {
            authService.clearAuthToken();
            setAuthState(prev => ({
              ...prev,
              token: null,
              user: null,
              isAuthenticated: false,
            }));
          }
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Auth init failed');
      } finally {
        setIsLoading(false);
      }
    };

    initializeAuth();
  }, [authService]);

  const login = useCallback(
    async (credentials: LoginCredentials) => {
      setIsLoading(true);
      setError(null);
      try {
        const response = await authService.login(credentials);
        setAuthState(prev => ({
          ...prev,
          token: response.accessToken,
          user: response.user,
          isAuthenticated: true,
        }));
      } catch (err) {
        const message = err instanceof Error ? err.message : 'Login failed';
        setError(message);
        throw err;
      } finally {
        setIsLoading(false);
      }
    },
    [authService]
  );

  const logout = useCallback(async () => {
    setIsLoading(true);
    try {
      await authService.logout();
      setAuthState({
        token: null,
        refreshToken: null,
        expiresAt: null,
        user: null,
        isAuthenticated: false,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Logout failed');
    } finally {
      setIsLoading(false);
    }
  }, [authService]);

  const refreshAccessToken = useCallback(async () => {
    try {
      const newToken = await authService.refreshAccessToken();
      setAuthState(prev => ({ ...prev, token: newToken }));
      return newToken;
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Token refresh failed');
      throw err;
    }
  }, [authService]);

  const value: AuthContextType = {
    ...authState,
    login,
    logout,
    refreshAccessToken,
    hasRole: (role: string) => authService.hasRole(authState.user, role),
    hasAnyRole: (...roles: string[]) =>
      authService.hasAnyRole(authState.user, ...roles),
    hasAllRoles: (...roles: string[]) =>
      authService.hasAllRoles(authState.user, ...roles),
    hasClaim: (claim: string, value?: any) =>
      authService.hasClaim(authState.user, claim, value),
    hasPermission: (permission: string) =>
      authService.hasPermission(authState.user, permission),
    hasAnyPermission: (...permissions: string[]) =>
      authService.hasAnyPermission(authState.user, ...permissions),
    isTokenExpired: () =>
      authState.token ? authService.isTokenExpired(authState.token) : true,
    isLoading,
    error,
  };

  return (
    <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
  );
};
```

---

## 4. Auth Hook

### 4.1 useAuth Hook
```typescript
// src/services/auth/useAuth.hook.ts
import { useContext } from 'react';
import { AuthContext } from '@/core/di/providers/AuthProvider';
import type { AuthContextType } from '@/core/types/auth.types';

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};
```

---

## 5. Route Guards

### 5.1 Protected Route Component
```typescript
// src/features/auth/components/ProtectedRoute.tsx
import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '@/services/auth/useAuth.hook';
import { UnauthorizedPage } from '../pages/UnauthorizedPage';

interface ProtectedRouteProps {
  children: ReactNode;
  requiredRoles?: string[];
  requiredPermissions?: string[];
  requireAll?: boolean; // If true, user needs ALL roles/permissions
}

export const ProtectedRoute = ({
  children,
  requiredRoles = [],
  requiredPermissions = [],
  requireAll = false,
}: ProtectedRouteProps) => {
  const auth = useAuth();

  // Not authenticated
  if (!auth.isAuthenticated || !auth.user) {
    return <Navigate to="/auth/login" replace />;
  }

  // Check roles
  if (requiredRoles.length > 0) {
    const hasAccess = requireAll
      ? auth.hasAllRoles(...requiredRoles)
      : auth.hasAnyRole(...requiredRoles);

    if (!hasAccess) {
      return <UnauthorizedPage />;
    }
  }

  // Check permissions
  if (requiredPermissions.length > 0) {
    const hasAccess = requireAll
      ? requiredPermissions.every(p => auth.hasPermission(p))
      : auth.hasAnyPermission(...requiredPermissions);

    if (!hasAccess) {
      return <UnauthorizedPage />;
    }
  }

  return <>{children}</>;
};
```

### 5.2 Public Route Component
```typescript
// src/features/auth/components/PublicRoute.tsx
import { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '@/services/auth/useAuth.hook';

interface PublicRouteProps {
  children: ReactNode;
  redirectIfAuthenticated?: boolean;
}

export const PublicRoute = ({
  children,
  redirectIfAuthenticated = true,
}: PublicRouteProps) => {
  const auth = useAuth();

  if (auth.isAuthenticated && redirectIfAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
};
```

---

## 6. Dynamic User Menus Based on Roles/Claims

### 6.1 Menu Configuration
```typescript
// src/config/menu.config.ts
export interface MenuItem {
  id: string;
  label: string;
  path?: string;
  icon?: string;
  children?: MenuItem[];
  requiredRoles?: string[];
  requiredPermissions?: string[];
  requiredClaim?: { claim: string; value?: any };
  divider?: boolean;
  externalLink?: boolean;
}

export const mainMenuConfig: MenuItem[] = [
  {
    id: 'dashboard',
    label: 'Dashboard',
    path: '/dashboard',
    icon: 'home',
  },
  {
    id: 'products',
    label: 'Products',
    path: '/products',
    icon: 'box',
    requiredRoles: ['user'],
  },
  {
    id: 'orders',
    label: 'Orders',
    path: '/orders',
    icon: 'shopping-cart',
    requiredPermissions: ['view:orders'],
  },
  {
    id: 'admin',
    label: 'Administration',
    icon: 'settings',
    requiredRoles: ['admin'],
    children: [
      {
        id: 'users',
        label: 'Users',
        path: '/admin/users',
        icon: 'users',
        requiredPermissions: ['manage:users'],
      },
      {
        id: 'roles',
        label: 'Roles',
        path: '/admin/roles',
        icon: 'shield',
        requiredPermissions: ['manage:roles'],
      },
      { divider: true },
      {
        id: 'settings',
        label: 'Settings',
        path: '/admin/settings',
        icon: 'sliders',
        requiredPermissions: ['manage:settings'],
      },
    ],
  },
  {
    id: 'profile-divider',
    divider: true,
  },
  {
    id: 'profile',
    label: 'Profile',
    path: '/profile',
    icon: 'user',
  },
  {
    id: 'logout',
    label: 'Logout',
    icon: 'logout',
  },
];

export const userMenuConfig: MenuItem[] = [
  {
    id: 'profile',
    label: 'My Profile',
    path: '/profile',
    icon: 'user',
  },
  {
    id: 'settings',
    label: 'Settings',
    path: '/settings',
    icon: 'settings',
  },
  {
    id: 'preferences',
    label: 'Preferences',
    path: '/preferences',
    icon: 'sliders',
  },
  { divider: true },
  {
    id: 'admin-panel',
    label: 'Admin Panel',
    path: '/admin',
    icon: 'shield-admin',
    requiredRoles: ['admin'],
  },
  { divider: true },
  {
    id: 'logout',
    label: 'Logout',
    icon: 'logout',
  },
];
```

### 6.2 Menu Filtering Hook
```typescript
// src/ui/components/navigation/useFilteredMenu.hook.ts
import { useMemo } from 'react';
import { useAuth } from '@/services/auth/useAuth.hook';
import { MenuItem } from '@/config/menu.config';

export const useFilteredMenu = (menu: MenuItem[]): MenuItem[] => {
  const auth = useAuth();

  return useMemo(() => {
    return menu.filter(item => {
      // Check roles
      if (item.requiredRoles && item.requiredRoles.length > 0) {
        if (!auth.hasAnyRole(...item.requiredRoles)) {
          return false;
        }
      }

      // Check permissions
      if (item.requiredPermissions && item.requiredPermissions.length > 0) {
        if (!auth.hasAnyPermission(...item.requiredPermissions)) {
          return false;
        }
      }

      // Check claims
      if (item.requiredClaim) {
        if (
          !auth.hasClaim(item.requiredClaim.claim, item.requiredClaim.value)
        ) {
          return false;
        }
      }

      // Recursively filter children
      if (item.children && item.children.length > 0) {
        const filteredChildren = useFilteredMenu(item.children);
        return filteredChildren.length > 0 || !item.path; // Keep parents with children
      }

      return true;
    });
  }, [auth, menu]);
};
```

### 6.3 Navigation Menu Component
```typescript
// src/ui/components/navigation/NavigationMenu.tsx
import { useAuth } from '@/services/auth/useAuth.hook';
import { useNavigate } from 'react-router-dom';
import { useFilteredMenu } from './useFilteredMenu.hook';
import { MenuItem } from '@/config/menu.config';
import './NavigationMenu.css';

interface NavigationMenuProps {
  items: MenuItem[];
  onItemClick?: (item: MenuItem) => void;
}

export const NavigationMenu = ({ items, onItemClick }: NavigationMenuProps) => {
  const auth = useAuth();
  const navigate = useNavigate();
  const filteredItems = useFilteredMenu(items);

  const handleClick = async (item: MenuItem) => {
    if (item.id === 'logout') {
      await auth.logout();
      navigate('/auth/login');
      return;
    }

    if (item.path) {
      if (item.externalLink) {
        window.open(item.path, '_blank');
      } else {
        navigate(item.path);
      }
    }

    onItemClick?.(item);
  };

  return (
    <nav className="navigation-menu">
      <ul className="menu-list">
        {filteredItems.map((item) => (
          <MenuItem
            key={item.id}
            item={item}
            onClick={handleClick}
            level={0}
          />
        ))}
      </ul>
    </nav>
  );
};

interface MenuItemProps {
  item: MenuItem;
  onClick: (item: MenuItem) => void;
  level: number;
}

const MenuItem = ({ item, onClick, level }: MenuItemProps) => {
  const filteredMenu = useFilteredMenu(item.children || []);
  const [isOpen, setIsOpen] = React.useState(false);

  if (item.divider) {
    return <li className="menu-divider" />;
  }

  const hasChildren = filteredMenu.length > 0;

  return (
    <li className={`menu-item level-${level}`}>
      <button
        className="menu-button"
        onClick={() => {
          if (hasChildren) {
            setIsOpen(!isOpen);
          } else {
            onClick(item);
          }
        }}
      >
        {item.icon && <span className={`icon icon-${item.icon}`} />}
        <span className="label">{item.label}</span>
        {hasChildren && (
          <span className={`arrow ${isOpen ? 'open' : ''}`} />
        )}
      </button>

      {hasChildren && isOpen && (
        <ul className="submenu">
          {filteredMenu.map((child) => (
            <MenuItem
              key={child.id}
              item={child}
              onClick={onClick}
              level={level + 1}
            />
          ))}
        </ul>
      )}
    </li>
  );
};
```

---

## 7. User Dropdown Menu

### 7.1 User Menu Component
```typescript
// src/ui/components/navigation/UserMenu.tsx
import { useRef, useState } from 'react';
import { useAuth } from '@/services/auth/useAuth.hook';
import { useNavigate } from 'react-router-dom';
import { userMenuConfig } from '@/config/menu.config';
import { useFilteredMenu } from './useFilteredMenu.hook';
import './UserMenu.css';

export const UserMenu = () => {
  const auth = useAuth();
  const navigate = useNavigate();
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const filteredMenu = useFilteredMenu(userMenuConfig);

  if (!auth.user) return null;

  const handleLogout = async () => {
    await auth.logout();
    navigate('/auth/login');
    setIsOpen(false);
  };

  return (
    <div className="user-menu" ref={menuRef}>
      <button
        className="user-menu-trigger"
        onClick={() => setIsOpen(!isOpen)}
        aria-label="User menu"
      >
        {auth.user.avatar && (
          <img src={auth.user.avatar} alt={auth.user.name} />
        )}
        <span className="user-name">{auth.user.name}</span>
      </button>

      {isOpen && (
        <div className="user-menu-dropdown">
          <div className="user-header">
            <div className="user-info">
              <p className="user-full-name">{auth.user.name}</p>
              <p className="user-email">{auth.user.email}</p>
              <div className="user-roles">
                {auth.user.roles.map((role) => (
                  <span key={role} className="role-badge">
                    {role}
                  </span>
                ))}
              </div>
            </div>
          </div>

          <ul className="menu-list">
            {filteredMenu.map((item) => {
              if (item.divider) {
                return <li key={item.id} className="menu-divider" />;
              }

              return (
                <li key={item.id}>
                  <button
                    className="menu-link"
                    onClick={() => {
                      if (item.id === 'logout') {
                        handleLogout();
                      } else if (item.path) {
                        navigate(item.path);
                      }
                      setIsOpen(false);
                    }}
                  >
                    {item.icon && (
                      <span className={`icon icon-${item.icon}`} />
                    )}
                    {item.label}
                  </button>
                </li>
              );
            })}
          </ul>
        </div>
      )}
    </div>
  );
};
```

---

## 8. Securing API Calls

### 8.1 API Interceptor Pattern
```typescript
// src/services/api/AuthApiInterceptor.ts
import { IApiClient } from '@abstractions/api.abstraction';

export class AuthApiInterceptor {
  constructor(
    private apiClient: IApiClient,
    private onTokenExpired?: () => Promise<void>
  ) {}

  async request<T>(
    method: 'get' | 'post' | 'put' | 'delete',
    url: string,
    data?: any
  ): Promise<T> {
    try {
      return await (this.apiClient as any)[method]<T>(url, data);
    } catch (error: any) {
      // Handle 401 Unauthorized
      if (error.status === 401) {
        if (this.onTokenExpired) {
          await this.onTokenExpired();
          // Retry request with new token
          return (this.apiClient as any)[method]<T>(url, data);
        }
      }
      throw error;
    }
  }
}
```

---

## 9. Best Practices Checklist

- [ ] Tokens stored securely (httpOnly cookies preferred)
- [ ] Auth state persisted in backend only
- [ ] RBAC enforced on server (never trust client)
- [ ] Protected routes validated both client and server
- [ ] Menu items filtered based on roles/permissions
- [ ] Unauthorized redirects to login
- [ ] Refresh token logic implemented
- [ ] Token expiry checked before API calls
- [ ] Logout clears all auth state
- [ ] User object updated after login

---

## References
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [React Authentication Patterns](https://www.freecodecamp.org/news/react-authentication/)
