import { handleActions } from "redux-actions";
import { INITIAL_STATE, ISubjectStateContext } from "./context";
import { SubjectActionEnums } from "./actions";

export const SubjectReducer = handleActions<ISubjectStateContext, ISubjectStateContext>(
  {
    [SubjectActionEnums.getSubjectsPending]: (state, action) => ({ ...state, ...action.payload }),
    [SubjectActionEnums.getSubjectsSuccess]: (state, action) => ({ ...state, ...action.payload }),
    [SubjectActionEnums.getSubjectsError]: (state, action) => ({ ...state, ...action.payload }),
    [SubjectActionEnums.getSubjectPending]: (state, action) => ({ ...state, ...action.payload }),
    [SubjectActionEnums.getSubjectSuccess]: (state, action) => ({ ...state, ...action.payload }),
    [SubjectActionEnums.getSubjectError]: (state, action) => ({ ...state, ...action.payload }),
    [SubjectActionEnums.createSubjectPending]: (state, action) => ({ ...state, ...action.payload }),
    [SubjectActionEnums.createSubjectSuccess]: (state, action) => ({ ...state, ...action.payload }),
    [SubjectActionEnums.createSubjectError]: (state, action) => ({ ...state, ...action.payload }),
    [SubjectActionEnums.updateSubjectPending]: (state, action) => ({ ...state, ...action.payload }),
    [SubjectActionEnums.updateSubjectSuccess]: (state, action) => ({ ...state, ...action.payload }),
    [SubjectActionEnums.updateSubjectError]: (state, action) => ({ ...state, ...action.payload }),
    [SubjectActionEnums.deleteSubjectPending]: (state, action) => ({ ...state, ...action.payload }),
    [SubjectActionEnums.deleteSubjectSuccess]: (state, action) => ({ ...state, ...action.payload }),
    [SubjectActionEnums.deleteSubjectError]: (state, action) => ({ ...state, ...action.payload }),
  },
  INITIAL_STATE
);
