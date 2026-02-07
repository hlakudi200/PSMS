import { createAction } from "redux-actions";
import { IOnlineLessonStateContext } from "./context";
import { IOnlineLesson, IOnlineLessonList, IPagedResult, IListResult } from "../shared/interfaces";

export enum OnlineLessonActionEnums {
  getOnlineLessonPending = "GET_ONLINE_LESSON_PENDING",
  getOnlineLessonSuccess = "GET_ONLINE_LESSON_SUCCESS",
  getOnlineLessonError = "GET_ONLINE_LESSON_ERROR",

  getAllOnlineLessonsPending = "GET_ALL_ONLINE_LESSONS_PENDING",
  getAllOnlineLessonsSuccess = "GET_ALL_ONLINE_LESSONS_SUCCESS",
  getAllOnlineLessonsError = "GET_ALL_ONLINE_LESSONS_ERROR",

  getByClassSubjectPending = "GET_ONLINE_LESSONS_BY_CLASS_SUBJECT_PENDING",
  getByClassSubjectSuccess = "GET_ONLINE_LESSONS_BY_CLASS_SUBJECT_SUCCESS",
  getByClassSubjectError = "GET_ONLINE_LESSONS_BY_CLASS_SUBJECT_ERROR",

  getUpcomingPending = "GET_UPCOMING_ONLINE_LESSONS_PENDING",
  getUpcomingSuccess = "GET_UPCOMING_ONLINE_LESSONS_SUCCESS",
  getUpcomingError = "GET_UPCOMING_ONLINE_LESSONS_ERROR",

  createOnlineLessonPending = "CREATE_ONLINE_LESSON_PENDING",
  createOnlineLessonSuccess = "CREATE_ONLINE_LESSON_SUCCESS",
  createOnlineLessonError = "CREATE_ONLINE_LESSON_ERROR",

  updateOnlineLessonPending = "UPDATE_ONLINE_LESSON_PENDING",
  updateOnlineLessonSuccess = "UPDATE_ONLINE_LESSON_SUCCESS",
  updateOnlineLessonError = "UPDATE_ONLINE_LESSON_ERROR",

  deleteOnlineLessonPending = "DELETE_ONLINE_LESSON_PENDING",
  deleteOnlineLessonSuccess = "DELETE_ONLINE_LESSON_SUCCESS",
  deleteOnlineLessonError = "DELETE_ONLINE_LESSON_ERROR",
  
  startOnlineLessonPending = "START_ONLINE_LESSON_PENDING",
  startOnlineLessonSuccess = "START_ONLINE_LESSON_SUCCESS",
  startOnlineLessonError = "START_ONLINE_LESSON_ERROR",

  endOnlineLessonPending = "END_ONLINE_LESSON_PENDING",
  endOnlineLessonSuccess = "END_ONLINE_LESSON_SUCCESS",
  endOnlineLessonError = "END_ONLINE_LESSON_ERROR",

  cancelOnlineLessonPending = "CANCEL_ONLINE_LESSON_PENDING",
  cancelOnlineLessonSuccess = "CANCEL_ONLINE_LESSON_SUCCESS",
  cancelOnlineLessonError = "CANCEL_ONLINE_LESSON_ERROR",

  rescheduleOnlineLessonPending = "RESCHEDULE_ONLINE_LESSON_PENDING",
  rescheduleOnlineLessonSuccess = "RESCHEDULE_ONLINE_LESSON_SUCCESS",
  rescheduleOnlineLessonError = "RESCHEDULE_ONLINE_LESSON_ERROR",

  addRecordingPending = "ADD_RECORDING_PENDING",
  addRecordingSuccess = "ADD_RECORDING_SUCCESS",
  addRecordingError = "ADD_RECORDING_ERROR",
}

// Get Single OnlineLesson Actions
export const getOnlineLessonPending = createAction<IOnlineLessonStateContext>(
  OnlineLessonActionEnums.getOnlineLessonPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getOnlineLessonSuccess = createAction<IOnlineLessonStateContext, IOnlineLesson>(
  OnlineLessonActionEnums.getOnlineLessonSuccess,
  (onlineLesson: IOnlineLesson) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    onlineLesson,
  })
);

export const getOnlineLessonError = createAction<IOnlineLessonStateContext>(
  OnlineLessonActionEnums.getOnlineLessonError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All OnlineLessons Actions
export const getAllOnlineLessonsPending = createAction<IOnlineLessonStateContext>(
  OnlineLessonActionEnums.getAllOnlineLessonsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllOnlineLessonsSuccess = createAction<
  IOnlineLessonStateContext,
  IPagedResult<IOnlineLessonList>
>(
  OnlineLessonActionEnums.getAllOnlineLessonsSuccess,
  (result: IPagedResult<IOnlineLessonList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    onlineLessons: result.items,
    totalCount: result.totalCount,
  })
);

export const getAllOnlineLessonsError = createAction<IOnlineLessonStateContext>(
  OnlineLessonActionEnums.getAllOnlineLessonsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By ClassSubject Actions
export const getByClassSubjectPending = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.getByClassSubjectPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const getByClassSubjectSuccess = createAction<
    IOnlineLessonStateContext,
    IListResult<IOnlineLessonList>
    >(
    OnlineLessonActionEnums.getByClassSubjectSuccess,
    (result: IListResult<IOnlineLessonList>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        onlineLessons: result.items,
    })
    );

export const getByClassSubjectError = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.getByClassSubjectError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );

// Get Upcoming Actions
export const getUpcomingPending = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.getUpcomingPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const getUpcomingSuccess = createAction<
    IOnlineLessonStateContext,
    IListResult<IOnlineLessonList>
    >(
    OnlineLessonActionEnums.getUpcomingSuccess,
    (result: IListResult<IOnlineLessonList>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        onlineLessons: result.items,
    })
    );

export const getUpcomingError = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.getUpcomingError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );

// Create OnlineLesson Actions
export const createOnlineLessonPending = createAction<IOnlineLessonStateContext>(
  OnlineLessonActionEnums.createOnlineLessonPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createOnlineLessonSuccess = createAction<IOnlineLessonStateContext, IOnlineLesson>(
  OnlineLessonActionEnums.createOnlineLessonSuccess,
  (onlineLesson: IOnlineLesson) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    onlineLesson,
  })
);

export const createOnlineLessonError = createAction<IOnlineLessonStateContext>(
  OnlineLessonActionEnums.createOnlineLessonError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update OnlineLesson Actions
export const updateOnlineLessonPending = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.updateOnlineLessonPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const updateOnlineLessonSuccess = createAction<IOnlineLessonStateContext, IOnlineLesson>(
    OnlineLessonActionEnums.updateOnlineLessonSuccess,
    (onlineLesson: IOnlineLesson) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        onlineLesson,
    })
    );

export const updateOnlineLessonError = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.updateOnlineLessonError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );

// Delete OnlineLesson Actions
export const deleteOnlineLessonPending = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.deleteOnlineLessonPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const deleteOnlineLessonSuccess = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.deleteOnlineLessonSuccess,
    () => ({
        isPending: false,
        isSuccess: true,
        isError: false,
    })
    );

export const deleteOnlineLessonError = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.deleteOnlineLessonError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );

// Start OnlineLesson Actions
export const startOnlineLessonPending = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.startOnlineLessonPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const startOnlineLessonSuccess = createAction<IOnlineLessonStateContext, IOnlineLesson>(
    OnlineLessonActionEnums.startOnlineLessonSuccess,
    (onlineLesson: IOnlineLesson) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        onlineLesson,
    })
    );

export const startOnlineLessonError = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.startOnlineLessonError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );

// End OnlineLesson Actions
export const endOnlineLessonPending = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.endOnlineLessonPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const endOnlineLessonSuccess = createAction<IOnlineLessonStateContext, IOnlineLesson>(
    OnlineLessonActionEnums.endOnlineLessonSuccess,
    (onlineLesson: IOnlineLesson) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        onlineLesson,
    })
    );

export const endOnlineLessonError = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.endOnlineLessonError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );

// Cancel OnlineLesson Actions
export const cancelOnlineLessonPending = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.cancelOnlineLessonPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const cancelOnlineLessonSuccess = createAction<IOnlineLessonStateContext, IOnlineLesson>(
    OnlineLessonActionEnums.cancelOnlineLessonSuccess,
    (onlineLesson: IOnlineLesson) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        onlineLesson,
    })
    );

export const cancelOnlineLessonError = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.cancelOnlineLessonError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );

// Reschedule OnlineLesson Actions
export const rescheduleOnlineLessonPending = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.rescheduleOnlineLessonPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const rescheduleOnlineLessonSuccess = createAction<IOnlineLessonStateContext, IOnlineLesson>(
    OnlineLessonActionEnums.rescheduleOnlineLessonSuccess,
    (onlineLesson: IOnlineLesson) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        onlineLesson,
    })
    );

export const rescheduleOnlineLessonError = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.rescheduleOnlineLessonError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );

// Add Recording Actions
export const addRecordingPending = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.addRecordingPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const addRecordingSuccess = createAction<IOnlineLessonStateContext, IOnlineLesson>(
    OnlineLessonActionEnums.addRecordingSuccess,
    (onlineLesson: IOnlineLesson) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        onlineLesson,
    })
    );

export const addRecordingError = createAction<IOnlineLessonStateContext>(
    OnlineLessonActionEnums.addRecordingError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );
