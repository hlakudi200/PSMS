import { createAction } from "redux-actions";
import { IMarkStateContext } from "./context";
import { IMark, IMarkList, IPagedResult, IListResult } from "../shared/interfaces";

export enum MarkActionEnums {
  getMarkPending = "GET_MARK_PENDING",
  getMarkSuccess = "GET_MARK_SUCCESS",
  getMarkError = "GET_MARK_ERROR",

  getAllMarksPending = "GET_ALL_MARKS_PENDING",
  getAllMarksSuccess = "GET_ALL_MARKS_SUCCESS",
  getAllMarksError = "GET_ALL_MARKS_ERROR",

  getByAssessmentPending = "GET_MARKS_BY_ASSESSMENT_PENDING",
  getByAssessmentSuccess = "GET_MARKS_BY_ASSESSMENT_SUCCESS",
  getByAssessmentError = "GET_MARKS_BY_ASSESSMENT_ERROR",

  getByStudentPending = "GET_MARKS_BY_STUDENT_PENDING",
  getByStudentSuccess = "GET_MARKS_BY_STUDENT_SUCCESS",
  getByStudentError = "GET_MARKS_BY_STUDENT_ERROR",

  recordMarkPending = "RECORD_MARK_PENDING",
  recordMarkSuccess = "RECORD_MARK_SUCCESS",
  recordMarkError = "RECORD_MARK_ERROR",

  bulkRecordMarksPending = "BULK_RECORD_MARKS_PENDING",
  bulkRecordMarksSuccess = "BULK_RECORD_MARKS_SUCCESS",
  bulkRecordMarksError = "BULK_RECORD_MARKS_ERROR",

  updateMarkPending = "UPDATE_MARK_PENDING",
  updateMarkSuccess = "UPDATE_MARK_SUCCESS",
  updateMarkError = "UPDATE_MARK_ERROR",

  markAsAbsentPending = "MARK_AS_ABSENT_PENDING",
  markAsAbsentSuccess = "MARK_AS_ABSENT_SUCCESS",
  markAsAbsentError = "MARK_AS_ABSENT_ERROR",

  applyModerationPending = "APPLY_MODERATION_PENDING",
  applyModerationSuccess = "APPLY_MODERATION_SUCCESS",
  applyModerationError = "APPLY_MODERATION_ERROR",

  unlockMarkPending = "UNLOCK_MARK_PENDING",
  unlockMarkSuccess = "UNLOCK_MARK_SUCCESS",
  unlockMarkError = "UNLOCK_MARK_ERROR",

  deleteMarkPending = "DELETE_MARK_PENDING",
  deleteMarkSuccess = "DELETE_MARK_SUCCESS",
  deleteMarkError = "DELETE_MARK_ERROR",
}

// Get Single Mark Actions
export const getMarkPending = createAction<IMarkStateContext>(
  MarkActionEnums.getMarkPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getMarkSuccess = createAction<IMarkStateContext, IMark>(
  MarkActionEnums.getMarkSuccess,
  (mark: IMark) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    mark,
  })
);

export const getMarkError = createAction<IMarkStateContext>(
  MarkActionEnums.getMarkError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All Marks Actions
export const getAllMarksPending = createAction<IMarkStateContext>(
  MarkActionEnums.getAllMarksPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllMarksSuccess = createAction<
  IMarkStateContext,
  IPagedResult<IMarkList>
>(
  MarkActionEnums.getAllMarksSuccess,
  (result: IPagedResult<IMarkList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    marks: result.items,
    totalCount: result.totalCount,
  })
);

export const getAllMarksError = createAction<IMarkStateContext>(
  MarkActionEnums.getAllMarksError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Assessment Actions
export const getByAssessmentPending = createAction<IMarkStateContext>(
  MarkActionEnums.getByAssessmentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByAssessmentSuccess = createAction<
  IMarkStateContext,
  IListResult<IMarkList>
>(
  MarkActionEnums.getByAssessmentSuccess,
  (result: IListResult<IMarkList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    marks: result.items,
  })
);

export const getByAssessmentError = createAction<IMarkStateContext>(
  MarkActionEnums.getByAssessmentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Student Actions
export const getByStudentPending = createAction<IMarkStateContext>(
  MarkActionEnums.getByStudentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByStudentSuccess = createAction<
  IMarkStateContext,
  IListResult<IMarkList>
>(
  MarkActionEnums.getByStudentSuccess,
  (result: IListResult<IMarkList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    marks: result.items,
  })
);

export const getByStudentError = createAction<IMarkStateContext>(
  MarkActionEnums.getByStudentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Record Mark Actions
export const recordMarkPending = createAction<IMarkStateContext>(
  MarkActionEnums.recordMarkPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const recordMarkSuccess = createAction<IMarkStateContext, IMark>(
  MarkActionEnums.recordMarkSuccess,
  (mark: IMark) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    mark,
  })
);

export const recordMarkError = createAction<IMarkStateContext>(
  MarkActionEnums.recordMarkError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Bulk Record Marks Actions
export const bulkRecordMarksPending = createAction<IMarkStateContext>(
  MarkActionEnums.bulkRecordMarksPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const bulkRecordMarksSuccess = createAction<
  IMarkStateContext,
  IListResult<IMark>
>(
  MarkActionEnums.bulkRecordMarksSuccess,
  (result: IListResult<IMark>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const bulkRecordMarksError = createAction<IMarkStateContext>(
  MarkActionEnums.bulkRecordMarksError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Mark Actions
export const updateMarkPending = createAction<IMarkStateContext>(
  MarkActionEnums.updateMarkPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateMarkSuccess = createAction<IMarkStateContext, IMark>(
  MarkActionEnums.updateMarkSuccess,
  (mark: IMark) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    mark,
  })
);

export const updateMarkError = createAction<IMarkStateContext>(
  MarkActionEnums.updateMarkError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Mark As Absent Actions
export const markAsAbsentPending = createAction<IMarkStateContext>(
  MarkActionEnums.markAsAbsentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const markAsAbsentSuccess = createAction<IMarkStateContext, IMark>(
  MarkActionEnums.markAsAbsentSuccess,
  (mark: IMark) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    mark,
  })
);

export const markAsAbsentError = createAction<IMarkStateContext>(
  MarkActionEnums.markAsAbsentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Apply Moderation Actions
export const applyModerationPending = createAction<IMarkStateContext>(
  MarkActionEnums.applyModerationPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const applyModerationSuccess = createAction<IMarkStateContext, IMark>(
  MarkActionEnums.applyModerationSuccess,
  (mark: IMark) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    mark,
  })
);

export const applyModerationError = createAction<IMarkStateContext>(
  MarkActionEnums.applyModerationError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Unlock Mark Actions
export const unlockMarkPending = createAction<IMarkStateContext>(
  MarkActionEnums.unlockMarkPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const unlockMarkSuccess = createAction<IMarkStateContext, IMark>(
  MarkActionEnums.unlockMarkSuccess,
  (mark: IMark) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    mark,
  })
);

export const unlockMarkError = createAction<IMarkStateContext>(
  MarkActionEnums.unlockMarkError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Mark Actions
export const deleteMarkPending = createAction<IMarkStateContext>(
  MarkActionEnums.deleteMarkPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteMarkSuccess = createAction<IMarkStateContext>(
  MarkActionEnums.deleteMarkSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteMarkError = createAction<IMarkStateContext>(
  MarkActionEnums.deleteMarkError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
