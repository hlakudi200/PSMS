import { handleActions } from "redux-actions";
import { INITIAL_STATE, ISchoolTransportStateContext } from "./context";
import { SchoolTransportActionEnums } from "./actions";

export const SchoolTransportReducer = handleActions<
  ISchoolTransportStateContext,
  ISchoolTransportStateContext
>(
  {
    [SchoolTransportActionEnums.getSchoolTransportPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.getSchoolTransportSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.getSchoolTransportError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.getAllSchoolTransportsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.getAllSchoolTransportsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.getAllSchoolTransportsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.createSchoolTransportPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.createSchoolTransportSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.createSchoolTransportError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.updateSchoolTransportPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.updateSchoolTransportSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.updateSchoolTransportError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.deleteSchoolTransportPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.deleteSchoolTransportSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.deleteSchoolTransportError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.activateSchoolTransportPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.activateSchoolTransportSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.activateSchoolTransportError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.deactivateSchoolTransportPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.deactivateSchoolTransportSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [SchoolTransportActionEnums.deactivateSchoolTransportError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
