// Pagination Interfaces
export interface IPagedAndSortedResultRequest {
  maxResultCount?: number;
  skipCount?: number;
  sorting?: string;
}

export interface IPagedResult<T> {
  totalCount: number;
  items: T[];
}

export interface IListResult<T> {
  items: T[];
}

// ============================================================
// AfterCare
// ============================================================
export interface IAfterCare {
  id: string;
  academicYearId: string;
  programName: string;
  description?: string;
  afterCareType: number;
  location?: string;
  startTime: string;
  endTime: string;
  daysAvailable?: string;
  capacity: number;
  currentEnrollment: number;
  supervisorName?: string;
  contactPhone?: string;
  activitiesIncluded?: string;
  includesMeals: boolean;
  includesHomeworkSupervision: boolean;
  monthlyFee: number;
  isActive: boolean;
  enrollmentCount: number;
}

export interface IAfterCareList {
  id: string;
  academicYearId: string;
  programName: string;
  afterCareType: number;
  location?: string;
  startTime: string;
  endTime: string;
  capacity: number;
  currentEnrollment: number;
  supervisorName?: string;
  monthlyFee: number;
  isActive: boolean;
  enrollmentCount: number;
}

export interface ICreateAfterCare {
  academicYearId: string;
  programName: string;
  description?: string;
  afterCareType: number;
  location?: string;
  startTime: string;
  endTime: string;
  daysAvailable?: string;
  capacity: number;
  supervisorName?: string;
  contactPhone?: string;
  activitiesIncluded?: string;
  includesMeals: boolean;
  includesHomeworkSupervision: boolean;
  monthlyFee: number;
}

export interface IUpdateAfterCare {
  programName?: string;
  description?: string;
  afterCareType?: number;
  location?: string;
  startTime?: string;
  endTime?: string;
  daysAvailable?: string;
  capacity?: number;
  supervisorName?: string;
  contactPhone?: string;
  activitiesIncluded?: string;
  includesMeals?: boolean;
  includesHomeworkSupervision?: boolean;
  monthlyFee?: number;
}

export interface IGetAfterCaresInput extends IPagedAndSortedResultRequest {
  academicYearId?: string;
  afterCareType?: number;
  isActive?: boolean;
  programName?: string;
}

// ============================================================
// StudentAfterCare
// ============================================================
export interface IStudentAfterCare {
  id: string;
  studentId: string;
  afterCareId: string;
  academicYearId: string;
  daysEnrolled?: string;
  status: number;
  startDate: string;
  endDate?: string;
  dietaryRequirements?: string;
  medicalNotes?: string;
  authorizedPickupPersons?: string;
  usualPickupTime?: string;
  notes?: string;
  studentName?: string;
  studentAdmissionNumber?: string;
  afterCareProgramName?: string;
  academicYearName?: string;
}

export interface IStudentAfterCareList {
  id: string;
  studentId: string;
  afterCareId: string;
  academicYearId: string;
  status: number;
  startDate: string;
  endDate?: string;
  daysEnrolled?: string;
  studentName?: string;
  studentAdmissionNumber?: string;
  afterCareProgramName?: string;
  academicYearName?: string;
}

export interface ICreateStudentAfterCare {
  studentId: string;
  afterCareId: string;
  academicYearId: string;
  startDate: string;
  daysEnrolled?: string;
  dietaryRequirements?: string;
  medicalNotes?: string;
  authorizedPickupPersons?: string;
  usualPickupTime?: string;
  notes?: string;
}

export interface IUpdateStudentAfterCare {
  daysEnrolled?: string;
  dietaryRequirements?: string;
  medicalNotes?: string;
  authorizedPickupPersons?: string;
  usualPickupTime?: string;
  notes?: string;
}

export interface IGetStudentAfterCaresInput extends IPagedAndSortedResultRequest {
  studentId?: string;
  afterCareId?: string;
  academicYearId?: string;
  status?: number;
}

// ============================================================
// ExtramuralActivity
// ============================================================
export interface IExtramuralActivity {
  id: string;
  academicYearId: string;
  activityName: string;
  description?: string;
  category: number;
  activityType: number;
  venue?: string;
  dayOfWeek?: number;
  startTime?: string;
  endTime?: string;
  season?: number;
  termNumber?: number;
  minGrade?: number;
  maxGrade?: number;
  genderRestriction?: number;
  coachName?: string;
  coachPhone?: string;
  maxCapacity?: number;
  currentEnrollment: number;
  requirements?: string;
  feePerTerm: number;
  isActive: boolean;
  isRegistrationOpen: boolean;
  enrollmentCount: number;
}

export interface IExtramuralActivityList {
  id: string;
  academicYearId: string;
  activityName: string;
  category: number;
  activityType: number;
  venue?: string;
  dayOfWeek?: number;
  season?: number;
  maxCapacity?: number;
  currentEnrollment: number;
  coachName?: string;
  feePerTerm: number;
  isActive: boolean;
  isRegistrationOpen: boolean;
  enrollmentCount: number;
}

export interface ICreateExtramuralActivity {
  academicYearId: string;
  activityName: string;
  description?: string;
  category: number;
  activityType: number;
  venue?: string;
  dayOfWeek?: number;
  startTime?: string;
  endTime?: string;
  season?: number;
  termNumber?: number;
  minGrade?: number;
  maxGrade?: number;
  genderRestriction?: number;
  coachName?: string;
  coachPhone?: string;
  maxCapacity?: number;
  requirements?: string;
  feePerTerm: number;
}

export interface IUpdateExtramuralActivity {
  activityName?: string;
  description?: string;
  category?: number;
  activityType?: number;
  venue?: string;
  dayOfWeek?: number;
  startTime?: string;
  endTime?: string;
  season?: number;
  termNumber?: number;
  minGrade?: number;
  maxGrade?: number;
  genderRestriction?: number;
  coachName?: string;
  coachPhone?: string;
  maxCapacity?: number;
  requirements?: string;
  feePerTerm?: number;
}

export interface IGetExtramuralActivitiesInput extends IPagedAndSortedResultRequest {
  academicYearId?: string;
  category?: number;
  activityType?: number;
  isActive?: boolean;
  isRegistrationOpen?: boolean;
  activityName?: string;
}

// ============================================================
// StudentExtramural
// ============================================================
export interface IStudentExtramural {
  id: string;
  studentId: string;
  extramuralActivityId: string;
  academicYearId: string;
  termNumber?: number;
  status: number;
  startDate: string;
  endDate?: string;
  teamAssignment?: string;
  positionRole?: string;
  medicalNotes?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  consentFormSigned: boolean;
  consentFormDate?: string;
  achievements?: string;
  attendanceRate?: number;
  notes?: string;
  studentName?: string;
  studentAdmissionNumber?: string;
  activityName?: string;
  academicYearName?: string;
}

export interface IStudentExtramuralList {
  id: string;
  studentId: string;
  extramuralActivityId: string;
  academicYearId: string;
  status: number;
  startDate: string;
  endDate?: string;
  consentFormSigned: boolean;
  teamAssignment?: string;
  studentName?: string;
  studentAdmissionNumber?: string;
  activityName?: string;
  academicYearName?: string;
}

export interface ICreateStudentExtramural {
  studentId: string;
  extramuralActivityId: string;
  academicYearId: string;
  startDate: string;
  termNumber?: number;
  teamAssignment?: string;
  positionRole?: string;
  medicalNotes?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  notes?: string;
}

export interface IUpdateStudentExtramural {
  termNumber?: number;
  teamAssignment?: string;
  positionRole?: string;
  medicalNotes?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  attendanceRate?: number;
  notes?: string;
}

export interface IGetStudentExtramuralsInput extends IPagedAndSortedResultRequest {
  studentId?: string;
  extramuralActivityId?: string;
  academicYearId?: string;
  status?: number;
}

// ============================================================
// SchoolTransport
// ============================================================
export interface ISchoolTransport {
  id: string;
  routeName: string;
  description?: string;
  vehicleNumber?: string;
  transportType: number;
  capacity: number;
  currentEnrollment: number;
  driverName?: string;
  driverPhone?: string;
  areasCovered?: string;
  morningPickupTime?: string;
  afternoonDepartureTime?: string;
  monthlyFee: number;
  isActive: boolean;
  enrollmentCount: number;
}

export interface ISchoolTransportList {
  id: string;
  routeName: string;
  vehicleNumber?: string;
  transportType: number;
  capacity: number;
  currentEnrollment: number;
  driverName?: string;
  monthlyFee: number;
  isActive: boolean;
  enrollmentCount: number;
}

export interface ICreateSchoolTransport {
  routeName: string;
  description?: string;
  vehicleNumber?: string;
  transportType: number;
  capacity: number;
  driverName?: string;
  driverPhone?: string;
  areasCovered?: string;
  morningPickupTime?: string;
  afternoonDepartureTime?: string;
  monthlyFee: number;
}

export interface IUpdateSchoolTransport {
  routeName?: string;
  description?: string;
  vehicleNumber?: string;
  transportType?: number;
  capacity?: number;
  driverName?: string;
  driverPhone?: string;
  areasCovered?: string;
  morningPickupTime?: string;
  afternoonDepartureTime?: string;
  monthlyFee?: number;
}

export interface IGetSchoolTransportsInput extends IPagedAndSortedResultRequest {
  transportType?: number;
  isActive?: boolean;
  routeName?: string;
}

// ============================================================
// StudentTransport
// ============================================================
export interface IStudentTransport {
  id: string;
  studentId: string;
  schoolTransportId: string;
  academicYearId: string;
  direction: number;
  pickupAddress?: string;
  dropoffAddress?: string;
  pickupTime?: string;
  status: number;
  startDate: string;
  endDate?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  notes?: string;
  studentName?: string;
  studentAdmissionNumber?: string;
  routeName?: string;
  academicYearName?: string;
}

export interface IStudentTransportList {
  id: string;
  studentId: string;
  schoolTransportId: string;
  academicYearId: string;
  direction: number;
  status: number;
  startDate: string;
  endDate?: string;
  pickupAddress?: string;
  studentName?: string;
  studentAdmissionNumber?: string;
  routeName?: string;
  academicYearName?: string;
}

export interface ICreateStudentTransport {
  studentId: string;
  schoolTransportId: string;
  academicYearId: string;
  direction: number;
  startDate: string;
  pickupAddress?: string;
  dropoffAddress?: string;
  pickupTime?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  notes?: string;
}

export interface IUpdateStudentTransport {
  direction?: number;
  pickupAddress?: string;
  dropoffAddress?: string;
  pickupTime?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  notes?: string;
}

export interface IGetStudentTransportsInput extends IPagedAndSortedResultRequest {
  studentId?: string;
  schoolTransportId?: string;
  academicYearId?: string;
  status?: number;
  direction?: number;
}
