import { createAction } from "redux-actions";
import { ILearningMaterialStateContext } from "./context";
import { ILearningMaterial, ILearningMaterialList, IPagedResult, IListResult } from "../shared/interfaces";

export enum LearningMaterialActionEnums {
  getLearningMaterialPending = "GET_LEARNING_MATERIAL_PENDING",
  getLearningMaterialSuccess = "GET_LEARNING_MATERIAL_SUCCESS",
  getLearningMaterialError = "GET_LEARNING_MATERIAL_ERROR",

  getAllLearningMaterialsPending = "GET_ALL_LEARNING_MATERIALS_PENDING",
  getAllLearningMaterialsSuccess = "GET_ALL_LEARNING_MATERIALS_SUCCESS",
  getAllLearningMaterialsError = "GET_ALL_LEARNING_MATERIALS_ERROR",

  getByClassSubjectPending = "GET_LEARNING_MATERIALS_BY_CLASS_SUBJECT_PENDING",
  getByClassSubjectSuccess = "GET_LEARNING_MATERIALS_BY_CLASS_SUBJECT_SUCCESS",
  getByClassSubjectError = "GET_LEARNING_MATERIALS_BY_CLASS_SUBJECT_ERROR",
  
  createLearningMaterialPending = "CREATE_LEARNING_MATERIAL_PENDING",
  createLearningMaterialSuccess = "CREATE_LEARNING_MATERIAL_SUCCESS",
  createLearningMaterialError = "CREATE_LEARNING_MATERIAL_ERROR",

  uploadLearningMaterialPending = "UPLOAD_LEARNING_MATERIAL_PENDING",
  uploadLearningMaterialSuccess = "UPLOAD_LEARNING_MATERIAL_SUCCESS",
  uploadLearningMaterialError = "UPLOAD_LEARNING_MATERIAL_ERROR",

  updateLearningMaterialPending = "UPDATE_LEARNING_MATERIAL_PENDING",
  updateLearningMaterialSuccess = "UPDATE_LEARNING_MATERIAL_SUCCESS",
  updateLearningMaterialError = "UPDATE_LEARNING_MATERIAL_ERROR",

  deleteLearningMaterialPending = "DELETE_LEARNING_MATERIAL_PENDING",
  deleteLearningMaterialSuccess = "DELETE_LEARNING_MATERIAL_SUCCESS",
  deleteLearningMaterialError = "DELETE_LEARNING_MATERIAL_ERROR",

  publishLearningMaterialPending = "PUBLISH_LEARNING_MATERIAL_PENDING",
  publishLearningMaterialSuccess = "PUBLISH_LEARNING_MATERIAL_SUCCESS",
  publishLearningMaterialError = "PUBLISH_LEARNING_MATERIAL_ERROR",

  unpublishLearningMaterialPending = "UNPUBLISH_LEARNING_MATERIAL_PENDING",
  unpublishLearningMaterialSuccess = "UNPUBLISH_LEARNING_MATERIAL_SUCCESS",
  unpublishLearningMaterialError = "UNPUBLISH_LEARNING_MATERIAL_ERROR",

  incrementViewCountPending = "INCREMENT_VIEW_COUNT_PENDING",
  incrementViewCountSuccess = "INCREMENT_VIEW_COUNT_SUCCESS",
  incrementViewCountError = "INCREMENT_VIEW_COUNT_ERROR",
}

// Get Single LearningMaterial Actions
export const getLearningMaterialPending = createAction<ILearningMaterialStateContext>(
  LearningMaterialActionEnums.getLearningMaterialPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getLearningMaterialSuccess = createAction<ILearningMaterialStateContext, ILearningMaterial>(
  LearningMaterialActionEnums.getLearningMaterialSuccess,
  (learningMaterial: ILearningMaterial) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    learningMaterial,
  })
);

export const getLearningMaterialError = createAction<ILearningMaterialStateContext>(
  LearningMaterialActionEnums.getLearningMaterialError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get All LearningMaterials Actions
export const getAllLearningMaterialsPending = createAction<ILearningMaterialStateContext>(
  LearningMaterialActionEnums.getAllLearningMaterialsPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const getAllLearningMaterialsSuccess = createAction<
  ILearningMaterialStateContext,
  IPagedResult<ILearningMaterialList>
>(
  LearningMaterialActionEnums.getAllLearningMaterialsSuccess,
  (result: IPagedResult<ILearningMaterialList>) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    learningMaterials: result.items,
    totalCount: result.totalCount,
  })
);

export const getAllLearningMaterialsError = createAction<ILearningMaterialStateContext>(
  LearningMaterialActionEnums.getAllLearningMaterialsError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Get By ClassSubject Actions
export const getByClassSubjectPending = createAction<ILearningMaterialStateContext>(
    LearningMaterialActionEnums.getByClassSubjectPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const getByClassSubjectSuccess = createAction<
    ILearningMaterialStateContext,
    IListResult<ILearningMaterialList>
    >(
    LearningMaterialActionEnums.getByClassSubjectSuccess,
    (result: IListResult<ILearningMaterialList>) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        learningMaterials: result.items,
    })
    );

export const getByClassSubjectError = createAction<ILearningMaterialStateContext>(
    LearningMaterialActionEnums.getByClassSubjectError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );


// Create LearningMaterial Actions
export const createLearningMaterialPending = createAction<ILearningMaterialStateContext>(
  LearningMaterialActionEnums.createLearningMaterialPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const createLearningMaterialSuccess = createAction<ILearningMaterialStateContext, ILearningMaterial>(
  LearningMaterialActionEnums.createLearningMaterialSuccess,
  (learningMaterial: ILearningMaterial) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    learningMaterial,
  })
);

export const createLearningMaterialError = createAction<ILearningMaterialStateContext>(
  LearningMaterialActionEnums.createLearningMaterialError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Upload LearningMaterial Actions (multipart/form-data)
export const uploadLearningMaterialPending = createAction<ILearningMaterialStateContext>(
  LearningMaterialActionEnums.uploadLearningMaterialPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const uploadLearningMaterialSuccess = createAction<ILearningMaterialStateContext, ILearningMaterial>(
  LearningMaterialActionEnums.uploadLearningMaterialSuccess,
  (learningMaterial: ILearningMaterial) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    learningMaterial,
  })
);

export const uploadLearningMaterialError = createAction<ILearningMaterialStateContext>(
  LearningMaterialActionEnums.uploadLearningMaterialError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// Update LearningMaterial Actions
export const updateLearningMaterialPending = createAction<ILearningMaterialStateContext>(
    LearningMaterialActionEnums.updateLearningMaterialPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const updateLearningMaterialSuccess = createAction<ILearningMaterialStateContext, ILearningMaterial>(
    LearningMaterialActionEnums.updateLearningMaterialSuccess,
    (learningMaterial: ILearningMaterial) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        learningMaterial,
    })
    );

export const updateLearningMaterialError = createAction<ILearningMaterialStateContext>(
    LearningMaterialActionEnums.updateLearningMaterialError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );

// Delete LearningMaterial Actions
export const deleteLearningMaterialPending = createAction<ILearningMaterialStateContext>(
    LearningMaterialActionEnums.deleteLearningMaterialPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const deleteLearningMaterialSuccess = createAction<ILearningMaterialStateContext>(
    LearningMaterialActionEnums.deleteLearningMaterialSuccess,
    () => ({
        isPending: false,
        isSuccess: true,
        isError: false,
    })
    );

export const deleteLearningMaterialError = createAction<ILearningMaterialStateContext>(
    LearningMaterialActionEnums.deleteLearningMaterialError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );

// Publish LearningMaterial Actions
export const publishLearningMaterialPending = createAction<ILearningMaterialStateContext>(
    LearningMaterialActionEnums.publishLearningMaterialPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const publishLearningMaterialSuccess = createAction<ILearningMaterialStateContext, ILearningMaterial>(
    LearningMaterialActionEnums.publishLearningMaterialSuccess,
    (learningMaterial: ILearningMaterial) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        learningMaterial,
    })
    );

export const publishLearningMaterialError = createAction<ILearningMaterialStateContext>(
    LearningMaterialActionEnums.publishLearningMaterialError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );

// Unpublish LearningMaterial Actions
export const unpublishLearningMaterialPending = createAction<ILearningMaterialStateContext>(
    LearningMaterialActionEnums.unpublishLearningMaterialPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const unpublishLearningMaterialSuccess = createAction<ILearningMaterialStateContext, ILearningMaterial>(
    LearningMaterialActionEnums.unpublishLearningMaterialSuccess,
    (learningMaterial: ILearningMaterial) => ({
        isPending: false,
        isSuccess: true,
        isError: false,
        learningMaterial,
    })
    );

export const unpublishLearningMaterialError = createAction<ILearningMaterialStateContext>(
    LearningMaterialActionEnums.unpublishLearningMaterialError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );

// Increment View Count Actions
export const incrementViewCountPending = createAction<ILearningMaterialStateContext>(
    LearningMaterialActionEnums.incrementViewCountPending,
    () => ({ isPending: true, isSuccess: false, isError: false })
    );

export const incrementViewCountSuccess = createAction<ILearningMaterialStateContext>(
    LearningMaterialActionEnums.incrementViewCountSuccess,
    () => ({
        isPending: false,
        isSuccess: true,
        isError: false,
    })
    );

export const incrementViewCountError = createAction<ILearningMaterialStateContext>(
    LearningMaterialActionEnums.incrementViewCountError,
    () => ({ isPending: false, isSuccess: false, isError: true })
    );
