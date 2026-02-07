import { handleActions } from "redux-actions";
import { INITIAL_STATE, IAnnouncementStateContext } from "./context";
import { AnnouncementActionEnums } from "./actions";

export const AnnouncementReducer = handleActions<
  IAnnouncementStateContext,
  IAnnouncementStateContext
>(
  {
    [AnnouncementActionEnums.getAnnouncementPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.getAnnouncementSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.getAnnouncementError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.getAllAnnouncementsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.getAllAnnouncementsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.getAllAnnouncementsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.createAnnouncementPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.createAnnouncementSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.createAnnouncementError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.updateAnnouncementPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.updateAnnouncementSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.updateAnnouncementError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.deleteAnnouncementPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.deleteAnnouncementSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.deleteAnnouncementError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.publishAnnouncementPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.publishAnnouncementSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.publishAnnouncementError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.unpublishAnnouncementPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.unpublishAnnouncementSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.unpublishAnnouncementError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.pinAnnouncementPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.pinAnnouncementSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.pinAnnouncementError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.unpinAnnouncementPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.unpinAnnouncementSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AnnouncementActionEnums.unpinAnnouncementError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
