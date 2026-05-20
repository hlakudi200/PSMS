import { handleActions } from "redux-actions";
import { INITIAL_STATE, ILearningMaterialStateContext } from "./context";
import { LearningMaterialActionEnums } from "./actions";

export const LearningMaterialReducer = handleActions<
  ILearningMaterialStateContext,
  ILearningMaterialStateContext
>(
  {
    [LearningMaterialActionEnums.getLearningMaterialPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [LearningMaterialActionEnums.getLearningMaterialSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [LearningMaterialActionEnums.getLearningMaterialError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [LearningMaterialActionEnums.getAllLearningMaterialsPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.getAllLearningMaterialsSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.getAllLearningMaterialsError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.getByClassSubjectPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.getByClassSubjectSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.getByClassSubjectError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.createLearningMaterialPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.createLearningMaterialSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.createLearningMaterialError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.uploadLearningMaterialPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.uploadLearningMaterialSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.uploadLearningMaterialError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.updateLearningMaterialPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.updateLearningMaterialSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.updateLearningMaterialError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.deleteLearningMaterialPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.deleteLearningMaterialSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.deleteLearningMaterialError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.publishLearningMaterialPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.publishLearningMaterialSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.publishLearningMaterialError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.unpublishLearningMaterialPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.unpublishLearningMaterialSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.unpublishLearningMaterialError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.incrementViewCountPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.incrementViewCountSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.incrementViewCountError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    // T-T07 versioning — same passthrough spread pattern.
    [LearningMaterialActionEnums.getVersionsPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.getVersionsSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.getVersionsError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.uploadNewVersionPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.uploadNewVersionSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.uploadNewVersionError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.restoreVersionPending]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.restoreVersionSuccess]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
    [LearningMaterialActionEnums.restoreVersionError]: (state, action) => ({
        ...state,
        ...action.payload,
    }),
  },
  INITIAL_STATE
);
