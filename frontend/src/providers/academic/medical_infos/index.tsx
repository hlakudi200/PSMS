"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  MedicalInfoActionContext,
  MedicalInfoStateContext,
} from "./context";
import {
  ICreateUpdateMedicalInfo,
} from "../shared/interfaces";
import { MedicalInfoReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getByStudentPending,
  getByStudentSuccess,
  getByStudentError,
  createOrUpdatePending,
  createOrUpdateSuccess,
  createOrUpdateError,
  deletePending,
  deleteSuccess,
  deleteError,
} from "./actions";

export const MedicalInfoProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(MedicalInfoReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getByStudentAsync = async (studentId: string) => {
    dispatch(getByStudentPending());
    const endpoint = `/api/services/app/MedicalInfo/GetByStudent?studentId=${studentId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByStudentSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByStudentError());
      });
  };

  const createOrUpdateAsync = async (input: ICreateUpdateMedicalInfo) => {
    dispatch(createOrUpdatePending());
    const endpoint = `/api/services/app/MedicalInfo/CreateOrUpdate`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createOrUpdateSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createOrUpdateError());
      });
  };

  const deleteAsync = async (studentId: string) => {
    dispatch(deletePending());
    const endpoint = `/api/services/app/MedicalInfo/Delete?studentId=${studentId}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteError());
      });
  };

  return (
    <MedicalInfoStateContext.Provider value={state}>
      <MedicalInfoActionContext.Provider
        value={{
          getByStudentAsync,
          createOrUpdateAsync,
          deleteAsync,
        }}
      >
        {children}
      </MedicalInfoActionContext.Provider>
    </MedicalInfoStateContext.Provider>
  );
};

export const useMedicalInfoState = () => {
  const context = useContext(MedicalInfoStateContext);
  if (!context) {
    throw new Error("useMedicalInfoState must be used within a MedicalInfoProvider");
  }
  return context;
};

export const useMedicalInfoActions = () => {
  const context = useContext(MedicalInfoActionContext);
  if (!context) {
    throw new Error("useMedicalInfoActions must be used within a MedicalInfoProvider");
  }
  return context;
};
