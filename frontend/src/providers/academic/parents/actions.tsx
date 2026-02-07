import { createAction } from "redux-actions";
import { IParentStateContext } from "./context";
import { IParent, IPagedResult } from "../shared/interfaces";

export enum ParentActionEnums {
  getParentsPending = "GET_PARENTS_PENDING",
  getParentsSuccess = "GET_PARENTS_SUCCESS",
  getParentsError = "GET_PARENTS_ERROR",

  getParentPending = "GET_PARENT_PENDING",
  getParentSuccess = "GET_PARENT_SUCCESS",
  getParentError = "GET_PARENT_ERROR",

  createParentPending = "CREATE_PARENT_PENDING",
  createParentSuccess = "CREATE_PARENT_SUCCESS",
  createParentError = "CREATE_PARENT_ERROR",

  updateParentPending = "UPDATE_PARENT_PENDING",
  updateParentSuccess = "UPDATE_PARENT_SUCCESS",
  updateParentError = "UPDATE_PARENT_ERROR",

  deleteParentPending = "DELETE_PARENT_PENDING",
  deleteParentSuccess = "DELETE_PARENT_SUCCESS",
  deleteParentError = "DELETE_PARENT_ERROR",
}

// Get All Parents Actions
export const getParentsPending = createAction<IParentStateContext>(
  ParentActionEnums.getParentsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getParentsSuccess = createAction<
  IParentStateContext,
  IPagedResult<IParent>
>(
  ParentActionEnums.getParentsSuccess,
  (result: IPagedResult<IParent>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    parents: result.items,
    totalCount: result.totalCount,
  })
);

export const getParentsError = createAction<IParentStateContext>(
  ParentActionEnums.getParentsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get Single Parent Actions
export const getParentPending = createAction<IParentStateContext>(
  ParentActionEnums.getParentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getParentSuccess = createAction<IParentStateContext, IParent>(
  ParentActionEnums.getParentSuccess,
  (parent: IParent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    parent,
  })
);

export const getParentError = createAction<IParentStateContext>(
  ParentActionEnums.getParentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Parent Actions
export const createParentPending = createAction<IParentStateContext>(
  ParentActionEnums.createParentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createParentSuccess = createAction<IParentStateContext, IParent>(
  ParentActionEnums.createParentSuccess,
  (parent: IParent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    parent,
  })
);

export const createParentError = createAction<IParentStateContext>(
  ParentActionEnums.createParentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Parent Actions
export const updateParentPending = createAction<IParentStateContext>(
  ParentActionEnums.updateParentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateParentSuccess = createAction<IParentStateContext, IParent>(
  ParentActionEnums.updateParentSuccess,
  (parent: IParent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    parent,
  })
);

export const updateParentError = createAction<IParentStateContext>(
  ParentActionEnums.updateParentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Parent Actions
export const deleteParentPending = createAction<IParentStateContext>(
  ParentActionEnums.deleteParentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteParentSuccess = createAction<IParentStateContext>(
  ParentActionEnums.deleteParentSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteParentError = createAction<IParentStateContext>(
  ParentActionEnums.deleteParentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
