import { createAction } from "redux-actions";
import { IPrincipalDashboardStateContext } from "./context";
import type { IPrincipalDashboardSummary } from "../shared/interfaces";

export enum PrincipalDashboardActionEnums {
  getSummaryPending = "GET_PRINCIPAL_DASHBOARD_SUMMARY_PENDING",
  getSummarySuccess = "GET_PRINCIPAL_DASHBOARD_SUMMARY_SUCCESS",
  getSummaryError = "GET_PRINCIPAL_DASHBOARD_SUMMARY_ERROR",
}

export const getSummaryPending = createAction<IPrincipalDashboardStateContext>(
  PrincipalDashboardActionEnums.getSummaryPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);
export const getSummarySuccess = createAction<IPrincipalDashboardStateContext, IPrincipalDashboardSummary>(
  PrincipalDashboardActionEnums.getSummarySuccess,
  (summary: IPrincipalDashboardSummary) => ({
    isPending: false, isSuccess: true, isError: false, summary,
  })
);
export const getSummaryError = createAction<IPrincipalDashboardStateContext>(
  PrincipalDashboardActionEnums.getSummaryError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
