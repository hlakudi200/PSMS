import { createAction } from "redux-actions";
import type { IChildSummary, IChildrenStateContext } from "./context";

type ChildrenPatch = Partial<IChildrenStateContext>;

export enum ChildrenActionEnums {
  getPending = "CHILDREN_GET_PENDING",
  getSuccess = "CHILDREN_GET_SUCCESS",
  getError = "CHILDREN_GET_ERROR",
  selectChild = "CHILDREN_SELECT_CHILD",
}

export const getPending = createAction<ChildrenPatch>(ChildrenActionEnums.getPending, () => ({
  isPending: true, isError: false,
}));

export const getSuccess = createAction<
  ChildrenPatch,
  {
    myChildren: IChildSummary[];
    selectedChildId?: string;
    unreadAnnouncements?: number;
    unreadNotifications?: number;
  }
>(
  ChildrenActionEnums.getSuccess,
  ({ myChildren, selectedChildId, unreadAnnouncements, unreadNotifications }) => ({
    isPending: false, isError: false, myChildren, selectedChildId, unreadAnnouncements, unreadNotifications,
  })
);

export const getError = createAction<ChildrenPatch>(ChildrenActionEnums.getError, () => ({
  isPending: false, isError: true,
}));

export const selectChild = createAction<ChildrenPatch, string>(
  ChildrenActionEnums.selectChild,
  (selectedChildId) => ({ selectedChildId })
);
