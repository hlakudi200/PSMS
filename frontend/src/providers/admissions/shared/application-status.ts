/**
 * ApplicationStatus, exactly as the API sends it.
 *
 * Mirrors psms.Domain.Shared.Enums.ApplicationStatus. Kept in one place
 * because it has been written out by hand twice already and both times the
 * numbers drifted from the enum — which is a quiet kind of wrong: a family
 * whose child had been **approved** was shown "Waitlisted", and one who had
 * **enrolled** was shown "Offer declined". Nothing failed; the screen simply
 * said the wrong thing about the most important question a parent has.
 *
 * If the C# enum changes, change it here and nowhere else.
 */
export const ApplicationStatus = {
  Draft: 1,
  Submitted: 2,
  PaymentPending: 3,
  UnderReview: 4,
  DocumentsRequired: 5,
  InterviewScheduled: 6,
  AssessmentScheduled: 7,
  UnderConsideration: 8,
  Approved: 9,
  Rejected: 10,
  Waitlisted: 11,
  Enrolled: 12,
  Withdrawn: 13,
  Expired: 14,
} as const;

export type ApplicationStatusValue = (typeof ApplicationStatus)[keyof typeof ApplicationStatus];

type Presentation = { label: string; color: string };

/**
 * How a status reads to the parent whose child it is about. Deliberately
 * plainer than the enum's own names: "UnderConsideration" is not something to
 * put in front of someone waiting to hear about their child.
 */
const forParents: Record<number, Presentation> = {
  [ApplicationStatus.Draft]: { label: 'Not sent yet', color: 'default' },
  [ApplicationStatus.Submitted]: { label: 'Submitted', color: 'processing' },
  [ApplicationStatus.PaymentPending]: { label: 'Waiting for the fee', color: 'warning' },
  [ApplicationStatus.UnderReview]: { label: 'Being reviewed', color: 'processing' },
  [ApplicationStatus.DocumentsRequired]: { label: 'Documents needed', color: 'warning' },
  [ApplicationStatus.InterviewScheduled]: { label: 'Interview arranged', color: 'processing' },
  [ApplicationStatus.AssessmentScheduled]: { label: 'Assessment arranged', color: 'processing' },
  [ApplicationStatus.UnderConsideration]: { label: 'Being decided', color: 'processing' },
  [ApplicationStatus.Approved]: { label: 'Offered a place', color: 'success' },
  [ApplicationStatus.Rejected]: { label: 'Not successful', color: 'error' },
  [ApplicationStatus.Waitlisted]: { label: 'On the waiting list', color: 'warning' },
  [ApplicationStatus.Enrolled]: { label: 'Enrolled', color: 'success' },
  [ApplicationStatus.Withdrawn]: { label: 'Withdrawn', color: 'default' },
  [ApplicationStatus.Expired]: { label: 'Expired', color: 'default' },
};

export const applicationStatusLabel = (status: number): string =>
  forParents[status]?.label ?? 'In progress';

export const applicationStatusColor = (status: number): string =>
  forParents[status]?.color ?? 'default';

/**
 * Whether the parent can still take this application back.
 *
 * The server's rule, mirrored: an enrolled learner is past the point of
 * withdrawing an application, and one already withdrawn or expired has
 * nothing left to withdraw. Everything in between — including a draft that
 * was never sent, which a parent otherwise has no way to close out — is fair.
 */
export const canWithdraw = (status: number): boolean =>
  status !== ApplicationStatus.Enrolled
  && status !== ApplicationStatus.Withdrawn
  && status !== ApplicationStatus.Expired;

/** Nothing more will happen to it. */
export const isFinished = (status: number): boolean =>
  status === ApplicationStatus.Rejected
  || status === ApplicationStatus.Withdrawn
  || status === ApplicationStatus.Expired
  || status === ApplicationStatus.Enrolled;

/** How far along the school's own process it is, for a progress bar. */
export const applicationStepIndex = (status: number): number => {
  const steps: Record<number, number> = {
    [ApplicationStatus.Draft]: 0,
    [ApplicationStatus.Submitted]: 1,
    [ApplicationStatus.PaymentPending]: 1,
    [ApplicationStatus.UnderReview]: 2,
    [ApplicationStatus.DocumentsRequired]: 2,
    [ApplicationStatus.InterviewScheduled]: 2,
    [ApplicationStatus.AssessmentScheduled]: 2,
    [ApplicationStatus.UnderConsideration]: 2,
    [ApplicationStatus.Approved]: 3,
    [ApplicationStatus.Enrolled]: 4,
    [ApplicationStatus.Rejected]: -1,
    [ApplicationStatus.Waitlisted]: -1,
    [ApplicationStatus.Withdrawn]: -1,
    [ApplicationStatus.Expired]: -1,
  };
  return steps[status] ?? 0;
};
