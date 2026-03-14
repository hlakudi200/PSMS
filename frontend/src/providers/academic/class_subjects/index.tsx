"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  ClassSubjectActionContext,
  ClassSubjectStateContext,
} from "./context";
import {
  ICreateClassSubject,
  IUpdateClassSubject,
  IGetClassSubjectsInput,
} from "../shared/interfaces";
import { ClassSubjectReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getClassSubjectsError,
  getClassSubjectsPending,
  getClassSubjectsSuccess,
  getClassSubjectError,
  getClassSubjectPending,
  getClassSubjectSuccess,
  getByClassPending,
  getByClassSuccess,
  getByClassError,
  getByTeacherPending,
  getByTeacherSuccess,
  getByTeacherError,
  createClassSubjectPending,
  createClassSubjectError,
  updateClassSubjectSuccess,
  createClassSubjectSuccess,
  updateClassSubjectPending,
  updateClassSubjectError,
  deleteClassSubjectPending,
  deleteClassSubjectSuccess,
  deleteClassSubjectError,
  assignTeacherPending,
  assignTeacherSuccess,
  assignTeacherError,
  removeTeacherPending,
  removeTeacherSuccess,
  removeTeacherError,
} from "./actions";

export const ClassSubjectProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(ClassSubjectReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getClassSubjectPending());
    const endpoint = `/api/services/app/ClassSubject/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getClassSubjectSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getClassSubjectError());
      });
  };

  const getAllAsync = async (input?: IGetClassSubjectsInput) => {
    dispatch(getClassSubjectsPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.classId) params.append('ClassId', input.classId);
    if (input?.subjectId) params.append('SubjectId', input.subjectId);
    if (input?.teacherId) params.append('TeacherId', input.teacherId);

    const endpoint = `/api/services/app/ClassSubject/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getClassSubjectsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getClassSubjectsError());
      });
  };

  const getByClassAsync = async (classId: string) => {
    dispatch(getByClassPending());
    const endpoint = `/api/services/app/ClassSubject/GetByClass?classId=${classId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByClassSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByClassError());
      });
  };

  const getByTeacherAsync = async (teacherId: string) => {
    dispatch(getByTeacherPending());
    const endpoint = `/api/services/app/ClassSubject/GetByTeacher?teacherId=${teacherId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByTeacherSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByTeacherError());
      });
  };

  const createAsync = async (input: ICreateClassSubject) => {
    dispatch(createClassSubjectPending());
    const endpoint = `/api/services/app/ClassSubject/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createClassSubjectSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createClassSubjectError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateClassSubject) => {
    dispatch(updateClassSubjectPending());
    const endpoint = `/api/services/app/ClassSubject/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateClassSubjectSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateClassSubjectError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteClassSubjectPending());
    const endpoint = `/api/services/app/ClassSubject/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteClassSubjectSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteClassSubjectError());
      });
  };

  const assignTeacherAsync = async (id: string, teacherId: string) => {
    dispatch(assignTeacherPending());
    const endpoint = `/api/services/app/ClassSubject/AssignTeacher?id=${id}`;
    await instance
      .put(endpoint, { teacherId })
      .then((response) => {
        dispatch(assignTeacherSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(assignTeacherError());
      });
  };

  const removeTeacherAsync = async (id: string) => {
    dispatch(removeTeacherPending());
    const endpoint = `/api/services/app/ClassSubject/RemoveTeacher?id=${id}`;
    await instance
      .put(endpoint)
      .then((response) => {
        dispatch(removeTeacherSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(removeTeacherError());
      });
  };

  return (
    <ClassSubjectStateContext.Provider value={state}>
      <ClassSubjectActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByClassAsync,
          getByTeacherAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          assignTeacherAsync,
          removeTeacherAsync,
        }}
      >
        {children}
      </ClassSubjectActionContext.Provider>
    </ClassSubjectStateContext.Provider>
  );
};

export const useClassSubjectState = () => {
  const context = useContext(ClassSubjectStateContext);
  if (!context) {
    throw new Error("useClassSubjectState must be used within a ClassSubjectProvider");
  }
  return context;
};

export const useClassSubjectActions = () => {
  const context = useContext(ClassSubjectActionContext);
  if (!context) {
    throw new Error("useClassSubjectActions must be used within a ClassSubjectProvider");
  }
  return context;
};
