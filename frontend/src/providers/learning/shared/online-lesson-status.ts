// Mirrors backend enum psms.Domain.Shared.Enums.OnlineLessonStatus so the
// frontend doesn't sprinkle magic numbers across the schedule modal, page
// content, and any future host shell.
export const ONLINE_LESSON_STATUS = {
  Scheduled: 1,
  InProgress: 2,
  Completed: 3,
  Cancelled: 4,
} as const;

export type OnlineLessonStatus =
  (typeof ONLINE_LESSON_STATUS)[keyof typeof ONLINE_LESSON_STATUS];

export const ACTIVE_LESSON_STATUSES: ReadonlyArray<OnlineLessonStatus> = [
  ONLINE_LESSON_STATUS.Scheduled,
  ONLINE_LESSON_STATUS.InProgress,
];
