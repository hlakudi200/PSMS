"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  RoleActionContext,
  RoleStateContext,
} from "./context";
import {
  ICreateRole,
  IUpdateRole,
  IGetRolesInput,
} from "../shared/interfaces";
import { RoleReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getRolePending,
  getRoleSuccess,
  getRoleError,
  getRolesPending,
  getRolesSuccess,
  getRolesError,
  createRolePending,
  createRoleSuccess,
  createRoleError,
  updateRolePending,
  updateRoleSuccess,
  updateRoleError,
  deleteRolePending,
  deleteRoleSuccess,
  deleteRoleError,
  getAllPermissionsPending,
  getAllPermissionsSuccess,
  getAllPermissionsError,
  getRoleForEditPending,
  getRoleForEditSuccess,
  getRoleForEditError,
} from "./actions";

export const RoleProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(RoleReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: number) => {
    dispatch(getRolePending());
    const endpoint = `/api/services/app/Role/Get?Id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getRoleSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getRoleError());
      });
  };

  const getAllAsync = async (input?: IGetRolesInput) => {
    dispatch(getRolesPending());
    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append("MaxResultCount", input.maxResultCount.toString());
    if (input?.skipCount !== undefined) params.append("SkipCount", input.skipCount.toString());
    if (input?.sorting) params.append("Sorting", input.sorting);
    if (input?.keyword) params.append("Keyword", input.keyword);
    const endpoint = `/api/services/app/Role/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getRolesSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getRolesError());
      });
  };

  const createAsync = async (input: ICreateRole) => {
    dispatch(createRolePending());
    const endpoint = `/api/services/app/Role/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createRoleSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createRoleError());
      });
  };

  const updateAsync = async (id: number, input: IUpdateRole) => {
    dispatch(updateRolePending());
    const endpoint = `/api/services/app/Role/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateRoleSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateRoleError());
      });
  };

  const deleteAsync = async (id: number) => {
    dispatch(deleteRolePending());
    const endpoint = `/api/services/app/Role/Delete?Id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteRoleSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteRoleError());
      });
  };

  const getAllPermissionsAsync = async () => {
    dispatch(getAllPermissionsPending());
    const endpoint = `/api/services/app/Role/GetAllPermissions`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllPermissionsSuccess(response.data.result.items));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllPermissionsError());
      });
  };

  const getRoleForEditAsync = async (id: number) => {
    dispatch(getRoleForEditPending());
    const endpoint = `/api/services/app/Role/GetRoleForEdit?Id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getRoleForEditSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getRoleForEditError());
      });
  };

  return (
    <RoleStateContext.Provider value={state}>
      <RoleActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          getAllPermissionsAsync,
          getRoleForEditAsync,
        }}
      >
        {children}
      </RoleActionContext.Provider>
    </RoleStateContext.Provider>
  );
};

export const useRoleState = () => {
  const context = useContext(RoleStateContext);
  if (!context) {
    throw new Error("useRoleState must be used within a RoleProvider");
  }
  return context;
};

export const useRoleActions = () => {
  const context = useContext(RoleActionContext);
  if (!context) {
    throw new Error("useRoleActions must be used within a RoleProvider");
  }
  return context;
};
