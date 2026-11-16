# Lazy Loading & Code Splitting Guidelines

## Overview
Guide for implementing route-based code splitting, component lazy loading, and on-demand resource loading to optimize bundle size and initial load time.

---

## 1. Route-Based Code Splitting

### 1.1 Lazy Route Configuration
```typescript
// src/config/routes.config.ts
import { lazy, ReactNode } from 'react';
import type { RouteObject } from 'react-router-dom';

// Lazy load route components
const AuthModule = lazy(() =>
  import('../features/auth').then(m => ({ default: m.LoginPage }))
);
const DashboardModule = lazy(() =>
  import('../features/dashboard/pages/DashboardPage')
);
const ProductsModule = lazy(() =>
  import('../features/products/pages/ProductsListPage')
);
const AdminModule = lazy(() =>
  import('../features/admin/pages/AdminDashboardPage')
);

export interface RouteConfig extends RouteObject {
  meta?: {
    title?: string;
    description?: string;
    requiredRoles?: string[];
    requiredPermissions?: string[];
    public?: boolean;
    preload?: boolean; // Preload bundle on route discovery
  };
}

export const routes: RouteConfig[] = [
  {
    path: '/',
    element: lazy(() => import('../ui/layouts/MainLayout')),
    children: [
      {
        index: true,
        element: lazy(() => import('../features/dashboard/pages/DashboardPage')),
        meta: {
          title: 'Dashboard',
          requiredRoles: ['user'],
        },
      },
      {
        path: 'products',
        element: lazy(() => import('../features/products/pages/ProductsListPage')),
        meta: {
          title: 'Products',
          requiredRoles: ['user'],
        },
      },
      {
        path: 'products/:id',
        element: lazy(() => import('../features/products/pages/ProductDetailPage')),
        meta: {
          title: 'Product Details',
          requiredRoles: ['user'],
        },
      },
      {
        path: 'admin',
        element: lazy(() => import('../features/admin/pages/AdminDashboardPage')),
        meta: {
          title: 'Administration',
          requiredRoles: ['admin'],
          preload: true, // Preload for admins
        },
        children: [
          {
            path: 'users',
            element: lazy(() => import('../features/admin/pages/UsersPage')),
          },
          {
            path: 'settings',
            element: lazy(() => import('../features/admin/pages/SettingsPage')),
          },
        ],
      },
    ],
  },
  {
    path: '/auth',
    element: lazy(() => import('../ui/layouts/AuthLayout')),
    children: [
      {
        path: 'login',
        element: lazy(() => import('../features/auth/pages/LoginPage')),
        meta: {
          title: 'Login',
          public: true,
        },
      },
      {
        path: 'register',
        element: lazy(() => import('../features/auth/pages/RegisterPage')),
        meta: {
          title: 'Register',
          public: true,
        },
      },
    ],
  },
  {
    path: '*',
    element: lazy(() => import('../pages/NotFoundPage')),
    meta: {
      title: '404 Not Found',
      public: true,
    },
  },
];
```

### 1.2 Route Loading Component
```typescript
// src/App.tsx
import { Suspense } from 'react';
import { RouterProvider, createBrowserRouter } from 'react-router-dom';
import { RootProvider } from '@/core/di/providers/RootProvider';
import { routes } from '@/config/routes.config';
import { LoadingSpinner } from '@/ui/components/LoadingSpinner';
import { ErrorBoundary } from '@/ui/components/ErrorBoundary';

const router = createBrowserRouter(routes, {
  future: { v7_normalizeFormMethod: true },
});

export const App = () => {
  return (
    <ErrorBoundary>
      <RootProvider>
        <Suspense fallback={<LoadingSpinner fullScreen />}>
          <RouterProvider router={router} />
        </Suspense>
      </RootProvider>
    </ErrorBoundary>
  );
};
```

---

## 2. Component-Level Lazy Loading

### 2.1 Lazy Load Heavy Components
```typescript
// src/features/dashboard/pages/DashboardPage.tsx
import { lazy, Suspense } from 'react';
import { LoadingSpinner } from '@/ui/components/LoadingSpinner';

// Lazy load heavy components
const AnalyticsChart = lazy(() =>
  import('../components/AnalyticsChart').then(m => ({ default: m.AnalyticsChart }))
);

const ReportGenerator = lazy(() =>
  import('../components/ReportGenerator').then(m => ({
    default: m.ReportGenerator,
  }))
);

export default function DashboardPage() {
  return (
    <div className="dashboard-page">
      <h1>Dashboard</h1>

      {/* Quick stats - loaded immediately */}
      <div className="stats-section">
        <StatCard title="Total Orders" value="1,234" />
        <StatCard title="Revenue" value="$45,234" />
      </div>

      {/* Heavy components - lazy loaded */}
      <Suspense fallback={<LoadingSpinner />}>
        <AnalyticsChart />
      </Suspense>

      <Suspense fallback={<LoadingSpinner />}>
        <ReportGenerator />
      </Suspense>
    </div>
  );
}
```

### 2.2 Modal Lazy Loading
```typescript
// src/ui/components/Modal.tsx
import { lazy, Suspense, useState } from 'react';

interface LazyModalProps<T> {
  isOpen: boolean;
  onClose: () => void;
  component: React.LazyExoticComponent<(props: T) => JSX.Element>;
  componentProps?: T;
}

export const LazyModal = <T extends Record<string, any>>({
  isOpen,
  onClose,
  component: Component,
  componentProps,
}: LazyModalProps<T>) => {
  if (!isOpen) return null;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={e => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>×</button>
        <Suspense fallback={<div>Loading...</div>}>
          <Component {...(componentProps || ({} as T))} />
        </Suspense>
      </div>
    </div>
  );
};

// Usage
const EditProductModal = lazy(() =>
  import('../features/products/components/EditProductModal').then(m => ({
    default: m.EditProductModal,
  }))
);

export const ProductDetailPage = () => {
  const [isEditOpen, setIsEditOpen] = useState(false);
  const { productId } = useParams();

  return (
    <>
      <button onClick={() => setIsEditOpen(true)}>Edit</button>

      <LazyModal
        isOpen={isEditOpen}
        onClose={() => setIsEditOpen(false)}
        component={EditProductModal}
        componentProps={{ productId }}
      />
    </>
  );
};
```

---

## 3. Preloading Strategies

### 3.1 Route Preloading
```typescript
// src/utils/preload.utils.ts
export const preloadRoute = (routePath: string) => {
  // Create a prefetch link
  const link = document.createElement('link');
  link.rel = 'prefetch';
  link.href = `/dist/${routePath}.js`;
  document.head.appendChild(link);
};

export const preloadComponent = (
  componentLoader: () => Promise<{ default: React.ComponentType<any> }>
) => {
  componentLoader().catch(err => {
    console.error('Preload failed:', err);
  });
};

// Usage
preloadRoute('dashboard');
preloadComponent(
  () => import('../features/admin/pages/AdminDashboardPage')
);
```

### 3.2 Preload on Route Discovery
```typescript
// src/hooks/useRoutePreload.hook.ts
import { useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { routes } from '@/config/routes.config';

export const useRoutePreload = () => {
  const location = useLocation();

  useEffect(() => {
    // Find current route
    const currentRoute = routes.find(r => r.path === location.pathname);

    // Preload if configured
    if (currentRoute?.meta?.preload) {
      // The component is already imported in lazy()
      // This hook can be enhanced with additional preloading logic
    }

    // Preload likely next routes
    const nextRoutes = routes.slice(0, 2); // Example: preload first 2 routes
    nextRoutes.forEach(route => {
      if (route.meta?.preload) {
        // Schedule preload
        setTimeout(() => {
          // Preload logic
        }, 2000);
      }
    });
  }, [location]);
};
```

### 3.3 Intersection Observer Preload
```typescript
// src/hooks/useIntersectionPreload.hook.ts
import { useEffect, useRef } from 'react';

interface PreloadConfig {
  component: () => Promise<any>;
  rootMargin?: string;
}

export const useIntersectionPreload = (
  config: PreloadConfig,
  dependencies: any[] = []
) => {
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting) {
          // Component is about to enter viewport, preload it
          config.component().catch(err =>
            console.error('Preload failed:', err)
          );
          observer.disconnect();
        }
      },
      {
        rootMargin: config.rootMargin || '50px',
      }
    );

    if (ref.current) {
      observer.observe(ref.current);
    }

    return () => observer.disconnect();
  }, dependencies);

  return ref;
};

// Usage
const HeavyComponent = lazy(() =>
  import('../components/HeavyReport')
);

export const Dashboard = () => {
  const ref = useIntersectionPreload({
    component: () => import('../components/HeavyReport'),
    rootMargin: '100px',
  });

  return (
    <>
      <div>Visible content</div>
      <div ref={ref}>
        <Suspense fallback={<Spinner />}>
          <HeavyComponent />
        </Suspense>
      </div>
    </>
  );
};
```

---

## 4. Data Lazy Loading

### 4.1 Infinite Scroll with Lazy Loading
```typescript
// src/hooks/useInfiniteScroll.hook.ts
import { useInfiniteQuery } from '@tanstack/react-query';
import { useEffect, useRef } from 'react';

interface UseInfiniteScrollOptions<T> {
  queryKey: string[];
  queryFn: (pageParam: number) => Promise<{
    items: T[];
    nextPage?: number;
  }>;
}

export const useInfiniteScroll = <T,>({
  queryKey,
  queryFn,
}: UseInfiniteScrollOptions<T>) => {
  const ref = useRef<HTMLDivElement>(null);

  const { data, fetchNextPage, hasNextPage, isFetching } = useInfiniteQuery({
    queryKey,
    queryFn: ({ pageParam = 1 }) => queryFn(pageParam),
    getNextPageParam: (lastPage) => lastPage.nextPage,
    initialPageParam: 1,
  });

  useEffect(() => {
    const observer = new IntersectionObserver((entries) => {
      if (entries[0].isIntersecting && hasNextPage && !isFetching) {
        fetchNextPage();
      }
    });

    if (ref.current) observer.observe(ref.current);
    return () => observer.disconnect();
  }, [fetchNextPage, hasNextPage, isFetching]);

  const items = data?.pages.flatMap(page => page.items) ?? [];

  return { items, ref, isFetching, hasNextPage };
};

// Usage
export const ProductsList = () => {
  const { items, ref, isFetching } = useInfiniteScroll({
    queryKey: ['products'],
    queryFn: async (page) => {
      const response = await fetch(`/api/products?page=${page}`);
      return response.json();
    },
  });

  return (
    <>
      {items.map(product => (
        <ProductCard key={product.id} product={product} />
      ))}
      <div ref={ref}>{isFetching && <Spinner />}</div>
    </>
  );
};
```

### 4.2 Virtual List for Large Data Sets
```typescript
// src/hooks/useVirtualList.hook.ts
import { useEffect, useRef, useState } from 'react';

interface VirtualListOptions {
  itemHeight: number;
  containerHeight: number;
  items: any[];
}

export const useVirtualList = ({
  itemHeight,
  containerHeight,
  items,
}: VirtualListOptions) => {
  const ref = useRef<HTMLDivElement>(null);
  const [scrollOffset, setScrollOffset] = useState(0);

  useEffect(() => {
    const handleScroll = () => {
      if (ref.current) {
        setScrollOffset(ref.current.scrollTop);
      }
    };

    ref.current?.addEventListener('scroll', handleScroll);
    return () => ref.current?.removeEventListener('scroll', handleScroll);
  }, []);

  const visibleRange = Math.ceil(containerHeight / itemHeight);
  const startIndex = Math.floor(scrollOffset / itemHeight);
  const endIndex = Math.min(startIndex + visibleRange + 1, items.length);

  const visibleItems = items.slice(startIndex, endIndex);
  const offsetY = startIndex * itemHeight;

  return {
    ref,
    visibleItems,
    offsetY,
    totalHeight: items.length * itemHeight,
  };
};

// Usage with react-window library
import { FixedSizeList } from 'react-window';

export const VirtualProductsList = ({ products }: { products: Product[] }) => {
  return (
    <FixedSizeList
      height={600}
      itemCount={products.length}
      itemSize={100}
      width="100%"
    >
      {({ index, style }) => (
        <div style={style}>
          <ProductCard product={products[index]} />
        </div>
      )}
    </FixedSizeList>
  );
};
```

---

## 5. Bundle Analysis

### 5.1 Analyze Bundle Size
```bash
# Install bundle analyzer
npm install --save-dev vite-plugin-visualizer

# In vite.config.ts
import { visualizer } from 'vite-plugin-visualizer';

export default {
  plugins: [
    visualizer({
      open: true,
      gzipSize: true,
      brotliSize: true,
    }),
  ],
};

# Run build
npm run build
```

### 5.2 Check Chunk Sizes
```typescript
// vite.config.ts
export default {
  build: {
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('node_modules')) {
            if (id.includes('react')) return 'vendor-react';
            if (id.includes('react-router')) return 'vendor-router';
            return 'vendor';
          }

          if (id.includes('features/auth')) return 'chunk-auth';
          if (id.includes('features/admin')) return 'chunk-admin';
          if (id.includes('features/products')) return 'chunk-products';
        },
      },
    },
    chunkSizeWarningLimit: 1000,
  },
};
```

---

## 6. Best Practices Checklist

- [ ] All route components lazy loaded
- [ ] Heavy components lazy loaded  
- [ ] Suspense boundaries with fallback UI
- [ ] Error boundaries around lazy components
- [ ] Preload admin/important routes
- [ ] Use Intersection Observer for viewport items
- [ ] Virtual list for 1000+ items
- [ ] Bundle analyzed regularly
- [ ] Chunk size monitored
- [ ] No blocking script tags

---

## 7. Performance Monitoring

```typescript
// src/utils/performance.utils.ts
export const reportWebVitals = () => {
  import('web-vitals').then(({ getCLS, getFID, getFCP, getLCP, getTTFB }) => {
    getCLS(console.log);
    getFID(console.log);
    getFCP(console.log);
    getLCP(console.log);
    getTTFB(console.log);
  });
};

export const logComponentRender = (componentName: string) => {
  if (import.meta.env.DEV) {
    console.time(`Render ${componentName}`);
    return () => console.timeEnd(`Render ${componentName}`);
  }
};
```

---

## References
- [React Code-Splitting](https://react.dev/reference/react/lazy)
- [Vite Chunking](https://vitejs.dev/guide/features.html#dynamic-import)
- [Web Vitals](https://web.dev/vitals/)
- [react-window Virtual List](https://github.com/bvaughn/react-window)
