'use client';

import { useRef, useCallback } from 'react';
import type { TelemetryConfig, ExportAuditEntry } from '../types';

export interface UseTableTelemetryReturn {
  recordLoadStart: () => void;
  recordLoadComplete: (rowCount: number, totalCount: number) => void;
  recordExport: (entry: ExportAuditEntry) => void;
  recordError: (message: string, endpoint?: string) => void;
  recordInteraction: (type: string, detail?: Record<string, unknown>) => void;
}

export function useTableTelemetry(config?: TelemetryConfig): UseTableTelemetryReturn {
  const loadStartRef = useRef<number>(0);
  const enabled = config?.enabled ?? false;

  const recordLoadStart = useCallback(() => {
    if (!enabled) return;
    loadStartRef.current = performance.now();
  }, [enabled]);

  const recordLoadComplete = useCallback((rowCount: number, totalCount: number) => {
    if (!enabled) return;
    const loadTimeMs = loadStartRef.current
      ? Math.round(performance.now() - loadStartRef.current)
      : 0;
    config?.onLoadComplete?.({ loadTimeMs, rowCount, totalCount });
  }, [enabled, config]);

  const recordExport = useCallback((entry: ExportAuditEntry) => {
    if (!enabled) return;
    config?.onExport?.(entry);
  }, [enabled, config]);

  const recordError = useCallback((message: string, endpoint?: string) => {
    if (!enabled) return;
    config?.onError?.({ message, endpoint });
  }, [enabled, config]);

  const recordInteraction = useCallback((type: string, detail?: Record<string, unknown>) => {
    if (!enabled) return;
    config?.onInteraction?.({ type, detail });
  }, [enabled, config]);

  return {
    recordLoadStart,
    recordLoadComplete,
    recordExport,
    recordError,
    recordInteraction,
  };
}
