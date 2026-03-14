'use client';

import { useReducer, useCallback, useRef, useEffect } from 'react';
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

// Action types for reducer
type TableAction =
  | { type: 'PAGE_CHANGE'; page: number; pageSize: number }
  | { type: 'SORT_CHANGE'; sorting?: string }
  | { type: 'FILTER_CHANGE'; key: string; value: unknown }
  | { type: 'FILTERS_RESET' }
  | { type: 'PAGE_SIZE_CHANGE'; pageSize: number }
  | { type: 'MARK_FETCHED' };

function tableReducer(state: TableInternalState, action: TableAction): TableInternalState {
  switch (action.type) {
    case 'PAGE_CHANGE':
      return { ...state, currentPage: action.page, pageSize: action.pageSize };
    case 'SORT_CHANGE':
      return { ...state, currentPage: 1, sorting: action.sorting };
    case 'FILTER_CHANGE': {
      const newFilters = { ...state.filters };
      if (action.value === undefined || action.value === null || action.value === '') {
        delete newFilters[action.key];
      } else {
        newFilters[action.key] = action.value;
      }
      return { ...state, currentPage: 1, filters: newFilters };
    }
    case 'FILTERS_RESET':
      return { ...state, currentPage: 1, filters: {} };
    case 'PAGE_SIZE_CHANGE':
      return { ...state, currentPage: 1, pageSize: action.pageSize };
    case 'MARK_FETCHED':
      return { ...state, lastFetchedAt: new Date() };
    default:
      return state;
  }
}

function buildQuery(state: TableInternalState): TableQuery {
  return {
    maxResultCount: state.pageSize,
    skipCount: (state.currentPage - 1) * state.pageSize,
    sorting: state.sorting,
    filters: state.filters,
  };
}

export function useTableState(options: UseTableStateOptions): UseTableStateReturn {
  const { defaultPageSize, onQueryChange, personalization, defaultSorting } = options;
  const debounceRef = useRef<ReturnType<typeof setTimeout>>(undefined);
  const onQueryChangeRef = useRef(onQueryChange);
  onQueryChangeRef.current = onQueryChange;

  const initialPageSize = personalization?.defaultView?.pageSize ?? defaultPageSize;
  const initialSorting = personalization?.defaultView?.sorting ?? defaultSorting;
  const initialFilters = personalization?.defaultView?.filters ?? {};

  const [state, dispatch] = useReducer(tableReducer, {
    currentPage: 1,
    pageSize: initialPageSize,
    sorting: initialSorting,
    filters: initialFilters,
    lastFetchedAt: undefined,
  });

  const query = buildQuery(state);
  const filtersKey = JSON.stringify(state.filters);

  // Fire onQueryChange whenever query values change
  useEffect(() => {
    onQueryChangeRef.current(buildQuery(state));
    dispatch({ type: 'MARK_FETCHED' });
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state.currentPage, state.pageSize, state.sorting, filtersKey]);

  const handlePageChange = useCallback((page: number, pageSize: number) => {
    dispatch({ type: 'PAGE_CHANGE', page, pageSize });
  }, []);

  const handleSortChange = useCallback((field: string, order: 'ascend' | 'descend' | null) => {
    dispatch({
      type: 'SORT_CHANGE',
      sorting: order ? `${field} ${order === 'ascend' ? 'asc' : 'desc'}` : undefined,
    });
  }, []);

  const handleFilterChange = useCallback((key: string, value: unknown) => {
    clearTimeout(debounceRef.current);

    const applyFilter = () => {
      dispatch({ type: 'FILTER_CHANGE', key, value });
    };

    if (typeof value === 'string') {
      debounceRef.current = setTimeout(applyFilter, 400);
    } else {
      applyFilter();
    }
  }, []);

  const handleFiltersReset = useCallback(() => {
    dispatch({ type: 'FILTERS_RESET' });
  }, []);

  const handleRefresh = useCallback(() => {
    onQueryChangeRef.current(query);
    dispatch({ type: 'MARK_FETCHED' });
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state.currentPage, state.pageSize, state.sorting, filtersKey]);

  const setPageSize = useCallback((size: number) => {
    dispatch({ type: 'PAGE_SIZE_CHANGE', pageSize: size });
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
