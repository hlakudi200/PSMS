'use client'
import { createContext } from "react";
import {
  IApplicationDocument,
  IUploadDocument,
  IVerifyDocument,
  IRequiredDocumentsStatus,
  IRequestDocumentUploadUrl,
  IFileUploadTicket,
} from "../shared/interfaces";

export interface IApplicationDocumentStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  document?: IApplicationDocument;
  documents?: IApplicationDocument[];
  requiredDocumentsStatus?: IRequiredDocumentsStatus;
  downloadUrl?: string;
}

export interface IApplicationDocumentActionContext {
  getAsync: (id: string) => void;
  getAllByApplicationAsync: (applicationId: string) => void;
  getRequiredDocumentsStatusAsync: (applicationId: string) => void;
  // SF-03 direct upload. Step 1: signed URL. Step 2: PUT to storage. Step 3: record.
  requestUploadUrlAsync: (input: IRequestDocumentUploadUrl) => Promise<IFileUploadTicket>;
  uploadFileToStorageAsync: (uploadUrl: string, file: File) => Promise<void>;
  uploadAsync: (input: IUploadDocument) => Promise<void>;
  deleteAsync: (id: string) => void;
  verifyAsync: (id: string, input: IVerifyDocument) => void;
  rejectAsync: (id: string, reason: string) => void;
  // Returns a short-lived signed URL for the private document, or undefined on error.
  getDownloadUrlAsync: (id: string) => Promise<string | undefined>;
}

export const INITIAL_STATE: IApplicationDocumentStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const ApplicationDocumentStateContext =
  createContext<IApplicationDocumentStateContext>(INITIAL_STATE);

export const ApplicationDocumentActionContext = createContext<
  IApplicationDocumentActionContext | undefined
>(undefined);
