"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  PrincipalDashboardActionContext,
  PrincipalDashboardStateContext,
} from "./context";
import { PrincipalDashboardReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getSummaryPending,
  getSummarySuccess,
  getSummaryError,
} from "./actions";

export const PrincipalDashboardProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(PrincipalDashboardReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getSummaryAsync = async (academicYearId?: string) => {
    dispatch(getSummaryPending());
    const params = new URLSearchParams();
    if (academicYearId) params.append("academicYearId", academicYearId);
    const query = params.toString();
    const endpoint = `/api/services/app/PrincipalDashboard/GetSummary${query ? `?${query}` : ""}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getSummarySuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getSummaryError());
      });
  };

  return (
    <PrincipalDashboardStateContext.Provider value={state}>
      <PrincipalDashboardActionContext.Provider value={{ getSummaryAsync }}>
        {children}
      </PrincipalDashboardActionContext.Provider>
    </PrincipalDashboardStateContext.Provider>
  );
};

export const usePrincipalDashboardState = () => {
  const context = useContext(PrincipalDashboardStateContext);
  if (!context) {
    throw new Error("usePrincipalDashboardState must be used within a PrincipalDashboardProvider");
  }
  return context;
};

export const usePrincipalDashboardActions = () => {
  const context = useContext(PrincipalDashboardActionContext);
  if (!context) {
    throw new Error("usePrincipalDashboardActions must be used within a PrincipalDashboardProvider");
  }
  return context;
};
