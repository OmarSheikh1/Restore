# React Architecture Instruction Files - Complete Guide

## 📚 Overview
Comprehensive set of instruction files for building scalable, modular, and maintainable React applications with best practices for architecture, patterns, and library abstraction.

---

## 📋 Available Instruction Files

### 1. **Dependency Inversion Principle (DIP)**
📄 File: `dependency-inversion.instructions.md`

**Purpose**: Teach how to depend on abstractions rather than concrete implementations.

**Key Topics**:
- Three-layer abstraction model (Interface → Implementation → Hook)
- Dependency Injection Container setup
- API Client abstraction (Fetch vs Axios)
- Storage Service abstraction
- Notification Service abstraction
- Testing with mock implementations
- Migration example: Swapping HTTP clients with one line change

**When to Use**:
- Before writing any service
- When adding new 3rd-party libraries
- During code reviews for architecture validation

**Example**: "I need to swap my HTTP client from Fetch to Axios"
→ Follow this guide to see how only 1 line changes!

---

### 2. **Theme Management System**
📄 File: `theme-management.instructions.md`

**Purpose**: Implement extensible, multi-theme system with instant theme switching.

**Key Topics**:
- Theme type definitions and interface
- Theme configurations (light, dark, exotic, nature)
- ThemeRegistry for extensible themes
- ThemeService with persistence and system preference detection
- CSS Variables application (no re-renders)
- ThemeSwitcher components (basic and advanced)
- Dynamic theme registration at runtime
- Best practices and testing

**When to Use**:
- Setting up theme system
- Adding new themes
- Implementing theme switcher UI
- Styling components with theme variables

**Example**: "I want to add a 'cyberpunk' theme"
→ Create config file, register in ThemeRegistry, done!

---

### 3. **Feature-Based Architecture**
📄 File: `feature-based-architecture.instructions.md`

**Purpose**: Organize code around business features for scalability and team collaboration.

**Key Topics**:
- Feature directory structure and layout
- Public API via index.ts (contracts)
- Feature routes configuration
- Feature-specific types and services
- Feature components vs UI components
- Feature pages and hooks
- Feature context (optional, for complex state)
- Cross-feature communication rules
- Large feature structure for scaling
- Testing features
- Checklist for new features

**When to Use**:
- Starting new feature
- Organizing existing code into features
- Setting up team collaboration
- Code review for feature structure

**Example**: "I'm building Products feature"
→ Follow the standard folder structure with pages, components, hooks, services

---

### 4. **Secure Authentication, Authorization & Navigation**
📄 File: `auth-authorization-navigation.instructions.md`

**Purpose**: Implement secure role-based access control with dynamic menus and auth guards.

**Key Topics**:
- Auth type definitions and state management
- Abstract Auth Service
- Complete AuthService implementation
- Auth Provider and Context setup
- useAuth hook for component access
- Protected Route and Public Route components
- Menu configuration based on roles/claims/permissions
- Dynamic menu filtering hook
- NavigationMenu component with role-based visibility
- UserMenu component with dropdown
- API call security and interceptors
- Best practices and security checklist

**When to Use**:
- Setting up authentication
- Creating protected routes
- Building role-based menus
- Implementing access control
- Handling user navigation

**Example**: "Only admins should see Admin Settings in menu"
→ Add `requiredRoles: ['admin']` to menu item config
→ Menu automatically filters and hides it for non-admins!

---

### 5. **Lazy Loading & Code Splitting**
📄 File: `lazy-loading-code-splitting.instructions.md`

**Purpose**: Optimize bundle size and performance with intelligent code splitting.

**Key Topics**:
- Route-based code splitting with React.lazy
- Component-level lazy loading
- Suspense boundaries and fallbacks
- Route preloading strategies
- Intersection Observer preloading
- Infinite scroll with lazy loading
- Virtual lists for 1000+ items
- Bundle analysis tools
- Performance monitoring
- Best practices checklist

**When to Use**:
- Setting up routes
- Optimizing bundle size
- Improving initial load time
- Handling large data sets
- Pre-loading heavy components

**Example**: "Dashboard has heavy analytics charts"
→ Lazy load the chart component with Suspense fallback
→ Shows LoadingSpinner while chart loads

---

### 6. **Library Abstraction & Adapter Pattern**
📄 File: `library-abstraction-adapters.instructions.md`

**Purpose**: Abstract 3rd-party libraries to enable painless library swaps.

**Key Topics**:
- Core philosophy: preventing library lock-in
- Three-step abstraction pattern (Interface → Adapter → Hook)
- HTTP Client abstraction (Fetch vs Axios)
- Storage Service abstraction (LocalStorage vs IndexedDB)
- Form Library abstraction (React Hook Form vs Formik)
- Notification abstraction (react-hot-toast vs Notistack)
- Registering adapters in DI container
- Testing with mock implementations
- Migration checklist
- Common migration paths
- Best practices

**When to Use**:
- Before integrating any 3rd-party library
- When considering library changes
- During architecture planning
- Code reviews for library usage

**Example**: "We want to replace react-hot-toast with Notistack"
→ Create NotistackAdapter implementing INotificationService
→ Update container registration (1 line)
→ Zero component changes! ✅

---

### 7. **API-First Development**
📄 File: `api-first-development.instructions.md`

**Purpose**: Design and develop with API contracts first, using Mock Service Worker.

**Key Topics**:
- API-first workflow (Design → Types → Mocks → Development → Real API)
- OpenAPI/Swagger contract definition
- TypeScript type generation from OpenAPI
- Mock API server setup with MSW
- Mock handlers for auth and products
- Building typed services with generated types
- Testing with mock API
- Switching from mock to real API
- API documentation with Swagger UI
- Best practices workflow

**When to Use**:
- Project startup (design contract first)
- Frontend development (use mocks)
- Integration phase (switch to real API)
- API changes (update contract first)

**Example**: "Frontend and Backend need to work in parallel"
→ Backend defines OpenAPI contract
→ Frontend generates types and creates mock handlers
→ Both teams work independently
→ Integration seamless when real API ready!

---

## 🎯 Quick Navigation by Use Case

### "I'm starting a new React project"
1. Read **Dependency Inversion Principle** - understand the architecture
2. Read **Feature-Based Architecture** - structure your code
3. Read **API-First Development** - design your API contract
4. Read **Theme Management** - setup theming
5. Read **Auth/Authorization** - implement security
6. Read **Lazy Loading** - optimize performance

### "I need to add a new feature"
→ Read **Feature-Based Architecture**

### "I need to replace a library"
→ Read **Library Abstraction & Adapter Pattern**

### "I need to secure my app"
→ Read **Secure Auth, Authorization & Navigation**

### "My bundle is too large"
→ Read **Lazy Loading & Code Splitting**

### "Frontend and Backend should work in parallel"
→ Read **API-First Development**

### "I need dynamic user menus based on roles"
→ Read **Secure Auth, Authorization & Navigation**

### "I need multiple themes"
→ Read **Theme Management System**

---

## 🔄 Implementation Order (Recommended)

```
Phase 1: Foundation (Week 1)
├─ Dependency Inversion Principle
├─ Feature-Based Architecture
├─ API-First Development
└─ Setup DI Container

Phase 2: Core Features (Week 2)
├─ Authentication & Authorization
├─ Theme Management
└─ Navigation with Role-Based Menus

Phase 3: Performance & UX (Week 3)
├─ Lazy Loading & Code Splitting
└─ Library Abstractions for remaining libs

Phase 4: Documentation & Polish (Week 4)
├─ Update project README
├─ Document API contract
└─ Setup monitoring
```

---

## 📊 Architecture Diagram

```
┌──────────────────────────────────────────────┐
│          React Application                   │
├──────────────────────────────────────────────┤
│                                              │
│  Features (Feature-Based Architecture)       │
│  ├─ auth/                                    │
│  ├─ dashboard/                               │
│  ├─ products/                                │
│  └─ admin/                                   │
│                                              │
│  ↓                                           │
│                                              │
│  Custom Hooks (useAuth, useTheme, etc)      │
│  ↓                                           │
│  Context & DI Container (Dependency Inv)    │
│  ↓                                           │
│  Services (Interfaces & Adapters)           │
│  │                                           │
│  ├─ AuthService (implements IAuthService)   │
│  ├─ ThemeService (implements IThemeService) │
│  └─ Others...                               │
│  ↓                                           │
│  Library Adapters (Abstraction)             │
│  │                                           │
│  ├─ FetchApiAdapter / AxiosApiAdapter       │
│  ├─ ToastAdapter / NotistackAdapter         │
│  └─ Others...                               │
│  ↓                                           │
│  3rd-Party Libraries                        │
│  ├─ react-hot-toast / notistack             │
│  ├─ axios / fetch                           │
│  └─ Others...                               │
│                                              │
└──────────────────────────────────────────────┘

Legend:
- Feature-Based: Organize code around business features
- Hooks: Bridge between components and services
- Services: Business logic with interfaces
- Adapters: Wrap 3rd-party libraries
- Libraries: External dependencies (easily swappable)

Result:
✅ Modular and scalable
✅ Easy to test
✅ Easy to swap libraries
✅ Team collaboration friendly
✅ Future-proof
```

---

## ✅ Benefits Summary

| Instruction | Benefit |
|------------|---------|
| **Dependency Inversion** | Swap libraries with 1 line change |
| **Feature-Based Architecture** | Scale teams and features independently |
| **Theme Management** | Add themes without touching components |
| **Auth/Authorization** | Secure app with role-based access control |
| **Dynamic Menus** | Auto-hide menu items based on user roles |
| **Lazy Loading** | Reduce initial bundle size by 50%+ |
| **Library Abstraction** | No vendor lock-in, future-proof design |
| **API-First Development** | Frontend and Backend teams work in parallel |

---

## 🛠️ Tools & Libraries Recommended

**Build & Development**:
- Vite (fast build)
- React Router v6 (routing)
- React 18+ (latest features)

**State Management**:
- React Context (app-wide state)
- React Query (server state)
- Zustand (optional: complex local state)

**HTTP Client**:
- Fetch API (built-in, abstracted)
- Axios (alternative via adapter)

**Forms**:
- React Hook Form (lightweight)
- Formik (alternative via adapter)

**Styling**:
- CSS Variables (theme switching)
- CSS Modules or Tailwind (component styling)

**Notifications**:
- react-hot-toast (default)
- Notistack (alternative via adapter)

**Testing**:
- Vitest (unit tests)
- React Testing Library (component tests)
- MSW (mock API)
- Playwright (E2E tests)

**Development**:
- TypeScript (strict typing)
- ESLint (code quality)
- Prettier (code formatting)

---

## 📚 File Structure After Following All Instructions

```
client/src/
├── .github/instructions/
│   ├── dependency-inversion.instructions.md
│   ├── theme-management.instructions.md
│   ├── feature-based-architecture.instructions.md
│   ├── auth-authorization-navigation.instructions.md
│   ├── lazy-loading-code-splitting.instructions.md
│   ├── library-abstraction-adapters.instructions.md
│   └── api-first-development.instructions.md
│
├── core/
│   ├── types/
│   │   ├── auth.types.ts
│   │   ├── user.types.ts
│   │   └── theme.types.ts
│   │
│   ├── di/
│   │   ├── container.ts
│   │   └── providers/
│   │       ├── RootProvider.tsx
│   │       ├── AuthProvider.tsx
│   │       └── ThemeProvider.tsx
│   │
│   └── api/
│       └── generated/
│           └── api.types.ts (auto-generated)
│
├── abstractions/
│   ├── auth.abstraction.ts
│   ├── api.abstraction.ts
│   ├── storage.abstraction.ts
│   ├── notification.abstraction.ts
│   └── form.abstraction.ts
│
├── services/
│   ├── auth/
│   │   ├── AuthService.ts
│   │   ├── useAuth.hook.ts
│   │   └── adapters/
│   │
│   ├── api/
│   │   ├── ApiClient.ts
│   │   ├── adapters/
│   │   │   ├── FetchApiAdapter.ts
│   │   │   └── AxiosApiAdapter.ts
│   │   └── useApi.hook.ts
│   │
│   └── theme/
│       ├── ThemeService.ts
│       ├── useTheme.hook.ts
│       └── adapters/
│
├── ui/
│   ├── components/
│   │   ├── Button.tsx
│   │   ├── Input.tsx
│   │   └── ...
│   │
│   ├── layouts/
│   │   ├── MainLayout.tsx
│   │   └── AuthLayout.tsx
│   │
│   └── themes/
│       ├── lightTheme.ts
│       ├── darkTheme.ts
│       ├── exoticTheme.ts
│       ├── natureTheme.ts
│       └── ThemeRegistry.ts
│
├── features/
│   ├── auth/
│   │   ├── index.ts (public API)
│   │   ├── auth.routes.tsx
│   │   ├── pages/
│   │   ├── components/
│   │   ├── hooks/
│   │   ├── services/
│   │   └── types/
│   │
│   ├── products/
│   │   ├── index.ts (public API)
│   │   ├── products.routes.tsx
│   │   ├── pages/
│   │   ├── components/
│   │   ├── hooks/
│   │   ├── services/
│   │   └── types/
│   │
│   └── admin/
│       ├── index.ts (public API)
│       ├── admin.routes.tsx
│       └── ...
│
├── config/
│   ├── routes.config.ts
│   ├── menu.config.ts
│   └── environment.ts
│
├── mocks/
│   ├── server.ts
│   └── handlers/
│       ├── auth.handlers.ts
│       └── product.handlers.ts
│
├── App.tsx
├── main.tsx
└── index.css
```

---

## 🔗 Cross-References

Each instruction file references others for complete understanding:

- **Dependency Inversion** → uses concepts from **Library Abstraction**
- **Feature-Based Architecture** → builds on **Dependency Inversion**
- **Auth/Authorization** → implements **Dependency Inversion** patterns
- **Theme Management** → implements **Library Abstraction** patterns
- **Lazy Loading** → works with **Feature-Based Architecture**
- **API-First Development** → generates types used in **all services**

---

## 📞 When to Reference Each File

| Scenario | Reference |
|----------|-----------|
| Integrating new library | Library Abstraction & Adapter Pattern |
| Adding new feature | Feature-Based Architecture |
| Setting up authentication | Auth, Authorization & Navigation |
| Creating new theme | Theme Management System |
| Improving performance | Lazy Loading & Code Splitting |
| Designing new API | API-First Development |
| Refactoring architecture | Dependency Inversion Principle |
| Code review | All files (architecture validation) |

---

## 🎓 Learning Path

**Beginner**: Start with **Feature-Based Architecture** and **Dependency Inversion**

**Intermediate**: Add **Theme Management** and **Auth/Authorization**

**Advanced**: Master **Library Abstraction**, **API-First**, and **Lazy Loading**

**Expert**: Combine all patterns for production-grade applications

---

## 📝 Notes

- All files use **TypeScript** for type safety
- Examples are **production-ready**
- Patterns follow **SOLID principles**
- Code examples are **testable**
- Best practices are **industry-standard**

---

**Last Updated**: November 2026
**Version**: 1.0
**Status**: Complete and Ready for Use ✅
