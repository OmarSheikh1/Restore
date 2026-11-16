# Library Abstraction & Adapter Pattern Guidelines

## Overview
How to abstract third-party libraries behind interfaces, implement adapters, and swap libraries without changing application code.

---

## 1. Core Philosophy

### 1.1 Problem: Library Lock-in
```
WITHOUT abstraction:
┌─────────────────────────────────┐
│ Components/Features             │
│ ├─ import toast from 'react-...'│
│ ├─ import axios from 'axios'    │
│ ├─ import { MUI } from '@mui/...'
│ └─ TIGHTLY COUPLED!             │
│    Changing libraries breaks    │
│    entire codebase!             │
└─────────────────────────────────┘
```

### 1.2 Solution: Abstraction Layer
```
WITH abstraction:
┌─────────────────────────────────┐
│ Components/Features             │
│ └─ useNotification() hook       │
│    └─ INotificationService       │
│       └─ ToastAdapter           │
│          └─ react-hot-toast     │
│                                 │
│ To swap library:                │
│ 1. Create NotistackAdapter      │
│ 2. Change one line in config    │
│ 3. Done! ✅                     │
└─────────────────────────────────┘
```

---

## 2. Three-Step Abstraction Pattern

### Step 1: Define Interface (Contract)
```typescript
// src/abstractions/notification.abstraction.ts
export interface INotificationService {
  success(message: string, options?: NotificationOptions): void;
  error(message: string, options?: NotificationOptions): void;
  warning(message: string, options?: NotificationOptions): void;
  info(message: string, options?: NotificationOptions): void;
  loading(message: string): NotificationHandle;
  dismiss(id: NotificationHandle): void;
  clear(): void;
}

export interface NotificationOptions {
  duration?: number;
  position?: 'top' | 'bottom' | 'center';
  action?: { label: string; onClick: () => void };
}

export type NotificationHandle = string | number;
```

### Step 2: Create Adapter (Implementation)
```typescript
// src/services/notification/adapters/ToastNotificationAdapter.ts
import toast from 'react-hot-toast';
import { INotificationService, NotificationOptions, NotificationHandle } from '@abstractions/notification.abstraction';

export class ToastNotificationAdapter implements INotificationService {
  success(message: string, options?: NotificationOptions): void {
    toast.success(message, {
      duration: options?.duration ?? 4000,
      position: this.mapPosition(options?.position),
    });
  }

  error(message: string, options?: NotificationOptions): void {
    toast.error(message, {
      duration: options?.duration ?? 4000,
      position: this.mapPosition(options?.position),
    });
  }

  warning(message: string, options?: NotificationOptions): void {
    toast(message, {
      duration: options?.duration ?? 4000,
      position: this.mapPosition(options?.position),
      icon: '⚠️',
    });
  }

  info(message: string, options?: NotificationOptions): void {
    toast(message, {
      duration: options?.duration ?? 4000,
      position: this.mapPosition(options?.position),
      icon: 'ℹ️',
    });
  }

  loading(message: string): NotificationHandle {
    return toast.loading(message);
  }

  dismiss(id: NotificationHandle): void {
    toast.dismiss(id);
  }

  clear(): void {
    toast.remove();
  }

  private mapPosition(
    position?: 'top' | 'bottom' | 'center'
  ): 'top-left' | 'top-right' | 'bottom-left' | 'bottom-right' {
    switch (position) {
      case 'top':
        return 'top-right';
      case 'bottom':
        return 'bottom-right';
      case 'center':
        return 'top-right';
      default:
        return 'top-right';
    }
  }
}
```

### Step 3: Use via Hook (Component-Agnostic)
```typescript
// src/services/notification/useNotification.hook.ts
import { useContext } from 'react';
import { NotificationContext } from '@/core/di/providers/NotificationProvider';
import { INotificationService } from '@abstractions/notification.abstraction';

export const useNotification = (): INotificationService => {
  const context = useContext(NotificationContext);
  if (!context) {
    throw new Error('useNotification must be used within NotificationProvider');
  }
  return context;
};

// src/features/auth/pages/LoginPage.tsx
export const LoginPage = () => {
  const notification = useNotification(); // No library knowledge here!

  const handleLogin = async (email: string, password: string) => {
    try {
      notification.loading('Logging in...');
      // ... login logic
      notification.success('Login successful!');
    } catch (error) {
      notification.error('Login failed. Please try again.');
    }
  };

  return <LoginForm onSubmit={handleLogin} />;
};
```

---

## 3. Common Library Abstractions

### 3.1 HTTP Client Abstraction

**Interface:**
```typescript
// src/abstractions/api.abstraction.ts
export interface IApiClient {
  get<T>(url: string, config?: RequestConfig): Promise<T>;
  post<T>(url: string, data?: any, config?: RequestConfig): Promise<T>;
  put<T>(url: string, data?: any, config?: RequestConfig): Promise<T>;
  patch<T>(url: string, data?: any, config?: RequestConfig): Promise<T>;
  delete<T>(url: string, config?: RequestConfig): Promise<T>;
  setAuthToken(token: string): void;
  clearAuthToken(): void;
}

export interface RequestConfig {
  headers?: Record<string, string>;
  timeout?: number;
  params?: Record<string, any>;
  onUploadProgress?: (event: ProgressEvent) => void;
}
```

**Fetch Adapter:**
```typescript
// src/services/api/adapters/FetchApiAdapter.ts
export class FetchApiAdapter implements IApiClient {
  private baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';
  private authToken: string | null = null;

  async get<T>(url: string, config?: RequestConfig): Promise<T> {
    return this.request<T>('GET', url, undefined, config);
  }

  async post<T>(url: string, data?: any, config?: RequestConfig): Promise<T> {
    return this.request<T>('POST', url, data, config);
  }

  private async request<T>(
    method: string,
    url: string,
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
      signal: AbortSignal.timeout((config?.timeout || 30) * 1000),
    });

    if (!response.ok) {
      throw new ApiError(response.status, response.statusText);
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

**Axios Adapter (Easy Swap!):**
```typescript
// src/services/api/adapters/AxiosApiAdapter.ts
import axios, { AxiosInstance, AxiosError } from 'axios';
import { IApiClient, RequestConfig } from '@abstractions/api.abstraction';

export class AxiosApiAdapter implements IApiClient {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
      timeout: 30000,
    });
  }

  async get<T>(url: string, config?: RequestConfig): Promise<T> {
    const response = await this.client.get<T>(url, {
      headers: config?.headers,
      params: config?.params,
      timeout: config?.timeout ? config.timeout * 1000 : undefined,
    });
    return response.data;
  }

  async post<T>(url: string, data?: any, config?: RequestConfig): Promise<T> {
    const response = await this.client.post<T>(url, data, {
      headers: config?.headers,
      params: config?.params,
      timeout: config?.timeout ? config.timeout * 1000 : undefined,
      onUploadProgress: config?.onUploadProgress,
    });
    return response.data;
  }

  // ... other methods

  setAuthToken(token: string): void {
    this.client.defaults.headers.common['Authorization'] = `Bearer ${token}`;
  }

  clearAuthToken(): void {
    delete this.client.defaults.headers.common['Authorization'];
  }
}
```

**Swap Implementation (One Line!):**
```typescript
// src/core/di/container.ts
// OLD:
// this.services.set('apiClient', () => new FetchApiAdapter());

// NEW:
this.services.set('apiClient', () => new AxiosApiAdapter());

// No component changes needed! ✅
```

---

### 3.2 Storage Service Abstraction

**Interface:**
```typescript
// src/abstractions/storage.abstraction.ts
export interface IStorageService {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
  clear(): void;
  subscribe(callback: StorageChangeCallback): () => void;
}

export type StorageChangeCallback = (key: string, value: string | null) => void;
```

**LocalStorage Adapter:**
```typescript
// src/services/storage/adapters/LocalStorageAdapter.ts
export class LocalStorageAdapter implements IStorageService {
  private subscribers: StorageChangeCallback[] = [];

  constructor() {
    window.addEventListener('storage', this.handleStorageChange);
  }

  getItem(key: string): string | null {
    return localStorage.getItem(key);
  }

  setItem(key: string, value: string): void {
    localStorage.setItem(key, value);
    this.notifySubscribers(key, value);
  }

  removeItem(key: string): void {
    localStorage.removeItem(key);
    this.notifySubscribers(key, null);
  }

  clear(): void {
    localStorage.clear();
  }

  subscribe(callback: StorageChangeCallback): () => void {
    this.subscribers.push(callback);
    return () => {
      this.subscribers = this.subscribers.filter(s => s !== callback);
    };
  }

  private handleStorageChange = (event: StorageEvent) => {
    this.notifySubscribers(event.key || '', event.newValue);
  };

  private notifySubscribers(key: string, value: string | null) {
    this.subscribers.forEach(cb => cb(key, value));
  }
}
```

**IndexedDB Adapter (Future Swap!):**
```typescript
// src/services/storage/adapters/IndexedDBAdapter.ts
export class IndexedDBAdapter implements IStorageService {
  private db: IDBDatabase | null = null;
  private dbName = 'AppDB';
  private storeName = 'AppStore';
  private subscribers: StorageChangeCallback[] = [];

  async init(): Promise<void> {
    return new Promise((resolve, reject) => {
      const request = indexedDB.open(this.dbName, 1);

      request.onerror = () => reject(request.error);
      request.onsuccess = () => {
        this.db = request.result;
        resolve();
      };

      request.onupgradeneeded = (event) => {
        const db = (event.target as IDBOpenDBRequest).result;
        if (!db.objectStoreNames.contains(this.storeName)) {
          db.createObjectStore(this.storeName);
        }
      };
    });
  }

  getItem(key: string): string | null {
    // Implementation with IndexedDB
    return '';
  }

  setItem(key: string, value: string): void {
    // Implementation with IndexedDB
    this.notifySubscribers(key, value);
  }

  // ... other methods

  private notifySubscribers(key: string, value: string | null) {
    this.subscribers.forEach(cb => cb(key, value));
  }
}
```

---

### 3.3 Form Library Abstraction

**Interface:**
```typescript
// src/abstractions/form.abstraction.ts
export interface IFormService {
  createForm<T>(config: FormConfig<T>): Form<T>;
}

export interface Form<T> {
  values: T;
  errors: Record<keyof T, string | undefined>;
  touched: Record<keyof T, boolean>;
  isSubmitting: boolean;
  isDirty: boolean;
  isValid: boolean;

  handleChange(e: ChangeEvent<HTMLInputElement>): void;
  handleBlur(e: FocusEvent<HTMLInputElement>): void;
  handleSubmit(cb: (values: T) => Promise<void>): (e: FormEvent) => Promise<void>;
  resetForm(): void;
  setFieldValue(field: keyof T, value: any): void;
  setFieldError(field: keyof T, error: string): void;
}

export interface FormConfig<T> {
  initialValues: T;
  validationSchema?: ValidationSchema<T>;
  onSubmit: (values: T) => Promise<void>;
}
```

**React Hook Form Adapter:**
```typescript
// src/services/form/adapters/ReactHookFormAdapter.ts
import { useForm as useRHF } from 'react-hook-form';
import { IFormService, FormConfig, Form } from '@abstractions/form.abstraction';

export class ReactHookFormAdapter implements IFormService {
  createForm<T>(config: FormConfig<T>): Form<T> {
    const {
      register,
      handleSubmit,
      formState,
      watch,
      setValue,
      reset,
    } = useRHF<T>({
      defaultValues: config.initialValues,
      resolver: config.validationSchema,
    });

    return {
      values: watch(),
      errors: formState.errors,
      touched: formState.touchedFields,
      isSubmitting: formState.isSubmitting,
      isDirty: formState.isDirty,
      isValid: formState.isValid,

      handleChange: (e) => {
        // RHF handles this automatically
      },

      handleBlur: (e) => {
        // RHF handles this automatically
      },

      handleSubmit: (cb) => handleSubmit(cb),
      resetForm: () => reset(),
      setFieldValue: (field, value) => setValue(field, value),
      setFieldError: (field, error) => {
        // Implementation
      },
    };
  }
}
```

**Formik Adapter (Easy Swap!):**
```typescript
// src/services/form/adapters/FormikAdapter.ts
import { useFormik as useFormikLib } from 'formik';
import { IFormService, FormConfig } from '@abstractions/form.abstraction';

export class FormikAdapter implements IFormService {
  createForm<T>(config: FormConfig<T>): Form<T> {
    const formik = useFormikLib({
      initialValues: config.initialValues,
      onSubmit: config.onSubmit,
      validationSchema: config.validationSchema,
    });

    return {
      values: formik.values,
      errors: formik.errors,
      touched: formik.touched,
      isSubmitting: formik.isSubmitting,
      isDirty: formik.dirty,
      isValid: formik.isValid,

      handleChange: formik.handleChange,
      handleBlur: formik.handleBlur,
      handleSubmit: formik.handleSubmit,
      resetForm: formik.resetForm,
      setFieldValue: formik.setFieldValue,
      setFieldError: formik.setFieldError,
    };
  }
}
```

---

## 4. Registering Adapters

### 4.1 Dependency Injection Container
```typescript
// src/core/di/container.ts
import { FetchApiAdapter } from '@services/api/adapters/FetchApiAdapter';
import { LocalStorageAdapter } from '@services/storage/adapters/LocalStorageAdapter';
import { ToastNotificationAdapter } from '@services/notification/adapters/ToastNotificationAdapter';

export class ServiceContainer {
  private services = new Map<string, any>();

  constructor() {
    this.registerDefaultServices();
  }

  private registerDefaultServices(): void {
    // API Client
    this.services.set('apiClient', () => new FetchApiAdapter());

    // Storage
    this.services.set('storageService', () => new LocalStorageAdapter());

    // Notification
    this.services.set('notificationService', () => new ToastNotificationAdapter());

    // Add more services as needed
  }

  get<T>(key: string): T {
    const factory = this.services.get(key);
    if (!factory) {
      throw new Error(`Service '${key}' not registered`);
    }
    return factory() as T;
  }

  register<T>(key: string, factory: () => T): void {
    this.services.set(key, factory);
  }

  // Swap implementation for testing or in config
  swap<T>(key: string, factory: () => T): void {
    this.services.set(key, factory);
  }
}

export const container = new ServiceContainer();

// Usage in providers
const apiClient = container.get<IApiClient>('apiClient');
const storage = container.get<IStorageService>('storageService');
const notification = container.get<INotificationService>('notificationService');
```

---

## 5. Testing with Mocks

### 5.1 Mock Implementation
```typescript
// src/services/api/__tests__/MockApiAdapter.ts
import { IApiClient, RequestConfig } from '@abstractions/api.abstraction';

export class MockApiAdapter implements IApiClient {
  private mockData: Record<string, any> = {
    '/auth/login': { token: 'test-token', user: { id: '1' } },
    '/products': [{ id: '1', name: 'Product 1' }],
  };

  async get<T>(url: string): Promise<T> {
    return this.mockData[url] || {};
  }

  async post<T>(url: string, data?: any): Promise<T> {
    return this.mockData[url] || {};
  }

  async put<T>(url: string, data?: any): Promise<T> {
    return data as T;
  }

  async patch<T>(url: string, data?: any): Promise<T> {
    return data as T;
  }

  async delete<T>(url: string): Promise<T> {
    return {} as T;
  }

  setAuthToken(token: string): void {}
  clearAuthToken(): void {}
}

// Usage in tests
describe('LoginService', () => {
  it('should login', async () => {
    const mockApi = new MockApiAdapter();
    const authService = new AuthService(mockApi, new MockStorageAdapter());

    const result = await authService.login({
      email: 'test@example.com',
      password: 'password',
    });

    expect(result.token).toBe('test-token');
  });
});
```

### 5.2 Test Container Setup
```typescript
// src/__tests__/testContainer.ts
import { ServiceContainer } from '@/core/di/container';
import { MockApiAdapter } from '@services/api/__tests__/MockApiAdapter';
import { MockStorageAdapter } from '@services/storage/__tests__/MockStorageAdapter';

export const createTestContainer = (): ServiceContainer => {
  const container = new ServiceContainer();

  // Swap with mocks
  container.swap('apiClient', () => new MockApiAdapter());
  container.swap('storageService', () => new MockStorageAdapter());

  return container;
};

// Usage in tests
const testContainer = createTestContainer();
const authService = testContainer.get('authService');
```

---

## 6. Migration Checklist

When migrating from one library to another:

- [ ] Interface already defined
- [ ] New adapter created (implements interface)
- [ ] All methods implemented
- [ ] Tests updated with mock
- [ ] Update container registration (one line)
- [ ] No component changes needed
- [ ] Test with new adapter
- [ ] Remove old adapter (optional)
- [ ] Update dependencies

---

## 7. Best Practices

| Practice | Why |
|----------|-----|
| **Interface first** | Defines contract before implementation |
| **One adapter per library** | Clear separation |
| **Service container registration** | Single source of truth |
| **Mock adapters for tests** | Testable without dependencies |
| **No library imports in components** | Prevents lock-in |
| **Adapter wraps library completely** | Abstraction complete |
| **Use custom hooks for access** | Components unaware of implementation |
| **Version-agnostic interfaces** | Library version changes don't affect app |

---

## 8. Common Migration Paths

### HTTP Client: Fetch → Axios
- FetchApiAdapter → AxiosApiAdapter
- Change one line in container
- Done! ✅

### Storage: LocalStorage → IndexedDB
- LocalStorageAdapter → IndexedDBAdapter
- Change one line in container
- Done! ✅

### Notification: react-hot-toast → Notistack
- ToastNotificationAdapter → NotistackNotificationAdapter
- Change one line in container
- Done! ✅

### Form: React Hook Form → Formik
- ReactHookFormAdapter → FormikAdapter
- Change one line in container
- Done! ✅

---

## References
- [Adapter Pattern](https://refactoring.guru/design-patterns/adapter)
- [Dependency Inversion Principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle)
- [React Context API](https://react.dev/reference/react/useContext)
- [Service Locator Pattern](https://martinfowler.com/articles/injection.html#ServiceLocator)
