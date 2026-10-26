import { useCallback, useContext, useEffect, useMemo, useReducer, useRef } from "react";
import { getApiErrorMessage, getAxiosInstance } from "../../utils/axios-instance";
import { NotificationsReducer } from "./reducer";
import {
  INITIAL_STATE, NotificationsActionContext, NotificationsStateContext,
  type INotification, type INotificationsActionContext,
} from "./context";
import {
  getError, getPending, getSuccess, loadMoreDone, loadMorePending, markAllDone, markAllPending, readChanged,
  unreadCountSuccess,
} from "./actions";

const NOTIFICATIONS = "/api/services/app/Notification";
const PAGE_SIZE = 30;

const mapNotification = (dto: any): INotification => ({
  id: dto.id,
  title: dto.title,
  message: dto.message ?? "",
  type: dto.type,
  priority: dto.priority,
  entityType: dto.entityType || undefined,
  entityId: dto.entityId || undefined,
  isRead: !!dto.isRead,
  creationTime: dto.creationTime,
});

export const NotificationsProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(NotificationsReducer, INITIAL_STATE);
  const instance = useRef(getAxiosInstance()).current;
  const stateRef = useRef(state);
  useEffect(() => { stateRef.current = state; }, [state]);
  // A refresh can start while a next page is still loading; only the latest list request may write.
  const listRequestRef = useRef(0);

  const fetchPage = useCallback(async (skipCount: number) => {
    const { data } = await instance.get(`${NOTIFICATIONS}/GetAll`, {
      params: { SkipCount: skipCount, MaxResultCount: PAGE_SIZE, Sorting: "CreationTime DESC" },
    });
    return {
      items: (data.result.items as any[]).map(mapNotification),
      totalCount: data.result.totalCount as number,
    };
  }, [instance]);

  const getUnreadCountAsync = useCallback(async () => {
    try {
      const { data } = await instance.get(`${NOTIFICATIONS}/GetUnreadCount`);
      dispatch(unreadCountSuccess(data.result ?? 0));
    } catch {
      // The badge is best-effort: keep the last count rather than show an error on every screen.
    }
  }, [instance]);

  const getNotificationsAsync = useCallback(async () => {
    const requestId = ++listRequestRef.current;
    dispatch(getPending());
    try {
      const page = await fetchPage(0);
      if (requestId === listRequestRef.current) dispatch(getSuccess({ notifications: page.items, totalCount: page.totalCount }));
    } catch {
      if (requestId === listRequestRef.current) dispatch(getError());
    }
    // Sequential: the shared database pool is small.
    await getUnreadCountAsync();
  }, [fetchPage, getUnreadCountAsync]);

  const loadMoreAsync = useCallback(async () => {
    const { notifications, totalCount, isLoadingMore, isPending } = stateRef.current;
    if (!notifications || isLoadingMore || isPending || totalCount == null || notifications.length >= totalCount) return;
    const requestId = listRequestRef.current;
    dispatch(loadMorePending());
    try {
      const page = await fetchPage(notifications.length);
      if (requestId !== listRequestRef.current) {
        dispatch(loadMoreDone({}));
        return;
      }
      // One that arrived since the first page shifts the rest along by one; don't list it twice.
      const seen = new Set(notifications.map((n) => n.id));
      dispatch(loadMoreDone({
        notifications: [...notifications, ...page.items.filter((n) => !seen.has(n.id))],
        totalCount: page.totalCount,
      }));
    } catch {
      dispatch(loadMoreDone({}));
    }
  }, [fetchPage]);

  const markAsReadAsync = useCallback(async (id: string) => {
    const { notifications, unreadCount } = stateRef.current;
    const target = notifications?.find((n) => n.id === id);
    if (!target || target.isRead) return;

    const setRead = (isRead: boolean) => stateRef.current.notifications?.map((n) => (n.id === id ? { ...n, isRead } : n));
    dispatch(readChanged({
      notifications: setRead(true),
      ...(unreadCount != null && { unreadCount: Math.max(0, unreadCount - 1) }),
    }));
    try {
      await instance.post(`${NOTIFICATIONS}/MarkAsRead`, null, { params: { id } });
    } catch (error) {
      const current = stateRef.current.unreadCount;
      dispatch(readChanged({
        notifications: setRead(false),
        ...(current != null && { unreadCount: current + 1 }),
        actionErrorMessage: getApiErrorMessage(error, "Couldn't mark that as read."),
      }));
    }
  }, [instance]);

  const markAllAsReadAsync = useCallback(async () => {
    dispatch(markAllPending());
    try {
      await instance.post(`${NOTIFICATIONS}/MarkAllAsRead`);
      dispatch(markAllDone({
        notifications: stateRef.current.notifications?.map((n) => (n.isRead ? n : { ...n, isRead: true })),
        unreadCount: 0,
      }));
    } catch (error) {
      dispatch(markAllDone({ actionErrorMessage: getApiErrorMessage(error, "Couldn't mark everything as read.") }));
    }
  }, [instance]);

  const actions = useMemo<INotificationsActionContext>(
    () => ({ getNotificationsAsync, loadMoreAsync, markAsReadAsync, markAllAsReadAsync, getUnreadCountAsync }),
    [getNotificationsAsync, loadMoreAsync, markAsReadAsync, markAllAsReadAsync, getUnreadCountAsync]
  );

  return (
    <NotificationsStateContext.Provider value={state}>
      <NotificationsActionContext.Provider value={actions}>{children}</NotificationsActionContext.Provider>
    </NotificationsStateContext.Provider>
  );
};

export const useNotificationsState = () => useContext(NotificationsStateContext);
export const useNotificationsActions = () => {
  const context = useContext(NotificationsActionContext);
  if (!context) throw new Error("useNotificationsActions must be used within a NotificationsProvider");
  return context;
};
