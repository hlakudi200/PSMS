'use client';

/**
 * What a teacher has been asked to do for admissions.
 *
 * Two steps of the admissions workflow belong to them — the interview and the
 * placement assessment — and both records could only be read through the
 * application they belong to, which a teacher cannot read. This replaces the
 * interviews-only page now that the assessment step exists too.
 */
export { default } from '@/components/modules/admissions/MyAdmissionsTasksContent';
