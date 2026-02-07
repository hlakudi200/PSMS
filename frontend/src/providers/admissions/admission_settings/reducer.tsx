import { handleActions } from "redux-actions";
import { INITIAL_STATE, IAdmissionSettingsStateContext } from "./context";
import { AdmissionSettingsActionEnums } from "./actions";

export const AdmissionSettingsReducer = handleActions<
  IAdmissionSettingsStateContext,
  IAdmissionSettingsStateContext
>(
  {
    [AdmissionSettingsActionEnums.getAdmissionSettingsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.getAdmissionSettingsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.getAdmissionSettingsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.getAdmissionSettingsByGradePending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.getAdmissionSettingsByGradeSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.getAdmissionSettingsByGradeError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.getAllByAcademicYearPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.getAllByAcademicYearSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.getAllByAcademicYearError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.createAdmissionSettingsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.createAdmissionSettingsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.createAdmissionSettingsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.updateAdmissionSettingsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.updateAdmissionSettingsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.updateAdmissionSettingsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.deleteAdmissionSettingsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.deleteAdmissionSettingsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.deleteAdmissionSettingsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.getCapacityStatusPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.getCapacityStatusSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.getCapacityStatusError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.openApplicationsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.openApplicationsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.openApplicationsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.closeApplicationsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.closeApplicationsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.closeApplicationsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.updateCapacityPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.updateCapacitySuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [AdmissionSettingsActionEnums.updateCapacityError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
