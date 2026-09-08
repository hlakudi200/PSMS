"use client";
import { createContext, useCallback, useContext, useMemo, useState } from "react";
import { getAxiosInstance } from "@/utils/axios-instance";
import type { IWorkflowExtensionCatalog } from "../shared/interfaces";

/**
 * WF-30/31/32: the catalogue of guards, effects and decision schemas a step can
 * reference, per entity type. Read-only reference data, cached per entity type
 * for the lifetime of the provider (the catalogue only changes with a deploy).
 */
export interface IWorkflowExtensionStateContext {
  isPending: boolean;
  isError: boolean;
  catalogs: Record<number, IWorkflowExtensionCatalog>;
}

export interface IWorkflowExtensionActionContext {
  getAvailableAsync: (entityType: number) => Promise<IWorkflowExtensionCatalog | undefined>;
}

const StateContext = createContext<IWorkflowExtensionStateContext>({
  isPending: false,
  isError: false,
  catalogs: {},
});
const ActionContext = createContext<IWorkflowExtensionActionContext | undefined>(undefined);

export const WorkflowExtensionProvider = ({ children }: { children: React.ReactNode }) => {
  const [catalogs, setCatalogs] = useState<Record<number, IWorkflowExtensionCatalog>>({});
  const [isPending, setIsPending] = useState(false);
  const [isError, setIsError] = useState(false);
  const instance = getAxiosInstance();

  const getAvailableAsync = useCallback(async (entityType: number) => {
    if (catalogs[entityType]) return catalogs[entityType];
    setIsPending(true);
    setIsError(false);
    try {
      const response = await instance.get(
        `/api/services/app/WorkflowExtension/GetAvailable?entityType=${entityType}`
      );
      const catalog = response.data.result as IWorkflowExtensionCatalog;
      setCatalogs((prev) => ({ ...prev, [entityType]: catalog }));
      return catalog;
    } catch (error) {
      console.error(error);
      setIsError(true);
      return undefined;
    } finally {
      setIsPending(false);
    }
  }, [catalogs, instance]);

  const state = useMemo(() => ({ isPending, isError, catalogs }), [isPending, isError, catalogs]);
  const actions = useMemo(() => ({ getAvailableAsync }), [getAvailableAsync]);

  return (
    <StateContext.Provider value={state}>
      <ActionContext.Provider value={actions}>{children}</ActionContext.Provider>
    </StateContext.Provider>
  );
};

export const useWorkflowExtensionState = () => useContext(StateContext);

export const useWorkflowExtensionActions = () => {
  const context = useContext(ActionContext);
  if (!context) {
    throw new Error("useWorkflowExtensionActions must be used within a WorkflowExtensionProvider");
  }
  return context;
};
