# API-First Development Guidelines

## Overview
Guide for designing and developing React applications with API-first approach: contract-driven development, TypeScript generation from API specs, mock servers, and seamless API integration.

---

## 1. API-First Workflow

### 1.1 Workflow Steps
```
1. Design API Contract (OpenAPI/Swagger)
   ↓
2. Generate TypeScript Types
   ↓
3. Setup Mock Server
   ↓
4. Develop Frontend (using mocks)
   ↓
5. Develop Backend (implementing contract)
   ↓
6. Integrate Real API
   ↓
7. Deploy
```

### 1.2 Benefits
- Frontend and backend teams can work in parallel
- No "undefined API" problems
- Clear contracts prevent misunderstandings
- Easy testing with mocks
- Better API documentation

---

## 2. OpenAPI/Swagger Contract

### 2.1 API Contract Definition
```yaml
# api/openapi.yaml
openapi: 3.0.0
info:
  title: Restore App API
  version: 1.0.0
  description: Backend API for Restore App

servers:
  - url: http://localhost:5000/api
    description: Development
  - url: https://api.restore.com/api
    description: Production

paths:
  /auth/login:
    post:
      summary: User login
      tags:
        - Authentication
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              properties:
                email:
                  type: string
                  format: email
                password:
                  type: string
                  format: password
              required:
                - email
                - password
      responses:
        '200':
          description: Login successful
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/LoginResponse'
        '401':
          description: Invalid credentials
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorResponse'

  /products:
    get:
      summary: Get all products
      tags:
        - Products
      parameters:
        - name: page
          in: query
          schema:
            type: integer
            default: 1
        - name: limit
          in: query
          schema:
            type: integer
            default: 20
        - name: search
          in: query
          schema:
            type: string
        - name: category
          in: query
          schema:
            type: string
      responses:
        '200':
          description: List of products
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ProductListResponse'
    post:
      summary: Create product
      tags:
        - Products
      security:
        - BearerAuth: []
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/CreateProductRequest'
      responses:
        '201':
          description: Product created
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Product'

  /products/{id}:
    get:
      summary: Get product by ID
      tags:
        - Products
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: string
      responses:
        '200':
          description: Product details
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/Product'
        '404':
          description: Product not found

components:
  schemas:
    LoginResponse:
      type: object
      properties:
        accessToken:
          type: string
        refreshToken:
          type: string
        expiresIn:
          type: integer
        user:
          $ref: '#/components/schemas/User'
      required:
        - accessToken
        - refreshToken
        - expiresIn
        - user

    User:
      type: object
      properties:
        id:
          type: string
        email:
          type: string
        name:
          type: string
        roles:
          type: array
          items:
            type: string
        claims:
          type: object
          additionalProperties: true

    Product:
      type: object
      properties:
        id:
          type: string
        name:
          type: string
        description:
          type: string
        price:
          type: number
        stock:
          type: integer
        category:
          type: string
        imageUrl:
          type: string
        createdAt:
          type: string
          format: date-time
        updatedAt:
          type: string
          format: date-time

    ProductListResponse:
      type: object
      properties:
        items:
          type: array
          items:
            $ref: '#/components/schemas/Product'
        total:
          type: integer
        page:
          type: integer
        pageSize:
          type: integer
        totalPages:
          type: integer

    ErrorResponse:
      type: object
      properties:
        code:
          type: string
        message:
          type: string
        details:
          type: object

  securitySchemes:
    BearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT
```

---

## 3. Generate TypeScript Types from OpenAPI

### 3.1 Setup OpenAPI Code Generation
```bash
# Install OpenAPI generator
npm install --save-dev @openapitools/openapi-generator-cli

# Or use openapi-typescript (simpler)
npm install --save-dev openapi-typescript
```

### 3.2 Configure Generation
```bash
# Create openapi.config.js
module.exports = {
  schemas: './api/openapi.yaml',
  output: './src/core/api/generated',
  client: 'fetch', // or 'axios'
};
```

### 3.3 Generate Types Script
```bash
# In package.json
{
  "scripts": {
    "api:generate": "openapi-typescript api/openapi.yaml -o src/core/api/generated/api.types.ts"
  }
}
```

### 3.4 Generated Types
```typescript
// src/core/api/generated/api.types.ts
// Auto-generated from openapi.yaml

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: User;
}

export interface User {
  id: string;
  email: string;
  name: string;
  roles: string[];
  claims: Record<string, any>;
}

export interface Product {
  id: string;
  name: string;
  description: string;
  price: number;
  stock: number;
  category: string;
  imageUrl?: string;
  createdAt: string; // ISO 8601
  updatedAt: string;
}

export interface ProductListResponse {
  items: Product[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
  stock: number;
  category: string;
  imageUrl?: string;
}

export interface ErrorResponse {
  code: string;
  message: string;
  details?: Record<string, any>;
}

// ... more types
```

### 3.5 Use Generated Types
```typescript
// src/core/types/auth.types.ts
import type { User, LoginResponse } from '@/core/api/generated/api.types';

export interface AuthState {
  token: string | null;
  user: User | null; // Using generated type!
  isAuthenticated: boolean;
}
```

---

## 4. Mock API Server Setup

### 4.1 Install MSW (Mock Service Worker)
```bash
npm install --save-dev msw
npm install --save-dev @mswjs/http-middleware
```

### 4.2 Mock Server Setup
```typescript
// src/mocks/server.ts
import { setupServer } from 'msw/node';
import { authHandlers } from './handlers/auth.handlers';
import { productHandlers } from './handlers/product.handlers';

export const server = setupServer(...authHandlers, ...productHandlers);
```

### 4.3 Auth Handlers
```typescript
// src/mocks/handlers/auth.handlers.ts
import { http, HttpResponse } from 'msw';
import type { LoginResponse, User } from '@/core/api/generated/api.types';

const mockUser: User = {
  id: '1',
  email: 'demo@example.com',
  name: 'Demo User',
  roles: ['user', 'admin'],
  claims: { 'email_verified': true },
};

export const authHandlers = [
  http.post('/api/auth/login', async ({ request }) => {
    const body = (await request.json()) as {
      email: string;
      password: string;
    };

    // Mock validation
    if (body.email === 'demo@example.com' && body.password === 'password') {
      const response: LoginResponse = {
        accessToken: 'mock-jwt-token-' + Date.now(),
        refreshToken: 'mock-refresh-token-' + Date.now(),
        expiresIn: 3600,
        user: mockUser,
      };
      return HttpResponse.json(response);
    }

    return HttpResponse.json(
      { code: 'INVALID_CREDENTIALS', message: 'Invalid email or password' },
      { status: 401 }
    );
  }),

  http.get('/api/auth/me', ({ request }) => {
    const auth = request.headers.get('Authorization');
    if (!auth?.startsWith('Bearer ')) {
      return HttpResponse.json(
        { code: 'UNAUTHORIZED', message: 'Missing token' },
        { status: 401 }
      );
    }
    return HttpResponse.json(mockUser);
  }),

  http.post('/api/auth/logout', () => {
    return HttpResponse.json({ success: true });
  }),
];
```

### 4.4 Product Handlers
```typescript
// src/mocks/handlers/product.handlers.ts
import { http, HttpResponse } from 'msw';
import type {
  Product,
  ProductListResponse,
  CreateProductRequest,
} from '@/core/api/generated/api.types';

const mockProducts: Product[] = [
  {
    id: '1',
    name: 'Laptop',
    description: 'High-performance laptop',
    price: 1299.99,
    stock: 5,
    category: 'Electronics',
    imageUrl: 'https://example.com/laptop.jpg',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: '2',
    name: 'Mouse',
    description: 'Wireless mouse',
    price: 29.99,
    stock: 50,
    category: 'Electronics',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

export const productHandlers = [
  http.get('/api/products', ({ request }) => {
    const url = new URL(request.url);
    const page = parseInt(url.searchParams.get('page') || '1');
    const limit = parseInt(url.searchParams.get('limit') || '20');
    const search = url.searchParams.get('search') || '';

    let filtered = mockProducts;
    if (search) {
      filtered = filtered.filter(p =>
        p.name.toLowerCase().includes(search.toLowerCase())
      );
    }

    const response: ProductListResponse = {
      items: filtered.slice((page - 1) * limit, page * limit),
      total: filtered.length,
      page,
      pageSize: limit,
      totalPages: Math.ceil(filtered.length / limit),
    };

    return HttpResponse.json(response);
  }),

  http.get('/api/products/:id', ({ params }) => {
    const product = mockProducts.find(p => p.id === params.id);
    if (!product) {
      return HttpResponse.json(
        { code: 'NOT_FOUND', message: 'Product not found' },
        { status: 404 }
      );
    }
    return HttpResponse.json(product);
  }),

  http.post('/api/products', async ({ request }) => {
    const data = (await request.json()) as CreateProductRequest;

    const newProduct: Product = {
      id: String(Math.random()),
      ...data,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };

    mockProducts.push(newProduct);
    return HttpResponse.json(newProduct, { status: 201 });
  }),
];
```

### 4.5 Enable Mocks in Development
```typescript
// src/main.tsx
import React from 'react';
import ReactDOM from 'react-dom/client';
import { App } from './App';

// Enable mocks in development
if (import.meta.env.DEV) {
  import('./mocks/server').then(({ server }) => {
    server.listen({ onUnhandledRequest: 'warn' });
  });
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
```

---

## 5. Building Services with Generated Types

### 5.1 Typed Service
```typescript
// src/services/auth/AuthService.ts
import { IAuthService } from '@abstractions/auth.abstraction';
import { IApiClient } from '@abstractions/api.abstraction';
import type {
  User,
  LoginResponse,
  ErrorResponse,
} from '@/core/api/generated/api.types';
import { LoginCredentials, AuthState } from '@/core/types/auth.types';

export class AuthService implements IAuthService {
  constructor(private apiClient: IApiClient) {}

  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    // Type-safe API call with generated types
    const response = await this.apiClient.post<LoginResponse>(
      '/auth/login',
      {
        email: credentials.email,
        password: credentials.password,
      }
    );

    return response; // Fully typed!
  }

  async getCurrentUser(): Promise<User | null> {
    // Type-safe API call
    return this.apiClient.get<User>('/auth/me');
  }

  hasRole(user: User | null, role: string): boolean {
    return user?.roles.includes(role) ?? false;
  }

  // ... other methods
}
```

### 5.2 Typed Product Service
```typescript
// src/features/products/services/ProductLocalService.ts
import { IApiClient } from '@abstractions/api.abstraction';
import type {
  Product,
  ProductListResponse,
  CreateProductRequest,
  ProductFilter,
} from '@/core/api/generated/api.types';

export class ProductLocalService {
  constructor(private apiClient: IApiClient) {}

  async getProducts(filter?: {
    page?: number;
    limit?: number;
    search?: string;
    category?: string;
  }): Promise<ProductListResponse> {
    // Type-safe API call with parameters
    return this.apiClient.get<ProductListResponse>('/products', {
      params: filter,
    });
  }

  async getProductById(id: string): Promise<Product> {
    // Type-safe API call with path parameter
    return this.apiClient.get<Product>(`/products/${id}`);
  }

  async createProduct(data: CreateProductRequest): Promise<Product> {
    // Type-safe POST with typed request/response
    return this.apiClient.post<Product>('/products', data);
  }

  async updateProduct(
    id: string,
    data: Partial<CreateProductRequest>
  ): Promise<Product> {
    // Type-safe PUT
    return this.apiClient.put<Product>(`/products/${id}`, data);
  }

  async deleteProduct(id: string): Promise<void> {
    await this.apiClient.delete(`/products/${id}`);
  }
}
```

---

## 6. Testing with Mock API

### 6.1 Test Setup
```typescript
// src/__tests__/setup.ts
import { server } from '@/mocks/server';
import { beforeAll, afterEach, afterAll, vi } from 'vitest';

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```

### 6.2 Test with Mocks
```typescript
// src/services/auth/__tests__/AuthService.test.ts
import { describe, it, expect } from 'vitest';
import { AuthService } from '../AuthService';
import { FetchApiAdapter } from '@/services/api/adapters/FetchApiAdapter';

describe('AuthService', () => {
  it('should login with valid credentials', async () => {
    const apiClient = new FetchApiAdapter();
    const authService = new AuthService(apiClient);

    const result = await authService.login({
      email: 'demo@example.com',
      password: 'password',
    });

    expect(result.accessToken).toBeDefined();
    expect(result.user).toBeDefined();
    expect(result.user.email).toBe('demo@example.com');
  });

  it('should fail with invalid credentials', async () => {
    const apiClient = new FetchApiAdapter();
    const authService = new AuthService(apiClient);

    await expect(
      authService.login({
        email: 'wrong@example.com',
        password: 'wrong',
      })
    ).rejects.toThrow();
  });
});
```

---

## 7. Switching from Mock to Real API

### 7.1 Environment Configuration
```typescript
// src/config/environment.ts
export const API_CONFIG = {
  baseUrl: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  useMocks: import.meta.env.DEV && import.meta.env.VITE_USE_MOCKS !== 'false',
};
```

### 7.2 Conditional Mock Enable
```typescript
// src/main.tsx
if (API_CONFIG.useMocks) {
  import('./mocks/server').then(({ server }) => {
    server.listen({ onUnhandledRequest: 'warn' });
  });
}
```

### 7.3 Environment Variables
```bash
# .env.development
VITE_API_URL=http://localhost:5000/api
VITE_USE_MOCKS=true

# .env.production
VITE_API_URL=https://api.restore.com/api
VITE_USE_MOCKS=false
```

---

## 8. API Documentation

### 8.1 Swagger UI Setup
```bash
npm install --save swagger-ui-express

# Serve at /api/docs
```

### 8.2 Generate Documentation
```bash
# In your backend repo
npm run build:openapi

# Copy to frontend
cp ../backend/openapi.yaml ./api/openapi.yaml
npm run api:generate
```

---

## 9. Best Practices Checklist

- [ ] OpenAPI contract defined before development
- [ ] TypeScript types generated from OpenAPI
- [ ] Mock handlers cover all API endpoints
- [ ] Services use generated types
- [ ] API changes update OpenAPI first
- [ ] Frontend and backend teams aligned on contract
- [ ] Mocks used during development
- [ ] Real API tested before production
- [ ] Error responses typed and handled
- [ ] API documentation up-to-date

---

## 10. Workflow Summary

```
Week 1: Backend Team
├─ Define OpenAPI contract
└─ Share openapi.yaml

Week 1: Frontend Team
├─ Generate types: npm run api:generate
├─ Setup MSW mock handlers
├─ Start development with mocks
└─ No API integration needed yet

Week 2: Backend Team
├─ Implement endpoints per contract
└─ Test against OpenAPI spec

Week 2: Frontend Team
├─ Continue feature development
├─ Test with mocks
└─ Ready to integrate

Week 3: Integration
├─ Disable mocks: VITE_USE_MOCKS=false
├─ Point to backend API
├─ Run integration tests
└─ Deploy! ✅
```

---

## References
- [OpenAPI 3.0 Specification](https://spec.openapis.org/oas/v3.0.0)
- [Mock Service Worker (MSW)](https://mswjs.io/)
- [openapi-typescript](https://github.com/drwpow/openapi-typescript)
- [API-First Development](https://swagger.io/tools/swagger-codegen/)
