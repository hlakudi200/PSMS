'use client';

import { useState, useCallback, useRef, useEffect, useMemo } from 'react';
import type { TableQuery, PersonalizationConfig } from '../types';

interface TableInternalState {
  currentPage: number;
  pageSize: number;
  sorting?: string;
  filters: Record<string, unknown>;
  lastFetchedAt?: Date;
}

export interface UseTableStateReturn {
  state: TableInternalState;
  query: TableQuery;
  handlePageChange: (page: number, pageSize: number) => void;
  handleSortChange: (field: string, order: 'ascend' | 'descend' | null) => void;
  handleFilterChange: (key: string, value: unknown) => void;
  handleFiltersReset: () => void;
  handleRefresh: () => void;
  setPageSize: (size: number) => void;
}

interface UseTableStateOptions {
  defaultPageSize: number;
  onQueryChange: (query: TableQuery) => void;
  personalization?: PersonalizationConfig;
  defaultSorting?: string;
}

export function useTableState(options: UseTableStateOptions): UseTableStateReturn {
  const { defaultPageSize, onQueryChange, personalization, defaultSorting } = options;
  const debounceRef = useRef<ReturnType<typeof setTimeout>>(undefined);
  const onQueryChangeRef = useRef(onQueryChange);
  onQueryChangeRef.current = onQueryChange;

  const initialPageSize = personalization?.defaultView?.pageSize ?? defaultPageSize;
  const initialSorting = personalization?.defaultView?.sorting ?? defaultSorting;
  const initialFilters = personalization?.defaultView?.filters ?? {};

  const [state, setState] = useState<TableInternalState>({
    currentPage: 1,
    pageSize: initialPageSize,
    sorting: initialSorting,
    filters: initialFilters,
    lastFetchedAt: undefined,
  });

  const query = useMemo<TableQuery>(() => ({
    maxResultCount: state.pageSize,
    skipCount: (state.currentPage - 1) * state.pageSize,
    sorting: state.sorting,
    filters: state.filters,
  }), [state.currentPage, state.pageSize, state.sorting, state.filters]);

  // Fire onQueryChange whenever query changes
  const isInitialMount = useRef(true);
  useEffect(() => {
    if (isInitialMount.current) {
      isInitialMount.current = false;
      // Fire initial query
      onQueryChangeRef.current(query);
      setState(prev => ({ ...prev, lastFetchedAt: new Date() }));
      return;
    }
    onQueryChangeRef.current(query);
    setState(prev => ({ ...prev, lastFetchedAt: new Date() }));
  }, [query]);

  const handlePageChange = useCallback((page: number, pageSize: number) => {
    setState(prev => ({
      ...prev,
      currentPage: page,
      pageSize,
    }));
  }, []);

  const handleSortChange = useCallback((field: string, order: 'ascend' | 'descend' | null) => {
    setState(prev => ({
      ...prev,
      currentPage: 1,
      sorting: order ? `${field} ${order === 'ascend' ? 'asc' : 'desc'}` : undefined,
    }));
  }, []);

  const handleFilterChange = useCallback((key: string, value: unknown) => {
    clearTimeout(debounceRef.current);

    const applyFilter = () => {
      setState(prev => {
        const newFilters = { ...prev.filters };
        if (value === undefined || value === null || value === '') {
          delete newFilters[key];
        } else {
          newFilters[key] = value;
        }
        return { ...prev, currentPage: 1, filters: newFilters };
      });
    };

    // Debounce text-like filter changes
    if (typeof value === 'string') {
      debounceRef.current = setTimeout(applyFilter, 400);
    } else {
      applyFilter();
    }
  }, []);

  const handleFiltersReset = useCallback(() => {
    setState(prev => ({
      ...prev,
      currentPage: 1,
      filters: {},
    }));
  }, []);

  const handleRefresh = useCallback(() => {
    // Re-trigger same query by updating lastFetchedAt
    onQueryChangeRef.current(query);
    setState(prev => ({ ...prev, lastFetchedAt: new Date() }));
  }, [query]);

  const setPageSize = useCallback((size: number) => {
    setState(prev => ({
      ...prev,
      currentPage: 1,
      pageSize: size,
    }));
  }, []);

  // Cleanup debounce on unmount
  useEffect(() => {
    return () => clearTimeout(debounceRef.current);
  }, []);

  return {
    state,
    query,
    handlePageChange,
    handleSortChange,
    handleFilterChange,
    handleFiltersReset,
    handleRefresh,
    setPageSize,
  };
}
