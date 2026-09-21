"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  ReportActionContext,
  ReportStateContext,
} from "./context";
import {
  IBulkGenerateReportPdfsInput,
  IBulkGenerateReports,
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
  generatePdfPending,
  generatePdfSuccess,
  generatePdfError,
  bulkGeneratePdfsPending,
  bulkGeneratePdfsSuccess,
  bulkGeneratePdfsError,
  previewBulkGeneratePending,
  previewBulkGenerateSuccess,
  previewBulkGenerateError,
  bulkGenerateReportsPending,
  bulkGenerateReportsSuccess,
  bulkGenerateReportsError,
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
        throw error;
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
        throw error;
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
        throw error;
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
        throw error;
      });
  };

  const submitForApprovalAsync = async (id: string) => {
    dispatch(submitForApprovalPending());
    const endpoint = `/api/services/app/Report/SubmitForApproval?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(submitForApprovalSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(submitForApprovalError());
        throw error;
      });
  };

  const approveAsync = async (id: string) => {
    dispatch(approveReportPending());
    const endpoint = `/api/services/app/Report/Approve?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(approveReportSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(approveReportError());
        throw error;
      });
  };

  const publishAsync = async (id: string) => {
    dispatch(publishReportPending());
    const endpoint = `/api/services/app/Report/Publish?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(publishReportSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(publishReportError());
        throw error;
      });
  };

  const addTeacherCommentAsync = async (id: string, input: IReportComment) => {
    dispatch(addTeacherCommentPending());
    const endpoint = `/api/services/app/Report/AddTeacherComment?id=${id}`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(addTeacherCommentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(addTeacherCommentError());
        throw error;
      });
  };

  const addPrincipalCommentAsync = async (id: string, input: IReportComment) => {
    dispatch(addPrincipalCommentPending());
    const endpoint = `/api/services/app/Report/AddPrincipalComment?id=${id}`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(addPrincipalCommentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(addPrincipalCommentError());
        throw error;
      });
  };

  const acknowledgeByParentAsync = async (id: string, input: IReportComment) => {
    dispatch(acknowledgeByParentPending());
    const endpoint = `/api/services/app/Report/AcknowledgeByParent?id=${id}`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(acknowledgeByParentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(acknowledgeByParentError());
        throw error;
      });
  };

  const recordPromotionAsync = async (id: string, decision: number, promotedToGradeId?: string) => {
    dispatch(recordPromotionPending());
    const endpoint = `/api/services/app/Report/RecordPromotion?id=${id}`;
    await instance
      .post(endpoint, { decision, promotedToGradeId })
      .then((response) => {
        dispatch(recordPromotionSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(recordPromotionError());
        throw error;
      });
  };

  const generatePdfAsync = async (id: string) => {
    dispatch(generatePdfPending());
    const endpoint = `/api/services/app/Report/GenerateReportPdf?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(generatePdfSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(generatePdfError());
        throw error;
      });
  };

  const bulkGeneratePdfsAsync = async (input: IBulkGenerateReportPdfsInput) => {
    dispatch(bulkGeneratePdfsPending());
    const endpoint = `/api/services/app/Report/BulkGenerateReportPdfs`;
    await instance
      .post(endpoint, input)
      .then(() => {
        dispatch(bulkGeneratePdfsSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(bulkGeneratePdfsError());
        throw error;
      });
  };

  // RC-01: shows what a bulk run would do without writing anything, so the
  // actor can see who is blocked before committing to a whole class.
  const previewBulkGenerateAsync = async (input: IBulkGenerateReports) => {
    dispatch(previewBulkGeneratePending());
    const endpoint = `/api/services/app/Report/PreviewBulkGenerate`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(previewBulkGenerateSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(previewBulkGenerateError());
        throw error;
      });
  };

  const bulkGenerateAsync = async (input: IBulkGenerateReports) => {
    dispatch(bulkGenerateReportsPending());
    const endpoint = `/api/services/app/Report/BulkGenerate`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(bulkGenerateReportsSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(bulkGenerateReportsError());
        throw error;
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
        throw error;
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
          generatePdfAsync,
          bulkGeneratePdfsAsync,
          previewBulkGenerateAsync,
          bulkGenerateAsync,
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
