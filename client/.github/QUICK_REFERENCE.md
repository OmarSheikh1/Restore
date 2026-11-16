# React Architecture - Quick Reference Guide

## 🎯 One-Page Cheat Sheet for React Architecture

### When You Need To... Reference This File:

| Need | File | Section |
|------|------|---------|
| **Replace a library** (Axios → Fetch, Toast → Notistack) | `library-abstraction-adapters.instructions.md` | 3-4: Adapter Pattern |
| **Add theme switcher** (Dark/Light/Custom themes) | `theme-management.instructions.md` | 7: Theme Switcher Component |
| **Create role-based menus** (Show/hide based on roles) | `auth-authorization-navigation.instructions.md` | 6: Dynamic User Menus |
| **Build new feature** (Products, Orders, etc) | `feature-based-architecture.instructions.md` | 1-3: Feature Structure |
| **Setup authentication** (Login, Protected routes) | `auth-authorization-navigation.instructions.md` | 2-5: Auth Flow |
| **Optimize bundle size** (Reduce initial load) | `lazy-loading-code-splitting.instructions.md` | 1-4: Code Splitting |
| **Design API contract** (Before frontend/backend split) | `api-first-development.instructions.md` | 2-3: OpenAPI Contract |
| **Setup DI & abstractions** (Foundation of architecture) | `dependency-inversion.instructions.md` | 1-4: Core Patterns |

---

## 🏗️ Architecture Layers (Top to Bottom)

```
┌─────────────────────────────────────┐
│  Components & Pages (JSX)           │ ← What users see
├─────────────────────────────────────┤
│  Custom Hooks (useAuth, useTheme)   │ ← Business logic
├─────────────────────────────────────┤
│  Services (AuthService, ThemeService)│ ← Core logic
├─────────────────────────────────────┤
│  Adapters (FetchAdapter, ToastAdapter)│ ← Library wrappers
├─────────────────────────────────────┤
│  3rd-Party Libraries                │ ← Easily swappable
└─────────────────────────────────────┘
```

**Rule**: Each layer knows only about layers BELOW it. Never know about layers ABOVE.

---

## 🔄 Three-Step Abstraction Pattern

### Step 1: Define Interface
```typescript
export interface INotificationService {
  success(msg: string): void;
  error(msg: string): void;
}
```

### Step 2: Create Adapter (wrap library)
```typescript
export class ToastAdapter implements INotificationService {
  success(msg: string) { toast.success(msg); }
  error(msg: string) { toast.error(msg); }
}
```

### Step 3: Use via Hook (never import library)
```typescript
const { success, error } = useNotification();
```

**To swap library**: Create new adapter, change 1 line in DI container. Done! ✅

---

## 📁 Folder Structure Quick Guide

```
src/
├── core/              ← Shared infrastructure
│   ├── types/         (Auth, User, Common types)
│   ├── di/            (DI Container, Providers)
│   └── api/           (Generated types from OpenAPI)
│
├── abstractions/      ← Service interfaces (contracts)
│   ├── auth.abstraction.ts
│   ├── api.abstraction.ts
│   ├── storage.abstraction.ts
│   └── notification.abstraction.ts
│
├── services/          ← Service implementations
│   ├── auth/
│   │   ├── AuthService.ts
│   │   └── useAuth.hook.ts
│   └── theme/
│       ├── ThemeService.ts
│       └── useTheme.hook.ts
│
├── ui/                ← Reusable UI components
│   ├── components/    (Button, Input, Card, etc)
│   ├── layouts/       (MainLayout, AuthLayout)
│   └── themes/        (lightTheme, darkTheme, etc)
│
└── features/          ← Business features (self-contained)
    ├── auth/
    │   ├── pages/     (LoginPage, RegisterPage)
    │   ├── components/(LoginForm, etc)
    │   ├── hooks/     (useLogin, useRegister)
    │   ├── services/  (AuthLocalService)
    │   └── index.ts   (public API)
    │
    └── products/
        ├── pages/
        ├── components/
        ├── hooks/
        ├── services/
        └── index.ts
```

**Key Rule**: Features are folders, not services. Each feature is independent.

---

## 🔐 Authentication Quick Flow

```
LoginPage (component)
    ↓
useAuth() hook
    ↓
AuthContext (provides service)
    ↓
AuthService (implements IAuthService)
    ↓
FetchApiAdapter (implements IApiClient)
    ↓
fetch() library

To swap: Only change FetchApiAdapter → AxiosApiAdapter
```

---

## 🎨 Theme System Quick Guide

```
1. Define Theme Config
   const lightTheme = { colors: {...}, typography: {...} }

2. Register in ThemeRegistry
   ThemeRegistry.register(lightTheme)

3. ThemeService applies CSS variables
   --color-primary: #0066ff
   --color-text: #1a1a1a

4. Components use CSS variables
   background: var(--color-primary)

5. useTheme hook for switcher
   const { changeTheme, availableThemes } = useTheme()

To add new theme: Create config → Register → Done! ✅
No component changes needed!
```

---

## 🚀 Feature Creation Checklist

```
Create new feature (e.g., Products):

✅ Create features/products/ folder
✅ Create pages/ProductsListPage.tsx
✅ Create components/ProductCard.tsx
✅ Create hooks/useProducts.hook.ts
✅ Create services/ProductLocalService.ts
✅ Create types/product.types.ts
✅ Create products.routes.tsx
✅ Export public API in index.ts
✅ Add routes to main router
✅ Create __tests__/ with tests

Done! Feature ready to use.
```

**Important**: Only export from index.ts. Everything else is internal!

---

## 🔒 Role-Based Access Control

```
1. Menu Item Config
   { label: 'Admin', path: '/admin', requiredRoles: ['admin'] }

2. Filter Menu with Hook
   const filtered = useFilteredMenu(menuItems)
   // Only shows items user has roles for

3. Navigate with Buttons
   <NavigationMenu items={filtered} />
   // Auto-hides items user can't access

4. Protect Routes
   <ProtectedRoute requiredRoles={['admin']}>
     <AdminPage />
   </ProtectedRoute>

Result: User sees only what they can access! ✅
```

---

## ⚡ Lazy Loading Strategy

```
Routes:
├─ /login → loaded immediately (public)
├─ /dashboard → lazy loaded
│   └─ Heavy chart → lazy loaded inside
└─ /admin → preloaded for admins

Result: Initial bundle 40% smaller! 🎉
```

**Simple**: Wrap component in `lazy()`
```typescript
const AdminPage = lazy(() => import('./pages/AdminPage'));
```

---

## 📡 API-First Development Flow

```
Week 1:
├─ Design OpenAPI contract (both teams agree)
├─ Frontend: Generate types → Create mocks
└─ Backend: Implement endpoints

Week 2-3:
├─ Frontend: Build features with mocks
└─ Backend: Build APIs per contract

Week 4:
├─ Point to real API (change 1 URL)
├─ Run integration tests
└─ Deploy! ✅

Key: Teams work in parallel, no dependencies!
```

---

## 🧪 Testing Pattern

```
1. Define Interface (contract)
   export interface IAuthService { ... }

2. Create Mock
   class MockAuthService implements IAuthService { ... }

3. Inject in Test
   const authService = new MockAuthService()
   const component = <LoginPage auth={authService} />

4. Test in Isolation
   expect(component).toShowLoginForm()

Result: Fast, isolated, no dependencies! ✅
```

---

## 📋 DI Container Pattern

```typescript
// Register (one place)
container.register('authService', () => new AuthService(apiClient))
container.register('themeService', () => new ThemeService())

// Use (everywhere)
const authService = container.get('authService')
const themeService = container.get('themeService')

// Swap (easy!)
container.register('apiClient', () => new AxiosAdapter()) // was FetchAdapter
```

**One location to update everything!**

---

## 🎯 Library Abstraction Examples

### HTTP Client
```
Interface: IApiClient
Adapters: FetchApiAdapter, AxiosApiAdapter
Swap: 1 line in container
```

### Notifications
```
Interface: INotificationService
Adapters: ToastAdapter, NotistackAdapter
Swap: 1 line in container
```

### Storage
```
Interface: IStorageService
Adapters: LocalStorageAdapter, IndexedDBAdapter
Swap: 1 line in container
```

### Forms
```
Interface: IFormService
Adapters: ReactHookFormAdapter, FormikAdapter
Swap: 1 line in container
```

---

## 📊 Bundle Size Optimization

```
WITHOUT Lazy Loading:
Initial: 500 KB
Dashboard: 200 KB included (but not used yet!)

WITH Lazy Loading:
Initial: 250 KB ⬇️ 50% smaller!
Dashboard: Loads on demand

WITH Code Splitting:
Initial: 150 KB ⬇️ 70% smaller!
Features load as separate chunks
```

---

## 🔄 Common Tasks & Solutions

| Task | Solution |
|------|----------|
| **Swap HTTP library** | Create adapter, change container.ts |
| **Add new theme** | Create config, register in ThemeRegistry |
| **Hide menu for non-admin** | Add `requiredRoles: ['admin']` to menu item |
| **Create new feature** | Follow feature checklist, use structure |
| **Make API call type-safe** | Generate types from OpenAPI |
| **Test service** | Create mock, inject in test |
| **Reduce bundle** | Lazy load routes and heavy components |
| **Parallel dev** | Use API mocks, both teams work independently |

---

## ✅ Production Checklist

- [ ] All services behind interfaces
- [ ] Mocks working for all API endpoints
- [ ] Routes lazy loaded
- [ ] Heavy components lazy loaded
- [ ] Auth service implemented
- [ ] Theme system working
- [ ] Menu filtering by roles
- [ ] Error boundaries in place
- [ ] All types generated from API
- [ ] No hardcoded URLs
- [ ] No direct library imports in components
- [ ] Tests passing
- [ ] Bundle analyzed
- [ ] Performance acceptable

---

## 🎓 Beginner → Advanced Path

### Beginner Level
- Read: README.md
- Focus: Feature-Based Architecture
- Build: Your first feature

### Intermediate Level
- Add: Authentication
- Add: Theme system
- Read: Dependency Inversion

### Advanced Level
- Master: Library Abstraction
- Implement: API-First
- Optimize: Lazy Loading
- Scale: Multi-team project

### Expert Level
- Combine: All patterns
- Customize: For your project
- Evolve: As requirements change
- Mentor: Your team

---

## 📚 File Reference

Quick links to files:

| File | Find | Purpose |
|------|------|---------|
| `README.md` | START HERE | Overview & navigation |
| `dependency-inversion.instructions.md` | FOUNDATION | Architecture fundamentals |
| `feature-based-architecture.instructions.md` | STRUCTURE | How to organize code |
| `auth-authorization-navigation.instructions.md` | SECURITY | User access & menus |
| `theme-management.instructions.md` | THEMING | Multi-theme system |
| `lazy-loading-code-splitting.instructions.md` | PERFORMANCE | Bundle optimization |
| `library-abstraction-adapters.instructions.md` | PATTERNS | Swappable libraries |
| `api-first-development.instructions.md` | INTEGRATION | Parallel development |

---

## 🚀 Getting Started

1. **Today**: Read README.md (15 minutes)
2. **Day 1**: Setup DI Container (dependency-inversion.instructions.md)
3. **Day 2**: Create first feature (feature-based-architecture.instructions.md)
4. **Day 3**: Setup Auth (auth-authorization-navigation.instructions.md)
5. **Day 4**: Add Themes (theme-management.instructions.md)
6. **Day 5**: Optimize bundle (lazy-loading-code-splitting.instructions.md)
7. **Week 2**: Master remaining files

---

## 💡 Key Takeaways

1. **Abstraction** = Flexibility (swap libraries easily)
2. **Features** = Scalability (team independence)
3. **Lazy Loading** = Performance (smaller bundles)
4. **API-First** = Parallel Development (no blocking)
5. **RBAC** = Security (role-based visibility)
6. **Theming** = Extensibility (add themes anytime)
7. **Testing** = Confidence (mock dependencies)
8. **Types** = Safety (generated from API)

---

## 🎉 Result

After implementing these patterns:

✅ **Modular** - Features are independent
✅ **Scalable** - Team can grow
✅ **Maintainable** - Easy to understand
✅ **Testable** - Dependencies mockable
✅ **Flexible** - Libraries swappable
✅ **Performant** - Lazy loaded
✅ **Secure** - RBAC built-in
✅ **Professional** - Production-ready

---

**Version**: 1.0
**Status**: Complete ✅
**Ready to Use**: Yes!

Start with `README.md` in the instructions folder! 🚀
