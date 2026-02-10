'use client';

import { useState, useCallback, useEffect } from 'react';
import type { PersonalizationConfig, TableView, ColumnConfig } from '../types';

export interface UseTablePersonalizationReturn {
  currentView: TableView | null;
  savedViews: TableView[];
  visibleColumnKeys: string[];
  saveView: (name: string) => void;
  loadView: (viewId: string) => void;
  deleteView: (viewId: string) => void;
  setDefaultView: (viewId: string) => void;
  toggleColumnVisibility: (columnKey: string) => void;
  reorderColumns: (fromIndex: number, toIndex: number) => void;
  persistedPageSize: number;
}

interface PersistedState {
  visibleColumnKeys: string[];
  columnOrder: string[];
  savedViews: TableView[];
  currentViewId: string | null;
  pageSize: number;
}

function loadFromStorage(key: string): PersistedState | null {
  try {
    const raw = localStorage.getItem(key);
    if (!raw) return null;
    return JSON.parse(raw) as PersistedState;
  } catch {
    return null;
  }
}

function saveToStorage(key: string, state: PersistedState): void {
  try {
    localStorage.setItem(key, JSON.stringify(state));
  } catch {
    // localStorage full or unavailable — silent fail
  }
}

export function useTablePersonalization<T>(
  config: PersonalizationConfig | undefined,
  columns: ColumnConfig<T>[],
  defaultPageSize: number,
): UseTablePersonalizationReturn {
  const allColumnKeys = columns.map(c => c.key);
  const defaultVisibleKeys = config?.defaultView?.columns ?? allColumnKeys.filter(
    (_, i) => !columns[i].hidden
  );

  const [visibleColumnKeys, setVisibleColumnKeys] = useState<string[]>(defaultVisibleKeys);
  const [columnOrder, setColumnOrder] = useState<string[]>(allColumnKeys);
  const [savedViews, setSavedViews] = useState<TableView[]>([]);
  const [currentView, setCurrentView] = useState<TableView | null>(null);
  const [persistedPageSize, setPersistedPageSize] = useState(
    config?.defaultView?.pageSize ?? defaultPageSize
  );

  // Load persisted state on mount
  useEffect(() => {
    if (!config?.enabled || !config.storageKey) return;
    const stored = loadFromStorage(config.storageKey);
    if (!stored) return;

    setVisibleColumnKeys(stored.visibleColumnKeys);
    setColumnOrder(stored.columnOrder);
    setSavedViews(stored.savedViews);
    setPersistedPageSize(stored.pageSize);

    if (stored.currentViewId) {
      const view = stored.savedViews.find(v => v.id === stored.currentViewId);
      if (view) setCurrentView(view);
    }
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  // Persist state changes
  const persist = useCallback(
    (updates: Partial<PersistedState>) => {
      if (!config?.enabled || !config.storageKey) return;
      const existing = loadFromStorage(config.storageKey);
      const merged: PersistedState = {
        visibleColumnKeys: updates.visibleColumnKeys ?? existing?.visibleColumnKeys ?? defaultVisibleKeys,
        columnOrder: updates.columnOrder ?? existing?.columnOrder ?? allColumnKeys,
        savedViews: updates.savedViews ?? existing?.savedViews ?? [],
        currentViewId: updates.currentViewId !== undefined ? updates.currentViewId : existing?.currentViewId ?? null,
        pageSize: updates.pageSize ?? existing?.pageSize ?? defaultPageSize,
      };
      saveToStorage(config.storageKey, merged);
    },
    [config, defaultVisibleKeys, allColumnKeys, defaultPageSize],
  );

  const toggleColumnVisibility = useCallback((columnKey: string) => {
    setVisibleColumnKeys(prev => {
      const next = prev.includes(columnKey)
        ? prev.filter(k => k !== columnKey)
        : [...prev, columnKey];
      persist({ visibleColumnKeys: next });
      return next;
    });
  }, [persist]);

  const reorderColumns = useCallback((fromIndex: number, toIndex: number) => {
    setColumnOrder(prev => {
      const next = [...prev];
      const [moved] = next.splice(fromIndex, 1);
      next.splice(toIndex, 0, moved);
      persist({ columnOrder: next });
      return next;
    });
  }, [persist]);

  const saveView = useCallback((name: string) => {
    if (!config?.allowSaveViews) return;
    const newView: TableView = {
      id: `view_${Date.now()}`,
      name,
      columns: visibleColumnKeys,
      filters: {},
      pageSize: persistedPageSize,
      scope: 'user',
    };
    setSavedViews(prev => {
      const next = [...prev, newView];
      persist({ savedViews: next });
      return next;
    });
    setCurrentView(newView);
    persist({ currentViewId: newView.id });
  }, [config, visibleColumnKeys, persistedPageSize, persist]);

  const loadView = useCallback((viewId: string) => {
    const view = savedViews.find(v => v.id === viewId);
    if (!view) return;
    setCurrentView(view);
    setVisibleColumnKeys(view.columns);
    setPersistedPageSize(view.pageSize);
    persist({
      currentViewId: viewId,
      visibleColumnKeys: view.columns,
      pageSize: view.pageSize,
    });
  }, [savedViews, persist]);

  const deleteView = useCallback((viewId: string) => {
    setSavedViews(prev => {
      const next = prev.filter(v => v.id !== viewId);
      persist({ savedViews: next });
      return next;
    });
    if (currentView?.id === viewId) {
      setCurrentView(null);
      persist({ currentViewId: null });
    }
  }, [currentView, persist]);

  const setDefaultView = useCallback((viewId: string) => {
    setSavedViews(prev => {
      const next = prev.map(v => ({ ...v, isDefault: v.id === viewId }));
      persist({ savedViews: next });
      return next;
    });
  }, [persist]);

  return {
    currentView,
    savedViews,
    visibleColumnKeys,
    saveView,
    loadView,
    deleteView,
    setDefaultView,
    toggleColumnVisibility,
    reorderColumns,
    persistedPageSize,
  };
}
