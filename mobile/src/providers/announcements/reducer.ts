import { handleActions } from "redux-actions";
import { INITIAL_STATE, type IAnnouncementsStateContext } from "./context";
import { AnnouncementsActionEnums } from "./actions";

export const AnnouncementsReducer = handleActions<IAnnouncementsStateContext, any>(
  Object.fromEntries(
    Object.values(AnnouncementsActionEnums).map((action) => [
      action,
      (state: IAnnouncementsStateContext, a: { payload: Partial<IAnnouncementsStateContext> }) => ({
        ...state,
        ...a.payload,
      }),
    ])
  ),
  INITIAL_STATE
);
