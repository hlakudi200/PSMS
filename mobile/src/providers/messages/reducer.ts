import { handleActions } from "redux-actions";
import { INITIAL_STATE, type IMessagesStateContext } from "./context";
import { MessagesActionEnums } from "./actions";

export const MessagesReducer = handleActions<IMessagesStateContext, any>(
  Object.fromEntries(
    Object.values(MessagesActionEnums).map((action) => [
      action,
      (state: IMessagesStateContext, a: { payload: Partial<IMessagesStateContext> }) => ({ ...state, ...a.payload }),
    ])
  ),
  INITIAL_STATE
);
