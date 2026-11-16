# Feature-Based Architecture Guidelines

## Overview
Feature-based architecture organizes code around business features instead of technical layers. Each feature is self-contained, reusable, and loosely coupled from other features.

**Benefits:**
- Easy to understand and navigate
- Features can be developed independently
- Clear separation of concerns
- Easy to add/remove features
- Scales well as application grows
- Team members can own features

---

## 1. Feature Structure

### 1.1 Standard Feature Layout
```
src/features/
├── auth/                          # Auth feature
│   ├── index.ts                   # Public API exports
│   ├── auth.routes.tsx            # Route definitions
│   ├── auth.context.tsx           # Feature context (if needed)
│   │
│   ├── pages/                     # Feature pages
│   │   ├── LoginPage.tsx
│   │   ├── RegisterPage.tsx
│   │   ├── ForgotPasswordPage.tsx
│   │   └── ResetPasswordPage.tsx
│   │
│   ├── components/                # Feature components
│   │   ├── LoginForm.tsx
│   │   ├── RegisterForm.tsx
│   │   ├── PasswordInput.tsx
│   │   └── SocialLoginButtons.tsx
│   │
│   ├── hooks/                     # Feature-specific hooks
│   │   ├── useLogin.hook.ts
│   │   ├── useRegister.hook.ts
│   │   ├── usePasswordReset.hook.ts
│   │   └── useAuthForm.hook.ts
│   │
│   ├── services/                  # Feature-specific services
│   │   ├── AuthLocalService.ts   # Business logic
│   │   └── index.ts
│   │
│   ├── types/                     # Feature-specific types
│   │   └── auth.types.ts
│   │
│   ├── utils/                     # Feature utilities
│   │   ├── validators.ts
│   │   └── formatters.ts
│   │
│   ├── constants/                 # Feature constants
│   │   └── authConstants.ts
│   │
│   └── __tests__/                 # Feature tests
│       ├── LoginPage.test.tsx
│       ├── AuthService.test.ts
│       └── useLogin.hook.test.ts
│
├── dashboard/
│   ├── index.ts
│   ├── dashboard.routes.tsx
│   ├── pages/
│   │   ├── DashboardPage.tsx
│   │   └── AnalyticsPage.tsx
│   ├── components/
│   │   ├── DashboardCard.tsx
│   │   ├── ChartWidget.tsx
│   │   └── StatsSummary.tsx
│   ├── hooks/
│   │   └── useDashboardData.hook.ts
│   └── __tests__/
│
├── products/
│   ├── index.ts
│   ├── products.routes.tsx
│   ├── pages/
│   │   ├── ProductsListPage.tsx
│   │   ├── ProductDetailPage.tsx
│   │   └── ProductFormPage.tsx
│   ├── components/
│   │   ├── ProductCard.tsx
│   │   ├── ProductForm.tsx
│   │   ├── ProductFilter.tsx
│   │   └── ProductList.tsx
│   ├── hooks/
│   │   ├── useProducts.hook.ts
│   │   ├── useProductForm.hook.ts
│   │   └── useProductFilter.hook.ts
│   ├── services/
│   │   ├── ProductLocalService.ts
│   │   └── index.ts
│   ├── types/
│   │   └── product.types.ts
│   └── __tests__/
│
└── admin/                         # Admin feature (conditional)
    ├── index.ts
    ├── admin.routes.tsx
    ├── pages/
    │   ├── AdminDashboardPage.tsx
    │   ├── UsersManagementPage.tsx
    │   └── SettingsPage.tsx
    ├── components/
    └── __tests__/
```

---

## 2. Feature Index File (Public API)

### 2.1 Purpose of Index File
Exports **only** what external features need. Everything else is internal.

### 2.2 Example: Auth Feature Index
```typescript
// src/features/auth/index.ts
// ONLY export what other features use

export type { User, AuthState } from './types/auth.types';
export { useAuth } from '@services/auth/useAuth.hook';
export { ProtectedRoute } from './components/ProtectedRoute';
export { LoginPage } from './pages/LoginPage';
export { RegisterPage } from './pages/RegisterPage';
export { authRoutes } from './auth.routes';

// DON'T export internal components/hooks
// ❌ export { LoginForm } from './components/LoginForm'; // Internal!
// ❌ export { useLoginForm } from './hooks/useLoginForm.hook'; // Internal!
```

### 2.3 Example: Products Feature Index
```typescript
// src/features/products/index.ts
export type { Product, ProductFilter } from './types/product.types';
export { useProducts } from './hooks/useProducts.hook';
export { ProductCard } from './components/ProductCard';
export { ProductsListPage } from './pages/ProductsListPage';
export { ProductDetailPage } from './pages/ProductDetailPage';
export { productRoutes } from './products.routes';
```

---

## 3. Feature Routes Configuration

### 3.1 Route Definition Pattern
```typescript
// src/features/auth/auth.routes.tsx
import { lazy } from 'react';
import type { RouteObject } from 'react-router-dom';

const LoginPage = lazy(() => import('./pages/LoginPage'));
const RegisterPage = lazy(() => import('./pages/RegisterPage'));
const ForgotPasswordPage = lazy(() => import('./pages/ForgotPasswordPage'));

export const authRoutes: RouteObject[] = [
  {
    path: '/auth/login',
    element: <LoginPage />,
    meta: { title: 'Login', public: true },
  },
  {
    path: '/auth/register',
    element: <RegisterPage />,
    meta: { title: 'Register', public: true },
  },
  {
    path: '/auth/forgot-password',
    element: <ForgotPasswordPage />,
    meta: { title: 'Forgot Password', public: true },
  },
];
```

### 3.2 Products Feature Routes
```typescript
// src/features/products/products.routes.tsx
import { lazy } from 'react';
import type { RouteObject } from 'react-router-dom';

const ProductsListPage = lazy(() => import('./pages/ProductsListPage'));
const ProductDetailPage = lazy(() => import('./pages/ProductDetailPage'));
const ProductFormPage = lazy(() => import('./pages/ProductFormPage'));

export const productRoutes: RouteObject[] = [
  {
    path: '/products',
    element: <ProductsListPage />,
    meta: { title: 'Products', requiredRoles: ['user'] },
  },
  {
    path: '/products/:id',
    element: <ProductDetailPage />,
    meta: { title: 'Product Details', requiredRoles: ['user'] },
  },
  {
    path: '/products/create',
    element: <ProductFormPage />,
    meta: { title: 'Create Product', requiredRoles: ['admin'] },
  },
  {
    path: '/products/:id/edit',
    element: <ProductFormPage />,
    meta: { title: 'Edit Product', requiredRoles: ['admin'] },
  },
];
```

---

## 4. Feature Types

### 4.1 Keep Types Local to Feature
```typescript
// src/features/products/types/product.types.ts
export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  category: string;
  imageUrl?: string;
  stock: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface ProductFilter {
  search?: string;
  category?: string;
  priceMin?: number;
  priceMax?: number;
  inStock?: boolean;
  sortBy?: 'name' | 'price' | 'created';
  sortOrder?: 'asc' | 'desc';
}

export interface ProductFormData {
  name: string;
  description: string;
  price: number;
  category: string;
  stock: number;
  imageUrl?: string;
}

export interface CreateProductRequest extends ProductFormData {}

export interface UpdateProductRequest extends Partial<ProductFormData> {}
```

### 4.2 Global Types (Core Only)
```typescript
// src/core/types/index.ts
// ONLY truly shared types go here

export interface User {
  id: string;
  email: string;
  name: string;
  roles: string[];
  claims: Record<string, string | boolean>;
}

export interface PaginatedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiError {
  code: string;
  message: string;
  details?: Record<string, any>;
}
```

---

## 5. Feature-Specific Services

### 5.1 Local Service Example
```typescript
// src/features/products/services/ProductLocalService.ts
// Business logic specific to products feature

import { IApiClient } from '@abstractions/api.abstraction';
import { Product, ProductFilter, ProductFormData } from '../types/product.types';

export class ProductLocalService {
  constructor(private apiClient: IApiClient) {}

  async getProducts(filter?: ProductFilter) {
    const params = this.buildFilterParams(filter);
    return this.apiClient.get<Product[]>('/products', { params });
  }

  async getProductById(id: string) {
    return this.apiClient.get<Product>(`/products/${id}`);
  }

  async createProduct(data: ProductFormData) {
    return this.apiClient.post<Product>('/products', data);
  }

  async updateProduct(id: string, data: Partial<ProductFormData>) {
    return this.apiClient.put<Product>(`/products/${id}`, data);
  }

  async deleteProduct(id: string) {
    return this.apiClient.delete(`/products/${id}`);
  }

  private buildFilterParams(filter?: ProductFilter) {
    if (!filter) return {};
    return {
      search: filter.search,
      category: filter.category,
      priceMin: filter.priceMin,
      priceMax: filter.priceMax,
      inStock: filter.inStock,
      sortBy: filter.sortBy,
      sortOrder: filter.sortOrder,
    };
  }
}
```

### 5.2 Using Local Service in Hook
```typescript
// src/features/products/hooks/useProducts.hook.ts
import { useEffect, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { container } from '@/core/di/container';
import { IApiClient } from '@abstractions/api.abstraction';
import { ProductLocalService } from '../services/ProductLocalService';
import { Product, ProductFilter } from '../types/product.types';

export const useProducts = (filter?: ProductFilter) => {
  const apiClient = container.get<IApiClient>('apiClient');
  const productService = new ProductLocalService(apiClient);

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['products', filter],
    queryFn: () => productService.getProducts(filter),
  });

  return {
    products: data ?? [],
    isLoading,
    error,
    refetch,
  };
};
```

---

## 6. Feature Components

### 6.1 Component Organization
```typescript
// src/features/products/components/ProductCard.tsx
// Feature-specific component - ONLY used within products feature

import { Product } from '../types/product.types';
import { useNavigate } from 'react-router-dom';

interface ProductCardProps {
  product: Product;
  onDelete?: (id: string) => void;
}

export const ProductCard = ({ product, onDelete }: ProductCardProps) => {
  const navigate = useNavigate();

  return (
    <div className="product-card">
      {product.imageUrl && <img src={product.imageUrl} alt={product.name} />}
      <h3>{product.name}</h3>
      <p className="description">{product.description}</p>
      <div className="footer">
        <span className="price">${product.price}</span>
        <button onClick={() => navigate(`/products/${product.id}`)}>
          View Details
        </button>
      </div>
    </div>
  );
};
```

### 6.2 Shared UI Components vs Feature Components

**UI Components** (reusable, in `ui/` folder):
```typescript
// src/ui/components/Card.tsx
// Generic card - used by MANY features

export const Card = ({ children, variant = 'default' }) => {
  return <div className={`card card-${variant}`}>{children}</div>;
};

// Used in multiple features
// ProductCard uses Card internally
// DashboardCard uses Card internally
```

**Feature Components** (specific, in `features/` folder):
```typescript
// src/features/products/components/ProductCard.tsx
// Uses generic Card from UI, adds product-specific logic

import { Card } from '@/ui/components/Card';

export const ProductCard = ({ product }) => {
  return (
    <Card variant="elevated">
      <ProductSpecificContent product={product} />
    </Card>
  );
};
```

---

## 7. Feature Pages

### 7.1 Page Component Example
```typescript
// src/features/products/pages/ProductsListPage.tsx
import { Suspense, useState } from 'react';
import { useProducts } from '../hooks/useProducts.hook';
import { ProductFilter } from '../types/product.types';
import { ProductList } from '../components/ProductList';
import { ProductFilter as FilterComponent } from '../components/ProductFilter';
import { LoadingSpinner } from '@/ui/components/LoadingSpinner';

export default function ProductsListPage() {
  const [filter, setFilter] = useState<ProductFilter>({});
  const { products, isLoading, error } = useProducts(filter);

  if (error) {
    return <div className="error">Failed to load products</div>;
  }

  return (
    <div className="products-list-page">
      <header>
        <h1>Products</h1>
      </header>

      <aside className="filters">
        <FilterComponent 
          onFilterChange={setFilter}
          currentFilter={filter}
        />
      </aside>

      <main className="products-main">
        {isLoading ? (
          <LoadingSpinner />
        ) : (
          <Suspense fallback={<LoadingSpinner />}>
            <ProductList products={products} />
          </Suspense>
        )}
      </main>
    </div>
  );
}
```

### 7.2 Page with Form Example
```typescript
// src/features/products/pages/ProductFormPage.tsx
import { useParams, useNavigate } from 'react-router-dom';
import { useProductForm } from '../hooks/useProductForm.hook';
import { ProductForm } from '../components/ProductForm';

export default function ProductFormPage() {
  const { id } = useParams<{ id?: string }>();
  const navigate = useNavigate();
  const { 
    product, 
    isLoading, 
    handleSubmit, 
    error 
  } = useProductForm(id);

  if (error) {
    return <div className="error">Failed to load product</div>;
  }

  return (
    <div className="product-form-page">
      <header>
        <h1>{id ? 'Edit Product' : 'Create Product'}</h1>
      </header>

      <ProductForm
        initialData={product}
        isLoading={isLoading}
        onSubmit={handleSubmit}
        onCancel={() => navigate('/products')}
      />
    </div>
  );
}
```

---

## 8. Feature Hooks

### 8.1 Custom Hook Pattern
```typescript
// src/features/products/hooks/useProductForm.hook.ts
import { useEffect, useState } from 'react';
import { useNotification } from '@/services/notification/useNotification.hook';
import { container } from '@/core/di/container';
import { IApiClient } from '@abstractions/api.abstraction';
import { ProductLocalService } from '../services/ProductLocalService';
import { Product, ProductFormData } from '../types/product.types';

interface UseProductFormReturn {
  product: Product | null;
  isLoading: boolean;
  error: string | null;
  handleSubmit: (data: ProductFormData) => Promise<void>;
}

export const useProductForm = (productId?: string): UseProductFormReturn => {
  const [product, setProduct] = useState<Product | null>(null);
  const [isLoading, setIsLoading] = useState(!!productId);
  const [error, setError] = useState<string | null>(null);
  const notification = useNotification();

  const apiClient = container.get<IApiClient>('apiClient');
  const productService = new ProductLocalService(apiClient);

  // Load product if editing
  useEffect(() => {
    if (!productId) return;

    const loadProduct = async () => {
      try {
        const data = await productService.getProductById(productId);
        setProduct(data);
      } catch (err) {
        setError('Failed to load product');
        notification.error('Failed to load product');
      } finally {
        setIsLoading(false);
      }
    };

    loadProduct();
  }, [productId]);

  const handleSubmit = async (data: ProductFormData) => {
    setIsLoading(true);
    try {
      if (productId) {
        await productService.updateProduct(productId, data);
        notification.success('Product updated successfully');
      } else {
        await productService.createProduct(data);
        notification.success('Product created successfully');
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to save product';
      setError(message);
      notification.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  return { product, isLoading, error, handleSubmit };
};
```

---

## 9. Feature Context (Optional)

### 9.1 When to Use Feature Context
- Complex state shared across multiple components in a feature
- Example: Shopping cart in e-commerce, Filters state in products

### 9.2 Example: Products Context
```typescript
// src/features/products/products.context.tsx
import { createContext, PropsWithChildren, useState } from 'react';
import { ProductFilter } from './types/product.types';

interface ProductsContextType {
  filter: ProductFilter;
  setFilter: (filter: ProductFilter) => void;
  clearFilter: () => void;
  viewMode: 'grid' | 'list';
  setViewMode: (mode: 'grid' | 'list') => void;
}

export const ProductsContext = createContext<ProductsContextType | null>(null);

export const ProductsProvider = ({ children }: PropsWithChildren) => {
  const [filter, setFilter] = useState<ProductFilter>({});
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');

  const clearFilter = () => setFilter({});

  return (
    <ProductsContext.Provider 
      value={{ 
        filter, 
        setFilter, 
        clearFilter, 
        viewMode, 
        setViewMode 
      }}
    >
      {children}
    </ProductsContext.Provider>
  );
};

// Hook to use context
export const useProductsContext = () => {
  const context = React.useContext(ProductsContext);
  if (!context) {
    throw new Error('useProductsContext must be used within ProductsProvider');
  }
  return context;
};
```

### 9.3 Using Feature Context
```typescript
// src/features/products/pages/ProductsListPage.tsx
import { ProductsProvider } from '../products.context';
import { ProductsList } from '../components/ProductsList';
import { ProductsFilters } from '../components/ProductsFilters';

export default function ProductsListPage() {
  return (
    <ProductsProvider>
      <div className="products-page">
        <ProductsFilters />
        <ProductsList />
      </div>
    </ProductsProvider>
  );
}
```

---

## 10. Feature Dependencies

### 10.1 Dependency Rules
```
✅ ALLOWED:
- Feature → Core (types, abstractions, DI)
- Feature → UI (components)
- Feature → Global Services (Auth, Theme, Notification)
- Feature → React Router
- Feature → External Libraries (via abstractions)

❌ FORBIDDEN:
- Feature A → Feature B (circular dependencies)
- Feature → Other Feature's types directly
- Feature → Other Feature's services directly
- Feature → Other Feature's components directly
```

### 10.2 Cross-Feature Communication
If features need to communicate:

**Option 1: Via Core Context**
```typescript
// Use global auth context instead of auth feature context
import { useAuth } from '@/services/auth/useAuth.hook';

// In products feature:
const { user } = useAuth();
```

**Option 2: Via Event System (if needed)**
```typescript
// src/core/events/EventBus.ts
class EventBus {
  private subscribers = new Map<string, Set<Function>>();

  subscribe(event: string, callback: Function) {
    if (!this.subscribers.has(event)) {
      this.subscribers.set(event, new Set());
    }
    this.subscribers.get(event)!.add(callback);
  }

  emit(event: string, data: any) {
    this.subscribers.get(event)?.forEach(cb => cb(data));
  }
}

export const eventBus = new EventBus();

// Feature A emits
eventBus.emit('product:created', product);

// Feature B listens
eventBus.subscribe('product:created', (product) => {
  // React to product creation
});
```

---

## 11. Testing Features

### 11.1 Feature Test Organization
```typescript
// src/features/products/__tests__/ProductsListPage.test.tsx
import { render, screen } from '@testing-library/react';
import { QueryClientProvider, QueryClient } from '@tanstack/react-query';
import ProductsListPage from '../pages/ProductsListPage';

const queryClient = new QueryClient();

describe('ProductsListPage', () => {
  it('should display products list', async () => {
    render(
      <QueryClientProvider client={queryClient}>
        <ProductsListPage />
      </QueryClientProvider>
    );

    expect(screen.getByRole('heading', { name: /products/i })).toBeInTheDocument();
  });
});
```

### 11.2 Service Testing
```typescript
// src/features/products/services/__tests__/ProductLocalService.test.ts
import { ProductLocalService } from '../ProductLocalService';

class MockApiClient implements IApiClient {
  async get() {
    return [{ id: '1', name: 'Product 1' }];
  }
  // ... other methods
}

describe('ProductLocalService', () => {
  it('should fetch products', async () => {
    const apiClient = new MockApiClient();
    const service = new ProductLocalService(apiClient);

    const products = await service.getProducts();

    expect(products).toHaveLength(1);
    expect(products[0].name).toBe('Product 1');
  });
});
```

---

## 12. Scaling to Large Features

### 12.1 Large Feature Structure
```
features/admin/
├── index.ts
├── admin.routes.tsx
├── pages/
│   ├── AdminDashboardPage.tsx
│   ├── UserManagementPage.tsx
│   ├── SettingsPage.tsx
│   └── ReportsPage.tsx
├── components/
│   ├── common/              # Shared within admin
│   │   ├── AdminHeader.tsx
│   │   └── AdminSidebar.tsx
│   ├── users/               # User management section
│   │   ├── UserTable.tsx
│   │   ├── UserForm.tsx
│   │   └── UserFilter.tsx
│   ├── settings/            # Settings section
│   │   ├── GeneralSettings.tsx
│   │   └── SecuritySettings.tsx
│   └── reports/             # Reports section
│       ├── ReportChart.tsx
│       └── ExportButton.tsx
├── hooks/
│   ├── useAdminData.hook.ts
│   ├── users/
│   │   ├── useUsers.hook.ts
│   │   └── useUserForm.hook.ts
│   └── settings/
│       └── useSettings.hook.ts
├── services/
│   ├── AdminLocalService.ts
│   ├── UserService.ts
│   └── SettingsService.ts
└── __tests__/
    ├── users/
    └── settings/
```

---

## 13. Checklist for New Feature

- [ ] Create feature folder in `src/features/`
- [ ] Create index.ts with public API
- [ ] Create feature routes (feature.routes.tsx)
- [ ] Create types in types/ folder (not in core)
- [ ] Create pages in pages/ folder
- [ ] Create components in components/ folder
- [ ] Create feature-specific hooks in hooks/
- [ ] Create feature service if API calls needed
- [ ] Create __tests__ folder with tests
- [ ] Export only necessary exports in index.ts
- [ ] Add feature routes to main router
- [ ] Document feature in README

---

## 14. Best Practices Summary

| Practice | Why |
|----------|-----|
| **Public API via index.ts** | Clear contract, prevents internal access |
| **Local types per feature** | Don't pollute core types |
| **Local services** | Feature-specific business logic |
| **Feature context (optional)** | Share state within feature only |
| **No cross-feature imports** | Maintains independence |
| **Export from index.ts only** | Controls what's public |
| **Pages lazy-loaded** | Code splitting per route |
| **Tests co-located** | Easy to maintain |
| **Feature-specific hooks** | Encapsulates logic |
| **One feature per folder** | Clear organization |

---

## References
- [Feature-Driven Development](https://en.wikipedia.org/wiki/Feature-driven_development)
- [Monorepo Best Practices](https://www.monorepo.tools/)
- [React Project Structure](https://react.dev/learn/start-a-new-react-project)
