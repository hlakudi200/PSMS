import { createAction } from "redux-actions";
import {
  DEFAULT_BRANDING,
  IBranding,
  IBrandingStateContext,
} from "./context";

export enum BrandingActionEnums {
  loadBrandingPending = "LOAD_BRANDING_PENDING",
  loadBrandingSuccess = "LOAD_BRANDING_SUCCESS",
  loadBrandingError = "LOAD_BRANDING_ERROR",

  saveBrandingPending = "SAVE_BRANDING_PENDING",
  saveBrandingSuccess = "SAVE_BRANDING_SUCCESS",
  saveBrandingError = "SAVE_BRANDING_ERROR",

  resetBrandingAction = "RESET_BRANDING",
  resetStateFlagsAction = "RESET_BRANDING_STATE_FLAGS",
}

export const loadBrandingPending = createAction<Partial<IBrandingStateContext>>(
  BrandingActionEnums.loadBrandingPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const loadBrandingSuccess = createAction<
  IBrandingStateContext,
  { branding: IBranding; isConfigured: boolean }
>(
  BrandingActionEnums.loadBrandingSuccess,
  (payload: { branding: IBranding; isConfigured: boolean }) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    branding: payload.branding,
    isConfigured: payload.isConfigured,
  })
);

/**
 * A failed load is not a blocking error for the app: the reducer keeps
 * whatever branding is already in state so the UI stays usable.
 */
export const loadBrandingError = createAction<Partial<IBrandingStateContext>>(
  BrandingActionEnums.loadBrandingError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const saveBrandingPending = createAction<Partial<IBrandingStateContext>>(
  BrandingActionEnums.saveBrandingPending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const saveBrandingSuccess = createAction<
  IBrandingStateContext,
  { branding: IBranding }
>(
  BrandingActionEnums.saveBrandingSuccess,
  (payload: { branding: IBranding }) => ({
    isPending: false,
    isSuccess: true,
    isError: false,
    branding: payload.branding,
    isConfigured: true,
  })
);

export const saveBrandingError = createAction<Partial<IBrandingStateContext>>(
  BrandingActionEnums.saveBrandingError,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

export const resetBrandingAction = createAction<Partial<IBrandingStateContext>>(
  BrandingActionEnums.resetBrandingAction,
  () => ({
    isPending: false,
    isSuccess: false,
    isError: false,
    branding: DEFAULT_BRANDING,
    isConfigured: false,
  })
);

export const resetStateFlagsAction = createAction<Partial<IBrandingStateContext>>(
  BrandingActionEnums.resetStateFlagsAction,
  () => ({ isPending: false, isSuccess: false, isError: false })
);
