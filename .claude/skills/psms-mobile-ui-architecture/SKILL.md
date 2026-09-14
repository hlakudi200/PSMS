---
name: psms-mobile-ui-architecture
description: Build or change the PSMS Expo mobile UI while preserving its role-scoped navigation, provider boundaries, and shared component structure. Use for mobile screens, navigation, layouts, or feature UI; not for web-only UI or backend work.
---

# PSMS Mobile UI Architecture

The Expo app is a companion for `Student` and `Parent` users only. All other roles use the web application. Treat this as a product boundary as well as a navigation boundary.

## Structure

Keep the mobile app organized as follows:

```text
mobile/
├── App.tsx                         # Composition root only
└── src/
    ├── navigation/                  # Auth and role-aware route decisions
    ├── screens/
    │   ├── auth/                    # Sign-in, help, loading, access denied
    │   ├── student/                 # Student-only screens
    │   └── parent/                  # Parent-only screens
    ├── components/                  # Reusable presentational mobile components
    ├── providers/                   # Context + reducer data domains
    ├── utils/                       # API, token, persistence, formatting utilities
    └── theme/                       # Shared design tokens and theme helpers
```

`App.tsx` mounts cross-cutting providers and renders `AppNavigator`; it must not contain feature UI or API calls. Screens compose domain state and present UI. Reusable, backend-agnostic UI belongs in `components`; it must not import providers or call APIs.

## Navigation and access

- `AppNavigator` is the only place that selects the unauthenticated, Student, or Parent application surface.
- While session restoration is unresolved, render the auth loading screen. Do not render a protected screen before `currentUser` is available.
- Unauthenticated users see only auth screens.
- `Student` users see only `screens/student`; `Parent` users see only `screens/parent`.
- An unsupported role must clear its mobile session and see the access-denied screen. Do not add a generic mobile portal for staff, administration, or applicants.
- Add nested navigation only when a role has multiple feature areas. Keep role-specific route definitions inside that role's navigator; never let a screen decide whether a role may enter another role's area.

## State and data

- Preserve the frontend provider contract where it is platform-neutral: separate state/action contexts, `useReducer`, `redux-actions`, and `isPending`/`isSuccess`/`isError` flags.
- Keep mobile-only concerns behind adapters: Expo SecureStore replaces browser session storage; the mobile axios instance replaces browser redirect/modal behavior.
- Screens use provider hooks. They do not call axios directly or duplicate API endpoints.
- Mount auth at the app root. Mount feature providers at the smallest shared navigator or screen boundary.
- Do not invent API endpoints. In particular, password reset is administrator-controlled by the existing backend; mobile account help directs users to their school office until a self-service endpoint exists.

## UI conventions

- Prefer native React Native primitives and Expo-compatible packages. Do not introduce web-only libraries or browser globals.
- Use `SafeAreaView`, keyboard-aware form layouts, accessible labels/roles, and touch targets suitable for phones.
- Validate mobile forms with Zod before provider actions.
- Keep reusable colour, spacing, typography, and status tokens in `src/theme` as the mobile surface grows. Do not copy web CSS or Ant Design components into React Native screens.
- Each async screen state needs a loading, error, empty (when applicable), and successful state.

## Feature workflow

1. Identify the eligible role(s) and add the screen under that role's directory.
2. Reuse or add the matching provider; adapt only platform-specific storage, navigation, or presentation code.
3. Register the screen in the relevant navigator, not in `App.tsx`.
4. Keep server authorization authoritative; client role gating is for navigation and user experience.
5. Run `cd mobile && npx tsc --noEmit` and bundle the affected native target before handoff.
