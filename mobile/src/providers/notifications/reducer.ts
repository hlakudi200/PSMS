import { handleActions } from "redux-actions";
import { INITIAL_STATE, type INotificationsStateContext } from "./context";
import { NotificationsActionEnums } from "./actions";

export const NotificationsReducer = handleActions<INotificationsStateContext, any>(
  Object.fromEntries(
    Object.values(NotificationsActionEnums).map((action) => [
      action,
      (state: INotificationsStateContext, a: { payload: Partial<INotificationsStateContext> }) => ({
        ...state,
        ...a.payload,
      }),
    ])
  ),
  INITIAL_STATE
);
