"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  EmergencyContactActionContext,
  EmergencyContactStateContext,
} from "./context";
import {
  ICreateEmergencyContact,
  IUpdateEmergencyContact,
} from "../shared/interfaces";
import { EmergencyContactReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getEmergencyContactPending,
  getEmergencyContactSuccess,
  getEmergencyContactError,
  getByStudentPending,
  getByStudentSuccess,
  getByStudentError,
  createEmergencyContactPending,
  createEmergencyContactSuccess,
  createEmergencyContactError,
  updateEmergencyContactPending,
  updateEmergencyContactSuccess,
  updateEmergencyContactError,
  deleteEmergencyContactPending,
  deleteEmergencyContactSuccess,
  deleteEmergencyContactError,
} from "./actions";

export const EmergencyContactProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(EmergencyContactReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getEmergencyContactPending());
    const endpoint = `/api/services/app/EmergencyContact/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getEmergencyContactSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getEmergencyContactError());
      });
  };

  const getByStudentAsync = async (studentId: string) => {
    dispatch(getByStudentPending());
    const endpoint = `/api/services/app/EmergencyContact/GetByStudent?studentId=${studentId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByStudentSuccess({
          items: response.data.result.items
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByStudentError());
      });
  };

  const createAsync = async (input: ICreateEmergencyContact) => {
    dispatch(createEmergencyContactPending());
    const endpoint = `/api/services/app/EmergencyContact/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createEmergencyContactSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createEmergencyContactError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateEmergencyContact) => {
    dispatch(updateEmergencyContactPending());
    const endpoint = `/api/services/app/EmergencyContact/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateEmergencyContactSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateEmergencyContactError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteEmergencyContactPending());
    const endpoint = `/api/services/app/EmergencyContact/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteEmergencyContactSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteEmergencyContactError());
      });
  };

  return (
    <EmergencyContactStateContext.Provider value={state}>
      <EmergencyContactActionContext.Provider
        value={{
          getAsync,
          getByStudentAsync,
          createAsync,
          updateAsync,
          deleteAsync,
        }}
      >
        {children}
      </EmergencyContactActionContext.Provider>
    </EmergencyContactStateContext.Provider>
  );
};

export const useEmergencyContactState = () => {
  const context = useContext(EmergencyContactStateContext);
  if (!context) {
    throw new Error("useEmergencyContactState must be used within an EmergencyContactProvider");
  }
  return context;
};

export const useEmergencyContactActions = () => {
  const context = useContext(EmergencyContactActionContext);
  if (!context) {
    throw new Error("useEmergencyContactActions must be used within an EmergencyContactProvider");
  }
  return context;
};
