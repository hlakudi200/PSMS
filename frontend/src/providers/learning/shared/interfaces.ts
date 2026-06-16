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

  // ------------------------------------------------------------------
  // Material Versioning (T-T07 / US-TCH-003)
  // ------------------------------------------------------------------
  export interface ILearningMaterialVersion {
    id: string;
    learningMaterialId: string;
    versionNumber: number;
    changeDescription: string;
    fileName?: string;
    fileUrl?: string;
    fileSizeBytes?: number;
    contentType?: string;
    uploadedByUserId: number;
    creationTime: string;
  }

  // Multipart payload for uploading a new version of an existing material.
  // Direct-upload ticket: the server mints a one-time signed URL the client
  // PUTs the file to (bytes bypass the server), plus the public URL to store.
  export interface IFileUploadTicket {
    uploadUrl: string;
    publicUrl: string;
    objectKey: string;
  }

  export interface IRequestMaterialUploadUrl {
    classSubjectId: string;
    fileName: string;
    materialType: number;
  }

  export interface IRequestVersionUploadUrl {
    learningMaterialId: string;
    fileName: string;
  }

  // Posted after the new version's file has been uploaded directly to storage.
  export interface IUploadNewVersion {
    learningMaterialId: string;
    changeDescription: string;
    fileUrl: string;
    fileName: string;
    fileSizeBytes: number;
    contentType?: string;
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
    keyword?: string;
    /**
     * When true, the backend scopes results to lessons attached to a
     * class-subject the calling teacher is assigned to. Lets the teacher
     * portal show correct counts and pagination without leaking other
     * teachers' lessons.
     */
    mineOnly?: boolean;
  }
  
  export interface IRescheduleOnlineLesson {
    newStartTime: string;
    newEndTime: string;
  }
  
  export interface IAddRecording {
    recordingUrl: string;
  }

  // Multipart upload of a recording file for a Completed lesson. Server
  // validates type (mp4 / mov / avi / webm), size (5 GB cap, OL-003), and
  // that the lesson is in Completed status before persisting the URL.
  export interface IUploadRecording {
    lessonId: string;
    file: File;
  }
  