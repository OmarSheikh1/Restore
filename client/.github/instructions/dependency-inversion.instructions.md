# Dependency Inversion Principle (DIP) Guidelines

## Overview
**Goal**: Depend on abstractions, not concrete implementations. This allows seamless library swaps and easier testing.

**Principle**: High-level modules should not depend on low-level modules. Both should depend on abstractions.

---

## 1. Core Pattern: Interface → Implementation → Usage

### ✅ Correct Pattern
```
┌─────────────────────────────────────────┐
│  Component (high-level)                 │
│  └─ useAuth() hook (abstraction)       │
│     └─ Context (abstraction)            │
│        └─ AuthService (implementation)  │
│           └─ Backend API (detail)       │
└─────────────────────────────────────────┘
```

### ❌ Wrong Pattern
```
Component → Direct API calls (tightly coupled)
Component → Direct library usage (library lock-in)
Component → Concrete service class (no abstraction)
```

---

## 2. Three-Layer Abstraction Model

### Layer 1: Define Interface (Contract)
```typescript
// src/abstractions/auth.abstraction.ts
export interface IAuthService {
  login(email: string, password: string): Promise<AuthState>;
  logout(): Promise<void>;
  refreshToken(): Promise<string>;
  getCurrentUser(): Promise<User | null>;
  hasRole(role: string): boolean;
  hasClaim(claim: string, value?: any): boolean;
  hasPermission(permission: string): boolean;
}

export interface AuthState {
  token: string | null;
  refreshToken: string | null;
  user: User | null;
}
```

### Layer 2: Implement Service
```typescript
// src/services/auth/AuthService.ts
import { IAuthService, AuthState } from '@abstractions/auth.abstraction';
import { IApiClient } from '@abstractions/api.abstraction';

export class AuthService implements IAuthService {
  constructor(private apiClient: IApiClient) {}

  async login(email: string, password: string): Promise<AuthState> {
    const response = await this.apiClient.post('/auth/login', { email, password });
    return response;
  }

  hasRole(role: string): boolean {
    return this.currentUser?.roles.includes(role) ?? false;
  }

  // ... other methods
}
```

### Layer 3: Use via Hook (Component-Agnostic)
```typescript
// src/services/auth/useAuth.hook.ts
import { useContext } from 'react';
import { AuthContext } from '@/core/di/providers/AuthProvider';
import type { IAuthService } from '@abstractions/auth.abstraction';

export const useAuth = (): IAuthService & { user: User | null } => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};

// src/features/auth/pages/LoginPage.tsx
export const LoginPage = () => {
  const auth = useAuth(); // Depends on interface, not implementation
  
  const handleSubmit = async (email: string, password: string) => {
    await auth.login(email, password); // Works with any IAuthService
  };

  return <LoginForm onSubmit={handleSubmit} />;
};
```

---

## 3. Dependency Injection Container

### Register All Services in One Place
```typescript
// src/core/di/container.ts
import { AuthService } from '@services/auth/AuthService';
import { IApiClient } from '@abstractions/api.abstraction';

export class ServiceContainer {
  private services = new Map<string, any>();
  
  constructor() {
    this.registerDefaultServices();
  }

  private registerDefaultServices(): void {
    // Register API client (can be swapped)
    this.services.set('apiClient', () => new FetchApiAdapter());
    
    // Register Auth service
    this.services.set('authService', () => 
      new AuthService(this.get<IApiClient>('apiClient'))
    );
    
    // Register other services
    this.services.set('themeService', () => new ThemeService());
    this.services.set('notificationService', () => new NotificationService());
  }

  get<T>(key: string): T {
    const factory = this.services.get(key);
    if (!factory) throw new Error(`Service '${key}' not registered`);
    return factory() as T;
  }

  register<T>(key: string, factory: () => T): void {
    this.services.set(key, factory);
  }
}

export const container = new ServiceContainer();
```

### Usage in Providers
```typescript
// src/core/di/providers/AuthProvider.tsx
import { container } from '@/core/di/container';
import { IAuthService } from '@abstractions/auth.abstraction';

export const AuthProvider = ({ children }: PropsWithChildren) => {
  const authService = container.get<IAuthService>('authService');
  // ... use authService

  return (
    <AuthContext.Provider value={authService}>
      {children}
    </AuthContext.Provider>
  );
};
```

---

## 4. API Client Abstraction Pattern

### Define API Interface
```typescript
// src/abstractions/api.abstraction.ts
export interface IApiClient {
  get<T>(url: string, config?: RequestConfig): Promise<T>;
  post<T>(url: string, data?: any, config?: RequestConfig): Promise<T>;
  put<T>(url: string, data?: any, config?: RequestConfig): Promise<T>;
  delete<T>(url: string, config?: RequestConfig): Promise<T>;
  setAuthToken(token: string): void;
  clearAuthToken(): void;
}

export interface RequestConfig {
  headers?: Record<string, string>;
  timeout?: number;
  retry?: boolean;
}
```

### Fetch Adapter Implementation
```typescript
// src/services/api/adapters/FetchApiAdapter.ts
import { IApiClient, RequestConfig } from '@abstractions/api.abstraction';

export class FetchApiAdapter implements IApiClient {
  private authToken: string | null = null;
  private baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

  async get<T>(url: string, config?: RequestConfig): Promise<T> {
    return this.request<T>(url, 'GET', undefined, config);
  }

  async post<T>(url: string, data?: any, config?: RequestConfig): Promise<T> {
    return this.request<T>(url, 'POST', data, config);
  }

  private async request<T>(
    url: string,
    method: string,
    body?: any,
    config?: RequestConfig
  ): Promise<T> {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...config?.headers,
    };

    if (this.authToken) {
      headers['Authorization'] = `Bearer ${this.authToken}`;
    }

    const response = await fetch(`${this.baseUrl}${url}`, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined,
    });

    if (!response.ok) {
      throw new HttpError(response.status, response.statusText);
    }

    return response.json();
  }

  setAuthToken(token: string): void {
    this.authToken = token;
  }

  clearAuthToken(): void {
    this.authToken = null;
  }
}
```

### Axios Adapter (Easily Swap!)
```typescript
// src/services/api/adapters/AxiosApiAdapter.ts
import axios, { AxiosInstance } from 'axios';
import { IApiClient, RequestConfig } from '@abstractions/api.abstraction';

export class AxiosApiAdapter implements IApiClient {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
    });
  }

  async get<T>(url: string, config?: RequestConfig): Promise<T> {
    const response = await this.client.get<T>(url, { headers: config?.headers });
    return response.data;
  }

  async post<T>(url: string, data?: any, config?: RequestConfig): Promise<T> {
    const response = await this.client.post<T>(url, data, { headers: config?.headers });
    return response.data;
  }

  setAuthToken(token: string): void {
    this.client.defaults.headers.common['Authorization'] = `Bearer ${token}`;
  }

  clearAuthToken(): void {
    delete this.client.defaults.headers.common['Authorization'];
  }
}
```

### Swap Implementation (Only One Line Changes!)
```typescript
// src/core/di/container.ts
// OLD:
// this.services.set('apiClient', () => new FetchApiAdapter());

// NEW:
this.services.set('apiClient', () => new AxiosApiAdapter());

// No component changes needed! ✅
```

---

## 5. Storage Service Abstraction

### Define Interface
```typescript
// src/abstractions/storage.abstraction.ts
export interface IStorageService {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
  clear(): void;
  subscribe(callback: (key: string, value: string | null) => void): () => void;
}
```

### Local Storage Adapter
```typescript
// src/services/storage/adapters/LocalStorageAdapter.ts
export class LocalStorageAdapter implements IStorageService {
  getItem(key: string): string | null {
    return localStorage.getItem(key);
  }

  setItem(key: string, value: string): void {
    localStorage.setItem(key, value);
  }

  removeItem(key: string): void {
    localStorage.removeItem(key);
  }

  clear(): void {
    localStorage.clear();
  }

  subscribe(callback: (key: string, value: string | null) => void): () => void {
    const handler = (event: StorageEvent) => {
      callback(event.key || '', event.newValue);
    };
    window.addEventListener('storage', handler);
    return () => window.removeEventListener('storage', handler);
  }
}
```

### IndexedDB Adapter (Future Swap!)
```typescript
// src/services/storage/adapters/IndexedDBAdapter.ts
export class IndexedDBAdapter implements IStorageService {
  private db: IDBDatabase | null = null;

  async init(): Promise<void> {
    // Initialize IndexedDB
  }

  getItem(key: string): string | null {
    // Get from IndexedDB
    return '';
  }

  // ... other methods implement IStorageService
}
```

---

## 6. Notification Service Abstraction

### Define Interface
```typescript
// src/abstractions/notification.abstraction.ts
export interface INotificationService {
  success(message: string, duration?: number): void;
  error(message: string, duration?: number): void;
  warning(message: string, duration?: number): void;
  info(message: string, duration?: number): void;
  loading(message: string): () => void; // Returns dismiss function
}
```

### React Hot Toast Adapter
```typescript
// src/services/notification/adapters/ToastNotificationAdapter.ts
import toast from 'react-hot-toast';

export class ToastNotificationAdapter implements INotificationService {
  success(message: string, duration = 3000): void {
    toast.success(message, { duration });
  }

  error(message: string, duration = 3000): void {
    toast.error(message, { duration });
  }

  warning(message: string, duration = 3000): void {
    toast(message, { duration }); // No warning variant, use default
  }

  info(message: string, duration = 3000): void {
    toast(message, { duration });
  }

  loading(message: string): () => void {
    const toastId = toast.loading(message);
    return () => toast.dismiss(toastId);
  }
}
```

### Notistack Adapter (Easy Swap!)
```typescript
// src/services/notification/adapters/NotistackNotificationAdapter.ts
import { useSnackbar } from 'notistack';

export class NotistackNotificationAdapter implements INotificationService {
  constructor(private snackbar = useSnackbar()) {}

  success(message: string, duration = 3000): void {
    this.snackbar.enqueueSnackbar(message, { 
      variant: 'success',
      autoHideDuration: duration,
    });
  }

  error(message: string, duration = 3000): void {
    this.snackbar.enqueueSnackbar(message, { 
      variant: 'error',
      autoHideDuration: duration,
    });
  }

  // ... other methods
}
```

---

## 7. Testing with Dependency Inversion

### Mock Implementation for Tests
```typescript
// src/services/auth/__tests__/AuthService.test.ts
import { AuthService } from '../AuthService';
import { IApiClient } from '@abstractions/api.abstraction';

// Mock implementation - only implements interface
class MockApiClient implements IApiClient {
  async get<T>(): Promise<T> { return {} as T; }
  async post<T>(): Promise<T> { return {} as T; }
  async put<T>(): Promise<T> { return {} as T; }
  async delete<T>(): Promise<T> { return {} as T; }
  setAuthToken(): void {}
  clearAuthToken(): void {}
}

describe('AuthService', () => {
  it('should login successfully', async () => {
    const mockApi = new MockApiClient();
    const authService = new AuthService(mockApi);
    
    // Test authService
    const result = await authService.login('test@example.com', 'password');
    expect(result).toBeDefined();
  });
});
```

### Component Tests with Mocked Context
```typescript
// src/features/auth/__tests__/LoginPage.test.tsx
import { render, screen } from '@testing-library/react';
import { LoginPage } from '../pages/LoginPage';

// Mock the hook
jest.mock('@services/auth/useAuth.hook', () => ({
  useAuth: () => ({
    login: jest.fn().mockResolvedValue({}),
    isLoading: false,
  }),
}));

describe('LoginPage', () => {
  it('should render login form', () => {
    render(<LoginPage />);
    expect(screen.getByRole('button', { name: /login/i })).toBeInTheDocument();
  });
});
```

---

## 8. Key Rules

| Rule | Why | Example |
|------|-----|---------|
| **Components depend on hooks, not services** | Hooks are abstraction layer | `useAuth()` not `new AuthService()` |
| **Hooks depend on context, not services** | Context provides DI | `useContext(AuthContext)` |
| **Services implement interfaces** | Interface = contract | `class AuthService implements IAuthService` |
| **Register in container once** | Single source of truth | `container.register('authService', ...)` |
| **No direct library imports in components** | Prevents lock-in | Don't: `import toast from 'react-hot-toast'` |
| **Adapters wrap libraries** | Enables swapping | `ToastNotificationAdapter` wraps `react-hot-toast` |
| **Mock implementations for tests** | Easy to test | `MockApiClient implements IApiClient` |
| **Pass abstractions to constructors** | Constructor injection | `constructor(private api: IApiClient)` |

---

## 9. Checklist for New Features

When adding a new service/feature:

- [ ] Create interface in `src/abstractions/`
  ```typescript
  export interface IYourService { /* methods */ }
  ```

- [ ] Create implementation in `src/services/your-service/`
  ```typescript
  export class YourService implements IYourService { /* implementation */ }
  ```

- [ ] Create custom hook in same directory
  ```typescript
  export const useYourService = () => useContext(YourServiceContext);
  ```

- [ ] Register in `src/core/di/container.ts`
  ```typescript
  this.services.set('yourService', () => new YourService());
  ```

- [ ] Create provider in `src/core/di/providers/`
  ```typescript
  export const YourServiceProvider = ({ children }) => (
    <YourServiceContext.Provider value={service}>
      {children}
    </YourServiceContext.Provider>
  );
  ```

- [ ] Add to `RootProvider.tsx`
  ```typescript
  <YourServiceProvider>
    {/* other providers */}
  </YourServiceProvider>
  ```

- [ ] Use only via hook in components
  ```typescript
  const service = useYourService();
  ```

---

## 10. Migration Example: Swapping HTTP Client

**Step 1: Current Setup (Fetch)**
```typescript
// src/core/di/container.ts
this.services.set('apiClient', () => new FetchApiAdapter());
```

**Step 2: Create Axios Adapter**
```typescript
// src/services/api/adapters/AxiosApiAdapter.ts
export class AxiosApiAdapter implements IApiClient { /* ... */ }
```

**Step 3: Swap Registration**
```typescript
// src/core/di/container.ts
this.services.set('apiClient', () => new AxiosApiAdapter());
```

**Step 4: Done! ✅**
- No component changes
- No service changes
- No hook changes
- All tests pass (with proper mocks)

---

## References
- [Dependency Inversion Principle (SOLID)](https://en.wikipedia.org/wiki/Dependency_inversion_principle)
- [React Context API](https://react.dev/reference/react/useContext)
- [Adapter Pattern](https://refactoring.guru/design-patterns/adapter)
- [Custom Hooks](https://react.dev/learn/reusing-logic-with-custom-hooks)
