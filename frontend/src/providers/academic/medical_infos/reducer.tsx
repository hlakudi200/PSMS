import { handleActions } from "redux-actions";
import { INITIAL_STATE, IMedicalInfoStateContext } from "./context";
import { MedicalInfoActionEnums } from "./actions";

export const MedicalInfoReducer = handleActions<
  IMedicalInfoStateContext,
  IMedicalInfoStateContext
>(
  {
    [MedicalInfoActionEnums.getByStudentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MedicalInfoActionEnums.getByStudentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MedicalInfoActionEnums.getByStudentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MedicalInfoActionEnums.createOrUpdatePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MedicalInfoActionEnums.createOrUpdateSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MedicalInfoActionEnums.createOrUpdateError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MedicalInfoActionEnums.deletePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MedicalInfoActionEnums.deleteSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [MedicalInfoActionEnums.deleteError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
