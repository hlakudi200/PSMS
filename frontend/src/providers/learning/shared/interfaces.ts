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
  
  // LearningMaterial Interfaces
  export interface ILearningMaterial {
    id: string;
    classSubjectId: string;
    termId?: string;
    title: string;
    description?: string;
    materialType: number;
    fileName?: string;
    fileUrl?: string;
    fileSizeBytes?: number;
    contentType?: string;
    externalLink?: string;
    displayOrder: number;
    isPublished: boolean;
    publishedDate?: string;
    uploadedByUserId: number;
    viewCount: number;
    className?: string;
    subjectName?: string;
    termName?: string;
  }
  
  export interface ILearningMaterialList {
    id: string;
    classSubjectId: string;
    title: string;
    materialType: number;
    isPublished: boolean;
    displayOrder: number;
    viewCount: number;
    fileName?: string;
    fileSizeBytes?: number;
  }
  
  export interface ICreateLearningMaterial {
    classSubjectId: string;
    termId?: string;
    title: string;
    description: string;
    materialType: number;
    fileName: string;
    fileUrl: string;
    fileSizeBytes?: number;
    contentType: string;
    externalLink: string;
    displayOrder: number;
  }
  
  export interface IUpdateLearningMaterial {
    title?: string;
    description?: string;
    materialType?: number;
    termId?: string;
    fileName?: string;
    fileUrl?: string;
    fileSizeBytes?: number;
    contentType?: string;
    externalLink?: string;
    displayOrder?: number;
  }
  
  export interface IGetLearningMaterialsInput extends IPagedAndSortedResultRequest {
    classSubjectId?: string;
    termId?: string;
    materialType?: number;
    isPublished?: boolean;
    keyword?: string;
  }
  
  // OnlineLesson Interfaces
  export interface IOnlineLesson {
    id: string;
    classSubjectId: string;
    title: string;
    description?: string;
    platform: number;
    meetingLink: string;
    meetingId?: string;
    meetingPassword?: string;
    scheduledStartTime: string;
    scheduledEndTime: string;
    durationMinutes: number;
    actualStartTime?: string;
    actualEndTime?: string;
    status: number;
    hostTeacherUserId: number;
    attendeeCount?: number;
    recordingUrl?: string;
    hasRecording: boolean;
    isRecurring: boolean;
    recurrencePattern?: string;
    className?: string;
    subjectName?: string;
  }
  
  export interface IOnlineLessonList {
    id: string;
    classSubjectId: string;
    title: string;
    platform: number;
    scheduledStartTime: string;
    scheduledEndTime: string;
    durationMinutes: number;
    status: number;
    hasRecording: boolean;
    className?: string;
    subjectName?: string;
  }
  
  export interface ICreateOnlineLesson {
    classSubjectId: string;
    title: string;
    description?: string;
    platform: number;
    meetingLink: string;
    meetingId?: string;
    meetingPassword?: string;
    scheduledStartTime: string;
    scheduledEndTime: string;
    isRecurring: boolean;
    recurrencePattern?: string;
  }
  
  export interface IUpdateOnlineLesson {
    title?: string;
    description?: string;
    meetingLink?: string;
    meetingId?: string;
    meetingPassword?: string;
  }
  
  export interface IGetOnlineLessonsInput extends IPagedAndSortedResultRequest {
    classSubjectId?: string;
    status?: number;
    startDate?: string;
    endDate?: string;
    hostTeacherUserId?: number;
  }
  
  export interface IRescheduleOnlineLesson {
    newStartTime: string;
    newEndTime: string;
  }
  
  export interface IAddRecording {
    recordingUrl: string;
  }
  