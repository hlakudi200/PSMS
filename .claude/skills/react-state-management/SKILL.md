---
name: react-state-management
description: The project's standard React state-management pattern — a Context + useReducer + redux-actions "provider triad/quad" with separate State and Action contexts, pending/isSuccess/isError flags, async action functions backed by an axios instance, and useXState/useXActions hooks. Use when creating a new data domain (CRUD feature, list+detail, etc.) or wiring a component to the backend without adding a global store like Redux/Zustand.
---

# React State Management — Context + useReducer + redux-actions

The standard way this codebase manages feature state. No global store (no Redux store, no Zustand). Each **data domain** (students, notifications, messages, …) gets its **own self-contained provider** built from the same four files. Forms still use **Zod** for validation — this pattern is about *data/state*, not form schemas.

## Why this shape

- **Two contexts per domain** — a read-only `StateContext` and an `ActionContext`. Components that only call actions don't re-render when state changes.
- **`redux-actions`** (`createAction` / `handleActions`) gives typed action creators + a flat reducer without boilerplate switch statements.
- **Flag triad** (`isPending` / `isSuccess` / `isError`) on every domain so any consumer can render loading/empty/error/success without local `useState`.
- **Provider isolation** — mount a provider right around the feature that needs it; unmounting resets its state. Cross-cutting domains (auth) mount at the root.

---

## The four files (`providers/<domain>/`)

Replace `Xxx` with the domain (e.g. `Student`), `xxx` with its lowercase form.

### 1. `context.ts` — shapes + contexts

```ts
import { createContext } from "react";

export interface IXxx { id: string; /* …entity fields… */ }

export interface IXxxStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  xxx?: IXxx;            // current single item (detail)
  xxxs?: IXxx[];         // list
  totalCount?: number;   // for server-side paging
}

export interface IXxxActionContext {
  getAsync: (id: string) => Promise<void>;
  getAllAsync: (params?: IQueryParams) => Promise<void>;
  createAsync: (payload: Partial<IXxx>) => Promise<void>;
  updateAsync: (payload: IXxx) => Promise<void>;
  deleteAsync: (id: string) => Promise<void>;
  resetStateFlags: () => void;
}

export const INITIAL_STATE: IXxxStateContext = { isPending: false, isSuccess: false, isError: false };

export const XxxStateContext = createContext<IXxxStateContext>(INITIAL_STATE);
export const XxxActionContext = createContext<IXxxActionContext | undefined>(undefined);
```

### 2. `actions.ts` — action enum + `createAction` creators

One **PENDING / SUCCESS / ERROR** trio per async operation. PENDING flips `isPending` and clears `isSuccess/isError`; SUCCESS carries the data; ERROR flips `isError`.

```ts
import { createAction } from "redux-actions";
import { IXxxStateContext } from "./context";

export enum XxxActionEnums {
  getAllPending = "GET_ALL_PENDING", getAllSuccess = "GET_ALL_SUCCESS", getAllError = "GET_ALL_ERROR",
  getPending = "GET_PENDING", getSuccess = "GET_SUCCESS", getError = "GET_ERROR",
  createPending = "CREATE_PENDING", createSuccess = "CREATE_SUCCESS", createError = "CREATE_ERROR",
  updatePending = "UPDATE_PENDING", updateSuccess = "UPDATE_SUCCESS", updateError = "UPDATE_ERROR",
  deletePending = "DELETE_PENDING", deleteSuccess = "DELETE_SUCCESS", deleteError = "DELETE_ERROR",
  resetStateFlags = "RESET_STATE_FLAGS",
}

const pending = () => ({ isPending: true, isSuccess: false, isError: false });
const error   = () => ({ isPending: false, isSuccess: false, isError: true });

export const getAllPending = createAction<IXxxStateContext>(XxxActionEnums.getAllPending, pending);
export const getAllSuccess = createAction<IXxxStateContext, { xxxs: IXxx[]; totalCount: number }>(
  XxxActionEnums.getAllSuccess,
  (p) => ({ isPending: false, isSuccess: true, isError: false, xxxs: p.xxxs, totalCount: p.totalCount })
);
export const getAllError = createAction<IXxxStateContext>(XxxActionEnums.getAllError, error);

export const getSuccess = createAction<IXxxStateContext, IXxx>(
  XxxActionEnums.getSuccess, (xxx) => ({ isPending: false, isSuccess: true, isError: false, xxx })
);
// …create/update/delete trios follow the same shape…

export const resetStateFlags = createAction<IXxxStateContext>(
  XxxActionEnums.resetStateFlags, () => ({ isPending: false, isSuccess: false, isError: false })
);
```

### 3. `reducer.ts` — `handleActions`, merge payload

Every case is `(state, action) => ({ ...state, ...action.payload })`. The action creators already shaped the partial state.

```ts
import { handleActions } from "redux-actions";
import { INITIAL_STATE, IXxxStateContext } from "./context";
import { XxxActionEnums } from "./actions";

export const XxxReducer = handleActions<IXxxStateContext>(
  Object.fromEntries(
    Object.values(XxxActionEnums).map((k) => [k, (state, action) => ({ ...state, ...action.payload })])
  ),
  INITIAL_STATE
);
```

### 4. `index.tsx` — provider, async action functions, hooks

```tsx
"use client";
import React, { useReducer, useRef, useMemo, useCallback, useContext } from "react";
import { getAxiosInstance } from "@/utils/axios-instance";
import { XxxReducer } from "./reducer";
import {
  INITIAL_STATE, XxxStateContext, XxxActionContext, IXxxActionContext,
} from "./context";
import {
  getAllPending, getAllSuccess, getAllError,
  getSuccess, /* …, */ resetStateFlags as resetStateFlagsAction,
} from "./actions";

const ENDPOINT = "/api/services/app/Xxx"; // ABP app-service convention

export const XxxProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(XxxReducer, INITIAL_STATE);
  const instance = useRef(getAxiosInstance()).current; // stable across renders

  const getAllAsync = useCallback(async (params?: IQueryParams) => {
    dispatch(getAllPending());
    try {
      const { data } = await instance.get(`${ENDPOINT}/GetAll`, { params });
      dispatch(getAllSuccess({ xxxs: data.result.items, totalCount: data.result.totalCount }));
    } catch { dispatch(getAllError()); }
  }, [instance]);

  const getAsync = useCallback(async (id: string) => {
    dispatch(getPending());
    try {
      const { data } = await instance.get(`${ENDPOINT}/Get`, { params: { id } });
      dispatch(getSuccess(data.result));
    } catch { dispatch(getError()); }
  }, [instance]);

  const createAsync = useCallback(async (payload: Partial<IXxx>) => {
    dispatch(createPending());
    try { const { data } = await instance.post(`${ENDPOINT}/Create`, payload); dispatch(createSuccess(data.result)); }
    catch { dispatch(createError()); }
  }, [instance]);

  const updateAsync = useCallback(async (payload: IXxx) => {
    dispatch(updatePending());
    try { const { data } = await instance.put(`${ENDPOINT}/Update`, payload); dispatch(updateSuccess(data.result)); }
    catch { dispatch(updateError()); }
  }, [instance]);

  const deleteAsync = useCallback(async (id: string) => {
    dispatch(deletePending());
    try { await instance.delete(`${ENDPOINT}/Delete`, { params: { id } }); dispatch(deleteSuccess(id)); }
    catch { dispatch(deleteError()); }
  }, [instance]);

  const actions = useMemo<IXxxActionContext>(() => ({
    getAsync, getAllAsync, createAsync, updateAsync, deleteAsync,
    resetStateFlags: () => dispatch(resetStateFlagsAction()),
  }), [getAsync, getAllAsync, createAsync, updateAsync, deleteAsync]);

  return (
    <XxxStateContext.Provider value={state}>
      <XxxActionContext.Provider value={actions}>{children}</XxxActionContext.Provider>
    </XxxStateContext.Provider>
  );
};

export const useXxxState = () => useContext(XxxStateContext);
export const useXxxActions = () => {
  const ctx = useContext(XxxActionContext);
  if (!ctx) throw new Error("useXxxActions must be used within a XxxProvider");
  return ctx;
};
```

---

## Using it in a component

Mount the provider around the feature, then consume via the hooks:

```tsx
const StudentsPage = () => (
  <XxxProvider><StudentsTable /></XxxProvider>
);

const StudentsTable = () => {
  const { xxxs, isPending } = useXxxState();
  const { getAllAsync, deleteAsync } = useXxxActions();

  useEffect(() => { getAllAsync(); }, [getAllAsync]);

  return <Table loading={isPending} dataSource={xxxs} rowKey="id" /* … */ />;
};
```

**React to success/error** (close modal, toast, refetch) with an effect on the flags:

```tsx
const { isSuccess, isError } = useXxxState();
const { getAllAsync, resetStateFlags } = useXxxActions();
useEffect(() => {
  if (isSuccess) { message.success("Saved"); closeModal(); getAllAsync(); resetStateFlags(); }
  if (isError)   { resetStateFlags(); } // axios interceptor already showed the error modal
}, [isSuccess, isError]);
```

---

## Conventions & rules

1. **One provider per data domain**, four files, exact filenames: `context.ts`, `actions.ts`, `reducer.ts`, `index.tsx`. Predictability is the point.
2. **Two contexts** — state vs actions. The action context value is memoized so it's referentially stable; depend on action functions in effects safely.
3. **Flag triad on every domain.** SUCCESS does **not** auto-refresh a list — call `getAllAsync()` again (or update the array in the reducer) after a create/update/delete.
4. **`getAxiosInstance()` in a `useRef`** — one instance per provider, interceptors registered once. Don't create it inline in each action.
5. **Errors are centralized** in the axios response interceptor (shows a `Modal.error`). Action `catch` blocks just dispatch `*Error()` to flip the flag — don't double-toast.
6. **`resetStateFlags`** after a consumer reacts to `isSuccess`/`isError`, so the next operation starts clean and the effect doesn't re-fire.
7. **ABP envelope:** responses are `{ result: ... }`; lists are `{ result: { items, totalCount } }`. Always read `data.result`.
8. **Nest providers** when a screen spans domains (`<StudentProvider><EnrollmentProvider>…`). Order doesn't matter unless one's actions call another's.
9. **Cross-cutting state** (auth, current user) mounts once at the root; **feature state** mounts locally and resets on unmount.
10. Keep **server paging/sorting** server-side: pass `{ SkipCount, MaxResultCount, Sorting }` as params to `GetAll` and store `totalCount`.

## When NOT to use this
- **Pure UI/local state** (a toggle, a controlled input) → plain `useState`.
- **Form field validation** → Zod schema (per project rules), separate from this state layer.
- **One-off fetch in a leaf component with no sharing** → a small `useEffect` + `useState` + `getAxiosInstance()` is fine (see analytics-style read-only panels). Reach for the full triad when state is shared, mutated, or needs the loading/error flags across components.

## Dependencies
`redux-actions` (+ `@types/redux-actions`), `react`, the shared `getAxiosInstance()` (see the **abp-react-auth** skill), `antd` for UI feedback.
