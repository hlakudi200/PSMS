import { useCallback, useContext, useEffect, useMemo, useReducer, useRef } from "react";
import { getApiErrorMessage, getAxiosInstance } from "../../utils/axios-instance";
import { clearSession, getSession, saveSession } from "../../utils/secure-session";
import { getMobileRole } from "../../utils/jwt-decoder";
import { AuthReducer } from "./reducer";
import { AuthActionContext, AuthStateContext, INITIAL_STATE, type ILoginData } from "./context";
import {
  bootstrapComplete, getCurrentUserError, getCurrentUserPending, getCurrentUserSuccess,
  loginUserError, loginUserPending, loginUserSuccess, resetStateFlagsAction, signOutUser,
} from "./actions";

const MOBILE_ONLY_MESSAGE = "This mobile application is available to Students and Parents only.";

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(AuthReducer, INITIAL_STATE);
  const signOut = useCallback(async () => {
    await clearSession();
    dispatch(signOutUser());
  }, []);
  const signOutRef = useRef(signOut);
  signOutRef.current = signOut;
  const instance = useRef(getAxiosInstance(() => { void signOutRef.current(); })).current;

  const getCurrentUser = useCallback(async (jwtToken: string) => {
    dispatch(getCurrentUserPending());
    try {
      const response = await instance.get("/api/services/app/Session/GetCurrentLoginInformations", {
        headers: { Authorization: `Bearer ${jwtToken}` },
      });
      const { user, tenant } = response.data.result;
      const mobileRole = getMobileRole(jwtToken, user?.roleNames);
      if (!mobileRole) {
        await signOut();
        dispatch(loginUserError({ errorMessage: MOBILE_ONLY_MESSAGE, isAccessDenied: true }));
        return;
      }
      dispatch(loginUserSuccess({ jwtToken, currentRole: mobileRole }));
      dispatch(getCurrentUserSuccess({ currentUser: user, currentTenant: tenant }));
    } catch (error) {
      dispatch(getCurrentUserError(getApiErrorMessage(error, "Unable to restore your session.")));
    }
  }, [instance, signOut]);

  useEffect(() => {
    const restoreSession = async () => {
      try {
        const session = await getSession();
        if (session) await getCurrentUser(session.accessToken);
      } finally {
        dispatch(bootstrapComplete());
      }
    };
    void restoreSession();
  }, [getCurrentUser]);

  const loginUser = useCallback(async (loginData: ILoginData) => {
    dispatch(loginUserPending());
    try {
      const tenancyName = loginData.tenancyName?.trim() || "Default";
      const tenantResponse = await instance.post("/api/services/app/Account/IsTenantAvailable", { tenancyName });
      const tenantId = tenantResponse.data.result?.tenantId;
      if (tenantResponse.data.result?.state !== 1 || !tenantId) {
        dispatch(loginUserError({ errorMessage: "This school is not available." }));
        return;
      }

      const authResponse = await instance.post(
        "/api/TokenAuth/Authenticate",
        { userNameOrEmailAddress: loginData.userNameOrEmailAddress, password: loginData.password },
        { headers: { "Abp-TenantId": String(tenantId) } }
      );
      const result = authResponse.data.result;
      const mobileRole = getMobileRole(result.accessToken);
      if (!mobileRole) {
        await clearSession();
        dispatch(loginUserError({ errorMessage: MOBILE_ONLY_MESSAGE, isAccessDenied: true }));
        return;
      }

      await saveSession({ accessToken: result.accessToken, encryptedAccessToken: result.encryptedAccessToken, tenantId: String(tenantId) });
      dispatch(loginUserSuccess({ jwtToken: result.accessToken, currentRole: mobileRole }));
      await getCurrentUser(result.accessToken);
    } catch (error) {
      dispatch(loginUserError({ errorMessage: getApiErrorMessage(error, "Unable to sign in. Please try again.") }));
    }
  }, [getCurrentUser, instance]);

  const resetStateFlags = useCallback(() => dispatch(resetStateFlagsAction()), []);
  const actions = useMemo(() => ({ loginUser, getCurrentUser, signOut, resetStateFlags }), [getCurrentUser, loginUser, resetStateFlags, signOut]);

  return <AuthStateContext.Provider value={state}><AuthActionContext.Provider value={actions}>{children}</AuthActionContext.Provider></AuthStateContext.Provider>;
};

export const useAuthState = () => useContext(AuthStateContext);
export const useAuthActions = () => {
  const context = useContext(AuthActionContext);
  if (!context) throw new Error("useAuthActions must be used within an AuthProvider");
  return context;
};
