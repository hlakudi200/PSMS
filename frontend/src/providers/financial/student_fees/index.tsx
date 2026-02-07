"use client";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  INITIAL_STATE,
  StudentFeeActionContext,
  StudentFeeStateContext,
} from "./context";
import {
  ICreateStudentFee,
  IBulkCreateStudentFees,
  IUpdateStudentFee,
  IGetStudentFeesInput,
} from "../shared/interfaces";
import { StudentFeeReducer } from "./reducer";
import { useContext, useReducer } from "react";
import {
    getStudentFeePending,
    getStudentFeeSuccess,
    getStudentFeeError,
    getAllStudentFeesPending,
    getAllStudentFeesSuccess,
    getAllStudentFeesError,
    getByStudentPending,
    getByStudentSuccess,
    getByStudentError,
    createStudentFeePending,
    createStudentFeeSuccess,
    createStudentFeeError,
    bulkCreatePending,
    bulkCreateSuccess,
    bulkCreateError,
    updateStudentFeePending,
    updateStudentFeeSuccess,
    updateStudentFeeError,
    deleteStudentFeePending,
    deleteStudentFeeSuccess,
    deleteStudentFeeError,
    applyDiscountPending,
    applyDiscountSuccess,
    applyDiscountError,
    waiveFeePending,
    waiveFeeSuccess,
    waiveFeeError,
    cancelFeePending,
    cancelFeeSuccess,
    cancelFeeError,
    checkOverdueFeesPending,
    checkOverdueFeesSuccess,
    checkOverdueFeesError,
} from "./actions";

export const StudentFeeProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(StudentFeeReducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(getStudentFeePending());
    const endpoint = `/api/services/app/StudentFee/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getStudentFeeSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getStudentFeeError());
      });
  };

  const getAllAsync = async (input?: IGetStudentFeesInput) => {
    dispatch(getAllStudentFeesPending());

    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append('MaxResultCount', input.maxResultCount.toString());
    if (input?.skipCount) params.append('SkipCount', input.skipCount.toString());
    if (input?.sorting) params.append('Sorting', input.sorting);
    if (input?.studentId) params.append('StudentId', input.studentId);
    if (input?.feeStructureId) params.append('FeeStructureId', input.feeStructureId);
    if (input?.gradeId) params.append('GradeId', input.gradeId);
    if (input?.academicYearId) params.append('AcademicYearId', input.academicYearId);
    if (input?.status) params.append('Status', input.status.toString());
    if (input?.studentName) params.append('StudentName', input.studentName);
    
    const endpoint = `/api/services/app/StudentFee/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAllStudentFeesSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAllStudentFeesError());
      });
  };

  const getByStudentAsync = async (studentId: string) => {
    dispatch(getByStudentPending());
    const endpoint = `/api/services/app/StudentFee/GetByStudent?studentId=${studentId}`;
    await instance
        .get(endpoint)
        .then((response) => {
            dispatch(getByStudentSuccess({
                items: response.data.result.items
            }));
        })
        .catch((error) => {
            console.error(error);
            dispatch(getByStudentError());
        });
    };

  const createAsync = async (input: ICreateStudentFee) => {
    dispatch(createStudentFeePending());
    const endpoint = `/api/services/app/StudentFee/Create`;

    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(createStudentFeeSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(createStudentFeeError());
      });
  };

  const bulkCreateAsync = async (input: IBulkCreateStudentFees) => {
    dispatch(bulkCreatePending());
    const endpoint = `/api/services/app/StudentFee/BulkCreate`;
    await instance
        .post(endpoint, input)
        .then((response) => {
            dispatch(bulkCreateSuccess({
                items: response.data.result.items
            }));
        })
        .catch((error) => {
            console.error(error);
            dispatch(bulkCreateError());
        });
    };

  const updateAsync = async (id: string, input: IUpdateStudentFee) => {
    dispatch(updateStudentFeePending());
    const endpoint = `/api/services/app/StudentFee/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(updateStudentFeeSuccess(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(updateStudentFeeError());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(deleteStudentFeePending());
    const endpoint = `/api/services/app/StudentFee/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(deleteStudentFeeSuccess());
      })
      .catch((error) => {
        console.error(error);
        dispatch(deleteStudentFeeError());
      });
  };

  const applyDiscountAsync = async (id: string, discountAmount: number) => {
    dispatch(applyDiscountPending());
    const endpoint = `/api/services/app/StudentFee/ApplyDiscount?id=${id}&discountAmount=${discountAmount}`;
    await instance
        .post(endpoint)
        .then((response) => {
            dispatch(applyDiscountSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(applyDiscountError());
        });
    };

    const waiveAsync = async (id: string) => {
    dispatch(waiveFeePending());
    const endpoint = `/api/services/app/StudentFee/Waive?id=${id}`;
    await instance
        .post(endpoint)
        .then((response) => {
            dispatch(waiveFeeSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(waiveFeeError());
        });
    };

    const cancelAsync = async (id: string) => {
    dispatch(cancelFeePending());
    const endpoint = `/api/services/app/StudentFee/Cancel?id=${id}`;
    await instance
        .post(endpoint)
        .then((response) => {
            dispatch(cancelFeeSuccess(response.data.result));
        })
        .catch((error) => {
            console.error(error);
            dispatch(cancelFeeError());
        });
    };

    const checkOverdueFeesAsync = async () => {
    dispatch(checkOverdueFeesPending());
    const endpoint = `/api/services/app/StudentFee/CheckOverdueFees`;
    await instance
        .post(endpoint)
        .then(() => {
            dispatch(checkOverdueFeesSuccess());
        })
        .catch((error) => {
            console.error(error);
            dispatch(checkOverdueFeesError());
        });
    };

  return (
    <StudentFeeStateContext.Provider value={state}>
      <StudentFeeActionContext.Provider
        value={{
          getAsync,
          getAllAsync,
          getByStudentAsync,
          createAsync,
          bulkCreateAsync,
          updateAsync,
          deleteAsync,
          applyDiscountAsync,
          waiveAsync,
          cancelAsync,
          checkOverdueFeesAsync,
        }}
      >
        {children}
      </StudentFeeActionContext.Provider>
    </StudentFeeStateContext.Provider>
  );
};

export const useStudentFeeState = () => {
  const context = useContext(StudentFeeStateContext);
  if (!context) {
    throw new Error("useStudentFeeState must be used within a StudentFeeProvider");
  }
  return context;
};

export const useStudentFeeActions = () => {
  const context = useContext(StudentFeeActionContext);
  if (!context) {
    throw new Error("useStudentFeeActions must be used within a StudentFeeProvider");
  }
  return context;
};
