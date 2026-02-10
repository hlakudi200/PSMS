'use client';

import React, { useMemo, useCallback } from 'react';
import { Table, Button, Space, Tag, Popconfirm, Result } from 'antd';
import { WarningOutlined, InboxOutlined } from '@ant-design/icons';
import type { TableProps } from 'antd';
import type {
  EnterpriseTableProps,
  ColumnConfig,
  RowAction,
  TableQuery,
} from './types';
import { useTableState } from './hooks/useTableState';
import { useTableSelection } from './hooks/useTableSelection';
import { useTableExport } from './hooks/useTableExport';
import { useTablePersonalization } from './hooks/useTablePersonalization';
import { useTableTelemetry } from './hooks/useTableTelemetry';
import { useResponsive } from './hooks/useResponsive';
import { applyRenderer } from './renderers';
import { TableToolbar } from './TableToolbar';

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

// Mobile card renderer for responsive layout
function MobileCard<T extends Record<string, any>>({
  record,
  columns,
  rowActions,
  currentUserRole,
  onRowClick,
}: {
  record: T;
  columns: ColumnConfig<T>[];
  rowActions?: RowAction<T>[];
  currentUserRole?: string;
  onRowClick?: (record: T) => void;
}) {
  const displayColumns = columns.slice(0, 4);
  const visibleActions = (rowActions ?? []).filter(
    a =>
      hasPermission(a.requiredPermissions, currentUserRole) &&
      (a.visible ? a.visible(record) : true),
  );

  return (
    <div
      className="psms-mobile-card"
      onClick={() => onRowClick?.(record)}
      style={{ cursor: onRowClick ? 'pointer' : 'default' }}
    >
      {displayColumns.map(col => {
        const value = getNestedValue(record, col.dataIndex);
        const rendered = col.render
          ? col.render(value, record, 0)
          : col.renderType
            ? applyRenderer(col.renderType, col.renderConfig, value, record, currentUserRole)
            : value;

        return (
          <div key={col.key} style={{ display: 'flex', gap: 8, marginBottom: 4 }}>
            <span style={{ color: '#595959', fontWeight: 500, minWidth: 100 }}>
              {col.title}:
            </span>
            <span>{rendered ?? '—'}</span>
          </div>
        );
      })}
      {visibleActions.length > 0 && (
        <div style={{ marginTop: 8, display: 'flex', gap: 8 }}>
          {visibleActions.map(action => {
            const isDisabled = action.disabled?.(record) ?? false;
            if (action.confirm) {
              return (
                <Popconfirm
                  key={action.key}
                  title={action.confirm.title}
                  description={action.confirm.description}
                  onConfirm={() => action.onClick(record)}
                >
                  <Button size="small" danger={action.danger} disabled={isDisabled}>
                    {action.icon} {action.label}
                  </Button>
                </Popconfirm>
              );
            }
            return (
              <Button
                key={action.key}
                size="small"
                danger={action.danger}
                disabled={isDisabled}
                onClick={e => {
                  e.stopPropagation();
                  action.onClick(record);
                }}
              >
                {action.icon} {action.label}
              </Button>
            );
          })}
        </div>
      )}
    </div>
  );
}

export function EnterpriseTable<T extends Record<string, any>>(
  props: EnterpriseTableProps<T>,
) {
  const {
    columns,
    data,
    totalCount,
    loading = false,
    error = false,
    errorMessage,
    onQueryChange,
    rowKey = 'id',
    onRowClick,
    rowActions,
    selectionMode = 'none',
    onSelectionChange,
    bulkActions,
    title,
    toolbarActions,
    exportConfig,
    personalization,
    currentUserRole,
    pageSize: defaultPageSize = 10,
    virtualizeThreshold = 200,
    telemetry: telemetryConfig,
    size = 'small',
    stickyHeader = true,
    scrollX = 'max-content',
    scrollY,
    emptyText,
    showLastUpdated = true,
  } = props;

  // Find default sorting from columns
  const defaultSorting = useMemo(() => {
    const col = columns.find(c => c.defaultSortOrder);
    if (!col) return undefined;
    const dataIdx = Array.isArray(col.dataIndex) ? col.dataIndex.join('.') : col.dataIndex;
    return `${dataIdx} ${col.defaultSortOrder === 'ascend' ? 'asc' : 'desc'}`;
  }, [columns]);

  // --- Hooks ---
  const tableState = useTableState({
    defaultPageSize,
    onQueryChange,
    personalization,
    defaultSorting,
  });

  const personalizationHook = useTablePersonalization(
    personalization,
    columns,
    defaultPageSize,
  );

  const selection = useTableSelection<T>({
    mode: selectionMode,
    data,
    rowKey,
    onSelectionChange,
  });

  const telemetry = useTableTelemetry(telemetryConfig);

  const responsive = useResponsive();

  // --- Column processing pipeline ---
  const processedColumns = useMemo(() => {
    let cols = columns
      // 1. Filter by permissions
      .filter(col => hasPermission(col.requiredPermissions, currentUserRole))
      // 2. Filter by visibility (personalization)
      .filter(col => personalizationHook.visibleColumnKeys.includes(col.key))
      // 3. Filter by responsive (hide on mobile)
      .filter(col => !(responsive.isMobile && col.hideOnMobile))
      // 4. Filter hidden columns
      .filter(col => !col.hidden);

    // 5. Map to Ant Design column format
    const antColumns: TableProps<T>['columns'] = cols.map(col => {
      const dataIdx = col.dataIndex;

      return {
        key: col.key,
        title: col.title,
        dataIndex: dataIdx,
        width: col.width,
        fixed: col.fixed,
        align: col.align,
        ellipsis: col.ellipsis,
        sorter: col.sortable ? true : undefined,
        defaultSortOrder: col.defaultSortOrder,
        render: (value: any, record: T, index: number) => {
          // Custom render takes priority
          if (col.render) {
            const node = col.render(value, record, index);
            if (col.conditionalStyle) {
              const style = col.conditionalStyle(value, record);
              if (style) return <span style={style}>{node}</span>;
            }
            return node;
          }

          // Built-in renderer
          if (col.renderType) {
            const rendered = applyRenderer(
              col.renderType,
              col.renderConfig,
              value,
              record,
              currentUserRole,
            );
            if (rendered !== undefined) {
              if (col.conditionalStyle) {
                const style = col.conditionalStyle(value, record);
                if (style) return <span style={style}>{rendered}</span>;
              }
              return rendered;
            }
          }

          // Default: raw value with conditional style
          if (col.conditionalStyle) {
            const style = col.conditionalStyle(value, record);
            if (style) return <span style={style}>{value ?? '—'}</span>;
          }
          return value ?? '—';
        },
      };
    });

    // 6. Append actions column if row actions exist
    if (rowActions && rowActions.length > 0) {
      antColumns.push({
        key: '_actions',
        title: 'Actions',
        fixed: 'right' as const,
        width: 120,
        align: 'center' as const,
        render: (_: any, record: T) => {
          const visibleActions = rowActions.filter(
            a =>
              hasPermission(a.requiredPermissions, currentUserRole) &&
              (a.visible ? a.visible(record) : true),
          );

          if (visibleActions.length === 0) return null;

          return (
            <Space size={4}>
              {visibleActions.map(action => {
                const isDisabled = action.disabled?.(record) ?? false;
                if (action.confirm) {
                  return (
                    <Popconfirm
                      key={action.key}
                      title={action.confirm.title}
                      description={action.confirm.description}
                      onConfirm={() => action.onClick(record)}
                    >
                      <Button
                        type="link"
                        size="small"
                        danger={action.danger}
                        disabled={isDisabled}
                        icon={action.icon}
                        style={{ padding: '0 4px' }}
                      >
                        {action.label}
                      </Button>
                    </Popconfirm>
                  );
                }
                return (
                  <Button
                    key={action.key}
                    type="link"
                    size="small"
                    danger={action.danger}
                    disabled={isDisabled}
                    icon={action.icon}
                    onClick={e => {
                      e.stopPropagation();
                      action.onClick(record);
                    }}
                    style={{ padding: '0 4px' }}
                  >
                    {action.label}
                  </Button>
                );
              })}
            </Space>
          );
        },
      });
    }

    return antColumns;
  }, [
    columns,
    currentUserRole,
    personalizationHook.visibleColumnKeys,
    responsive.isMobile,
    rowActions,
  ]);

  // Visible columns for export (non-action columns mapped back to ColumnConfig)
  const visibleColumnsForExport = useMemo(() => {
    return columns
      .filter(col => hasPermission(col.requiredPermissions, currentUserRole))
      .filter(col => personalizationHook.visibleColumnKeys.includes(col.key))
      .filter(col => !col.hidden);
  }, [columns, currentUserRole, personalizationHook.visibleColumnKeys]);

  const exportHook = useTableExport<T>({
    config: exportConfig,
    data,
    visibleColumns: visibleColumnsForExport,
    query: tableState.query,
    currentUserRole,
    onAudit: telemetry.recordExport,
  });

  // --- Active filters for summary bar ---
  const activeFilters = useMemo(() => {
    return Object.entries(tableState.state.filters).filter(
      ([, v]) => v !== undefined && v !== null && v !== '',
    );
  }, [tableState.state.filters]);

  // --- Ant Design Table onChange handler ---
  const handleTableChange: TableProps<T>['onChange'] = useCallback(
    (pagination: any, _filters: any, sorter: any) => {
      // Handle pagination
      if (pagination?.current && pagination?.pageSize) {
        tableState.handlePageChange(pagination.current, pagination.pageSize);
      }

      // Handle sorting
      if (sorter && !Array.isArray(sorter)) {
        const field = Array.isArray(sorter.field) ? sorter.field.join('.') : sorter.field;
        tableState.handleSortChange(field as string, sorter.order ?? null);
      }
    },
    [tableState],
  );

  // --- Telemetry: track load complete ---
  React.useEffect(() => {
    if (!loading && data.length >= 0) {
      telemetry.recordLoadComplete(data.length, totalCount ?? data.length);
    }
  }, [loading, data.length, totalCount]); // eslint-disable-line react-hooks/exhaustive-deps

  // --- Error state ---
  if (error && data.length === 0) {
    return (
      <div className="psms-enterprise-table">
        <TableToolbar
          title={title}
          toolbarActions={toolbarActions}
          bulkActions={bulkActions}
          selectionCount={selection.selectionCount}
          selectedRows={selection.selectedRows}
          currentUserRole={currentUserRole}
          exportConfig={exportConfig}
          isExporting={exportHook.isExporting}
          onExport={exportHook.handleExport}
          columns={columns}
          visibleColumnKeys={personalizationHook.visibleColumnKeys}
          onToggleColumn={personalizationHook.toggleColumnVisibility}
          personalization={personalization}
          savedViews={personalizationHook.savedViews}
          currentView={personalizationHook.currentView}
          onSaveView={personalizationHook.saveView}
          onLoadView={personalizationHook.loadView}
          onDeleteView={personalizationHook.deleteView}
          onRefresh={tableState.handleRefresh}
          lastFetchedAt={tableState.state.lastFetchedAt}
          showLastUpdated={showLastUpdated}
        />
        <Result
          status="error"
          title={errorMessage ?? 'Failed to load data'}
          extra={
            <Button type="primary" onClick={tableState.handleRefresh}>
              Retry
            </Button>
          }
        />
      </div>
    );
  }

  // --- Mobile card layout ---
  if (responsive.isMobile) {
    const visibleMobileColumns = columns
      .filter(col => hasPermission(col.requiredPermissions, currentUserRole))
      .filter(col => !col.hidden);

    return (
      <div className="psms-enterprise-table">
        <TableToolbar
          title={title}
          toolbarActions={toolbarActions}
          bulkActions={bulkActions}
          selectionCount={selection.selectionCount}
          selectedRows={selection.selectedRows}
          currentUserRole={currentUserRole}
          exportConfig={exportConfig}
          isExporting={exportHook.isExporting}
          onExport={exportHook.handleExport}
          columns={columns}
          visibleColumnKeys={personalizationHook.visibleColumnKeys}
          onToggleColumn={personalizationHook.toggleColumnVisibility}
          personalization={personalization}
          savedViews={personalizationHook.savedViews}
          currentView={personalizationHook.currentView}
          onSaveView={personalizationHook.saveView}
          onLoadView={personalizationHook.loadView}
          onDeleteView={personalizationHook.deleteView}
          onRefresh={tableState.handleRefresh}
          lastFetchedAt={tableState.state.lastFetchedAt}
          showLastUpdated={showLastUpdated}
        />

        {error && (
          <div style={{ background: '#FFF7E6', borderBottom: '1px solid #FFD591', padding: '4px 16px', fontSize: 12, color: '#D46B08' }}>
            <WarningOutlined /> Data may be incomplete due to an error.
          </div>
        )}

        {data.length === 0 && !loading ? (
          <div className="psms-empty">
            <InboxOutlined style={{ fontSize: 32, marginBottom: 8 }} />
            <div>{emptyText ?? 'No data'}</div>
          </div>
        ) : (
          data.map((record, idx) => (
            <MobileCard
              key={typeof rowKey === 'function' ? rowKey(record) : String((record as any)[rowKey]) ?? idx}
              record={record}
              columns={visibleMobileColumns}
              rowActions={rowActions}
              currentUserRole={currentUserRole}
              onRowClick={onRowClick}
            />
          ))
        )}

        {/* Simple mobile pagination */}
        <div style={{ padding: '8px 16px', textAlign: 'center', borderTop: '1px solid #d9d9d9' }}>
          <Space>
            <Button
              size="small"
              disabled={tableState.state.currentPage <= 1}
              onClick={() => tableState.handlePageChange(tableState.state.currentPage - 1, tableState.state.pageSize)}
            >
              Previous
            </Button>
            <span style={{ fontSize: 12, color: '#595959' }}>
              Page {tableState.state.currentPage} of {Math.ceil((totalCount ?? data.length) / tableState.state.pageSize)}
            </span>
            <Button
              size="small"
              disabled={tableState.state.currentPage >= Math.ceil((totalCount ?? data.length) / tableState.state.pageSize)}
              onClick={() => tableState.handlePageChange(tableState.state.currentPage + 1, tableState.state.pageSize)}
            >
              Next
            </Button>
          </Space>
        </div>
      </div>
    );
  }

  // --- Desktop table layout ---
  return (
    <div className="psms-enterprise-table">
      <TableToolbar
        title={title}
        toolbarActions={toolbarActions}
        bulkActions={bulkActions}
        selectionCount={selection.selectionCount}
        selectedRows={selection.selectedRows}
        currentUserRole={currentUserRole}
        exportConfig={exportConfig}
        isExporting={exportHook.isExporting}
        onExport={exportHook.handleExport}
        columns={columns}
        visibleColumnKeys={personalizationHook.visibleColumnKeys}
        onToggleColumn={personalizationHook.toggleColumnVisibility}
        personalization={personalization}
        savedViews={personalizationHook.savedViews}
        currentView={personalizationHook.currentView}
        onSaveView={personalizationHook.saveView}
        onLoadView={personalizationHook.loadView}
        onDeleteView={personalizationHook.deleteView}
        onRefresh={tableState.handleRefresh}
        lastFetchedAt={tableState.state.lastFetchedAt}
        showLastUpdated={showLastUpdated}
      />

      {/* Error warning banner (when data exists but error occurred) */}
      {error && data.length > 0 && (
        <div style={{ background: '#FFF7E6', borderBottom: '1px solid #FFD591', padding: '4px 16px', fontSize: 12, color: '#D46B08' }}>
          <WarningOutlined /> Data may be incomplete due to an error.{' '}
          <Button type="link" size="small" onClick={tableState.handleRefresh} style={{ padding: 0, fontSize: 12 }}>
            Retry
          </Button>
        </div>
      )}

      {/* Active filter summary */}
      {activeFilters.length > 0 && (
        <div className="psms-filter-summary">
          <span>Filters:</span>
          {activeFilters.map(([key, value]) => (
            <Tag
              key={key}
              closable
              onClose={() => tableState.handleFilterChange(key, undefined)}
              className="psms-filter-tag"
            >
              {key}: {String(value)}
            </Tag>
          ))}
          <Button type="link" size="small" onClick={tableState.handleFiltersReset} style={{ padding: 0, fontSize: 12 }}>
            Clear all
          </Button>
        </div>
      )}

      <Table<T>
        columns={processedColumns}
        dataSource={data}
        rowKey={rowKey}
        loading={loading}
        size={size}
        onChange={handleTableChange}
        rowSelection={selection.rowSelection}
        scroll={{ x: scrollX, y: scrollY }}
        sticky={stickyHeader}
        virtual={data.length > virtualizeThreshold}
        pagination={{
          current: tableState.state.currentPage,
          pageSize: tableState.state.pageSize,
          total: totalCount ?? data.length,
          showSizeChanger: true,
          showQuickJumper: true,
          pageSizeOptions: ['10', '25', '50', '100'],
          showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} records`,
          size: 'small',
        }}
        locale={{
          emptyText: (
            <div style={{ padding: 24 }}>
              <InboxOutlined style={{ fontSize: 32, color: '#8c8c8c', marginBottom: 8 }} />
              <div style={{ color: '#8c8c8c' }}>{emptyText ?? 'No data'}</div>
            </div>
          ),
        }}
        onRow={
          onRowClick
            ? (record) => ({
                onClick: () => onRowClick(record),
                style: { cursor: 'pointer' },
              })
            : undefined
        }
      />
    </div>
  );
}
