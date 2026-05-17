"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import { buildQueryParams } from "@/utils/query-params";
import { INITIAL_STATE, SubjectActionContext, SubjectStateContext } from "./context";
import { ISubject, ICreateSubject, IUpdateSubject, IPagedAndSortedResultRequest } from "../shared/interfaces";
import { SubjectReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getSubjectsError, getSubjectsPending, getSubjectsSuccess,
  getSubjectError, getSubjectPending, getSubjectSuccess,
  createSubjectPending, createSubjectError, createSubjectSuccess,
  updateSubjectPending, updateSubjectSuccess, updateSubjectError,
  deleteSubjectPending, deleteSubjectSuccess, deleteSubjectError,
  activateSubjectPending, activateSubjectSuccess, activateSubjectError,
  deactivateSubjectPending, deactivateSubjectSuccess, deactivateSubjectError,
} from "./actions";

export const SubjectProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(SubjectReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getSubjectPending());
    await instance.get(`/api/services/app/Subject/Get?id=${id}`)
      .then((response) => dispatch(getSubjectSuccess(response.data.result)))
      .catch((error) => { console.error(error); dispatch(getSubjectError()); throw error; });
  };

  const getAllAsync = async (input?: IPagedAndSortedResultRequest) => {
    dispatch(getSubjectsPending());
    const params = buildQueryParams(input as Record<string, unknown>);
    await instance.get(`/api/services/app/Subject/GetAll?${params.toString()}`)
      .then((response) => dispatch(getSubjectsSuccess({ items: response.data.result.items, totalCount: response.data.result.totalCount })))
      .catch((error) => { console.error(error); dispatch(getSubjectsError()); throw error; });
  };

  const createAsync = async (input: ICreateSubject) => {
    dispatch(createSubjectPending());
    await instance.post(`/api/services/app/Subject/Create`, input)
      .then((response) => dispatch(createSubjectSuccess(response.data.result)))
      .catch((error) => { console.error(error); dispatch(createSubjectError()); throw error; });
  };

  const updateAsync = async (id: string, input: IUpdateSubject) => {
    dispatch(updateSubjectPending());
    await instance.put(`/api/services/app/Subject/Update`, { id, ...input })
      .then((response) => dispatch(updateSubjectSuccess(response.data.result)))
      .catch((error) => { console.error(error); dispatch(updateSubjectError()); throw error; });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteSubjectPending());
    await instance.delete(`/api/services/app/Subject/Delete?id=${id}`)
      .then(() => dispatch(deleteSubjectSuccess()))
      .catch((error) => { console.error(error); dispatch(deleteSubjectError()); throw error; });
  };

  const activateAsync = async (id: string) => {
    dispatch(activateSubjectPending());
    await instance.post(`/api/services/app/Subject/Activate`, { id })
      .then(() => dispatch(activateSubjectSuccess()))
      .catch((error) => { console.error(error); dispatch(activateSubjectError()); throw error; });
  };

  const deactivateAsync = async (id: string) => {
    dispatch(deactivateSubjectPending());
    await instance.post(`/api/services/app/Subject/Deactivate`, { id })
      .then(() => dispatch(deactivateSubjectSuccess()))
      .catch((error) => { console.error(error); dispatch(deactivateSubjectError()); throw error; });
  };

  return (
    <SubjectStateContext.Provider value={state}>
      <SubjectActionContext.Provider value={{ getAsync, getAllAsync, createAsync, updateAsync, deleteAsync, activateAsync, deactivateAsync }}>
        {children}
      </SubjectActionContext.Provider>
    </SubjectStateContext.Provider>
  );
};

export const useSubjectState = () => {
  const context = useContext(SubjectStateContext);
  if (!context) throw new Error("useSubjectState must be used within a SubjectProvider");
  return context;
};

export const useSubjectActions = () => {
  const context = useContext(SubjectActionContext);
  if (!context) throw new Error("useSubjectActions must be used within a SubjectProvider");
  return context;
};
