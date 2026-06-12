import { handleActions } from "redux-actions";
import { INITIAL_STATE, IOnlineLessonStateContext } from "./context";
import { OnlineLessonActionEnums } from "./actions";

export const OnlineLessonReducer = handleActions<
  IOnlineLessonStateContext,
  IOnlineLessonStateContext
>(
  {
    [OnlineLessonActionEnums.getOnlineLessonPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [OnlineLessonActionEnums.getOnlineLessonSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [OnlineLessonActionEnums.getOnlineLessonError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [OnlineLessonActionEnums.getAllOnlineLessonsPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.getAllOnlineLessonsSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.getAllOnlineLessonsError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.getByClassSubjectPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.getByClassSubjectSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.getByClassSubjectError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.getUpcomingPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.getUpcomingSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.getUpcomingError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.createOnlineLessonPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.createOnlineLessonSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.createOnlineLessonError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.updateOnlineLessonPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.updateOnlineLessonSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.updateOnlineLessonError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.deleteOnlineLessonPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.deleteOnlineLessonSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.deleteOnlineLessonError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.startOnlineLessonPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.startOnlineLessonSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.startOnlineLessonError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.endOnlineLessonPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.endOnlineLessonSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.endOnlineLessonError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.cancelOnlineLessonPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.cancelOnlineLessonSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.cancelOnlineLessonError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.rescheduleOnlineLessonPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.rescheduleOnlineLessonSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.rescheduleOnlineLessonError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.addRecordingPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.addRecordingSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.addRecordingError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.uploadRecordingPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.uploadRecordingSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [OnlineLessonActionEnums.uploadRecordingError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
  },
  INITIAL_STATE
);
