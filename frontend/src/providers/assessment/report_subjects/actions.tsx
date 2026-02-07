import { createAction } from "redux-actions";
import { IReportSubjectStateContext } from "./context";
import { IReportSubject, IListResult } from "../shared/interfaces";

export enum ReportSubjectActionEnums {
    getReportSubjectPending = "GET_REPORT_SUBJECT_PENDING",
    getReportSubjectSuccess = "GET_REPORT_SUBJECT_SUCCESS",
    getReportSubjectError = "GET_REPORT_SUBJECT_ERROR",

    getByReportPending = "GET_REPORT_SUBJECTS_BY_REPORT_PENDING",
    getByReportSuccess = "GET_REPORT_SUBJECTS_BY_REPORT_SUCCESS",
    getByReportError = "GET_REPORT_SUBJECTS_BY_REPORT_ERROR",

    recordMarksPending = "RECORD_REPORT_SUBJECT_MARKS_PENDING",
    recordMarksSuccess = "RECORD_REPORT_SUBJECT_MARKS_SUCCESS",
    recordMarksError = "RECORD_REPORT_SUBJECT_MARKS_ERROR",

    bulkRecordMarksPending = "BULK_RECORD_REPORT_SUBJECT_MARKS_PENDING",
    bulkRecordMarksSuccess = "BULK_RECORD_REPORT_SUBJECT_MARKS_SUCCESS",
    bulkRecordMarksError = "BULK_RECORD_REPORT_SUBJECT_MARKS_ERROR",

    addTeacherCommentPending = "ADD_REPORT_SUBJECT_TEACHER_COMMENT_PENDING",
    addTeacherCommentSuccess = "ADD_REPORT_SUBJECT_TEACHER_COMMENT_SUCCESS",
    addTeacherCommentError = "ADD_REPORT_SUBJECT_TEACHER_COMMENT_ERROR",
}

// Get Single ReportSubject Actions
export const getReportSubjectPending = createAction<IReportSubjectStateContext>(
    ReportSubjectActionEnums.getReportSubjectPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getReportSubjectSuccess = createAction<IReportSubjectStateContext, IReportSubject>(
    ReportSubjectActionEnums.getReportSubjectSuccess,
    (reportSubject: IReportSubject) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        reportSubject,
    })
);

export const getReportSubjectError = createAction<IReportSubjectStateContext>(
    ReportSubjectActionEnums.getReportSubjectError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By Report Actions
export const getByReportPending = createAction<IReportSubjectStateContext>(
    ReportSubjectActionEnums.getByReportPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getByReportSuccess = createAction<
    IReportSubjectStateContext,
    IListResult<IReportSubject>
>(
    ReportSubjectActionEnums.getByReportSuccess,
    (result: IListResult<IReportSubject>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        reportSubjects: result.items,
    })
);

export const getByReportError = createAction<IReportSubjectStateContext>(
    ReportSubjectActionEnums.getByReportError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Record Marks Actions
export const recordMarksPending = createAction<IReportSubjectStateContext>(
    ReportSubjectActionEnums.recordMarksPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const recordMarksSuccess = createAction<IReportSubjectStateContext, IReportSubject>(
    ReportSubjectActionEnums.recordMarksSuccess,
    (reportSubject: IReportSubject) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        reportSubject,
    })
);

export const recordMarksError = createAction<IReportSubjectStateContext>(
    ReportSubjectActionEnums.recordMarksError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Bulk Record Marks Actions
export const bulkRecordMarksPending = createAction<IReportSubjectStateContext>(
    ReportSubjectActionEnums.bulkRecordMarksPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const bulkRecordMarksSuccess = createAction<
    IReportSubjectStateContext,
    IListResult<IReportSubject>
>(
    ReportSubjectActionEnums.bulkRecordMarksSuccess,
    (result: IListResult<IReportSubject>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        reportSubjects: result.items,
    })
);

export const bulkRecordMarksError = createAction<IReportSubjectStateContext>(
    ReportSubjectActionEnums.bulkRecordMarksError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);

// Add Teacher Comment Actions
export const addTeacherCommentPending = createAction<IReportSubjectStateContext>(
    ReportSubjectActionEnums.addTeacherCommentPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
);

export const addTeacherCommentSuccess = createAction<IReportSubjectStateContext, IReportSubject>(
    ReportSubjectActionEnums.addTeacherCommentSuccess,
    (reportSubject: IReportSubject) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        reportSubject,
    })
);

export const addTeacherCommentError = createAction<IReportSubjectStateContext>(
    ReportSubjectActionEnums.addTeacherCommentError,
    () => ({ isPending: false, isSuccess: false, isError: true })
);
