import { handleActions } from "redux-actions";
import { INITIAL_STATE, IBrandingStateContext } from "./context";
import { BrandingActionEnums } from "./actions";

// Payload is Partial: the pending/error/flag actions carry only the flags and
// rely on the spread below to preserve whatever branding is already loaded.
export const BrandingReducer = handleActions<
  IBrandingStateContext,
  Partial<IBrandingStateContext>
>(
  {
    [BrandingActionEnums.loadBrandingPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [BrandingActionEnums.loadBrandingSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [BrandingActionEnums.loadBrandingError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),

    [BrandingActionEnums.saveBrandingPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [BrandingActionEnums.saveBrandingSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [BrandingActionEnums.saveBrandingError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),

    [BrandingActionEnums.resetBrandingAction]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [BrandingActionEnums.resetStateFlagsAction]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
