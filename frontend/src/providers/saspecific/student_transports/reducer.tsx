import { handleActions } from "redux-actions";
import { INITIAL_STATE, IStudentTransportStateContext } from "./context";
import { StudentTransportActionEnums } from "./actions";

export const StudentTransportReducer = handleActions<
  IStudentTransportStateContext,
  IStudentTransportStateContext
>(
  {
    [StudentTransportActionEnums.getStudentTransportPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.getStudentTransportSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.getStudentTransportError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.getStudentTransportsPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.getStudentTransportsSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.getStudentTransportsError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.getByTransportPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.getByTransportSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.getByTransportError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.getByStudentPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.getByStudentSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.getByStudentError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.createStudentTransportPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.createStudentTransportSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.createStudentTransportError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.updateStudentTransportPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.updateStudentTransportSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.updateStudentTransportError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.deleteStudentTransportPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.deleteStudentTransportSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.deleteStudentTransportError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.suspendPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.suspendSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.suspendError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.reactivatePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.reactivateSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.reactivateError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.terminatePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.terminateSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [StudentTransportActionEnums.terminateError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
