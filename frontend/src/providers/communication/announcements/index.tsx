"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  AnnouncementActionContext,
  AnnouncementStateContext,
} from "./context";
import {
  ICreateAnnouncement,
  IUpdateAnnouncement,
  IGetAnnouncementsInput,
} from "../shared/interfaces";
import { AnnouncementReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getAnnouncementPending,
  getAnnouncementSuccess,
  getAnnouncementError,
  getAllAnnouncementsPending,
  getAllAnnouncementsSuccess,
  getAllAnnouncementsError,
  createAnnouncementPending,
  createAnnouncementSuccess,
  createAnnouncementError,
  updateAnnouncementPending,
  updateAnnouncementSuccess,
  updateAnnouncementError,
  deleteAnnouncementPending,
  deleteAnnouncementSuccess,
  deleteAnnouncementError,
  publishAnnouncementPending,
  publishAnnouncementSuccess,
  publishAnnouncementError,
  unpublishAnnouncementPending,
  unpublishAnnouncementSuccess,
  unpublishAnnouncementError,
  pinAnnouncementPending,
  pinAnnouncementSuccess,
  pinAnnouncementError,
  unpinAnnouncementPending,
  unpinAnnouncementSuccess,
  unpinAnnouncementError,
} from "./actions";

export const AnnouncementProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(AnnouncementReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getAnnouncementPending());
    const endpoint = `/api/services/app/Announcement/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAnnouncementSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAnnouncementError());
      });
  };

  const getAllAsync = async (input?: IGetAnnouncementsInput) => {
    dispatch(getAllAnnouncementsPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.type !== undefined) params.append('Type', input.type.toString());
    if (input?.priority !== undefined) params.append('Priority', input.priority.toString());
    if (input?.targetAudience !== undefined) params.append('TargetAudience', input.targetAudience.toString());
    if (input?.isPublished !== undefined) params.append('IsPublished', input.isPublished.toString());
    if (input?.isPinned !== undefined) params.append('IsPinned', input.isPinned.toString());
    if (input?.search) params.append('Search', input.search);

    const endpoint = `/api/services/app/Announcement/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllAnnouncementsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllAnnouncementsError());
      });
  };

  const createAsync = async (input: ICreateAnnouncement) => {
    dispatch(createAnnouncementPending());
    const endpoint = `/api/services/app/Announcement/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createAnnouncementSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createAnnouncementError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateAnnouncement) => {
    dispatch(updateAnnouncementPending());
    const endpoint = `/api/services/app/Announcement/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateAnnouncementSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateAnnouncementError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteAnnouncementPending());
    const endpoint = `/api/services/app/Announcement/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteAnnouncementSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteAnnouncementError());
      });
  };

  const publishAsync = async (id: string) => {
    dispatch(publishAnnouncementPending());
    const endpoint = `/api/services/app/Announcement/Publish`;
    await instance
      .post(endpoint, { id })
      .then(() => {
        dispatch(publishAnnouncementSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(publishAnnouncementError());
      });
  };

  const unpublishAsync = async (id: string) => {
    dispatch(unpublishAnnouncementPending());
    const endpoint = `/api/services/app/Announcement/Unpublish`;
    await instance
      .post(endpoint, { id })
      .then(() => {
        dispatch(unpublishAnnouncementSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(unpublishAnnouncementError());
      });
  };

  const pinAsync = async (id: string) => {
    dispatch(pinAnnouncementPending());
    const endpoint = `/api/services/app/Announcement/Pin`;
    await instance
      .post(endpoint, { id })
      .then(() => {
        dispatch(pinAnnouncementSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(pinAnnouncementError());
      });
  };

  const unpinAsync = async (id: string) => {
    dispatch(unpinAnnouncementPending());
    const endpoint = `/api/services/app/Announcement/Unpin`;
    await instance
      .post(endpoint, { id })
      .then(() => {
        dispatch(unpinAnnouncementSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(unpinAnnouncementError());
      });
  };

  return (
    <AnnouncementStateContext.Provider value={state}>
      <AnnouncementActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          publishAsync,
          unpublishAsync,
          pinAsync,
          unpinAsync,
        }}
      >
        {children}
      </AnnouncementActionContext.Provider>
    </AnnouncementStateContext.Provider>
  );
};

export const useAnnouncementState = () => {
  const context = useContext(AnnouncementStateContext);
  if (!context) {
    throw new Error("useAnnouncementState must be used within an AnnouncementProvider");
  }
  return context;
};

export const useAnnouncementActions = () => {
  const context = useContext(AnnouncementActionContext);
  if (!context) {
    throw new Error("useAnnouncementActions must be used within an AnnouncementProvider");
  }
  return context;
};
