'use client'
import { createContext } from "react";
import {
  IOnlineLesson,
  IOnlineLessonList,
  ICreateOnlineLesson,
  IUpdateOnlineLesson,
  IGetOnlineLessonsInput,
  IRescheduleOnlineLesson,
  IAddRecording,
  IUploadRecording,
  IRequestRecordingUploadUrl,
  IFileUploadTicket,
  ILiveClassJoin,
  ILiveClassAttendance,
} from "../shared/interfaces";

export interface IOnlineLessonStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  onlineLesson?: IOnlineLesson;
  onlineLessons?: IOnlineLessonList[];
  totalCount?: number;
  // Separate slot for getByClassSubject so the modal's conflict pre-check
  // does not clobber the main list while a Schedule modal is open.
  lessonsByClassSubject?: IOnlineLessonList[];
  lessonsByClassSubjectPending?: boolean;
  lessonsByClassSubjectError?: boolean;
}

export interface IOnlineLessonActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetOnlineLessonsInput) => void;
  getByClassSubjectAsync: (classSubjectId: string) => void;
  getUpcomingAsync: () => void;
  createAsync: (input: ICreateOnlineLesson) => void;
  updateAsync: (id: string, input: IUpdateOnlineLesson) => void;
  deleteAsync: (id: string) => void;
  startAsync: (id: string) => void;
  endAsync: (id: string, attendeeCount: number) => void;
  cancelAsync: (id: string) => void;
  rescheduleAsync: (id: string, input: IRescheduleOnlineLesson) => void;
  addRecordingAsync: (id: string, input: IAddRecording) => void;
  // SF-02 direct upload. Step 1: get a signed URL. Step 2: PUT the bytes
  // straight to storage. Step 3: record the object key.
  requestRecordingUploadUrlAsync: (input: IRequestRecordingUploadUrl) => Promise<IFileUploadTicket>;
  uploadFileToStorageAsync: (uploadUrl: string, file: File) => Promise<void>;
  uploadRecordingAsync: (input: IUploadRecording) => Promise<void>;
  // LC-02: fetch a LiveKit join token (server decides publish vs view-only).
  getJoinTokenAsync: (lessonId: string) => Promise<ILiveClassJoin>;
  // LC-06: fetch a short-lived signed URL to play a lesson's (private) recording.
  getRecordingDownloadUrlAsync: (lessonId: string) => Promise<string | undefined>;
  // LC-05: fetch the distinct attendee roll-call (host/staff only).
  getLiveAttendanceAsync: (lessonId: string) => Promise<ILiveClassAttendance | undefined>;
}

export const INITIAL_STATE: IOnlineLessonStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const OnlineLessonStateContext =
  createContext<IOnlineLessonStateContext>(INITIAL_STATE);

export const OnlineLessonActionContext = createContext<
  IOnlineLessonActionContext | undefined
>(undefined);
