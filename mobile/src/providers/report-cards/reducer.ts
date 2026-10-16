import { handleActions } from "redux-actions";
import { ReportCardsActionEnums } from "./actions";
import { INITIAL_STATE, type IReportCard, type IReportCardsStateContext } from "./context";

const shallowMergeHandlers = Object.fromEntries(
  Object.values(ReportCardsActionEnums)
    .filter((action) => action !== ReportCardsActionEnums.cacheReport)
    .map((action) => [
      action,
      (state: IReportCardsStateContext, a: { payload: Partial<IReportCardsStateContext> }) => ({ ...state, ...a.payload }),
    ])
);

export const ReportCardsReducer = handleActions<IReportCardsStateContext, any>(
  {
    ...shallowMergeHandlers,
    // Loading or acknowledging a card both land here, so either clears its own pending flag.
    [ReportCardsActionEnums.cacheReport]: (state, action: { payload: IReportCard }) => ({
      ...state,
      isDetailPending: false,
      isDetailError: false,
      isAcknowledgePending: false,
      reportsById: { ...state.reportsById, [action.payload.id]: action.payload },
    }),
  },
  INITIAL_STATE
);
