import { handleActions } from "redux-actions";
import { INITIAL_STATE, IParentStateContext } from "./context";
import { ParentActionEnums } from "./actions";

export const ParentReducer = handleActions<
  IParentStateContext,
  IParentStateContext
>(
  {
    [ParentActionEnums.getParentsPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ParentActionEnums.getParentsSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ParentActionEnums.getParentsError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ParentActionEnums.getParentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ParentActionEnums.getParentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ParentActionEnums.getParentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ParentActionEnums.createParentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ParentActionEnums.createParentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ParentActionEnums.createParentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ParentActionEnums.updateParentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ParentActionEnums.updateParentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ParentActionEnums.updateParentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ParentActionEnums.deleteParentPending]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ParentActionEnums.deleteParentSuccess]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
    [ParentActionEnums.deleteParentError]: (state, action) => ({
      ...state,
      ...action.payload,
    }),
  },
  INITIAL_STATE
);
