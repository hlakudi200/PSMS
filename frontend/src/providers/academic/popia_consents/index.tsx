"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  POPIAConsentActionContext,
  POPIAConsentStateContext,
} from "./context";
import {
  ICreatePOPIAConsent,
  IUpdatePOPIAConsent,
  IGetPOPIAConsentsInput,
} from "../shared/interfaces";
import { POPIAConsentReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getByStudentPending,
  getByStudentSuccess,
  getByStudentError,
  getAllPending,
  getAllSuccess,
  getAllError,
  createPending,
  createSuccess,
  createError,
  updatePending,
  updateSuccess,
  updateError,
  revokePending,
  revokeSuccess,
  revokeError,
} from "./actions";

export const POPIAConsentProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(POPIAConsentReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getByStudentAsync = async (studentId: string) => {
    dispatch(getByStudentPending());
    const endpoint = `/api/services/app/POPIAConsent/GetByStudent?studentId=${studentId}`;
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

  const getAllAsync = async (input?: IGetPOPIAConsentsInput) => {
    dispatch(getAllPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.studentId) params.append('StudentId', input.studentId);

    const endpoint = `/api/services/app/POPIAConsent/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllError());
      });
  };

  const createAsync = async (input: ICreatePOPIAConsent) => {
    dispatch(createPending());
    const endpoint = `/api/services/app/POPIAConsent/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createError());
      });
  };

  const updateAsync = async (id: string, input: IUpdatePOPIAConsent) => {
    dispatch(updatePending());
    const endpoint = `/api/services/app/POPIAConsent/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateError());
      });
  };

  const revokeAsync = async (id: string) => {
    dispatch(revokePending());
    const endpoint = `/api/services/app/POPIAConsent/Revoke?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(revokeSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(revokeError());
      });
  };

  return (
    <POPIAConsentStateContext.Provider value={state}>
      <POPIAConsentActionContext.Provider
        value={{
          getByStudentAsync,
          getAllAsync,
          createAsync,
          updateAsync,
          revokeAsync,
        }}
      >
        {children}
      </POPIAConsentActionContext.Provider>
    </POPIAConsentStateContext.Provider>
  );
};

export const usePOPIAConsentState = () => {
  const context = useContext(POPIAConsentStateContext);
  if (!context) {
    throw new Error("usePOPIAConsentState must be used within a POPIAConsentProvider");
  }
  return context;
};

export const usePOPIAConsentActions = () => {
  const context = useContext(POPIAConsentActionContext);
  if (!context) {
    throw new Error("usePOPIAConsentActions must be used within a POPIAConsentProvider");
  }
  return context;
};
