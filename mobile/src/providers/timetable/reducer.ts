import { handleActions } from "redux-actions";
import { INITIAL_STATE, type ITimetableStateContext } from "./context";
import { TimetableActionEnums } from "./actions";

export const TimetableReducer = handleActions<ITimetableStateContext, any>(
  Object.fromEntries(
    Object.values(TimetableActionEnums).map((action) => [
      action,
      (state: ITimetableStateContext, a: { payload: Partial<ITimetableStateContext> }) => ({ ...state, ...a.payload }),
    ])
  ),
  INITIAL_STATE
);
