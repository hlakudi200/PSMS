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
// Announcement
// ============================================================
export interface IAnnouncement {
  id: string;
  title: string;
  content: string;
  type: number;
  priority: number;
  targetAudience: number;
  targetGradeId?: string;
  targetClassId?: string;
  attachmentUrl?: string;
  publishDate: string;
  expiryDate?: string;
  isPublished: boolean;
  isPinned: boolean;
  sendEmailNotification: boolean;
  sendPushNotification: boolean;
  createdByUserId: number;
  readCount: number;
}

export interface IAnnouncementList {
  id: string;
  title: string;
  type: number;
  priority: number;
  targetAudience: number;
  publishDate: string;
  expiryDate?: string;
  isPublished: boolean;
  isPinned: boolean;
  createdByUserId: number;
  readCount: number;
}

export interface ICreateAnnouncement {
  title: string;
  content: string;
  type: number;
  priority: number;
  targetAudience: number;
  targetGradeId?: string;
  targetClassId?: string;
  attachmentUrl?: string;
  publishDate?: string;
  expiryDate?: string;
  sendEmailNotification: boolean;
  sendPushNotification: boolean;
}

export interface IUpdateAnnouncement {
  title?: string;
  content?: string;
  type?: number;
  priority?: number;
  targetAudience?: number;
  targetGradeId?: string;
  targetClassId?: string;
  attachmentUrl?: string;
  publishDate?: string;
  expiryDate?: string;
  clearExpiryDate?: boolean;
  sendEmailNotification?: boolean;
  sendPushNotification?: boolean;
}

export interface IGetAnnouncementsInput extends IPagedAndSortedResultRequest {
  type?: number;
  priority?: number;
  targetAudience?: number;
  isPublished?: boolean;
  isPinned?: boolean;
  search?: string;
}

// ============================================================
// AnnouncementRead
// ============================================================
export interface IAnnouncementRead {
  id: string;
  announcementId: string;
  userId: number;
  readDate: string;
}

// ============================================================
// Document
// ============================================================
export interface IDocument {
  id: string;
  title: string;
  description?: string;
  fileName: string;
  fileUrl: string;
  fileSizeBytes: number;
  contentType?: string;
  category?: string;
  documentType: number;
  targetAudience: number;
  isPublished: boolean;
  publishedDate?: string;
  academicYearId?: string;
  uploadedByUserId: number;
  downloadCount: number;
}

export interface IDocumentList {
  id: string;
  title: string;
  fileName: string;
  fileSizeBytes: number;
  contentType?: string;
  category?: string;
  documentType: number;
  targetAudience: number;
  isPublished: boolean;
  publishedDate?: string;
  uploadedByUserId: number;
  downloadCount: number;
}

export interface ICreateDocument {
  title: string;
  description?: string;
  fileName: string;
  fileUrl: string;
  fileSizeBytes: number;
  contentType?: string;
  category?: string;
  documentType: number;
  targetAudience: number;
  academicYearId?: string;
}

export interface IUpdateDocument {
  title?: string;
  description?: string;
  fileName?: string;
  fileUrl?: string;
  fileSizeBytes?: number;
  contentType?: string;
  category?: string;
  documentType?: number;
  targetAudience?: number;
  academicYearId?: string;
  clearAcademicYearId?: boolean;
}

export interface IGetDocumentsInput extends IPagedAndSortedResultRequest {
  documentType?: number;
  targetAudience?: number;
  category?: string;
  isPublished?: boolean;
  academicYearId?: string;
  search?: string;
}

// ============================================================
// Message
// ============================================================
export interface IMessage {
  id: string;
  senderUserId: number;
  recipientUserId: number;
  subject?: string;
  content: string;
  attachmentUrl?: string;
  isRead: boolean;
  readDate?: string;
  parentMessageId?: string;
  threadId?: string;
}

export interface IMessageList {
  id: string;
  senderUserId: number;
  recipientUserId: number;
  subject?: string;
  isRead: boolean;
  readDate?: string;
  threadId?: string;
  creationTime: string;
}

export interface ICreateMessage {
  recipientUserId: number;
  subject?: string;
  content: string;
  attachmentUrl?: string;
  parentMessageId?: string;
}

export interface IGetMessagesInput extends IPagedAndSortedResultRequest {
  isRead?: boolean;
  search?: string;
}

// ============================================================
// Notification
// ============================================================
export interface INotification {
  id: string;
  userId: number;
  title: string;
  message: string;
  type: number;
  priority: number;
  actionUrl?: string;
  entityType?: string;
  entityId?: string;
  isRead: boolean;
  readDate?: string;
  emailSent: boolean;
  emailSentDate?: string;
  pushSent: boolean;
  pushSentDate?: string;
}

export interface INotificationList {
  id: string;
  title: string;
  message: string;
  type: number;
  priority: number;
  actionUrl?: string;
  isRead: boolean;
  readDate?: string;
  creationTime: string;
}

export interface ICreateNotification {
  userId: number;
  title: string;
  message: string;
  type: number;
  priority: number;
  actionUrl?: string;
  entityType?: string;
  entityId?: string;
}

export interface IGetNotificationsInput extends IPagedAndSortedResultRequest {
  type?: number;
  priority?: number;
  isRead?: boolean;
  search?: string;
}
