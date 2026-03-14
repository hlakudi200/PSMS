import React from 'react';

// ============================================================
// Query Model (backend-friendly)
// ============================================================
export interface TableQuery {
  maxResultCount: number;
  skipCount: number;
  sorting?: string;
  filters: Record<string, unknown>;
}

// ============================================================
// Column Configuration
// ============================================================
export type ColumnFilterType = 'text' | 'number' | 'date' | 'dateRange' | 'enum';

export interface EnumOption {
  label: string;
  value: string | number | boolean;
}

export interface StatusRenderConfig {
  statusMap: Record<string | number, { label: string; color: string }>;
  fallbackLabel?: string;
}

export interface MoneyRenderConfig {
  currency?: string;
  locale?: string;
}

export interface MaskedRenderConfig {
  maskChar?: string;
  visibleChars?: number;
  unmaskPermissions?: string[];
}

export interface ColumnConfig<T> {
  key: string;
  title: string;
  dataIndex: string | string[];

  // Sorting
  sortable?: boolean;
  defaultSortOrder?: 'ascend' | 'descend';

  // Filtering (per-column)
  filterable?: boolean;
  filterType?: ColumnFilterType;
  filterOptions?: EnumOption[];
  filterKey?: string;

  // Display
  width?: number | string;
  minWidth?: number;
  fixed?: 'left' | 'right';
  align?: 'left' | 'center' | 'right';
  ellipsis?: boolean;

  // Rendering
  render?: (value: any, record: T, index: number) => React.ReactNode;
  renderType?: 'status' | 'date' | 'datetime' | 'money' | 'boolean' | 'masked';
  renderConfig?: StatusRenderConfig | MoneyRenderConfig | MaskedRenderConfig;
  conditionalStyle?: (value: any, record: T) => React.CSSProperties | undefined;

  // Visibility & Security
  hidden?: boolean;
  hideOnMobile?: boolean;
  requiredPermissions?: string[];
  sensitive?: boolean;
}

// ============================================================
// Row Actions
// ============================================================
export interface RowAction<T> {
  key: string;
  label: string;
  icon?: React.ReactNode;
  onClick: (record: T) => void;
  visible?: (record: T) => boolean;
  disabled?: (record: T) => boolean;
  requiredPermissions?: string[];
  confirm?: { title: string; description?: string };
  danger?: boolean;
}

// ============================================================
// Bulk Actions
// ============================================================
export interface BulkAction<T> {
  key: string;
  label: string;
  icon?: React.ReactNode;
  onClick: (selectedRows: T[]) => void;
  requiredPermissions?: string[];
  confirm?: { title: string; description?: string };
  danger?: boolean;
}

// ============================================================
// Toolbar Actions
// ============================================================
export interface ToolbarAction {
  key: string;
  label: string;
  icon?: React.ReactNode;
  onClick: () => void;
  type?: 'primary' | 'default' | 'dashed' | 'text' | 'link';
  danger?: boolean;
  disabled?: boolean;
  requiredPermissions?: string[];
}

// ============================================================
// Export Configuration
// ============================================================
export interface ExportAuditEntry {
  exportedAt: string;
  exportedBy?: string;
  format: string;
  rowCount: number;
  filters: Record<string, unknown>;
  columns: string[];
  scope: 'currentPage' | 'allResults';
}

export interface ExportConfig {
  enabled: boolean;
  formats?: ('csv' | 'xlsx')[];
  serverSideExport?: (query: TableQuery, format: string) => Promise<Blob>;
  maxClientRows?: number;
  includeMetadata?: boolean;
  auditHook?: (metadata: ExportAuditEntry) => void;
  requiredPermissions?: string[];
}

// ============================================================
// Personalization / Saved Views
// ============================================================
export interface TableView {
  id: string;
  name: string;
  columns: string[];
  filters: Record<string, unknown>;
  sorting?: string;
  pageSize: number;
  scope: 'user' | 'role' | 'global';
  isDefault?: boolean;
}

export interface PersonalizationConfig {
  enabled: boolean;
  storageKey: string;
  defaultView?: Partial<TableView>;
  allowSaveViews?: boolean;
}

// ============================================================
// Telemetry
// ============================================================
export interface TelemetryConfig {
  enabled: boolean;
  onLoadComplete?: (metrics: { loadTimeMs: number; rowCount: number; totalCount: number }) => void;
  onExport?: (entry: ExportAuditEntry) => void;
  onError?: (error: { message: string; endpoint?: string }) => void;
  onInteraction?: (event: { type: string; detail?: Record<string, unknown> }) => void;
}

// ============================================================
// Main Props
// ============================================================
export interface EnterpriseTableProps<T extends Record<string, any>> {
  // Data & State
  columns: ColumnConfig<T>[];
  data: T[];
  totalCount?: number;
  loading?: boolean;
  error?: boolean;
  errorMessage?: string;
  onQueryChange: (query: TableQuery) => void;
  rowKey?: string | ((record: T) => string);

  // Interaction
  onRowClick?: (record: T) => void;
  rowActions?: RowAction<T>[];
  selectionMode?: 'none' | 'single' | 'multi';
  onSelectionChange?: (keys: string[], rows: T[]) => void;
  bulkActions?: BulkAction<T>[];

  // Toolbar
  title?: string;
  toolbarActions?: ToolbarAction[];

  // Search
  searchable?: boolean;
  searchPlaceholder?: string;
  searchFilterKey?: string;

  // Export
  exportConfig?: ExportConfig;

  // Personalization
  personalization?: PersonalizationConfig;

  // Security
  currentUserRole?: string;

  // Performance
  pageSize?: number;
  virtualizeThreshold?: number;

  // Telemetry
  telemetry?: TelemetryConfig;

  // Layout
  size?: 'small' | 'middle' | 'large';
  stickyHeader?: boolean;
  scrollX?: number | string | true;
  scrollY?: number | string;
  emptyText?: string;
  showLastUpdated?: boolean;
}
