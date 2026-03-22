"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  MarkActionContext,
  MarkStateContext,
} from "./context";
import {
  IRecordMark,
  IBulkRecordMarks,
  IGetMarksInput,
} from "../shared/interfaces";
import { MarkReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getMarkPending,
  getMarkSuccess,
  getMarkError,
  getAllMarksPending,
  getAllMarksSuccess,
  getAllMarksError,
  getByAssessmentPending,
  getByAssessmentSuccess,
  getByAssessmentError,
  getByStudentPending,
  getByStudentSuccess,
  getByStudentError,
  recordMarkPending,
  recordMarkSuccess,
  recordMarkError,
  bulkRecordMarksPending,
  bulkRecordMarksSuccess,
  bulkRecordMarksError,
  updateMarkPending,
  updateMarkSuccess,
  updateMarkError,
  markAsAbsentPending,
  markAsAbsentSuccess,
  markAsAbsentError,
  applyModerationPending,
  applyModerationSuccess,
  applyModerationError,
  unlockMarkPending,
  unlockMarkSuccess,
  unlockMarkError,
  deleteMarkPending,
  deleteMarkSuccess,
  deleteMarkError,
} from "./actions";

export const MarkProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(MarkReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getMarkPending());
    const endpoint = `/api/services/app/Mark/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getMarkSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getMarkError());
      });
  };

  const getAllAsync = async (input?: IGetMarksInput) => {
    dispatch(getAllMarksPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.assessmentId) params.append('AssessmentId', input.assessmentId);
    if (input?.studentId) params.append('StudentId', input.studentId);
    if (input?.classId) params.append('ClassId', input.classId);
    if (input?.subjectId) params.append('SubjectId', input.subjectId);
    if (input?.termId) params.append('TermId', input.termId);
    if (input?.status !== undefined) params.append('Status', input.status.toString());
    if (input?.studentName) params.append('StudentName', input.studentName);

    const endpoint = `/api/services/app/Mark/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllMarksSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllMarksError());
      });
  };

  const getByAssessmentAsync = async (assessmentId: string) => {
    dispatch(getByAssessmentPending());
    const endpoint = `/api/services/app/Mark/GetByAssessment?assessmentId=${assessmentId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByAssessmentSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByAssessmentError());
      });
  };

  const getByStudentAsync = async (studentId: string, termId?: string) => {
    dispatch(getByStudentPending());

    const params = new URLSearchParams();
    params.append('studentId', studentId);
    if (termId) params.append('termId', termId);

    const endpoint = `/api/services/app/Mark/GetByStudent?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByStudentSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByStudentError());
      });
  };

  const recordMarkAsync = async (input: IRecordMark) => {
    dispatch(recordMarkPending());
    const endpoint = `/api/services/app/Mark/RecordMark`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(recordMarkSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(recordMarkError());
      });
  };

  const bulkRecordMarksAsync = async (input: IBulkRecordMarks) => {
    dispatch(bulkRecordMarksPending());
    const endpoint = `/api/services/app/Mark/BulkRecordMarks`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(bulkRecordMarksSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(bulkRecordMarksError());
      });
  };

  const updateMarkAsync = async (id: string, input: IRecordMark) => {
    dispatch(updateMarkPending());
    const endpoint = `/api/services/app/Mark/UpdateMark?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateMarkSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateMarkError());
      });
  };

  const markAsAbsentAsync = async (id: string) => {
    dispatch(markAsAbsentPending());
    const endpoint = `/api/services/app/Mark/MarkAsAbsent?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(markAsAbsentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(markAsAbsentError());
      });
  };

  const applyModerationAsync = async (id: string, adjustment: number) => {
    dispatch(applyModerationPending());
    const endpoint = `/api/services/app/Mark/ApplyModeration?id=${id}`;
    await instance
      .post(endpoint, { adjustment })
      .then((response) => {
        dispatch(applyModerationSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(applyModerationError());
      });
  };

  const unlockMarkAsync = async (id: string) => {
    dispatch(unlockMarkPending());
    const endpoint = `/api/services/app/Mark/UnlockMark?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(unlockMarkSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(unlockMarkError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteMarkPending());
    const endpoint = `/api/services/app/Mark/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteMarkSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteMarkError());
      });
  };

  return (
    <MarkStateContext.Provider value={state}>
      <MarkActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByAssessmentAsync,
          getByStudentAsync,
          recordMarkAsync,
          bulkRecordMarksAsync,
          updateMarkAsync,
          markAsAbsentAsync,
          applyModerationAsync,
          unlockMarkAsync,
          deleteAsync,
        }}
      >
        {children}
      </MarkActionContext.Provider>
    </MarkStateContext.Provider>
  );
};

export const useMarkState = () => {
  const context = useContext(MarkStateContext);
  if (!context) {
    throw new Error("useMarkState must be used within a MarkProvider");
  }
  return context;
};

export const useMarkActions = () => {
  const context = useContext(MarkActionContext);
  if (!context) {
    throw new Error("useMarkActions must be used within a MarkProvider");
  }
  return context;
};
