"use client";

import { useCallback, useContext, useEffect, useMemo, useReducer, useRef } from "react";
import { AuthReducer } from "./reducer";
import {
  AuthActionContext,
  AuthStateContext,
  ILoginData,
  INITIAL_STATE,
} from "./context";
import {
  getCurrentUserError,
  getCurrentUserPending,
  getCurrentUserSuccess,
  loginUserError,
  loginUserPending,
  loginUserSuccess,
  resetStateFlagsAction,
  signOutUser,
} from "./actions";
import { getAxiosInstance } from "@/utils/axios-instance";
import { getRole } from "@/utils/jwt-decoder";

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(AuthReducer, INITIAL_STATE);
  const instanceRef = useRef(getAxiosInstance());
  const instance = instanceRef.current;

  const getCurrentUser = useCallback(async (jwtToken: string) => {
    dispatch(getCurrentUserPending());

    const endpoint = `/api/services/app/Session/GetCurrentLoginInformations`;

    await instance
      .get(endpoint, {
        headers: {
          Authorization: `Bearer ${jwtToken}`,
        },
      })
      .then((response) => {
        if (response.status === 200 && response.data) {
          const { user, tenant } = response.data.result;
          dispatch(
            getCurrentUserSuccess({
              currentUser: user,
              currentTenant: tenant,
            })
          );
        }
      })
      .catch((error) => {
        console.error("Get current user failed:", error);
        dispatch(getCurrentUserError());
      });
  }, [instance]);

  useEffect(() => {
    const token = sessionStorage.getItem("accessToken");
    if (token) {
      const role = getRole(token);
      dispatch(
        loginUserSuccess({ jwtToken: token, currentRole: role })
      );
      getCurrentUser(token);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const loginUser = useCallback(async (loginData: ILoginData) => {
    dispatch(loginUserPending());

    try {
      // Step 1: Resolve tenant by school name
      const tenancyName = loginData.tenancyName || "Default";
      {
        const tenantResponse = await instance.post(
          `/api/services/app/Account/IsTenantAvailable`,
          { tenancyName }
        );
        const tenantResult = tenantResponse.data.result;
        if (tenantResult.state === 1 && tenantResult.tenantId) {
          sessionStorage.setItem("tenantId", String(tenantResult.tenantId));
        } else {
          console.error("Tenant not available:", tenantResult);
          dispatch(loginUserError());
          return;
        }
      }

      // Step 2: Authenticate with explicit tenant header
      const storedTenantId = sessionStorage.getItem("tenantId");
      const endpoint = `/api/TokenAuth/Authenticate`;
      const response = await instance.post(endpoint, {
        userNameOrEmailAddress: loginData.userNameOrEmailAddress,
        password: loginData.password,
      }, {
        headers: {
          ...(storedTenantId ? { "Abp-TenantId": storedTenantId } : {}),
        },
      });

      const token = response.data.result.accessToken;
      sessionStorage.setItem("accessToken", token);
      const role = getRole(token);
      dispatch(loginUserSuccess({ jwtToken: token, currentRole: role }));
      getCurrentUser(token);
    } catch (error) {
      console.error("Login failed:", error);
      dispatch(loginUserError());
    }
  }, [instance, getCurrentUser]);

  const signOut = useCallback(() => {
    sessionStorage.clear();
    dispatch(signOutUser());
  }, []);

  const resetStateFlags = useCallback(() => {
    dispatch(resetStateFlagsAction());
  }, []);

  const actions = useMemo(() => ({
    loginUser,
    getCurrentUser,
    signOut,
    resetStateFlags,
  }), [loginUser, getCurrentUser, signOut, resetStateFlags]);

  return (
    <AuthStateContext.Provider value={state}>
      <AuthActionContext.Provider value={actions}>
        {children}
      </AuthActionContext.Provider>
    </AuthStateContext.Provider>
  );
};

export const useAuthState = () => {
  const context = useContext(AuthStateContext);
  if (!context) {
    throw new Error("useAuthState must be used within an AuthProvider");
  }
  return context;
};

export const useAuthActions = () => {
  const context = useContext(AuthActionContext);
  if (!context) {
    throw new Error("useAuthActions must be used within an AuthProvider");
  }
  return context;
};
