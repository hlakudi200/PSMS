'use client';

import { useState, useCallback, useEffect, useMemo } from 'react';

type SelectionMode = 'none' | 'single' | 'multi';

export interface UseTableSelectionReturn<T> {
  selectedRowKeys: React.Key[];
  selectedRows: T[];
  rowSelection: Record<string, any> | undefined;
  handleSelectAll: (scope: 'page' | 'all') => void;
  clearSelection: () => void;
  selectionCount: number;
}

interface UseTableSelectionOptions<T> {
  mode: SelectionMode;
  data: T[];
  rowKey: string | ((record: T) => string);
  onSelectionChange?: (keys: string[], rows: T[]) => void;
}

function getRowKeyValue<T>(record: T, rowKey: string | ((record: T) => string)): string {
  if (typeof rowKey === 'function') return rowKey(record);
  return String((record as any)[rowKey]);
}

export function useTableSelection<T extends Record<string, any>>(
  options: UseTableSelectionOptions<T>
): UseTableSelectionReturn<T> {
  const { mode, data, rowKey, onSelectionChange } = options;
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);

  const selectedRows = useMemo(() => {
    const keySet = new Set(selectedRowKeys.map(String));
    return data.filter(record => keySet.has(getRowKeyValue(record, rowKey)));
  }, [selectedRowKeys, data, rowKey]);

  // Notify parent of selection changes
  useEffect(() => {
    if (mode !== 'none') {
      onSelectionChange?.(selectedRowKeys.map(String), selectedRows);
    }
  }, [selectedRowKeys]); // eslint-disable-line react-hooks/exhaustive-deps

  // Clear selection when data changes (page change, filter change)
  const dataFingerprint = useMemo(() => {
    if (data.length === 0) return '';
    return data.map(r => getRowKeyValue(r, rowKey)).join(',');
  }, [data, rowKey]);

  useEffect(() => {
    setSelectedRowKeys([]);
  }, [dataFingerprint]);

  const clearSelection = useCallback(() => {
    setSelectedRowKeys([]);
  }, []);

  const handleSelectAll = useCallback((scope: 'page' | 'all') => {
    if (scope === 'page') {
      const keys = data.map(record => getRowKeyValue(record, rowKey));
      setSelectedRowKeys(keys);
    }
    // 'all' scope would require server-side support — select current page for now
    if (scope === 'all') {
      const keys = data.map(record => getRowKeyValue(record, rowKey));
      setSelectedRowKeys(keys);
    }
  }, [data, rowKey]);

  const rowSelection = useMemo(() => {
    if (mode === 'none') return undefined;

    return {
      type: mode === 'single' ? ('radio' as const) : ('checkbox' as const),
      selectedRowKeys,
      onChange: (keys: React.Key[]) => {
        setSelectedRowKeys(keys);
      },
    };
  }, [mode, selectedRowKeys]);

  return {
    selectedRowKeys,
    selectedRows,
    rowSelection,
    handleSelectAll,
    clearSelection,
    selectionCount: selectedRowKeys.length,
  };
}
