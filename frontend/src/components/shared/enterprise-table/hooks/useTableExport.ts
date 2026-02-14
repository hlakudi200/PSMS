'use client';

import { useState, useCallback } from 'react';
import type {
  ExportConfig,
  ExportAuditEntry,
  TableQuery,
  ColumnConfig,
} from '../types';

export interface UseTableExportReturn {
  isExporting: boolean;
  handleExport: (format: 'csv' | 'xlsx', scope: 'currentPage' | 'allResults') => Promise<void>;
}

interface UseTableExportOptions<T> {
  config?: ExportConfig;
  data: T[];
  visibleColumns: ColumnConfig<T>[];
  query: TableQuery;
  currentUserRole?: string;
  onAudit?: (entry: ExportAuditEntry) => void;
}

function hasPermission(requiredPermissions: string[] | undefined, role?: string): boolean {
  if (!requiredPermissions || requiredPermissions.length === 0) return true;
  if (!role) return false;
  return requiredPermissions.includes(role);
}

function getNestedValue(record: any, dataIndex: string | string[]): any {
  if (Array.isArray(dataIndex)) {
    return dataIndex.reduce((obj, key) => obj?.[key], record);
  }
  return record?.[dataIndex];
}

function buildCsvContent<T>(columns: ColumnConfig<T>[], data: T[], includeMetadata: boolean, query: TableQuery): string {
  const lines: string[] = [];

  if (includeMetadata) {
    lines.push(`# Exported: ${new Date().toISOString()}`);
    const activeFilters = Object.entries(query.filters)
      .map(([k, v]) => `${k}=${v}`)
      .join('; ');
    if (activeFilters) {
      lines.push(`# Filters: ${activeFilters}`);
    }
    if (query.sorting) {
      lines.push(`# Sorting: ${query.sorting}`);
    }
    lines.push('');
  }

  // Header row
  const headers = columns.map(c => `"${String(c.title).replace(/"/g, '""')}"`);
  lines.push(headers.join(','));

  // Data rows
  for (const record of data) {
    const cells = columns.map(col => {
      const val = getNestedValue(record, col.dataIndex);
      if (val === null || val === undefined) return '""';
      const str = String(val).replace(/"/g, '""');
      return `"${str}"`;
    });
    lines.push(cells.join(','));
  }

  return lines.join('\n');
}

function downloadBlob(blob: Blob, filename: string): void {
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

export function useTableExport<T extends Record<string, any>>(
  options: UseTableExportOptions<T>,
): UseTableExportReturn {
  const { config, data, visibleColumns, query, currentUserRole, onAudit } = options;
  const [isExporting, setIsExporting] = useState(false);

  const handleExport = useCallback(async (
    format: 'csv' | 'xlsx',
    scope: 'currentPage' | 'allResults',
  ) => {
    if (!config?.enabled) return;
    if (!hasPermission(config.requiredPermissions, currentUserRole)) return;

    setIsExporting(true);
    try {
      const exportColumns = visibleColumns.filter(c => !c.sensitive);
      const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, 19);
      const filename = `export_${timestamp}`;

      // Server-side export for all results
      if (scope === 'allResults' && config.serverSideExport) {
        const blob = await config.serverSideExport(query, format);
        downloadBlob(blob, `${filename}.${format}`);
      } else if (format === 'csv') {
        const csv = buildCsvContent(
          exportColumns,
          data,
          config.includeMetadata ?? false,
          query,
        );
        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        downloadBlob(blob, `${filename}.csv`);
      } else if (format === 'xlsx') {
        // Dynamic import for xlsx
        const XLSX = await import('xlsx');
        const wsData: any[][] = [];

        // Metadata rows
        if (config.includeMetadata) {
          wsData.push([`Exported: ${new Date().toISOString()}`]);
          const activeFilters = Object.entries(query.filters)
            .map(([k, v]) => `${k}=${v}`)
            .join('; ');
          if (activeFilters) wsData.push([`Filters: ${activeFilters}`]);
          if (query.sorting) wsData.push([`Sorting: ${query.sorting}`]);
          wsData.push([]);
        }

        // Header row
        wsData.push(exportColumns.map(c => c.title));

        // Data rows
        for (const record of data) {
          wsData.push(exportColumns.map(col => {
            const val = getNestedValue(record, col.dataIndex);
            return val ?? '';
          }));
        }

        const wb = XLSX.utils.book_new();
        const ws = XLSX.utils.aoa_to_sheet(wsData);
        XLSX.utils.book_append_sheet(wb, ws, 'Export');
        const xlsxBuffer = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
        const blob = new Blob([xlsxBuffer], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        });
        downloadBlob(blob, `${filename}.xlsx`);
      }

      // Audit
      const auditEntry: ExportAuditEntry = {
        exportedAt: new Date().toISOString(),
        format,
        rowCount: data.length,
        filters: query.filters,
        columns: visibleColumns.map(c => c.key),
        scope,
      };
      config.auditHook?.(auditEntry);
      onAudit?.(auditEntry);
    } finally {
      setIsExporting(false);
    }
  }, [config, data, visibleColumns, query, currentUserRole, onAudit]);

  return { isExporting, handleExport };
}
