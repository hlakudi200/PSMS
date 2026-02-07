"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  AssessmentActionContext,
  AssessmentStateContext,
} from "./context";
import {
  ICreateAssessment,
  IUpdateAssessment,
  IGetAssessmentsInput,
} from "../shared/interfaces";
import { AssessmentReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getAssessmentPending,
  getAssessmentSuccess,
  getAssessmentError,
  getAllAssessmentsPending,
  getAllAssessmentsSuccess,
  getAllAssessmentsError,
  createAssessmentPending,
  createAssessmentSuccess,
  createAssessmentError,
  updateAssessmentPending,
  updateAssessmentSuccess,
  updateAssessmentError,
  deleteAssessmentPending,
  deleteAssessmentSuccess,
  deleteAssessmentError,
  publishAssessmentPending,
  publishAssessmentSuccess,
  publishAssessmentError,
  unpublishAssessmentPending,
  unpublishAssessmentSuccess,
  unpublishAssessmentError,
  releaseMarksPending,
  releaseMarksSuccess,
  releaseMarksError,
} from "./actions";

export const AssessmentProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(AssessmentReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getAssessmentPending());
    const endpoint = `/api/services/app/Assessment/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAssessmentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAssessmentError());
      });
  };

  const getAllAsync = async (input?: IGetAssessmentsInput) => {
    dispatch(getAllAssessmentsPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.classSubjectId) params.append('ClassSubjectId', input.classSubjectId);
    if (input?.termId) params.append('TermId', input.termId);
    if (input?.classId) params.append('ClassId', input.classId);
    if (input?.subjectId) params.append('SubjectId', input.subjectId);
    if (input?.assessmentType !== undefined && input?.assessmentType !== null) params.append('AssessmentType', input.assessmentType.toString());
    if (input?.isPublished !== undefined && input?.isPublished !== null) params.append('IsPublished', input.isPublished.toString());
    if (input?.name) params.append('Name', input.name);

    const endpoint = `/api/services/app/Assessment/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllAssessmentsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllAssessmentsError());
      });
  };

  const createAsync = async (input: ICreateAssessment) => {
    dispatch(createAssessmentPending());
    const endpoint = `/api/services/app/Assessment/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createAssessmentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createAssessmentError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateAssessment) => {
    dispatch(updateAssessmentPending());
    const endpoint = `/api/services/app/Assessment/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateAssessmentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateAssessmentError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteAssessmentPending());
    const endpoint = `/api/services/app/Assessment/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteAssessmentSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteAssessmentError());
      });
  };

  const publishAsync = async (id: string) => {
    dispatch(publishAssessmentPending());
    const endpoint = `/api/services/app/Assessment/Publish`;
    await instance
      .post(endpoint, { id })
      .then((response) => {
        dispatch(publishAssessmentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(publishAssessmentError());
      });
  };

  const unpublishAsync = async (id: string) => {
    dispatch(unpublishAssessmentPending());
    const endpoint = `/api/services/app/Assessment/Unpublish`;
    await instance
      .post(endpoint, { id })
      .then((response) => {
        dispatch(unpublishAssessmentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(unpublishAssessmentError());
      });
  };

  const releaseMarksAsync = async (id: string) => {
    dispatch(releaseMarksPending());
    const endpoint = `/api/services/app/Assessment/ReleaseMarks`;
    await instance
      .post(endpoint, { id })
      .then((response) => {
        dispatch(releaseMarksSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(releaseMarksError());
      });
  };

  return (
    <AssessmentStateContext.Provider value={state}>
      <AssessmentActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          publishAsync,
          unpublishAsync,
          releaseMarksAsync,
        }}
      >
        {children}
      </AssessmentActionContext.Provider>
    </AssessmentStateContext.Provider>
  );
};

export const useAssessmentState = () => {
  const context = useContext(AssessmentStateContext);
  if (!context) {
    throw new Error("useAssessmentState must be used within an AssessmentProvider");
  }
  return context;
};

export const useAssessmentActions = () => {
  const context = useContext(AssessmentActionContext);
  if (!context) {
    throw new Error("useAssessmentActions must be used within an AssessmentProvider");
  }
  return context;
};
