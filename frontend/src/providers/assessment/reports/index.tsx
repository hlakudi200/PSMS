"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  ReportActionContext,
  ReportStateContext,
} from "./context";
import {
  IGenerateReport,
  IGetReportsInput,
  IReportComment,
} from "../shared/interfaces";
import { ReportReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getReportPending,
  getReportSuccess,
  getReportError,
  getAllReportsPending,
  getAllReportsSuccess,
  getAllReportsError,
  getByStudentTermPending,
  getByStudentTermSuccess,
  getByStudentTermError,
  generateReportPending,
  generateReportSuccess,
  generateReportError,
  submitForApprovalPending,
  submitForApprovalSuccess,
  submitForApprovalError,
  approveReportPending,
  approveReportSuccess,
  approveReportError,
  publishReportPending,
  publishReportSuccess,
  publishReportError,
  addTeacherCommentPending,
  addTeacherCommentSuccess,
  addTeacherCommentError,
  addPrincipalCommentPending,
  addPrincipalCommentSuccess,
  addPrincipalCommentError,
  acknowledgeByParentPending,
  acknowledgeByParentSuccess,
  acknowledgeByParentError,
  recordPromotionPending,
  recordPromotionSuccess,
  recordPromotionError,
  deleteReportPending,
  deleteReportSuccess,
  deleteReportError,
} from "./actions";

export const ReportProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(ReportReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getReportPending());
    const endpoint = `/api/services/app/Report/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getReportSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getReportError());
      });
  };

  const getAllAsync = async (input?: IGetReportsInput) => {
    dispatch(getAllReportsPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.studentId) params.append('StudentId', input.studentId);
    if (input?.classId) params.append('ClassId', input.classId);
    if (input?.termId) params.append('TermId', input.termId);
    if (input?.academicYearId) params.append('AcademicYearId', input.academicYearId);
    if (input?.reportType !== undefined) params.append('ReportType', input.reportType.toString());
    if (input?.status !== undefined) params.append('Status', input.status.toString());
    if (input?.studentName) params.append('StudentName', input.studentName);

    const endpoint = `/api/services/app/Report/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllReportsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllReportsError());
      });
  };

  const getByStudentTermAsync = async (studentId: string, termId: string, reportType: number) => {
    dispatch(getByStudentTermPending());

    const params = new URLSearchParams();
    params.append('studentId', studentId);
    params.append('termId', termId);
    params.append('reportType', reportType.toString());

    const endpoint = `/api/services/app/Report/GetByStudentTerm?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByStudentTermSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByStudentTermError());
      });
  };

  const generateAsync = async (input: IGenerateReport) => {
    dispatch(generateReportPending());
    const endpoint = `/api/services/app/Report/Generate`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(generateReportSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(generateReportError());
      });
  };

  const submitForApprovalAsync = async (id: string) => {
    dispatch(submitForApprovalPending());
    const endpoint = `/api/services/app/Report/SubmitForApproval`;
    await instance
      .post(endpoint, { id })
      .then((response) => {
        dispatch(submitForApprovalSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(submitForApprovalError());
      });
  };

  const approveAsync = async (id: string) => {
    dispatch(approveReportPending());
    const endpoint = `/api/services/app/Report/Approve`;
    await instance
      .post(endpoint, { id })
      .then((response) => {
        dispatch(approveReportSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(approveReportError());
      });
  };

  const publishAsync = async (id: string) => {
    dispatch(publishReportPending());
    const endpoint = `/api/services/app/Report/Publish`;
    await instance
      .post(endpoint, { id })
      .then((response) => {
        dispatch(publishReportSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(publishReportError());
      });
  };

  const addTeacherCommentAsync = async (id: string, input: IReportComment) => {
    dispatch(addTeacherCommentPending());
    const endpoint = `/api/services/app/Report/AddTeacherComment`;
    await instance
      .post(endpoint, { id, ...input })
      .then((response) => {
        dispatch(addTeacherCommentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(addTeacherCommentError());
      });
  };

  const addPrincipalCommentAsync = async (id: string, input: IReportComment) => {
    dispatch(addPrincipalCommentPending());
    const endpoint = `/api/services/app/Report/AddPrincipalComment`;
    await instance
      .post(endpoint, { id, ...input })
      .then((response) => {
        dispatch(addPrincipalCommentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(addPrincipalCommentError());
      });
  };

  const acknowledgeByParentAsync = async (id: string, input: IReportComment) => {
    dispatch(acknowledgeByParentPending());
    const endpoint = `/api/services/app/Report/AcknowledgeByParent`;
    await instance
      .post(endpoint, { id, ...input })
      .then((response) => {
        dispatch(acknowledgeByParentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(acknowledgeByParentError());
      });
  };

  const recordPromotionAsync = async (id: string, decision: number, promotedToGradeId?: string) => {
    dispatch(recordPromotionPending());
    const endpoint = `/api/services/app/Report/RecordPromotion`;
    await instance
      .post(endpoint, { id, decision, promotedToGradeId })
      .then((response) => {
        dispatch(recordPromotionSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(recordPromotionError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteReportPending());
    const endpoint = `/api/services/app/Report/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteReportSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteReportError());
      });
  };

  return (
    <ReportStateContext.Provider value={state}>
      <ReportActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByStudentTermAsync,
          generateAsync,
          submitForApprovalAsync,
          approveAsync,
          publishAsync,
          addTeacherCommentAsync,
          addPrincipalCommentAsync,
          acknowledgeByParentAsync,
          recordPromotionAsync,
          deleteAsync,
        }}
      >
        {children}
      </ReportActionContext.Provider>
    </ReportStateContext.Provider>
  );
};

export const useReportState = () => {
  const context = useContext(ReportStateContext);
  if (!context) {
    throw new Error("useReportState must be used within a ReportProvider");
  }
  return context;
};

export const useReportActions = () => {
  const context = useContext(ReportActionContext);
  if (!context) {
    throw new Error("useReportActions must be used within a ReportProvider");
  }
  return context;
};
