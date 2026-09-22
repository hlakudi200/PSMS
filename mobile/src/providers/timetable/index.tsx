import { useCallback, useContext, useMemo, useReducer, useRef } from "react";
import axios from "axios";
import { getAxiosInstance } from "../../utils/axios-instance";
import { TimetableReducer } from "./reducer";
import {
  INITIAL_STATE, TimetableActionContext, TimetableStateContext,
  type ITimetableActionContext, type ITimetableSlot,
} from "./context";
import { getError, getPending, getSuccess } from "./actions";

const mapSlot = (dto: any): ITimetableSlot => ({
  id: dto.id,
  dayOfWeek: dto.dayOfWeek,
  periodNumber: dto.periodNumber,
  startTime: dto.startTime,
  endTime: dto.endTime,
  subjectName: dto.subjectName ?? undefined,
  teacherName: dto.teacherName ?? undefined,
  roomNumber: dto.roomNumber ?? undefined,
});

export const TimetableProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(TimetableReducer, INITIAL_STATE);
  const instance = useRef(getAxiosInstance()).current;

  const getForClassAsync = useCallback(async (classId: string) => {
    dispatch(getPending());
    try {
      const { data } = await instance.get("/api/services/app/Timetable/GetByClass", { params: { classId } });
      const timetableId: string = data.result.id;

      const { data: slotsData } = await instance.get("/api/services/app/TimetableSlot/GetByTimetable", {
        params: { timetableId },
      });
      const slots = (slotsData.result.items as any[]).map(mapSlot);
      dispatch(getSuccess({ timetableId, slots }));
    } catch (error) {
      // A "no active timetable for this class" business error (the server
      // responded, it just has nothing to give) is a legitimate empty state,
      // not a failure — only a transport-level failure with no response
      // should show the hard error/retry state.
      if (axios.isAxiosError(error) && error.response) {
        dispatch(getSuccess({ timetableId: undefined, slots: [] }));
      } else {
        dispatch(getError());
      }
    }
  }, [instance]);

  const actions = useMemo<ITimetableActionContext>(() => ({ getForClassAsync }), [getForClassAsync]);

  return (
    <TimetableStateContext.Provider value={state}>
      <TimetableActionContext.Provider value={actions}>{children}</TimetableActionContext.Provider>
    </TimetableStateContext.Provider>
  );
};

export const useTimetableState = () => useContext(TimetableStateContext);
export const useTimetableActions = () => {
  const context = useContext(TimetableActionContext);
  if (!context) throw new Error("useTimetableActions must be used within a TimetableProvider");
  return context;
};
