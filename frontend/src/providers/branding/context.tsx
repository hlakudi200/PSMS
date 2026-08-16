"use client";

import { createContext } from "react";

/**
 * A tenant's visual identity (issue #56). Mirrors the backend
 * SchoolBrandingDto / PublicSchoolBrandingDto.
 */
export interface IBranding {
  /** Action colour — Ant Design `colorPrimary`. "#RRGGBB". */
  primaryColor: string;
  /** Chrome colour — app header background and sidebar accent. "#RRGGBB". */
  secondaryColor: string;
  /** Public logo URL, or null to fall back to the school name. */
  logoUrl?: string | null;
  /** Public favicon URL, or null for the stock icon. */
  faviconUrl?: string | null;
  /** Display name for the sidebar, header and login page. */
  schoolName: string;
}

export interface IUpdateBrandingInput {
  primaryColor: string;
  secondaryColor: string;
  schoolName?: string;
}

export type BrandingAssetType = "Logo" | "Favicon";

/**
 * One-time signed upload URL minted by the backend. The browser PUTs the file
 * bytes straight to storage, then posts the object key back via setAsset.
 */
export interface IUploadTicket {
  uploadUrl: string;
  publicUrl: string;
  objectKey: string;
}

/**
 * The stock PSMS palette. Must stay in sync with the backend
 * `BrandingDefaults` so an unbranded tenant renders identically whichever
 * side supplied the values.
 */
export const DEFAULT_BRANDING: IBranding = {
  primaryColor: "#0066CC",
  secondaryColor: "#003D73",
  logoUrl: null,
  faviconUrl: null,
  schoolName: "Private School Management System",
};

export interface IBrandingStateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  /** Never undefined — falls back to DEFAULT_BRANDING until something loads. */
  branding: IBranding;
  /** False while showing defaults rather than a stored row. */
  isConfigured: boolean;
}

export interface IBrandingActionContext {
  /** Loads the signed-in tenant's branding. */
  loadBranding: () => Promise<void>;
  /** Loads branding for a tenancy name without auth (the login page). */
  loadPublicBranding: (tenancyName: string) => Promise<void>;
  /** Saves colours and display name. */
  updateBranding: (input: IUpdateBrandingInput) => Promise<boolean>;
  /** Uploads an image and points the branding at it. */
  uploadAsset: (assetType: BrandingAssetType, file: File) => Promise<boolean>;
  /** Clears an image. */
  removeAsset: (assetType: BrandingAssetType) => Promise<boolean>;
  /** Reverts to the stock palette in memory (used on sign-out). */
  resetBranding: () => void;
  resetStateFlags: () => void;
}

export const INITIAL_STATE: IBrandingStateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
  branding: DEFAULT_BRANDING,
  isConfigured: false,
};

export const BrandingStateContext =
  createContext<IBrandingStateContext>(INITIAL_STATE);

export const BrandingActionContext = createContext<
  IBrandingActionContext | undefined
>(undefined);
