"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  AnnouncementReadActionContext,
  AnnouncementReadStateContext,
} from "./context";
import { AnnouncementReadReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  markAsReadPending,
  markAsReadSuccess,
  markAsReadError,
  getByAnnouncementPending,
  getByAnnouncementSuccess,
  getByAnnouncementError,
  getUnreadCountPending,
  getUnreadCountSuccess,
  getUnreadCountError,
} from "./actions";

export const AnnouncementReadProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(AnnouncementReadReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const markAsReadAsync = async (announcementId: string) => {
    dispatch(markAsReadPending());
    const endpoint = `/api/services/app/AnnouncementRead/MarkAsRead`;
    await instance
      .post(endpoint, { announcementId })
      .then(() => {
        dispatch(markAsReadSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(markAsReadError());
      });
  };

  const getByAnnouncementAsync = async (announcementId: string) => {
    dispatch(getByAnnouncementPending());
    const endpoint = `/api/services/app/AnnouncementRead/GetByAnnouncement?announcementId=${announcementId}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getByAnnouncementSuccess({
          items: response.data.result.items,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getByAnnouncementError());
      });
  };

  const getUnreadCountAsync = async () => {
    dispatch(getUnreadCountPending());
    const endpoint = `/api/services/app/AnnouncementRead/GetUnreadCount`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getUnreadCountSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getUnreadCountError());
      });
  };

  return (
    <AnnouncementReadStateContext.Provider value={state}>
      <AnnouncementReadActionContext.Provider
        value={{
          markAsReadAsync,
          getByAnnouncementAsync,
          getUnreadCountAsync,
        }}
      >
        {children}
      </AnnouncementReadActionContext.Provider>
    </AnnouncementReadStateContext.Provider>
  );
};

export const useAnnouncementReadState = () => {
  const context = useContext(AnnouncementReadStateContext);
  if (!context) {
    throw new Error("useAnnouncementReadState must be used within an AnnouncementReadProvider");
  }
  return context;
};

export const useAnnouncementReadActions = () => {
  const context = useContext(AnnouncementReadActionContext);
  if (!context) {
    throw new Error("useAnnouncementReadActions must be used within an AnnouncementReadProvider");
  }
  return context;
};
