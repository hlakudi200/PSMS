'use client'
import { createContext } from "react";
import {
  IApplicationDocument,
  IUploadDocument,
  IVerifyDocument,
  IRequiredDocumentsStatus,
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
  uploadAsync: (input: IUploadDocument) => void;
  deleteAsync: (id: string) => void;
  verifyAsync: (id: string, input: IVerifyDocument) => void;
  rejectAsync: (id: string, reason: string) => void;
  getDownloadUrlAsync: (id: string) => void;
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
