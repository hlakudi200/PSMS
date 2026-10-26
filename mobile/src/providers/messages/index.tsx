import { useCallback, useContext, useEffect, useMemo, useReducer, useRef } from "react";
import { getApiErrorMessage, getAxiosInstance } from "../../utils/axios-instance";
import { replySubject, type ComposeForm } from "../../utils/message-forms";
import { MessagesReducer } from "./reducer";
import {
  INITIAL_STATE, MessagesActionContext, MessagesStateContext,
  type IMessage, type IMessageList, type IMessageRecipient, type IMessagesActionContext, type IMessageSummary, type MessageBox,
} from "./context";
import {
  listError, listPending, listSuccess, loadMoreDone, loadMorePending, recipientsError, recipientsPending,
  recipientsSuccess, sendError, sendPending, sendSuccess, threadError, threadPending, threadRead, threadSuccess,
  unreadCountSuccess,
} from "./actions";

const MESSAGES = "/api/services/app/Message";
const PAGE_SIZE = 20;
const BOX_ENDPOINT: Record<MessageBox, string> = { inbox: "GetInbox", sent: "GetSent" };

const mapSummary = (dto: any): IMessageSummary => ({
  id: dto.id,
  // Every message carries its thread; a conversation's first message uses its own id.
  threadId: dto.threadId ?? dto.id,
  senderUserId: dto.senderUserId,
  recipientUserId: dto.recipientUserId,
  senderName: dto.senderName || undefined,
  recipientName: dto.recipientName || undefined,
  subject: dto.subject || undefined,
  isRead: !!dto.isRead,
  creationTime: dto.creationTime,
});

const mapMessage = (dto: any): IMessage => ({
  ...mapSummary(dto),
  content: dto.content ?? "",
  parentMessageId: dto.parentMessageId ?? undefined,
});

export const MessagesProvider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer(MessagesReducer, INITIAL_STATE);
  const instance = useRef(getAxiosInstance()).current;
  const stateRef = useRef(state);
  useEffect(() => { stateRef.current = state; }, [state]);
  // Switching box or conversation before the last load lands: only the latest may write.
  const listRequestRef = useRef(0);
  const threadRequestRef = useRef(0);

  const fetchPage = useCallback(async (box: MessageBox, skipCount: number): Promise<IMessageList> => {
    const { data } = await instance.get(`${MESSAGES}/${BOX_ENDPOINT[box]}`, {
      params: { SkipCount: skipCount, MaxResultCount: PAGE_SIZE, Sorting: "CreationTime DESC" },
    });
    return { items: (data.result.items as any[]).map(mapSummary), totalCount: data.result.totalCount };
  }, [instance]);

  const getBoxAsync = useCallback(async (box: MessageBox) => {
    const requestId = ++listRequestRef.current;
    dispatch(listPending());
    try {
      const page = await fetchPage(box, 0);
      if (requestId === listRequestRef.current) dispatch(listSuccess({ [box]: page }));
    } catch {
      if (requestId === listRequestRef.current) dispatch(listError());
    }
  }, [fetchPage]);

  const loadMoreAsync = useCallback(async (box: MessageBox) => {
    const { isLoadingMore, isListPending } = stateRef.current;
    const loaded = stateRef.current[box];
    if (!loaded || isLoadingMore || isListPending || loaded.items.length >= loaded.totalCount) return;
    const requestId = listRequestRef.current;
    dispatch(loadMorePending());
    try {
      const page = await fetchPage(box, loaded.items.length);
      if (requestId !== listRequestRef.current) {
        dispatch(loadMoreDone({}));
        return;
      }
      // A message that arrived since the first page shifts the rest along by one; don't list it twice.
      const seen = new Set(loaded.items.map((m) => m.id));
      dispatch(loadMoreDone({
        [box]: { items: [...loaded.items, ...page.items.filter((m) => !seen.has(m.id))], totalCount: page.totalCount },
      }));
    } catch {
      dispatch(loadMoreDone({}));
    }
  }, [fetchPage]);

  const loadThread = useCallback(async (threadId: string) => {
    const { data } = await instance.get(`${MESSAGES}/GetThread`, { params: { threadId } });
    return (data.result.items as any[]).map(mapMessage);
  }, [instance]);

  const openThreadAsync = useCallback(async (threadId: string, currentUserId: number) => {
    const requestId = ++threadRequestRef.current;
    dispatch(threadPending(threadId));
    let thread: IMessage[];
    try {
      thread = await loadThread(threadId);
    } catch {
      if (requestId === threadRequestRef.current) dispatch(threadError());
      return;
    }
    if (requestId !== threadRequestRef.current) return;
    dispatch(threadSuccess(thread));

    // Opening it is reading it. One at a time: the shared database pool is small.
    const unread = thread.filter((m) => m.recipientUserId === currentUserId && !m.isRead);
    const marked = new Set<string>();
    for (const message of unread) {
      try {
        await instance.post(`${MESSAGES}/MarkAsRead`, null, { params: { id: message.id } });
        marked.add(message.id);
      } catch {
        // Left unread; it will be tried again next time the conversation opens.
      }
    }
    if (marked.size === 0 || requestId !== threadRequestRef.current) return;
    const { inbox, unreadCount } = stateRef.current;
    dispatch(threadRead({
      thread: thread.map((m) => (marked.has(m.id) ? { ...m, isRead: true } : m)),
      ...(inbox && { inbox: { ...inbox, items: inbox.items.map((m) => (marked.has(m.id) ? { ...m, isRead: true } : m)) } }),
      ...(unreadCount != null && { unreadCount: Math.max(0, unreadCount - marked.size) }),
    }));
  }, [instance, loadThread]);

  const getUnreadCountAsync = useCallback(async () => {
    try {
      // The count is the inbox's unread total; one row is the least the server will page.
      const { data } = await instance.get(`${MESSAGES}/GetInbox`, { params: { IsRead: false, MaxResultCount: 1 } });
      dispatch(unreadCountSuccess(data.result.totalCount ?? 0));
    } catch {
      // The badge is best-effort: keep the last count rather than show an error on every screen.
    }
  }, [instance]);

  const getRecipientsAsync = useCallback(async () => {
    dispatch(recipientsPending());
    try {
      const { data } = await instance.get(`${MESSAGES}/GetMyRecipients`);
      dispatch(recipientsSuccess((data.result.items as any[]).map((r): IMessageRecipient => ({
        userId: r.userId, name: r.name, roles: r.roles ?? [],
      }))));
    } catch {
      dispatch(recipientsError());
    }
  }, [instance]);

  const sendAsync = useCallback(async (form: ComposeForm) => {
    dispatch(sendPending());
    try {
      const { data } = await instance.post(`${MESSAGES}/Send`, {
        recipientUserId: form.recipientUserId,
        subject: form.subject || undefined,
        content: form.content,
      });
      dispatch(sendSuccess({}));
      return mapMessage(data.result).threadId;
    } catch (error) {
      dispatch(sendError(getApiErrorMessage(error, "Couldn't send your message.")));
      return undefined;
    }
  }, [instance]);

  const replyAsync = useCallback(async (content: string, currentUserId: number) => {
    const { thread, threadId } = stateRef.current;
    const last = thread?.[thread.length - 1];
    if (!last || !threadId) return false;
    // The server only accepts a reply to the other person in the conversation.
    const otherParty = last.senderUserId === currentUserId ? last.recipientUserId : last.senderUserId;
    dispatch(sendPending());
    try {
      await instance.post(`${MESSAGES}/Send`, {
        recipientUserId: otherParty,
        subject: replySubject(thread[0]?.subject),
        content,
        parentMessageId: last.id,
      });
      // Read back, so the conversation shows what the server stored.
      const refreshed = await loadThread(threadId).catch(() => undefined);
      dispatch(sendSuccess({ thread: refreshed }));
      return true;
    } catch (error) {
      dispatch(sendError(getApiErrorMessage(error, "Couldn't send your reply.")));
      return false;
    }
  }, [instance, loadThread]);

  const clearSendError = useCallback(() => dispatch(sendError(undefined)), []);

  const actions = useMemo<IMessagesActionContext>(
    () => ({
      getBoxAsync, loadMoreAsync, openThreadAsync, getRecipientsAsync, sendAsync, replyAsync, clearSendError,
      getUnreadCountAsync,
    }),
    [getBoxAsync, loadMoreAsync, openThreadAsync, getRecipientsAsync, sendAsync, replyAsync, clearSendError,
      getUnreadCountAsync]
  );

  return (
    <MessagesStateContext.Provider value={state}>
      <MessagesActionContext.Provider value={actions}>{children}</MessagesActionContext.Provider>
    </MessagesStateContext.Provider>
  );
};

export const useMessagesState = () => useContext(MessagesStateContext);
export const useMessagesActions = () => {
  const context = useContext(MessagesActionContext);
  if (!context) throw new Error("useMessagesActions must be used within a MessagesProvider");
  return context;
};
