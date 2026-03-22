"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  OnlineLessonActionContext,
  OnlineLessonStateContext,
} from "./context";
import {
  ICreateOnlineLesson,
  IUpdateOnlineLesson,
  IGetOnlineLessonsInput,
  IRescheduleOnlineLesson,
  IAddRecording,
} from "../shared/interfaces";
import { OnlineLessonReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
    getOnlineLessonPending,
    getOnlineLessonSuccess,
    getOnlineLessonError,
    getAllOnlineLessonsPending,
    getAllOnlineLessonsSuccess,
    getAllOnlineLessonsError,
    getByClassSubjectPending,
    getByClassSubjectSuccess,
    getByClassSubjectError,
    getUpcomingPending,
    getUpcomingSuccess,
    getUpcomingError,
    createOnlineLessonPending,
    createOnlineLessonSuccess,
    createOnlineLessonError,
    updateOnlineLessonPending,
    updateOnlineLessonSuccess,
    updateOnlineLessonError,
    deleteOnlineLessonPending,
    deleteOnlineLessonSuccess,
    deleteOnlineLessonError,
    startOnlineLessonPending,
    startOnlineLessonSuccess,
    startOnlineLessonError,
    endOnlineLessonPending,
    endOnlineLessonSuccess,
    endOnlineLessonError,
    cancelOnlineLessonPending,
    cancelOnlineLessonSuccess,
    cancelOnlineLessonError,
    rescheduleOnlineLessonPending,
    rescheduleOnlineLessonSuccess,
    rescheduleOnlineLessonError,
    addRecordingPending,
    addRecordingSuccess,
    addRecordingError,
} from "./actions";

export const OnlineLessonProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(OnlineLessonReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getOnlineLessonPending());
    const endpoint = `/api/services/app/OnlineLesson/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getOnlineLessonSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getOnlineLessonError());
      });
  };

  const getAllAsync = async (input?: IGetOnlineLessonsInput) => {
    dispatch(getAllOnlineLessonsPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.classSubjectId) params.append('ClassSubjectId', input.classSubjectId);
    if (input?.status) params.append('Status', input.status.toString());
    if (input?.startDate) params.append('StartDate', input.startDate);
    if (input?.endDate) params.append('EndDate', input.endDate);
    if (input?.hostTeacherUserId) params.append('HostTeacherUserId', input.hostTeacherUserId.toString());
    
    const endpoint = `/api/services/app/OnlineLesson/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllOnlineLessonsSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllOnlineLessonsError());
      });
  };

  const getByClassSubjectAsync = async (classSubjectId: string) => {
    dispatch(getByClassSubjectPending());
    const endpoint = `/api/services/app/OnlineLesson/GetByClassSubject?classSubjectId=${classSubjectId}`;
    await instance
        .get(endpoint)
        .then((response) => {
            dispatch(getByClassSubjectSuccess({
                items: response.data.result.items
            }));
        })
        .catch((error) => {
            console.error(error);
            dispatch(getByClassSubjectError());
        });
    };

    const getUpcomingAsync = async () => {
    dispatch(getUpcomingPending());
    const endpoint = `/api/services/app/OnlineLesson/GetUpcoming`;
    await instance
        .get(endpoint)
        .then((response) => {
            dispatch(getUpcomingSuccess({
                items: response.data.result.items
            }));
        })
        .catch((error) => {
            console.error(error);
            dispatch(getUpcomingError());
        });
    };

  const createAsync = async (input: ICreateOnlineLesson) => {
    dispatch(createOnlineLessonPending());
    const endpoint = `/api/services/app/OnlineLesson/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createOnlineLessonSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createOnlineLessonError());
      });
  };

  const updateAsync = async (id: string, input: IUpdateOnlineLesson) => {
    dispatch(updateOnlineLessonPending());
    const endpoint = `/api/services/app/OnlineLesson/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateOnlineLessonSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateOnlineLessonError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteOnlineLessonPending());
    const endpoint = `/api/services/app/OnlineLesson/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteOnlineLessonSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteOnlineLessonError());
      });
  };

  const startAsync = async (id: string) => {
    dispatch(startOnlineLessonPending());
    const endpoint = `/api/services/app/OnlineLesson/Start?id=${id}`;
    await instance
        .post(endpoint)
        .then((response) => {
            dispatch(startOnlineLessonSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(startOnlineLessonError());
        });
    };

    const endAsync = async (id: string, attendeeCount: number) => {
    dispatch(endOnlineLessonPending());
    const endpoint = `/api/services/app/OnlineLesson/End?id=${id}&attendeeCount=${attendeeCount}`;
    await instance
        .post(endpoint)
        .then((response) => {
            dispatch(endOnlineLessonSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(endOnlineLessonError());
        });
    };

    const cancelAsync = async (id: string) => {
    dispatch(cancelOnlineLessonPending());
    const endpoint = `/api/services/app/OnlineLesson/Cancel?id=${id}`;
    await instance
        .post(endpoint)
        .then((response) => {
            dispatch(cancelOnlineLessonSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(cancelOnlineLessonError());
        });
    };

    const rescheduleAsync = async (id: string, input: IRescheduleOnlineLesson) => {
    dispatch(rescheduleOnlineLessonPending());
    const endpoint = `/api/services/app/OnlineLesson/Reschedule?id=${id}`;
    await instance
        .put(endpoint, input)
        .then((response) => {
            dispatch(rescheduleOnlineLessonSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(rescheduleOnlineLessonError());
        });
    };

    const addRecordingAsync = async (id: string, input: IAddRecording) => {
    dispatch(addRecordingPending());
    const endpoint = `/api/services/app/OnlineLesson/AddRecording?id=${id}`;
    await instance
        .post(endpoint, input)
        .then((response) => {
            dispatch(addRecordingSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(addRecordingError());
        });
    };

  return (
    <OnlineLessonStateContext.Provider value={state}>
      <OnlineLessonActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByClassSubjectAsync,
          getUpcomingAsync,
          createAsync,
          updateAsync,
          deleteAsync,
          startAsync,
          endAsync,
          cancelAsync,
          rescheduleAsync,
          addRecordingAsync,
        }}
      >
        {children}
      </OnlineLessonActionContext.Provider>
    </OnlineLessonStateContext.Provider>
  );
};

export const useOnlineLessonState = () => {
  const context = useContext(OnlineLessonStateContext);
  if (!context) {
    throw new Error("useOnlineLessonState must be used within a OnlineLessonProvider");
  }
  return context;
};

export const useOnlineLessonActions = () => {
  const context = useContext(OnlineLessonActionContext);
  if (!context) {
    throw new Error("useOnlineLessonActions must be used within a OnlineLessonProvider");
  }
  return context;
};
