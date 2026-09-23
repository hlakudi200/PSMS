"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  ReportSubjectActionContext,
  ReportSubjectStateContext,
} from "./context";
import {
  IRecordReportSubjectMarks,
  IBulkRecordReportSubjectMarks,
} from "../shared/interfaces";
import { ReportSubjectReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
    getReportSubjectPending,
    getReportSubjectSuccess,
    getReportSubjectError,
    getByReportPending,
    getByReportSuccess,
    getByReportError,
    recordMarksPending,
    recordMarksSuccess,
    recordMarksError,
    bulkRecordMarksPending,
    bulkRecordMarksSuccess,
    bulkRecordMarksError,
    addTeacherCommentPending,
    addTeacherCommentSuccess,
    addTeacherCommentError,
} from "./actions";

export const ReportSubjectProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(ReportSubjectReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getReportSubjectPending());
    const endpoint = `/api/services/app/ReportSubject/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getReportSubjectSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getReportSubjectError());
        throw error;
      });
  };

  const getByReportAsync = async (reportId: string) => {
    dispatch(getByReportPending());
    const endpoint = `/api/services/app/ReportSubject/GetByReport?reportId=${reportId}`;
    await instance
        .get(endpoint)
        .then((response) => {
            dispatch(getByReportSuccess({
                items: response.data.result.items
            }));
        })
        .catch((error) => {
            console.error(error);
            dispatch(getByReportError());
        throw error;
        });
    };

  const recordMarksAsync = async (input: IRecordReportSubjectMarks) => {
    dispatch(recordMarksPending());
    const endpoint = `/api/services/app/ReportSubject/RecordMarks`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(recordMarksSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(recordMarksError());
        throw error;
      });
  };

  const bulkRecordMarksAsync = async (input: IBulkRecordReportSubjectMarks) => {
    dispatch(bulkRecordMarksPending());
    const endpoint = `/api/services/app/ReportSubject/BulkRecordMarks`;
    await instance
        .post(endpoint, input)
        .then((response) => {
            dispatch(bulkRecordMarksSuccess({
                items: response.data.result.items
            }));
        })
        .catch((error) => {
            console.error(error);
            dispatch(bulkRecordMarksError());
        throw error;
        });
    };

  const addTeacherCommentAsync = async (id: string, comment: string) => {
    dispatch(addTeacherCommentPending());
    const endpoint = `/api/services/app/ReportSubject/AddTeacherComment?id=${id}`;
    await instance
      .post(endpoint, { comment })
      .then((response) => {
        dispatch(addTeacherCommentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(addTeacherCommentError());
        throw error;
      });
  };

  return (
    <ReportSubjectStateContext.Provider value={state}>
      <ReportSubjectActionContext.Provider
        value={{
          getAsync,
          getByReportAsync,
          recordMarksAsync,
          bulkRecordMarksAsync,
          addTeacherCommentAsync,
        }}
      >
        {children}
      </ReportSubjectActionContext.Provider>
    </ReportSubjectStateContext.Provider>
  );
};

export const useReportSubjectState = () => {
  const context = useContext(ReportSubjectStateContext);
  if (!context) {
    throw new Error("useReportSubjectState must be used within a ReportSubjectProvider");
  }
  return context;
};

export const useReportSubjectActions = () => {
  const context = useContext(ReportSubjectActionContext);
  if (!context) {
    throw new Error("useReportSubjectActions must be used within a ReportSubjectProvider");
  }
  return context;
};
