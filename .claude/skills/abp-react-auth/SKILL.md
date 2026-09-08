---
name: abp-react-auth
description: Implement JWT authentication for an ASP.NET Boilerplate (classic ABP) backend with a React/Next.js + Ant Design frontend — token auth, multi-tenancy, an axios instance with interceptors, a Context+useReducer auth provider, JWT decoding, and a role-gated ProtectedRoute. Use when adding or extending login, sessions, roles, or tenant resolution.
---

# ABP + React Authentication

A battle-tested JWT auth pattern for **classic ASP.NET Boilerplate** (Abp 10.x, NOT Volo) + **Next.js (App Router) / React + Ant Design**, multi-tenant.

## Architecture at a glance

```
Login form ─▶ AuthProvider.loginUser()
  1. POST /api/services/app/Account/IsTenantAvailable {tenancyName}  → resolve tenantId
  2. POST /api/TokenAuth/Authenticate {user,password} + header Abp-TenantId  → { accessToken, encryptedAccessToken }
  3. sessionStorage.setItem('accessToken') ; decode role from JWT
  4. GET /api/services/app/Session/GetCurrentLoginInformations  → { user, tenant }
  ▼
AuthState { jwtToken, currentRole, currentUser, currentTenant, isPending/Success/Error }
  ▼
ProtectedRoute (per portal layout) gates by role ; axios interceptor attaches token + tenant on every request
```

Backend is mostly the ABP template (`TokenAuthController`, `AuthConfigurer`). The work is the frontend provider + the glue.

---

## Backend (ABP template — usually already present)

**`TokenAuthController.Authenticate`** returns `{ AccessToken, EncryptedAccessToken, ExpireInSeconds, UserId }`. Keep `EncryptedAccessToken` — SignalR/websocket auth needs it (ABP's hub auth reads an `enc_auth_token` query param, decrypted server-side).

**`AuthConfigurer`** wires JWT bearer validation. Key points:
- `TokenValidationParameters`: validate issuer signing key, issuer, audience, lifetime; `ClockSkew = TimeSpan.Zero`.
- A `QueryStringTokenResolver` on `OnMessageReceived` lets SignalR clients pass the (encrypted) token via query string for any path starting with `/signalr`.

```csharp
options.Events = new JwtBearerEvents { OnMessageReceived = QueryStringTokenResolver };
// resolver: if path starts with "/signalr" and query has enc_auth_token →
//   context.Token = SimpleStringCipher.Instance.Decrypt(qsAuthToken);
```

**Multi-tenancy:** the client sends `Abp-TenantId` header; ABP resolves the tenant. `Account/IsTenantAvailable` maps a tenancy name → tenant id.

**JWT claims:** ABP emits role(s) under `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` — a **string for one role, an array for multiple**. User id is the `nameidentifier` claim.

---

## Frontend

### 1. Axios instance with interceptors — `utils/axios-instance.ts`

One factory that every API call and the auth provider use. The request interceptor attaches the tenant + bearer token from `sessionStorage`; the response interceptor centralizes 401-redirect and ABP error parsing.

```ts
import axios from "axios";
import { Modal } from "antd";

const baseURL = process.env.NEXT_PUBLIC_BASE_URL ?? "http://localhost:21021";

export const getAxiosInstance = () => {
  const instance = axios.create({ baseURL, headers: { "Content-Type": "application/json;charset=utf-8" } });

  instance.interceptors.request.use((config) => {
    config.headers = config.headers ?? {};
    const tenantId = sessionStorage.getItem("tenantId");
    if (tenantId) config.headers["Abp-TenantId"] = tenantId;          // ABP multi-tenancy
    const token = sessionStorage.getItem("accessToken");
    if (token) config.headers["Authorization"] = `Bearer ${token}`;
    return config;
  });

  instance.interceptors.response.use(
    (r) => r,
    (error) => {
      if (!axios.isAxiosError(error) || !error.response) {
        Modal.error({ title: "Network Error", content: "Unable to connect to the server." });
        return Promise.reject(error);
      }
      const { status, data } = error.response;
      if (status === 401) {                                            // session expired/invalid
        sessionStorage.removeItem("accessToken");
        sessionStorage.removeItem("tenantId");
        if (typeof window !== "undefined" && !window.location.pathname.includes("/auth/login")) {
          window.location.href = "/auth/login";
        }
        return Promise.reject(error);
      }
      // ABP error shape: { error: { message, details, validationErrors:[{message}] } }
      const abpError = data?.error;
      if (abpError?.validationErrors?.length) {
        Modal.error({ title: "Validation Error", content: abpError.validationErrors.map((v) => v.message).join("\n") });
      } else if (abpError) {
        Modal.error({ title: abpError.message || "Request Failed", content: abpError.details || undefined });
      } else {
        Modal.error({ title: `Error ${status}`, content: "An unexpected error occurred." });
      }
      return Promise.reject(error);
    }
  );
  return instance;
};
```

### 2. JWT decoder — `utils/jwt-decoder.ts`

ABP uses the long WS-* claim URIs. Always lowercase the role and handle the string-or-array shape.

```ts
import { jwtDecode } from "jwt-decode";

export enum AbpTokenProperties {
  nameidentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
  name = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
  emailaddress = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
  role = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
}

export const decodeToken = (t: string) => jwtDecode<Record<string, string | string[]>>(t);

export const getRole = (token: string): string => {
  if (!token) return "";
  const role = decodeToken(token)[AbpTokenProperties.role];
  if (Array.isArray(role)) return role[0]?.toLowerCase() ?? "";       // multi-role → primary
  return role ? String(role).toLowerCase() : "";
};

export const getUserId = (token: string): string =>
  token ? `${decodeToken(token)[AbpTokenProperties.nameidentifier]}` : "";
```

### 3. Auth provider — `providers/auth/` (Context + useReducer + redux-actions)

Four files, following the project's standard provider shape (see the **react-state-management** skill for the generic version).

**`context.ts`** — state shape + action interface + two contexts:

```ts
export interface IAuthStateContext {
  isPending: boolean; isSuccess: boolean; isError: boolean;
  currentUser?: IUser; currentTenant?: ITenant; jwtToken?: string; currentRole?: string;
}
export interface IAuthActionContext {
  loginUser: (d: ILoginData) => Promise<void>;
  getCurrentUser: (jwt: string) => Promise<void>;
  signOut: () => void;
  resetStateFlags: () => void;
}
export const INITIAL_STATE: IAuthStateContext = { isPending: false, isSuccess: false, isError: false };
export const AuthStateContext = createContext<IAuthStateContext>(INITIAL_STATE);
export const AuthActionContext = createContext<IAuthActionContext | undefined>(undefined);
```

**`actions.ts`** — `redux-actions` `createAction`; success carries data + flips flags:

```ts
export enum AuthActionEnums {
  loginUserPending = "LOGIN_USER_PENDING", loginUserSuccess = "LOGIN_USER_SUCCESS", loginUserError = "LOGIN_USER_ERROR",
  getCurrentUserPending = "GET_CURRENT_USER_PENDING", getCurrentUserSuccess = "GET_CURRENT_USER_SUCCESS", getCurrentUserError = "GET_CURRENT_USER_ERROR",
  signOutUser = "SIGN_OUT_USER", resetStateFlagsAction = "RESET_STATE_FLAGS",
}
export const loginUserSuccess = createAction<IAuthStateContext, { jwtToken: string; currentRole: string }>(
  AuthActionEnums.loginUserSuccess,
  (p) => ({ isPending: false, isSuccess: true, isError: false, jwtToken: p.jwtToken, currentRole: p.currentRole })
);
export const signOutUser = createAction<IAuthStateContext>(AuthActionEnums.signOutUser, () => ({
  isPending: false, isSuccess: false, isError: false,
  currentUser: undefined, currentTenant: undefined, jwtToken: undefined, currentRole: undefined,
}));
// ...pending/error/getCurrentUser*/resetStateFlags follow the same shape
```

**`reducer.ts`** — `handleActions`, every case merges the payload:

```ts
export const AuthReducer = handleActions<IAuthStateContext>(
  Object.fromEntries(Object.values(AuthActionEnums).map((k) => [k, (s, a) => ({ ...s, ...a.payload })])),
  INITIAL_STATE
);
```

**`index.tsx`** — the provider + hooks + the real login flow:

```tsx
export const AuthProvider = ({ children }) => {
  const [state, dispatch] = useReducer(AuthReducer, INITIAL_STATE);
  const instance = useRef(getAxiosInstance()).current;

  const getCurrentUser = useCallback(async (jwtToken: string) => {
    dispatch(getCurrentUserPending());
    try {
      const { data } = await instance.get("/api/services/app/Session/GetCurrentLoginInformations",
        { headers: { Authorization: `Bearer ${jwtToken}` } });
      const { user, tenant } = data.result;
      dispatch(getCurrentUserSuccess({ currentUser: user, currentTenant: tenant }));
      // Fallback: if the JWT had no role claim, derive from roleNames
      if (!getRole(jwtToken) && user?.roleNames?.length) {
        dispatch(loginUserSuccess({ jwtToken, currentRole: user.roleNames[0].toLowerCase() }));
      }
    } catch { dispatch(getCurrentUserError()); }
  }, [instance]);

  // Bootstrap from a prior session on mount
  useEffect(() => {
    const token = sessionStorage.getItem("accessToken");
    if (token) { dispatch(loginUserSuccess({ jwtToken: token, currentRole: getRole(token) })); getCurrentUser(token); }
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const loginUser = useCallback(async (loginData: ILoginData) => {
    dispatch(loginUserPending());
    try {
      // 1) resolve tenant by tenancy name
      const tenancyName = loginData.tenancyName || "Default";
      const t = (await instance.post("/api/services/app/Account/IsTenantAvailable", { tenancyName })).data.result;
      if (t.state === 1 && t.tenantId) sessionStorage.setItem("tenantId", String(t.tenantId));
      else { dispatch(loginUserError()); return; }
      // 2) authenticate with the tenant header
      const res = await instance.post("/api/TokenAuth/Authenticate",
        { userNameOrEmailAddress: loginData.userNameOrEmailAddress, password: loginData.password },
        { headers: { "Abp-TenantId": sessionStorage.getItem("tenantId") } });
      const token = res.data.result.accessToken;
      sessionStorage.setItem("accessToken", token);
      dispatch(loginUserSuccess({ jwtToken: token, currentRole: getRole(token) }));
      getCurrentUser(token);
    } catch { dispatch(loginUserError()); }
  }, [instance, getCurrentUser]);

  const signOut = useCallback(() => { sessionStorage.clear(); dispatch(signOutUser()); }, []);
  const actions = useMemo(() => ({ loginUser, getCurrentUser, signOut,
    resetStateFlags: () => dispatch(resetStateFlagsAction()) }), [loginUser, getCurrentUser, signOut]);

  return (
    <AuthStateContext.Provider value={state}>
      <AuthActionContext.Provider value={actions}>{children}</AuthActionContext.Provider>
    </AuthStateContext.Provider>
  );
};

export const useAuthState = () => useContext(AuthStateContext);
export const useAuthActions = () => {
  const c = useContext(AuthActionContext);
  if (!c) throw new Error("useAuthActions must be used within an AuthProvider");
  return c;
};
```

### 4. Mount the provider at the root — `app/providers.tsx`

```tsx
export const Providers = ({ children }) => (
  <ConfigProvider theme={...}><AuthProvider>{children}</AuthProvider></ConfigProvider>
);
```
Rendered once in the root layout so every route sees auth state.

### 5. Role-gated `ProtectedRoute` — wrap each portal layout

```tsx
export const ProtectedRoute = ({ children, allowedRoles, redirectTo = "/auth/login" }) => {
  const router = useRouter();
  const { currentUser, jwtToken, currentRole, isPending } = useAuthState();
  const roleRequired = !!allowedRoles?.length;
  const hasRole = roleRequired ? !!currentRole && allowedRoles.some((r) => r.toLowerCase() === currentRole.toLowerCase()) : true;

  useEffect(() => {
    if (isPending) return;
    if (!jwtToken) router.push(redirectTo);
    else if (roleRequired && currentRole && !hasRole) router.push("/unauthorized");
  }, [jwtToken, currentRole, isPending, hasRole, roleRequired]);

  if (isPending || (jwtToken && !currentUser)) return <FullPageSpin />;   // wait for auth to resolve
  if (!jwtToken) return null;                                              // redirecting
  if (roleRequired && (!currentRole || !hasRole)) return null;            // role pending / denied
  return <>{children}</>;
};
```

Each portal's layout wraps content: `<ProtectedRoute allowedRoles={['Admin']}>...`. Login routing after success: read `currentRole` and `router.push('/' + role)` (or a role→home map).

---

## Critical gotchas (the things that bite)

1. **Role claim is string OR array** — always normalize (`getRole` does). Lowercase everywhere for comparisons.
2. **Render-gate on auth resolution** — `ProtectedRoute` must return a loader while `jwtToken && !currentUser`, or pages flash/poll before the token is ready (and any data fetch 401s).
3. **`Abp-TenantId` on every request** — the interceptor handles it; login sets it *before* `Authenticate`. Forgetting it = login against the wrong/host tenant.
4. **Keep `EncryptedAccessToken`** from the auth response if you'll add SignalR — the hub auth needs `enc_auth_token`, not the raw bearer.
5. **`sessionStorage`, not localStorage** here (per-tab session). Bootstrap from it on mount so a refresh restores the session.
6. **The axios factory is called fresh per consumer** — hold it in a `useRef` in the provider so the interceptors aren't re-registered each render.
7. **401 redirect lives in the response interceptor** (hard redirect), independent of React — covers token expiry mid-session.

## Dependencies
`axios`, `jwt-decode`, `redux-actions` (+ `@types/redux-actions`), `antd`, Next.js App Router.
