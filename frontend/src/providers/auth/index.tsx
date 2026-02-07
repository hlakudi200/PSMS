"use client";

import { useContext, useEffect, useReducer } from "react";
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
  const instance = getAxiosInstance();

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

  const loginUser = async (loginData: ILoginData) => {
    dispatch(loginUserPending());

    const endpoint = `/api/TokenAuth/Authenticate`;

    await instance
      .post(endpoint, loginData)
      .then((response) => {
        const token = response.data.result.accessToken;
        sessionStorage.setItem("accessToken", token);
        const role = getRole(token);
        dispatch(loginUserSuccess({ jwtToken: token, currentRole: role }));
        getCurrentUser(token);
      })
      .catch((error) => {
        console.error("Login failed:", error);
        dispatch(loginUserError());
      });
  };

  const getCurrentUser = async (jwtToken: string) => {
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
  };

  const signOut = () => {
    sessionStorage.clear();
    dispatch(signOutUser());
  };

  const resetStateFlags = () => {
    dispatch(resetStateFlagsAction());
  };

  return (
    <AuthStateContext.Provider value={state}>
      <AuthActionContext.Provider
        value={{
          loginUser,
          getCurrentUser,
          signOut,
          resetStateFlags,
        }}
      >
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
