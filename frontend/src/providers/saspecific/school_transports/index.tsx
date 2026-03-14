"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  SchoolTransportActionContext,
  SchoolTransportStateContext,
} from "./context";
import {
  ICreateSchoolTransport,
  IUpdateSchoolTransport,
  IGetSchoolTransportsInput,
} from "../shared/interfaces";
import { SchoolTransportReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
    getSchoolTransportPending,
    getSchoolTransportSuccess,
    getSchoolTransportError,
    getAllSchoolTransportsPending,
    getAllSchoolTransportsSuccess,
    getAllSchoolTransportsError,
    createSchoolTransportPending,
    createSchoolTransportSuccess,
    createSchoolTransportError,
    updateSchoolTransportPending,
    updateSchoolTransportSuccess,
    updateSchoolTransportError,
    deleteSchoolTransportPending,
    deleteSchoolTransportSuccess,
    deleteSchoolTransportError,
    activateSchoolTransportPending,
    activateSchoolTransportSuccess,
    activateSchoolTransportError,
    deactivateSchoolTransportPending,
    deactivateSchoolTransportSuccess,
    deactivateSchoolTransportError,
} from "./actions";

export const SchoolTransportProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(SchoolTransportReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getSchoolTransportPending());
    const endpoint = `/api/services/app/SchoolTransport/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getSchoolTransportSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getSchoolTransportError());
      });
  };

  const getAllAsync = async (input?: IGetSchoolTransportsInput) => {
    dispatch(getAllSchoolTransportsPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.transportType !== undefined) params.append('TransportType', input.transportType.toString());
    if (input?.isActive !== undefined) params.append('IsActive', input.isActive.toString());
    if (input?.routeName) params.append('RouteName', input.routeName);

    const endpoint = `/api/services/app/SchoolTransport/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllSchoolTransportsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllSchoolTransportsError());
      });
  };

  const createAsync = async (input: ICreateSchoolTransport) => {
    dispatch(createSchoolTransportPending());
    const endpoint = `/api/services/app/SchoolTransport/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createSchoolTransportSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createSchoolTransportError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateSchoolTransport) => {
    dispatch(updateSchoolTransportPending());
    const endpoint = `/api/services/app/SchoolTransport/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateSchoolTransportSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateSchoolTransportError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteSchoolTransportPending());
    const endpoint = `/api/services/app/SchoolTransport/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteSchoolTransportSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteSchoolTransportError());
      });
  };

  const activateAsync = async (id: string) => {
    dispatch(activateSchoolTransportPending());
    const endpoint = `/api/services/app/SchoolTransport/Activate?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(activateSchoolTransportSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(activateSchoolTransportError());
      });
  };

  const deactivateAsync = async (id: string) => {
    dispatch(deactivateSchoolTransportPending());
    const endpoint = `/api/services/app/SchoolTransport/Deactivate?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(deactivateSchoolTransportSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deactivateSchoolTransportError());
      });
  };

  return (
    <SchoolTransportStateContext.Provider value={state}>
      <SchoolTransportActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          activateAsync,
          deactivateAsync,
        }}
      >
        {children}
      </SchoolTransportActionContext.Provider>
    </SchoolTransportStateContext.Provider>
  );
};

export const useSchoolTransportState = () => {
  const context = useContext(SchoolTransportStateContext);
  if (!context) {
    throw new Error("useSchoolTransportState must be used within a SchoolTransportProvider");
  }
  return context;
};

export const useSchoolTransportActions = () => {
  const context = useContext(SchoolTransportActionContext);
  if (!context) {
    throw new Error("useSchoolTransportActions must be used within a SchoolTransportProvider");
  }
  return context;
};
