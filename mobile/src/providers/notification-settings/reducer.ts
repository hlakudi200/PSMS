import { handleActions } from "redux-actions";
import { INITIAL_STATE, type INotificationSettingsStateContext } from "./context";
import { NotificationSettingsActionEnums } from "./actions";

export const NotificationSettingsReducer = handleActions<INotificationSettingsStateContext, any>(
  Object.fromEntries(
    Object.values(NotificationSettingsActionEnums).map((action) => [
      action,
      (state: INotificationSettingsStateContext, a: { payload: Partial<INotificationSettingsStateContext> }) => ({
        ...state,
        ...a.payload,
      }),
    ])
  ),
  INITIAL_STATE
);
