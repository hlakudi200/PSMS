'use client'
import { createContext } from "react";
import {
  IDocument,
  IDocumentList,
  ICreateDocument,
  IUpdateDocument,
  IGetDocumentsInput
} from "../shared/interfaces";

export interface IDocumentStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  document?: IDocument;
  documents?: IDocumentList[];
  totalCount?: number;
}

export interface IDocumentActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetDocumentsInput) => void;
  createAsync: (input: ICreateDocument) => void;
  updateAsync: (id: string, input: IUpdateDocument) => void;
  deleteAsync: (id: string) => void;
  publishAsync: (id: string) => void;
  unpublishAsync: (id: string) => void;
  recordDownloadAsync: (id: string) => void;
}

export const INITIAL_STATE: IDocumentStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const DocumentStateContext =
  createContext<IDocumentStateContext>(INITIAL_STATE);

export const DocumentActionContext = createContext<
  IDocumentActionContext | undefined
>(undefined);
