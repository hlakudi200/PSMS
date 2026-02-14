"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  UserActionContext,
  UserStateContext,
} from "./context";
import {
  ICreateUser,
  IUpdateUser,
  IGetUsersInput,
  IResetPasswordInput,
} from "../shared/interfaces";
import { UserReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getUserPending,
  getUserSuccess,
  getUserError,
  getUsersPending,
  getUsersSuccess,
  getUsersError,
  createUserPending,
  createUserSuccess,
  createUserError,
  updateUserPending,
  updateUserSuccess,
  updateUserError,
  deleteUserPending,
  deleteUserSuccess,
  deleteUserError,
  activateUserPending,
  activateUserSuccess,
  activateUserError,
  deactivateUserPending,
  deactivateUserSuccess,
  deactivateUserError,
  resetPasswordPending,
  resetPasswordSuccess,
  resetPasswordError,
  getRolesPending,
  getRolesSuccess,
  getRolesError,
} from "./actions";

export const UserProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(UserReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: number) => {
    dispatch(getUserPending());
    const endpoint = `/api/services/app/User/Get?Id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getUserSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getUserError());
      });
  };

  const getAllAsync = async (input?: IGetUsersInput) => {
    dispatch(getUsersPending());
    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append("MaxResultCount", input.maxResultCount.toString());
    if (input?.skipCount !== undefined) params.append("SkipCount", input.skipCount.toString());
    if (input?.sorting) params.append("Sorting", input.sorting);
    if (input?.keyword) params.append("Keyword", input.keyword);
    if (input?.isActive !== undefined) params.append("IsActive", input.isActive.toString());
    const endpoint = `/api/services/app/User/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getUsersSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getUsersError());
      });
  };

  const createAsync = async (input: ICreateUser) => {
    dispatch(createUserPending());
    const endpoint = `/api/services/app/User/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createUserSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createUserError());
      });
  };

  const updateAsync = async (id: number, input: IUpdateUser) => {
    dispatch(updateUserPending());
    const endpoint = `/api/services/app/User/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateUserSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateUserError());
      });
  };

  const deleteAsync = async (id: number) => {
    dispatch(deleteUserPending());
    const endpoint = `/api/services/app/User/Delete?Id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteUserSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteUserError());
      });
  };

  const activateAsync = async (id: number) => {
    dispatch(activateUserPending());
    const endpoint = `/api/services/app/User/Activate`;
    await instance
      .post(endpoint, { id })
      .then(() => {
        dispatch(activateUserSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(activateUserError());
      });
  };

  const deactivateAsync = async (id: number) => {
    dispatch(deactivateUserPending());
    const endpoint = `/api/services/app/User/DeActivate`;
    await instance
      .post(endpoint, { id })
      .then(() => {
        dispatch(deactivateUserSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deactivateUserError());
      });
  };

  const resetPasswordAsync = async (input: IResetPasswordInput) => {
    dispatch(resetPasswordPending());
    const endpoint = `/api/services/app/User/ResetPassword`;
    await instance
      .post(endpoint, input)
      .then(() => {
        dispatch(resetPasswordSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(resetPasswordError());
      });
  };

  const getRolesAsync = async () => {
    dispatch(getRolesPending());
    const endpoint = `/api/services/app/User/GetRoles`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getRolesSuccess(response.data.result.items));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getRolesError());
      });
  };

  return (
    <UserStateContext.Provider value={state}>
      <UserActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          activateAsync,
          deactivateAsync,
          resetPasswordAsync,
          getRolesAsync,
        }}
      >
        {children}
      </UserActionContext.Provider>
    </UserStateContext.Provider>
  );
};

export const useUserState = () => {
  const context = useContext(UserStateContext);
  if (!context) {
    throw new Error("useUserState must be used within a UserProvider");
  }
  return context;
};

export const useUserActions = () => {
  const context = useContext(UserActionContext);
  if (!context) {
    throw new Error("useUserActions must be used within a UserProvider");
  }
  return context;
};
