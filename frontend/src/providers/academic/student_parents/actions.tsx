import { createAction } from "redux-actions";
import { IStudentParentStateContext } from "./context";
import { IStudentParent, IListResult } from "../shared/interfaces";

export enum StudentParentActionEnums {
  getByStudentPending = "GET_STUDENT_PARENTS_BY_STUDENT_PENDING",
  getByStudentSuccess = "GET_STUDENT_PARENTS_BY_STUDENT_SUCCESS",
  getByStudentError = "GET_STUDENT_PARENTS_BY_STUDENT_ERROR",

  getByParentPending = "GET_STUDENT_PARENTS_BY_PARENT_PENDING",
  getByParentSuccess = "GET_STUDENT_PARENTS_BY_PARENT_SUCCESS",
  getByParentError = "GET_STUDENT_PARENTS_BY_PARENT_ERROR",

  linkPending = "LINK_STUDENT_PARENT_PENDING",
  linkSuccess = "LINK_STUDENT_PARENT_SUCCESS",
  linkError = "LINK_STUDENT_PARENT_ERROR",

  updateLinkPending = "UPDATE_LINK_STUDENT_PARENT_PENDING",
  updateLinkSuccess = "UPDATE_LINK_STUDENT_PARENT_SUCCESS",
  updateLinkError = "UPDATE_LINK_STUDENT_PARENT_ERROR",

  unlinkPending = "UNLINK_STUDENT_PARENT_PENDING",
  unlinkSuccess = "UNLINK_STUDENT_PARENT_SUCCESS",
  unlinkError = "UNLINK_STUDENT_PARENT_ERROR",
}

// Get By Student Actions
export const getByStudentPending = createAction<IStudentParentStateContext>(
  StudentParentActionEnums.getByStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByStudentSuccess = createAction<
  IStudentParentStateContext,
  IListResult<IStudentParent>
>(
  StudentParentActionEnums.getByStudentSuccess,
  (result: IListResult<IStudentParent>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentParents: result.items,
  })
);

export const getByStudentError = createAction<IStudentParentStateContext>(
  StudentParentActionEnums.getByStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Parent Actions
export const getByParentPending = createAction<IStudentParentStateContext>(
  StudentParentActionEnums.getByParentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByParentSuccess = createAction<
  IStudentParentStateContext,
  IListResult<IStudentParent>
>(
  StudentParentActionEnums.getByParentSuccess,
  (result: IListResult<IStudentParent>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentParents: result.items,
  })
);

export const getByParentError = createAction<IStudentParentStateContext>(
  StudentParentActionEnums.getByParentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Link Actions
export const linkPending = createAction<IStudentParentStateContext>(
  StudentParentActionEnums.linkPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const linkSuccess = createAction<IStudentParentStateContext, IStudentParent>(
  StudentParentActionEnums.linkSuccess,
  (studentParent: IStudentParent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentParent,
  })
);

export const linkError = createAction<IStudentParentStateContext>(
  StudentParentActionEnums.linkError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Link Actions
export const updateLinkPending = createAction<IStudentParentStateContext>(
  StudentParentActionEnums.updateLinkPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateLinkSuccess = createAction<IStudentParentStateContext, IStudentParent>(
  StudentParentActionEnums.updateLinkSuccess,
  (studentParent: IStudentParent) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    studentParent,
  })
);

export const updateLinkError = createAction<IStudentParentStateContext>(
  StudentParentActionEnums.updateLinkError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Unlink Actions
export const unlinkPending = createAction<IStudentParentStateContext>(
  StudentParentActionEnums.unlinkPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const unlinkSuccess = createAction<IStudentParentStateContext>(
  StudentParentActionEnums.unlinkSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const unlinkError = createAction<IStudentParentStateContext>(
  StudentParentActionEnums.unlinkError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
