import { handleActions } from "redux-actions";
import { INITIAL_STATE, IMarkStateContext } from "./context";
import { MarkActionEnums } from "./actions";

export const MarkReducer = handleActions<
  IMarkStateContext,
  IMarkStateContext
>(
  {
    [MarkActionEnums.getMarkPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.getMarkSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.getMarkError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.getAllMarksPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.getAllMarksSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.getAllMarksError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.getByAssessmentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.getByAssessmentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.getByAssessmentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.getByStudentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.getByStudentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.getByStudentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.recordMarkPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.recordMarkSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.recordMarkError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.bulkRecordMarksPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.bulkRecordMarksSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.bulkRecordMarksError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.updateMarkPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.updateMarkSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.updateMarkError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.markAsAbsentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.markAsAbsentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.markAsAbsentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.applyModerationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.applyModerationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.applyModerationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.unlockMarkPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.unlockMarkSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.unlockMarkError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.deleteMarkPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.deleteMarkSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MarkActionEnums.deleteMarkError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
