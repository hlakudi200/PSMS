"use client";

import {
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useReducer,
  useRef,
} from "react";
import { BrandingReducer } from "./reducer";
import {
  BrandingActionContext,
  BrandingAssetType,
  BrandingStateContext,
  DEFAULT_BRANDING,
  IBranding,
  IUpdateBrandingInput,
  IUploadTicket,
  INITIAL_STATE,
} from "./context";
import {
  loadBrandingError,
  loadBrandingPending,
  loadBrandingSuccess,
  resetBrandingAction,
  resetStateFlagsAction,
  saveBrandingError,
  saveBrandingPending,
  saveBrandingSuccess,
} from "./actions";
import { getAxiosInstance } from "@/utils/axios-instance";
import { useAuthState } from "@/providers/auth";

const ENDPOINT = "/api/services/app/SchoolBranding";

/**
 * Backend BrandingAssetType. Serialised as an integer — the API has no
 * StringEnumConverter registered.
 */
const ASSET_TYPE_VALUE: Record<BrandingAssetType, number> = {
  Logo: 1,
  Favicon: 2,
};

/** Narrows an API payload to IBranding, filling any gap with the defaults. */
const toBranding = (raw: Partial<IBranding> | undefined | null): IBranding => ({
  primaryColor: raw?.primaryColor || DEFAULT_BRANDING.primaryColor,
  secondaryColor: raw?.secondaryColor || DEFAULT_BRANDING.secondaryColor,
  logoUrl: raw?.logoUrl ?? null,
  faviconUrl: raw?.faviconUrl ?? null,
  schoolName: raw?.schoolName || DEFAULT_BRANDING.schoolName,
  // Absent from the anonymous payload, and legitimately null when unset —
  // `?? null` keeps those two indistinguishable, which is correct here since
  // only the settings editor reads it and it never runs unauthenticated.
  configuredSchoolName: raw?.configuredSchoolName ?? null,
});

export const BrandingProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const [state, dispatch] = useReducer(BrandingReducer, INITIAL_STATE);
  const instanceRef = useRef(getAxiosInstance());
  const instance = instanceRef.current;

  // Monotonic token for branding reads. Every load and every reset claims the
  // next value; a response only applies if its claim is still current.
  //
  // Without this, three things go wrong: clearing the login page's school field
  // resets the palette but an already-issued response repaints the old school
  // over it; two lookups typed in quick succession apply in completion order
  // rather than input order; and a load in flight when the session ends repaints
  // the previous tenant after sign-out.
  const requestSeq = useRef(0);
  const claimRequest = () => (requestSeq.current += 1);
  const isCurrent = (claim: number) => requestSeq.current === claim;

  // Branding follows the session: load on sign-in, revert on sign-out.
  const { jwtToken } = useAuthState();

  const loadBranding = useCallback(async () => {
    const claim = claimRequest();
    dispatch(loadBrandingPending());
    try {
      // Nobody asked for this fetch, so its failure must not throw a modal in
      // front of whatever the user is actually doing.
      const response = await instance.get(`${ENDPOINT}/Get`, {
        suppressErrorModal: true,
      });
      if (!isCurrent(claim)) return;

      const result = response.data?.result;
      dispatch(
        loadBrandingSuccess({
          branding: toBranding(result),
          isConfigured: Boolean(result?.isConfigured),
        })
      );
    } catch (error) {
      if (!isCurrent(claim)) return;
      // Non-fatal: the app keeps whatever palette is already in state.
      console.error("Failed to load branding:", error);
      dispatch(loadBrandingError());
    }
  }, [instance]);

  const loadPublicBranding = useCallback(
    async (tenancyName: string) => {
      if (!tenancyName?.trim()) return;

      const claim = claimRequest();
      dispatch(loadBrandingPending());
      try {
        // Fires on every pause in typing — a modal per failed keystroke burst
        // over the login form would be intolerable.
        const response = await instance.get(`${ENDPOINT}/GetPublic`, {
          params: { tenancyName: tenancyName.trim() },
          suppressErrorModal: true,
        });
        if (!isCurrent(claim)) return;

        dispatch(
          loadBrandingSuccess({
            branding: toBranding(response.data?.result),
            isConfigured: false,
          })
        );
      } catch (error) {
        if (!isCurrent(claim)) return;
        console.error("Failed to load public branding:", error);
        dispatch(loadBrandingError());
      }
    },
    [instance]
  );

  const updateBranding = useCallback(
    async (input: IUpdateBrandingInput): Promise<boolean> => {
      dispatch(saveBrandingPending());
      try {
        const response = await instance.put(`${ENDPOINT}/Update`, input);
        // A save is user-initiated and authoritative: claim the sequence so a
        // background load issued earlier cannot land on top of it.
        claimRequest();
        dispatch(
          saveBrandingSuccess({ branding: toBranding(response.data?.result) })
        );
        return true;
      } catch (error) {
        // The axios interceptor has already surfaced the ABP error modal.
        console.error("Failed to save branding:", error);
        dispatch(saveBrandingError());
        return false;
      }
    },
    [instance]
  );

  const uploadAsset = useCallback(
    async (assetType: BrandingAssetType, file: File): Promise<boolean> => {
      dispatch(saveBrandingPending());
      try {
        // 1. Mint a one-time signed URL.
        const ticketResponse = await instance.post(
          `${ENDPOINT}/RequestAssetUploadUrl`,
          { assetType: ASSET_TYPE_VALUE[assetType], fileName: file.name }
        );
        const ticket: IUploadTicket = ticketResponse.data?.result;
        if (!ticket?.uploadUrl || !ticket?.objectKey) {
          throw new Error("Upload ticket was missing a URL or object key.");
        }

        // 2. PUT the bytes straight to storage. Deliberately a bare fetch:
        //    the app's axios instance would attach the Authorization and
        //    Abp-TenantId headers, which the storage host must not receive.
        const putResponse = await fetch(ticket.uploadUrl, {
          method: "PUT",
          body: file,
          headers: { "Content-Type": file.type || "application/octet-stream" },
        });
        if (!putResponse.ok) {
          throw new Error(`Storage upload failed (${putResponse.status}).`);
        }

        // 3. Record it. The server re-reads the real size/type from storage.
        const response = await instance.post(`${ENDPOINT}/SetAsset`, {
          assetType: ASSET_TYPE_VALUE[assetType],
          objectKey: ticket.objectKey,
        });
        // A save is user-initiated and authoritative: claim the sequence so a
        // background load issued earlier cannot land on top of it.
        claimRequest();
        dispatch(
          saveBrandingSuccess({ branding: toBranding(response.data?.result) })
        );
        return true;
      } catch (error) {
        console.error(`Failed to upload ${assetType}:`, error);
        dispatch(saveBrandingError());
        return false;
      }
    },
    [instance]
  );

  const removeAsset = useCallback(
    async (assetType: BrandingAssetType): Promise<boolean> => {
      dispatch(saveBrandingPending());
      try {
        const response = await instance.post(`${ENDPOINT}/ClearAsset`, {
          assetType: ASSET_TYPE_VALUE[assetType],
        });
        // A save is user-initiated and authoritative: claim the sequence so a
        // background load issued earlier cannot land on top of it.
        claimRequest();
        dispatch(
          saveBrandingSuccess({ branding: toBranding(response.data?.result) })
        );
        return true;
      } catch (error) {
        console.error(`Failed to remove ${assetType}:`, error);
        dispatch(saveBrandingError());
        return false;
      }
    },
    [instance]
  );

  const resetBranding = useCallback(() => {
    // Claim the sequence so any in-flight load is discarded rather than
    // repainting the palette we are clearing.
    claimRequest();
    dispatch(resetBrandingAction());
  }, []);

  const resetStateFlags = useCallback(() => {
    dispatch(resetStateFlagsAction());
  }, []);

  useEffect(() => {
    if (jwtToken) {
      loadBranding();
    } else {
      // Signed out — drop the previous school's palette so the next login
      // screen doesn't briefly wear it. Claims the sequence too, so a load
      // still in flight when the token cleared cannot repaint it afterwards.
      claimRequest();
      dispatch(resetBrandingAction());
    }
  }, [jwtToken, loadBranding]);

  const actions = useMemo(
    () => ({
      loadBranding,
      loadPublicBranding,
      updateBranding,
      uploadAsset,
      removeAsset,
      resetBranding,
      resetStateFlags,
    }),
    [
      loadBranding,
      loadPublicBranding,
      updateBranding,
      uploadAsset,
      removeAsset,
      resetBranding,
      resetStateFlags,
    ]
  );

  return (
    <BrandingStateContext.Provider value={state}>
      <BrandingActionContext.Provider value={actions}>
        {children}
      </BrandingActionContext.Provider>
    </BrandingStateContext.Provider>
  );
};

export const useBrandingState = () => {
  const context = useContext(BrandingStateContext);
  if (!context) {
    throw new Error("useBrandingState must be used within a BrandingProvider");
  }
  return context;
};

export const useBrandingActions = () => {
  const context = useContext(BrandingActionContext);
  if (!context) {
    throw new Error(
      "useBrandingActions must be used within a BrandingProvider"
    );
  }
  return context;
};
