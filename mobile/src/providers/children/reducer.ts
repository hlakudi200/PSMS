import { handleActions } from "redux-actions";
import { INITIAL_STATE, type IChildrenStateContext } from "./context";
import { ChildrenActionEnums } from "./actions";

export const ChildrenReducer = handleActions<IChildrenStateContext, any>(
  Object.fromEntries(
    Object.values(ChildrenActionEnums).map((action) => [
      action,
      (state: IChildrenStateContext, a: { payload: Partial<IChildrenStateContext> }) => ({ ...state, ...a.payload }),
    ])
  ),
  INITIAL_STATE
);
