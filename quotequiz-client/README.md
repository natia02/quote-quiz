# QuoteQuiz — Frontend

React 19 + TypeScript + Vite single-page application that consumes the QuoteQuiz REST API. Players test their literary knowledge by guessing quote authors in Binary (Yes/No) or Multiple Choice mode. Admins can manage users, quotes, and view aggregated game history.

---

## Tech Stack

| Layer | Technology | Why |
|---|---|---|
| UI Library | React 19 | Latest stable; concurrent rendering ready |
| Language | TypeScript 5.9 | Full type safety across features |
| Build tool | Vite 7 | Sub-second HMR, fast cold starts |
| Routing | React Router v7 | First-class loader/action API, nested routes |
| Global state | Zustand 5 | Minimal boilerplate, no Provider wrapping, built-in `persist` middleware |
| HTTP client | Axios 1.x | Interceptors for auto-attaching JWT; cleaner than raw fetch |
| Styling | Tailwind CSS v4 | Zero-runtime, utility-first, v4 Vite plugin (no PostCSS config needed) |

---

## Getting Started

**Prerequisites:** Node 20+, pnpm 9+ (`npm i -g pnpm`)

The backend must be running at `http://localhost:5294` before starting the client.

```bash
# from the repo root
cd quotequiz-client
pnpm install
pnpm run dev
```

App opens at `http://localhost:5173`.

To build for production:

```bash
pnpm run build   # output in dist/
pnpm run preview # serves the dist/ build locally
```

---

## Project Structure

```
quotequiz-client/
├── public/
├── src/
│   ├── features/               # Feature-based modules (see Architecture below)
│   │   ├── auth/
│   │   │   ├── components/
│   │   │   │   ├── LoginForm.tsx
│   │   │   │   └── RegisterForm.tsx
│   │   │   ├── hooks/
│   │   │   │   └── useAuth.ts
│   │   │   └── types/
│   │   │       └── auth.types.ts
│   │   ├── quiz/
│   │   │   ├── components/
│   │   │   │   ├── QuizCard.tsx          # Outer card + pink quote block
│   │   │   │   ├── BinaryOptions.tsx     # Yes / No buttons
│   │   │   │   ├── MultiChoiceOptions.tsx # 4-option grid
│   │   │   │   └── ResultFeedback.tsx    # Correct / Wrong feedback with server message
│   │   │   ├── hooks/
│   │   │   │   └── useQuiz.ts            # All quiz state machine logic
│   │   │   └── types/
│   │   │       └── quiz.types.ts
│   │   ├── history/
│   │   │   ├── components/
│   │   │   │   ├── HistoryList.tsx
│   │   │   │   └── StatsGrid.tsx
│   │   │   ├── hooks/
│   │   │   │   └── useHistory.ts
│   │   │   └── types/
│   │   │       └── history.types.ts
│   │   └── admin/
│   │       ├── components/
│   │       │   ├── UsersTab.tsx
│   │       │   ├── UserForm.tsx
│   │       │   ├── QuotesTab.tsx
│   │       │   ├── QuoteForm.tsx
│   │       │   └── AchievementsTab.tsx
│   │       └── hooks/
│   │           ├── useUsers.ts
│   │           ├── useAdminQuotes.ts
│   │           └── useAchievements.ts
│   ├── pages/                  # Route-level components
│   │   ├── AuthPage.tsx        # Login + Register tabs
│   │   ├── QuizPage.tsx        # Active quiz session
│   │   ├── HistoryPage.tsx     # Personal game history + stats
│   │   └── SettingsPage.tsx    # Mode selector + Admin panel (role-gated)
│   ├── shared/
│   │   ├── api/
│   │   │   └── client.ts       # Axios instance with JWT interceptor
│   │   ├── components/
│   │   │   ├── Layout.tsx      # Navbar + <Outlet />
│   │   │   └── ProtectedRoute.tsx
│   │   └── stores/
│   │       ├── authStore.ts    # Zustand: user + token, persisted to localStorage
│   │       └── settingsStore.ts # Zustand: quiz mode preference, persisted
│   ├── App.tsx                 # Route definitions
│   ├── main.tsx
│   └── index.css               # Tailwind import + base body styles
├── index.html
├── vite.config.ts
├── tsconfig.app.json
└── package.json
```

---

## Architecture

### Why Feature-Based, Not Type-Based

A common beginner structure groups files by type: `components/`, `hooks/`, `types/`. This looks clean initially but creates long-distance coupling — you change a quiz concept and touch four unrelated directories.

This project uses **feature-based** (also called vertical slice) architecture instead:

```
# Type-based (avoided)           # Feature-based (used)
components/                       features/quiz/
  QuizCard.tsx                      components/QuizCard.tsx
  BinaryOptions.tsx                 components/BinaryOptions.tsx
hooks/                              hooks/useQuiz.ts
  useQuiz.ts                        types/quiz.types.ts
types/
  quiz.types.ts
```

Each feature owns its components, hooks, and types. When you delete a feature, you delete one folder. When you read a feature, everything related is co-located.

The `shared/` folder holds only genuinely cross-cutting concerns: the HTTP client, the global auth/settings stores, the Layout wrapper, and ProtectedRoute.

---

## State Management

### Why Zustand

State management choice at a glance:

| Option | Verdict |
|---|---|
| React Context | Good for static config (theme, locale). Causes full subtree re-renders when value changes — wrong for frequently-updated auth/quiz state |
| Redux Toolkit | Excellent for large teams; heavy boilerplate for a focused project like this |
| Zustand 5 | Minimal API, no Provider, built-in `persist` middleware, selectors prevent unnecessary re-renders |

Two global stores:

**`authStore`** — persisted to `localStorage`
```ts
{ user: User | null, token: string | null }
actions: setAuth(user, token), clearAuth()
```

**`settingsStore`** — persisted to `localStorage`
```ts
{ mode: 'Binary' | 'MultipleChoice' }
actions: setMode(mode)
```

Everything else (quiz questions, history data, admin data) is local state managed by custom hooks — it doesn't need to survive a page refresh.

---

## Routing and Route Protection

Routes are defined in `App.tsx`:

```
/               → redirect to /quiz
/auth           → AuthPage (public)
/quiz           → QuizPage (protected)
/history        → HistoryPage (protected)
/settings       → SettingsPage (protected)
*               → redirect to /quiz (catch-all)
```

`ProtectedRoute` reads `authStore`. If no token exists it navigates to `/auth`. All authenticated routes are nested inside `<Layout>` which renders the navbar and `<Outlet />`.

Admin-specific UI inside `SettingsPage` is gated at the component level (`user?.role === 'Admin'`), not at the route level — the API enforces authorization anyway, and a separate `/admin` route would add complexity for what is essentially a tabbed panel inside Settings.

---

## API Layer

`src/shared/api/client.ts` creates a single Axios instance:

```ts
const client = axios.create({ baseURL: 'http://localhost:5294/api' });

client.interceptors.request.use(config => {
  const token = localStorage.getItem('token');
  if (token) config.headers['Authorization'] = `Bearer ${token}`;
  return config;
});
```

**Why Axios over fetch:**
- Request interceptors let us attach the JWT in one place rather than every call site
- Automatic JSON serialization/deserialization
- Consistent error objects with `error.response.status`

Every feature module has its own API file (e.g., `features/quiz/api/quizApi.ts`) that calls `client`. No feature imports another feature's API — they go through the shared client only.

---

## Custom Hooks Pattern

Business logic lives in custom hooks, not in components. Components are responsible only for rendering.

Example — `useQuiz`:

```ts
// What it manages:
const [question, setQuestion]     = useState<QuizQuestion | null>(null);
const [result, setResult]         = useState<AnswerResult | null>(null);
const [isLoading, setIsLoading]   = useState(false);
const [error, setError]           = useState<string | null>(null);
const [isFinished, setIsFinished] = useState(false);

// Exposed API:
return { question, result, isLoading, error, isFinished, fetchQuestion, submitAnswer };
```

`QuizPage` calls `useQuiz()` and passes slices down to `QuizCard`, `BinaryOptions`, `ResultFeedback`. The component has zero business logic — if the backend API changes, only the hook changes.

Same pattern across all features:

| Hook | Manages |
|---|---|
| `useAuth` | login, register, logout calls + authStore updates |
| `useQuiz` | fetch question, submit answer, finished state |
| `useHistory` | fetch history list + statistics |
| `useUsers` | admin CRUD for users |
| `useAdminQuotes` | admin CRUD for quotes |
| `useAchievements` | admin view of all game history |

---

## TypeScript Strategy

- **No `any`** — unknown is used where the type isn't known at compile time; proper narrowing is applied before use
- **Feature-owned types** — each feature defines its own types in `types/feature.types.ts`. No single global `types.ts` that everything imports
- **API response shapes match DTOs** — types mirror the backend response contracts exactly, keeping the mapping surface small

---

## Styling

Tailwind CSS v4 is configured via the `@tailwindcss/vite` plugin — no `tailwind.config.js` or PostCSS config is needed:

```ts
// vite.config.ts
import tailwindcss from '@tailwindcss/vite'
export default defineConfig({ plugins: [react(), tailwindcss()] })
```

```css
/* index.css */
@import "tailwindcss";
body { background-color: #f8fafc; color: #1e293b; }
```

Design decisions:
- **White cards on slate-50 background** — clear visual hierarchy without heavy contrast
- **Dark gray navbar (`bg-gray-900`)** — anchors the page, matches the project wireframe
- **Pink quote block (`bg-pink-500`)** — makes the quote visually distinct from the answer UI
- **Indigo accent** — used for active states, focus rings, mode badges; provides consistent interactive colour
- **Green/red semantic colours** — correct/wrong feedback uses green-50/red-50 backgrounds matching universal conventions

---

## Pages Overview

| Page | Route | Access | Purpose |
|---|---|---|---|
| `AuthPage` | `/auth` | Public | Login and Register tabs; redirects to `/quiz` on success |
| `QuizPage` | `/quiz` | Auth | Fetches a question, renders answer UI, shows result feedback, loops |
| `HistoryPage` | `/history` | Auth | Lists past games with correct/wrong badge and score; shows aggregated stats |
| `SettingsPage` | `/settings` | Auth | Mode selector (Binary / Multiple Choice) for all users. Admins additionally see Users, Quotes, and Achievements management tabs |

`SettingsPage` is intentionally dual-purpose: it avoids creating a separate admin route tree while keeping admin functionality behind a real role check. The backend rejects any admin API call that lacks the Admin claim regardless of what the client renders.

---

## Environment

The API base URL is hardcoded in `src/shared/api/client.ts` as `http://localhost:5294/api`. For a production deployment this would be extracted to an environment variable via `import.meta.env.VITE_API_URL`.

---

## Scripts

| Command | Description |
|---|---|
| `pnpm run dev` | Start Vite dev server with HMR at `localhost:5173` |
| `pnpm run build` | Type-check + build to `dist/` |
| `pnpm run preview` | Serve the production build locally |
| `pnpm run lint` | Run ESLint across all `.ts` / `.tsx` files |
