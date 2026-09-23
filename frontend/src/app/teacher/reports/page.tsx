'use client';

/**
 * A teacher's report cards. The server scopes this to the classes they register
 * or teach a subject in (ReportAppService.TeacherClassScopeAsync), and the list
 * hides the submit/approve/publish controls a teacher has no permission for —
 * what is left is the route to open a card, write the subject comment printed
 * under their own name, and sign the Class Teacher line.
 *
 * Until this page existed the teacher portal had a report detail route and no
 * way to reach it, so neither was possible.
 */
export { default } from '@/components/modules/assessment/ReportsPageContent';
