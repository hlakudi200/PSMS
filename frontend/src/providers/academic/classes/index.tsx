"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  ClassActionContext,
  ClassStateContext,
} from "./context";
import {
  ICreateClass,
  IUpdateClass,
  IPagedAndSortedResultRequest,
} from "../shared/interfaces";
import { ClassReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getClassesError,
  getClassesPending,
  getClassesSuccess,
  getClassError,
  getClassPending,
  getClassSuccess,
  createClassPending,
  createClassSuccess,
  createClassError,
  updateClassPending,
  updateClassSuccess,
  updateClassError,
  deleteClassPending,
  deleteClassSuccess,
  deleteClassError,
  getActiveClassesPending,
  getActiveClassesSuccess,
  getActiveClassesError,
  getClassesByGradePending,
  getClassesByGradeSuccess,
  getClassesByGradeError,
  getClassesByAcademicYearPending,
  getClassesByAcademicYearSuccess,
  getClassesByAcademicYearError,
  assignClassTeacherPending,
  assignClassTeacherSuccess,
  assignClassTeacherError,
  removeClassTeacherPending,
  removeClassTeacherSuccess,
  removeClassTeacherError,
  activateClassPending,
  activateClassSuccess,
  activateClassError,
  deactivateClassPending,
  deactivateClassSuccess,
  deactivateClassError,
} from "./actions";

export const ClassProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(ClassReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getClassPending());
    const endpoint = `/api/services/app/Class/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getClassSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getClassError());
      });
  };

  const getAllAsync = async (input?: IPagedAndSortedResultRequest) => {
    dispatch(getClassesPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);

    const endpoint = `/api/services/app/Class/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getClassesSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getClassesError());
      });
  };

  const createAsync = async (input: ICreateClass) => {
    dispatch(createClassPending());
    const endpoint = `/api/services/app/Class/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createClassSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createClassError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateClass) => {
    dispatch(updateClassPending());
    const endpoint = `/api/services/app/Class/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateClassSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateClassError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteClassPending());
    const endpoint = `/api/services/app/Class/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteClassSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteClassError());
      });
  };

  const getActiveClassesAsync = async () => {
    dispatch(getActiveClassesPending());
    const endpoint = `/api/services/app/Class/GetActiveClasses`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getActiveClassesSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getActiveClassesError());
      });
  };

  const getClassesByGradeAsync = async (gradeId: string) => {
    dispatch(getClassesByGradePending());
    const endpoint = `/api/services/app/Class/GetClassesByGrade?gradeId=${gradeId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getClassesByGradeSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getClassesByGradeError());
      });
  };

  const getClassesByAcademicYearAsync = async (academicYearId: string) => {
    dispatch(getClassesByAcademicYearPending());
    const endpoint = `/api/services/app/Class/GetClassesByAcademicYear?academicYearId=${academicYearId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getClassesByAcademicYearSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getClassesByAcademicYearError());
      });
  };

  const assignClassTeacherAsync = async (id: string, teacherId: string) => {
    dispatch(assignClassTeacherPending());
    const endpoint = `/api/services/app/Class/AssignClassTeacher?id=${id}&teacherId=${teacherId}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(assignClassTeacherSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(assignClassTeacherError());
      });
  };

  const removeClassTeacherAsync = async (id: string) => {
    dispatch(removeClassTeacherPending());
    const endpoint = `/api/services/app/Class/RemoveClassTeacher?id=${id}`;
    await instance
      .post(endpoint)
      .then((response) => {
        dispatch(removeClassTeacherSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(removeClassTeacherError());
      });
  };

  const activateAsync = async (id: string) => {
    dispatch(activateClassPending());
    const endpoint = `/api/services/app/Class/Activate?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(activateClassSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(activateClassError());
      });
  };

  const deactivateAsync = async (id: string) => {
    dispatch(deactivateClassPending());
    const endpoint = `/api/services/app/Class/Deactivate?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(deactivateClassSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deactivateClassError());
      });
  };

  return (
    <ClassStateContext.Provider value={state}>
      <ClassActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          getActiveClassesAsync,
          getClassesByGradeAsync,
          getClassesByAcademicYearAsync,
          assignClassTeacherAsync,
          removeClassTeacherAsync,
          activateAsync,
          deactivateAsync,
        }}
      >
        {children}
      </ClassActionContext.Provider>
    </ClassStateContext.Provider>
  );
};

export const useClassState = () => {
  const context = useContext(ClassStateContext);
  if (!context) {
    throw new Error("useClassState must be used within a ClassProvider");
  }
  return context;
};

export const useClassActions = () => {
  const context = useContext(ClassActionContext);
  if (!context) {
    throw new Error("useClassActions must be used within a ClassProvider");
  }
  return context;
};
