import axios from "axios";
import { useCallback, useContext, useEffect, useMemo, useReducer, useRef } from "react";
import { getAxiosInstance } from "../../utils/axios-instance";
import { AnnouncementsReducer } from "./reducer";
import {
  AnnouncementsActionContext, AnnouncementsStateContext, INITIAL_STATE,
  type IAnnouncement, type IAnnouncementsActionContext, type IAnnouncementSummary,
} from "./context";
import {
  detailError, detailPending, detailSuccess, getError, getPending, getSuccess, loadMoreDone, loadMorePending,
  markedRead, unreadCountSuccess,
} from "./actions";

const ANNOUNCEMENTS = "/api/services/app/Announcement";
const READS = "/api/services/app/AnnouncementRead";
const PAGE_SIZE = 20;
// The server only returns, counts and marks what is addressed to this family (#203).
const FEED_SORTING = "IsPinned DESC, PublishDate DESC";
const ALREADY_READ = "COMM_ALREADY_MARKED_AS_READ";

const mapSummary = (dto: any): IAnnouncementSummary => ({
  id: dto.id,
  title: dto.title,
  type: dto.type,
  priority: dto.priority,
  targetAudience: dto.targetAudience,
  publishDate: dto.publishDate,
  expiryDate: dto.expiryDate ?? undefined,
  isPinned: !!dto.isPinned,
  isReadByMe: !!dto.isReadByMe,
});

const mapAnnouncement = (dto: any): IAnnouncement => ({
  ...mapSummary(dto),
  content: dto.content ?? "",
  attachmentUrl: dto.attachmentUrl || undefined,
});

export const AnnouncementsProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(AnnouncementsReducer, INITIAL_STATE);
  const instance = useRef(getAxiosInstance()).current;
  const stateRef = useRef(state);
  useEffect(() => { stateRef.current = state; }, [state]);
  // A refresh can start while a next page is still loading; only the latest feed request may write.
  const feedRequestRef = useRef(0);
  const detailRequestRef = useRef(0);

  const fetchPage = useCallback(async (skipCount: number) => {
    const { data } = await instance.get(`${ANNOUNCEMENTS}/GetAll`, {
      params: { SkipCount: skipCount, MaxResultCount: PAGE_SIZE, Sorting: FEED_SORTING },
    });
    return {
      items: (data.result.items as any[]).map(mapSummary),
      totalCount: data.result.totalCount as number,
    };
  }, [instance]);

  const getUnreadCountAsync = useCallback(async () => {
    try {
      const { data } = await instance.get(`${READS}/GetUnreadCount`);
      dispatch(unreadCountSuccess(data.result ?? 0));
    } catch {
      // The badge is best-effort: keep the last count rather than show an error on every screen.
    }
  }, [instance]);

  const getAnnouncementsAsync = useCallback(async () => {
    const requestId = ++feedRequestRef.current;
    dispatch(getPending());
    try {
      const page = await fetchPage(0);
      if (requestId !== feedRequestRef.current) return;
      dispatch(getSuccess({ announcements: page.items, totalCount: page.totalCount }));
    } catch {
      if (requestId === feedRequestRef.current) dispatch(getError());
    }
    // Sequential: the shared database pool is small.
    await getUnreadCountAsync();
  }, [fetchPage, getUnreadCountAsync]);

  const loadMoreAsync = useCallback(async () => {
    const { announcements, totalCount, isLoadingMore, isPending } = stateRef.current;
    if (!announcements || isLoadingMore || isPending || totalCount == null || announcements.length >= totalCount) return;
    const requestId = feedRequestRef.current;
    dispatch(loadMorePending());
    try {
      const page = await fetchPage(announcements.length);
      if (requestId !== feedRequestRef.current) {
        dispatch(loadMoreDone({}));
        return;
      }
      // A notice published since the first page shifts the rest along by one; don't list it twice.
      const seen = new Set(announcements.map((a) => a.id));
      dispatch(loadMoreDone({
        announcements: [...announcements, ...page.items.filter((a) => !seen.has(a.id))],
        totalCount: page.totalCount,
      }));
    } catch {
      dispatch(loadMoreDone({}));
    }
  }, [fetchPage]);

  const openAnnouncementAsync = useCallback(async (id: string) => {
    const requestId = ++detailRequestRef.current;
    dispatch(detailPending());
    let detail: IAnnouncement;
    try {
      const { data } = await instance.get(`${ANNOUNCEMENTS}/Get`, { params: { id } });
      detail = mapAnnouncement(data.result);
    } catch {
      if (requestId === detailRequestRef.current) dispatch(detailError());
      return;
    }
    if (requestId !== detailRequestRef.current) return;
    dispatch(detailSuccess(detail));
    if (detail.isReadByMe) return;

    try {
      await instance.post(`${READS}/MarkAsRead`, null, { params: { announcementId: id } });
    } catch (error) {
      // Read on another device in the meantime: it is read either way. The server
      // puts its error code in a header and a sentence in the body.
      const code = axios.isAxiosError(error) ? error.response?.headers?.["x-error-code"] : undefined;
      if (code !== ALREADY_READ) return;
    }
    // Only the parts that are loaded: an undefined key would clear what the reducer merges into.
    const current = stateRef.current;
    dispatch(markedRead({
      ...(current.detail?.id === id && { detail: { ...current.detail, isReadByMe: true } }),
      ...(current.announcements && {
        announcements: current.announcements.map((a) => (a.id === id ? { ...a, isReadByMe: true } : a)),
      }),
      ...(current.unreadCount != null && { unreadCount: Math.max(0, current.unreadCount - 1) }),
    }));
  }, [instance]);

  const actions = useMemo<IAnnouncementsActionContext>(
    () => ({ getAnnouncementsAsync, loadMoreAsync, openAnnouncementAsync, getUnreadCountAsync }),
    [getAnnouncementsAsync, loadMoreAsync, openAnnouncementAsync, getUnreadCountAsync]
  );

  return (
    <AnnouncementsStateContext.Provider value={state}>
      <AnnouncementsActionContext.Provider value={actions}>{children}</AnnouncementsActionContext.Provider>
    </AnnouncementsStateContext.Provider>
  );
};

export const useAnnouncementsState = () => useContext(AnnouncementsStateContext);
export const useAnnouncementsActions = () => {
  const context = useContext(AnnouncementsActionContext);
  if (!context) throw new Error("useAnnouncementsActions must be used within an AnnouncementsProvider");
  return context;
};
