import { handleActions } from "redux-actions";
import { INITIAL_STATE, IAnnouncementReadStateContext } from "./context";
import { AnnouncementReadActionEnums } from "./actions";

export const AnnouncementReadReducer = handleActions<
  IAnnouncementReadStateContext,
  IAnnouncementReadStateContext
>(
  {
    [AnnouncementReadActionEnums.markAsReadPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementReadActionEnums.markAsReadSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementReadActionEnums.markAsReadError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementReadActionEnums.getByAnnouncementPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementReadActionEnums.getByAnnouncementSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementReadActionEnums.getByAnnouncementError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementReadActionEnums.getUnreadCountPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementReadActionEnums.getUnreadCountSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementReadActionEnums.getUnreadCountError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
