"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import { buildQueryParams } from "@/utils/query-params";
import {
  INITIAL_STATE,
  DisciplinaryCaseActionContext,
  DisciplinaryCaseStateContext,
} from "./context";
import { DisciplinaryCaseReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
  getCasePending, getCaseSuccess, getCaseError,
  getCasesPending, getCasesSuccess, getCasesError,
  createCasePending, createCaseSuccess, createCaseError,
  updateCasePending, updateCaseSuccess, updateCaseError,
  deleteCasePending, deleteCaseSuccess, deleteCaseError,
  submitPending, submitSuccess, submitError,
  startInvestigationPending, startInvestigationSuccess, startInvestigationError,
  scheduleHearingPending, scheduleHearingSuccess, scheduleHearingError,
  recordOutcomePending, recordOutcomeSuccess, recordOutcomeError,
  resolvePending, resolveSuccess, resolveError,
  cancelPending, cancelSuccess, cancelError,
} from "./actions";

export const DisciplinaryCaseProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(DisciplinaryCaseReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getCasePending());
    const endpoint = `/api/services/app/DisciplinaryCase/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getCaseSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getCaseError());
      });
  };

  const getAllAsync = async (input?: Record<string, unknown>) => {
    dispatch(getCasesPending());
    const params = buildQueryParams(input as Record<string, unknown>);
    const endpoint = `/api/services/app/DisciplinaryCase/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getCasesSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getCasesError());
      });
  };

  const createAsync = async (input: Record<string, unknown>) => {
    dispatch(createCasePending());
    const endpoint = `/api/services/app/DisciplinaryCase/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createCaseSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createCaseError());
      });
  };

  const updateAsync = async (id: string, input: Record<string, unknown>) => {
    dispatch(updateCasePending());
    const endpoint = `/api/services/app/DisciplinaryCase/Update?id=${id}`;
    await instance
      .put(endpoint, input)
      .then((response) => {
        dispatch(updateCaseSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateCaseError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteCasePending());
    const endpoint = `/api/services/app/DisciplinaryCase/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteCaseSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteCaseError());
      });
  };

  const submitAsync = async (id: string) => {
    dispatch(submitPending());
    const endpoint = `/api/services/app/DisciplinaryCase/Submit?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(submitSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(submitError());
      });
  };

  const startInvestigationAsync = async (id: string) => {
    dispatch(startInvestigationPending());
    const endpoint = `/api/services/app/DisciplinaryCase/StartInvestigation?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(startInvestigationSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(startInvestigationError());
      });
  };

  const scheduleHearingAsync = async (id: string, date: string) => {
    dispatch(scheduleHearingPending());
    const endpoint = `/api/services/app/DisciplinaryCase/ScheduleHearing`;
    await instance
      .post(endpoint, { id, hearingDate: date })
      .then(() => {
        dispatch(scheduleHearingSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(scheduleHearingError());
      });
  };

  const recordOutcomeAsync = async (id: string, outcome: string, description: string) => {
    dispatch(recordOutcomePending());
    const endpoint = `/api/services/app/DisciplinaryCase/RecordOutcome`;
    await instance
      .post(endpoint, { id, outcome, description })
      .then(() => {
        dispatch(recordOutcomeSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(recordOutcomeError());
      });
  };

  const resolveAsync = async (id: string) => {
    dispatch(resolvePending());
    const endpoint = `/api/services/app/DisciplinaryCase/Resolve?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(resolveSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(resolveError());
      });
  };

  const cancelAsync = async (id: string) => {
    dispatch(cancelPending());
    const endpoint = `/api/services/app/DisciplinaryCase/Cancel?id=${id}`;
    await instance
      .post(endpoint)
      .then(() => {
        dispatch(cancelSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(cancelError());
      });
  };

  return (
    <DisciplinaryCaseStateContext.Provider value={state}>
      <DisciplinaryCaseActionContext.Provider
        value={{
          getAsync, getAllAsync, createAsync, updateAsync, deleteAsync,
          submitAsync, startInvestigationAsync, scheduleHearingAsync,
          recordOutcomeAsync, resolveAsync, cancelAsync,
        }}
      >
        {children}
      </DisciplinaryCaseActionContext.Provider>
    </DisciplinaryCaseStateContext.Provider>
  );
};

export const useDisciplinaryCaseState = () => {
  const context = useContext(DisciplinaryCaseStateContext);
  if (!context) {
    throw new Error("useDisciplinaryCaseState must be used within a DisciplinaryCaseProvider");
  }
  return context;
};

export const useDisciplinaryCaseActions = () => {
  const context = useContext(DisciplinaryCaseActionContext);
  if (!context) {
    throw new Error("useDisciplinaryCaseActions must be used within a DisciplinaryCaseProvider");
  }
  return context;
};
