import { createAction } from "redux-actions";
import { IAssessmentQuestionStateContext } from "./context";
import { IAssessmentQuestion, IAssessmentQuestionList, IListResult } from "../shared/interfaces";

export enum AssessmentQuestionActionEnums {
  getQuestionPending = "GET_ASSESSMENT_QUESTION_PENDING",
  getQuestionSuccess = "GET_ASSESSMENT_QUESTION_SUCCESS",
  getQuestionError = "GET_ASSESSMENT_QUESTION_ERROR",

  getByAssessmentPending = "GET_QUESTIONS_BY_ASSESSMENT_PENDING",
  getByAssessmentSuccess = "GET_QUESTIONS_BY_ASSESSMENT_SUCCESS",
  getByAssessmentError = "GET_QUESTIONS_BY_ASSESSMENT_ERROR",

  createQuestionPending = "CREATE_ASSESSMENT_QUESTION_PENDING",
  createQuestionSuccess = "CREATE_ASSESSMENT_QUESTION_SUCCESS",
  createQuestionError = "CREATE_ASSESSMENT_QUESTION_ERROR",

  updateQuestionPending = "UPDATE_ASSESSMENT_QUESTION_PENDING",
  updateQuestionSuccess = "UPDATE_ASSESSMENT_QUESTION_SUCCESS",
  updateQuestionError = "UPDATE_ASSESSMENT_QUESTION_ERROR",

  deleteQuestionPending = "DELETE_ASSESSMENT_QUESTION_PENDING",
  deleteQuestionSuccess = "DELETE_ASSESSMENT_QUESTION_SUCCESS",
  deleteQuestionError = "DELETE_ASSESSMENT_QUESTION_ERROR",

  reorderPending = "REORDER_QUESTIONS_PENDING",
  reorderSuccess = "REORDER_QUESTIONS_SUCCESS",
  reorderError = "REORDER_QUESTIONS_ERROR",
}

// Get Single Question Actions
export const getQuestionPending = createAction<IAssessmentQuestionStateContext>(
  AssessmentQuestionActionEnums.getQuestionPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getQuestionSuccess = createAction<IAssessmentQuestionStateContext, IAssessmentQuestion>(
  AssessmentQuestionActionEnums.getQuestionSuccess,
  (question: IAssessmentQuestion) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    question,
  })
);

export const getQuestionError = createAction<IAssessmentQuestionStateContext>(
  AssessmentQuestionActionEnums.getQuestionError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Assessment Actions
export const getByAssessmentPending = createAction<IAssessmentQuestionStateContext>(
  AssessmentQuestionActionEnums.getByAssessmentPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByAssessmentSuccess = createAction<
  IAssessmentQuestionStateContext,
  IListResult<IAssessmentQuestionList>
>(
  AssessmentQuestionActionEnums.getByAssessmentSuccess,
  (result: IListResult<IAssessmentQuestionList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    questions: result.items,
  })
);

export const getByAssessmentError = createAction<IAssessmentQuestionStateContext>(
  AssessmentQuestionActionEnums.getByAssessmentError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Create Question Actions
export const createQuestionPending = createAction<IAssessmentQuestionStateContext>(
  AssessmentQuestionActionEnums.createQuestionPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createQuestionSuccess = createAction<IAssessmentQuestionStateContext, IAssessmentQuestion>(
  AssessmentQuestionActionEnums.createQuestionSuccess,
  (question: IAssessmentQuestion) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    question,
  })
);

export const createQuestionError = createAction<IAssessmentQuestionStateContext>(
  AssessmentQuestionActionEnums.createQuestionError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update Question Actions
export const updateQuestionPending = createAction<IAssessmentQuestionStateContext>(
  AssessmentQuestionActionEnums.updateQuestionPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const updateQuestionSuccess = createAction<IAssessmentQuestionStateContext, IAssessmentQuestion>(
  AssessmentQuestionActionEnums.updateQuestionSuccess,
  (question: IAssessmentQuestion) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    question,
  })
);

export const updateQuestionError = createAction<IAssessmentQuestionStateContext>(
  AssessmentQuestionActionEnums.updateQuestionError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Delete Question Actions
export const deleteQuestionPending = createAction<IAssessmentQuestionStateContext>(
  AssessmentQuestionActionEnums.deleteQuestionPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const deleteQuestionSuccess = createAction<IAssessmentQuestionStateContext>(
  AssessmentQuestionActionEnums.deleteQuestionSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const deleteQuestionError = createAction<IAssessmentQuestionStateContext>(
  AssessmentQuestionActionEnums.deleteQuestionError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Reorder Questions Actions
export const reorderPending = createAction<IAssessmentQuestionStateContext>(
  AssessmentQuestionActionEnums.reorderPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const reorderSuccess = createAction<IAssessmentQuestionStateContext>(
  AssessmentQuestionActionEnums.reorderSuccess,
  () => ({
    isPending: false,
    isSuccess: true,
    isError: false,
  })
);

export const reorderError = createAction<IAssessmentQuestionStateContext>(
  AssessmentQuestionActionEnums.reorderError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);
