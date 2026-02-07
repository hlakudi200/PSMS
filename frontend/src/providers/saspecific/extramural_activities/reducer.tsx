import { handleActions } from "redux-actions";
import { INITIAL_STATE, IExtramuralActivityStateContext } from "./context";
import { ExtramuralActivityActionEnums } from "./actions";

export const ExtramuralActivityReducer = handleActions<
  IExtramuralActivityStateContext,
  IExtramuralActivityStateContext
>(
  {
    [ExtramuralActivityActionEnums.getExtramuralActivityPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.getExtramuralActivitySuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.getExtramuralActivityError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.getAllExtramuralActivitiesPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.getAllExtramuralActivitiesSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.getAllExtramuralActivitiesError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.getByAcademicYearPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.getByAcademicYearSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.getByAcademicYearError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.createExtramuralActivityPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.createExtramuralActivitySuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.createExtramuralActivityError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.updateExtramuralActivityPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.updateExtramuralActivitySuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.updateExtramuralActivityError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.deleteExtramuralActivityPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.deleteExtramuralActivitySuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.deleteExtramuralActivityError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.activateExtramuralActivityPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.activateExtramuralActivitySuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.activateExtramuralActivityError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.deactivateExtramuralActivityPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.deactivateExtramuralActivitySuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.deactivateExtramuralActivityError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.openRegistrationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.openRegistrationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.openRegistrationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.closeRegistrationPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.closeRegistrationSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ExtramuralActivityActionEnums.closeRegistrationError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
