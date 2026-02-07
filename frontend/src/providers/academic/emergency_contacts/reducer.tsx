import { handleActions } from "redux-actions";
import { INITIAL_STATE, IEmergencyContactStateContext } from "./context";
import { EmergencyContactActionEnums } from "./actions";

export const EmergencyContactReducer = handleActions<
  IEmergencyContactStateContext,
  IEmergencyContactStateContext
>(
  {
    [EmergencyContactActionEnums.getEmergencyContactPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EmergencyContactActionEnums.getEmergencyContactSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EmergencyContactActionEnums.getEmergencyContactError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EmergencyContactActionEnums.getByStudentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EmergencyContactActionEnums.getByStudentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EmergencyContactActionEnums.getByStudentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EmergencyContactActionEnums.createEmergencyContactPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EmergencyContactActionEnums.createEmergencyContactSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EmergencyContactActionEnums.createEmergencyContactError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EmergencyContactActionEnums.updateEmergencyContactPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EmergencyContactActionEnums.updateEmergencyContactSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EmergencyContactActionEnums.updateEmergencyContactError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EmergencyContactActionEnums.deleteEmergencyContactPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EmergencyContactActionEnums.deleteEmergencyContactSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [EmergencyContactActionEnums.deleteEmergencyContactError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
