import { handleActions } from "redux-actions";
import { INITIAL_STATE, IWorkflowDefinitionStateContext } from "./context";
import { WorkflowDefinitionActionEnums } from "./actions";

export const WorkflowDefinitionReducer = handleActions<
  IWorkflowDefinitionStateContext,
  IWorkflowDefinitionStateContext
>(
  {
    [WorkflowDefinitionActionEnums.getDefinitionPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.getDefinitionSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.getDefinitionError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.getDefinitionsPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.getDefinitionsSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.getDefinitionsError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.createDefinitionPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.createDefinitionSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.createDefinitionError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.updateDefinitionPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.updateDefinitionSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.updateDefinitionError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.deleteDefinitionPending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.deleteDefinitionSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.deleteDefinitionError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.activatePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.activateSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.activateError]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.deactivatePending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.deactivateSuccess]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [WorkflowDefinitionActionEnums.deactivateError]: (state, action) => ({
      ...state, ...action.payload,
    }),
  },
  INITIAL_STATE
);
