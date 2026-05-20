'use client'
import { createContext } from "react";
import {
  ILearningMaterial,
  ILearningMaterialList,
  ICreateLearningMaterial,
  IUpdateLearningMaterial,
  IGetLearningMaterialsInput,
  ILearningMaterialVersion,
  IUploadNewVersion
} from "../shared/interfaces";

export interface ILearningMaterialStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  learningMaterial?: ILearningMaterial;
  learningMaterials?: ILearningMaterialList[];
  totalCount?: number;
  // Version history for whichever material the drawer is currently
  // showing. Replaced wholesale on each GetVersions call.
  versions?: ILearningMaterialVersion[];
  versionsLoading?: boolean;
  versionsError?: boolean;
}

// Payload for the multipart upload flow — combines metadata with the raw
// File object so the provider can hand it to FormData. Mirrors the backend
// UploadLearningMaterialDto contracts (Description is required, min length
// 10; File is required unless MaterialType is ExternalLink — the latter
// is enforced in the modal's submit handler).
export interface IUploadLearningMaterial {
  classSubjectId: string;
  termId?: string;
  title: string;
  description: string;
  materialType: number;
  file?: File;
  externalLink?: string;
  displayOrder?: number;
}

export interface ILearningMaterialActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IGetLearningMaterialsInput) => void;
  getByClassSubjectAsync: (classSubjectId: string) => void;
  createAsync: (input: ICreateLearningMaterial) => void;
  uploadAsync: (input: IUploadLearningMaterial) => Promise<void>;
  updateAsync: (id: string, input: IUpdateLearningMaterial) => void;
  deleteAsync: (id: string) => void;
  publishAsync: (id: string) => void;
  unpublishAsync: (id: string) => void;
  incrementViewCountAsync: (id: string) => void;
  // T-T07 Versioning. `getVersionsAsync` populates `versions` in state;
  // `uploadNewVersionAsync` returns Promise<void> so the modal can await
  // it and only fire the success toast on actual success.
  getVersionsAsync: (learningMaterialId: string) => void;
  uploadNewVersionAsync: (input: IUploadNewVersion) => Promise<void>;
  restoreVersionAsync: (learningMaterialId: string, versionId: string) => Promise<void>;
}

export const INITIAL_STATE: ILearningMaterialStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const LearningMaterialStateContext =
  createContext<ILearningMaterialStateContext>(INITIAL_STATE);

export const LearningMaterialActionContext = createContext<
  ILearningMaterialActionContext | undefined
>(undefined);
