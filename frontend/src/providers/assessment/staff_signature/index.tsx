'use client';

import { createContext, useCallback, useContext, useMemo, useRef, useState } from 'react';
import { getAxiosInstance } from '@/utils/axios-instance';

export interface IStaffSignature {
  svgContent?: string;
  updatedAt?: string;
}

interface IState {
  signature?: IStaffSignature;
  isPending: boolean;
  isError: boolean;
  /** False until we have actually asked the server. */
  isLoaded: boolean;
}

interface IActions {
  /** The caller's own signature, or undefined when they have not drawn one. */
  getMineAsync: () => Promise<IStaffSignature | undefined>;
  saveMineAsync: (svgContent: string) => Promise<IStaffSignature>;
  deleteMineAsync: () => Promise<void>;
}

const StateContext = createContext<IState>({ isPending: false, isError: false, isLoaded: false });
const ActionContext = createContext<IActions | undefined>(undefined);

/**
 * RC-17. A staff member's handwritten signature, drawn once and reused whenever
 * they sign a report card.
 *
 * Every call is about the signed-in user's own signature and takes no user id —
 * a signature somebody else can write or read is not a signature.
 */
export const StaffSignatureProvider = ({ children }: { children: React.ReactNode }) => {
  const instanceRef = useRef(getAxiosInstance());
  const instance = instanceRef.current;
  const [state, setState] = useState<IState>({ isPending: false, isError: false, isLoaded: false });

  const getMineAsync = useCallback(async () => {
    setState((s) => ({ ...s, isPending: true, isError: false }));
    try {
      /* A teacher who has never drawn one is the normal case, not a failure, so
         this must not raise the global error dialog on first open. */
      const res = await instance.get('/api/services/app/StaffSignature/GetMine', {
        suppressErrorModal: true,
      });
      const signature = (res.data?.result ?? undefined) as IStaffSignature | undefined;
      setState({ signature, isPending: false, isError: false, isLoaded: true });
      return signature;
    } catch {
      setState({ signature: undefined, isPending: false, isError: true, isLoaded: true });
      return undefined;
    }
  }, [instance]);

  const saveMineAsync = useCallback(async (svgContent: string) => {
    setState((s) => ({ ...s, isPending: true, isError: false }));
    try {
      const res = await instance.post('/api/services/app/StaffSignature/SaveMine', { svgContent });
      const signature = res.data?.result as IStaffSignature;
      setState({ signature, isPending: false, isError: false, isLoaded: true });
      return signature;
    } catch (error) {
      setState((s) => ({ ...s, isPending: false, isError: true }));
      throw error;
    }
  }, [instance]);

  const deleteMineAsync = useCallback(async () => {
    await instance.delete('/api/services/app/StaffSignature/DeleteMine');
    setState({ signature: undefined, isPending: false, isError: false, isLoaded: true });
  }, [instance]);

  const actions = useMemo(
    () => ({ getMineAsync, saveMineAsync, deleteMineAsync }),
    [getMineAsync, saveMineAsync, deleteMineAsync],
  );

  return (
    <StateContext.Provider value={state}>
      <ActionContext.Provider value={actions}>{children}</ActionContext.Provider>
    </StateContext.Provider>
  );
};

export const useStaffSignatureState = () => useContext(StateContext);

export const useStaffSignatureActions = () => {
  const ctx = useContext(ActionContext);
  if (!ctx) throw new Error('useStaffSignatureActions must be used within a StaffSignatureProvider');
  return ctx;
};
