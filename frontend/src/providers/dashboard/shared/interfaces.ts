// ============================================================
// Principal Dashboard Summary
// ============================================================
export interface IGradePerformance {
  gradeId: string;
  gradeName: string;
  gradeLevel: number;
  studentCount: number;
  reportCount: number;
  averagePercentage?: number | null;
  passRate?: number | null;
}

export interface IPrincipalDashboardSummary {
  totalStudents: number;
  totalTeachers: number;
  totalClasses: number;

  pendingAdmissions: number;
  openDisciplinaryCases: number;
  pendingLeaveRequests: number;
  pendingFieldTrips: number;
  pendingExpenses: number;
  pendingFeeWaivers: number;
  pendingTransfers: number;

  outstandingFeesTotal: number;
  overdueFeesCount: number;

  passMarkPercentage: number;
  gradePerformance: IGradePerformance[];
}
