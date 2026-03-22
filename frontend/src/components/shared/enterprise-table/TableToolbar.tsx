'use client';

import React, { useState } from 'react';
import {
  Button,
  Space,
  Dropdown,
  Checkbox,
  Input,
  Typography,
  Badge,
  Popconfirm,
} from 'antd';
import {
  ReloadOutlined,
  DownloadOutlined,
  SettingOutlined,
  AppstoreOutlined,
  SaveOutlined,
} from '@ant-design/icons';
import type {
  ToolbarAction,
  BulkAction,
  ExportConfig,
  PersonalizationConfig,
  ColumnConfig,
  TableView,
} from './types';

const { Text } = Typography;

interface TableToolbarProps<T> {
  title?: string;
  toolbarActions?: ToolbarAction[];
  bulkActions?: BulkAction<T>[];
  selectionCount: number;
  selectedRows: T[];
  currentUserRole?: string;

  // Export
  exportConfig?: ExportConfig;
  isExporting: boolean;
  onExport: (format: 'csv' | 'xlsx', scope: 'currentPage' | 'allResults') => void;

  // Column visibility
  columns: ColumnConfig<T>[];
  visibleColumnKeys: string[];
  onToggleColumn: (key: string) => void;

  // Personalization
  personalization?: PersonalizationConfig;
  savedViews: TableView[];
  currentView: TableView | null;
  onSaveView: (name: string) => void;
  onLoadView: (viewId: string) => void;
  onDeleteView: (viewId: string) => void;

  // Search
  searchable?: boolean;
  searchPlaceholder?: string;
  searchValue?: string;
  onSearchChange?: (value: string) => void;

  // Refresh
  onRefresh: () => void;
  lastFetchedAt?: Date;
  showLastUpdated: boolean;
}

function hasPermission(requiredPermissions: string[] | undefined, role?: string): boolean {
  if (!requiredPermissions || requiredPermissions.length === 0) return true;
  if (!role) return false;
  const roleLower = role.toLowerCase();
  return requiredPermissions.some(p => p.toLowerCase() === roleLower);
}

function formatTimeAgo(date: Date): string {
  const seconds = Math.floor((Date.now() - date.getTime()) / 1000);
  if (seconds < 60) return 'Just now';
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `Updated ${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  return `Updated ${hours}h ago`;
}

export function TableToolbar<T extends Record<string, any>>(
  props: TableToolbarProps<T>,
) {
  const {
    title,
    toolbarActions,
    bulkActions,
    selectionCount,
    selectedRows,
    currentUserRole,
    exportConfig,
    isExporting,
    onExport,
    columns,
    visibleColumnKeys,
    onToggleColumn,
    personalization,
    savedViews,
    currentView,
    onSaveView,
    onLoadView,
    onDeleteView,
    searchable,
    searchPlaceholder,
    searchValue,
    onSearchChange,
    onRefresh,
    lastFetchedAt,
    showLastUpdated,
  } = props;

  const [viewName, setViewName] = useState('');

  // Filter actions by permission
  const visibleToolbarActions = (toolbarActions ?? []).filter(a =>
    hasPermission(a.requiredPermissions, currentUserRole),
  );

  const visibleBulkActions = (bulkActions ?? []).filter(a =>
    hasPermission(a.requiredPermissions, currentUserRole),
  );

  const canExport =
    exportConfig?.enabled && hasPermission(exportConfig.requiredPermissions, currentUserRole);

  const formats = exportConfig?.formats ?? ['csv', 'xlsx'];

  // Column visibility dropdown items
  const columnVisibilityItems = columns.map(col => ({
    key: col.key,
    label: (
      <Checkbox
        checked={visibleColumnKeys.includes(col.key)}
        onChange={() => onToggleColumn(col.key)}
      >
        {col.title}
      </Checkbox>
    ),
  }));

  // Export dropdown items
  const exportItems = [
    ...formats.map(fmt => ({
      key: `current-${fmt}`,
      label: `Current Page (${fmt.toUpperCase()})`,
      onClick: () => onExport(fmt, 'currentPage'),
    })),
    { type: 'divider' as const, key: 'divider' },
    ...formats.map(fmt => ({
      key: `all-${fmt}`,
      label: `All Results (${fmt.toUpperCase()})`,
      onClick: () => onExport(fmt, 'allResults'),
    })),
  ];

  // Saved views dropdown items
  const viewItems = [
    ...savedViews.map(v => ({
      key: v.id,
      label: (
        <Space>
          <span>{v.name}</span>
          {v.isDefault && <Badge count="Default" style={{ backgroundColor: '#52c41a' }} />}
        </Space>
      ),
      onClick: () => onLoadView(v.id),
    })),
    ...(personalization?.allowSaveViews
      ? [
          { type: 'divider' as const, key: 'view-divider' },
          {
            key: 'save-new',
            label: (
              <Space>
                <Input
                  size="small"
                  placeholder="View name"
                  value={viewName}
                  onChange={e => setViewName(e.target.value)}
                  onClick={e => e.stopPropagation()}
                  style={{ width: 120 }}
                />
                <Button
                  size="small"
                  type="primary"
                  icon={<SaveOutlined />}
                  disabled={!viewName.trim()}
                  onClick={e => {
                    e.stopPropagation();
                    if (viewName.trim()) {
                      onSaveView(viewName.trim());
                      setViewName('');
                    }
                  }}
                >
                  Save
                </Button>
              </Space>
            ),
          },
        ]
      : []),
  ];

  return (
    <div className="psms-toolbar" style={{
      padding: '8px 16px',
      borderBottom: '1px solid #d9d9d9',
      background: '#fafafa',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      flexWrap: 'wrap',
      gap: 8,
    }}>
      {/* Left side */}
      <Space wrap>
        {title && (
          <Text strong style={{ fontSize: 14, marginRight: 8 }}>
            {title}
          </Text>
        )}

        {searchable && (
          <Input.Search
            placeholder={searchPlaceholder ?? 'Search...'}
            allowClear
            size="small"
            style={{ width: 220 }}
            value={searchValue}
            onChange={e => onSearchChange?.(e.target.value)}
            onSearch={value => onSearchChange?.(value)}
          />
        )}

        {visibleToolbarActions.map(action => (
          <Button
            key={action.key}
            type={action.type ?? 'default'}
            icon={action.icon}
            danger={action.danger}
            disabled={action.disabled}
            onClick={action.onClick}
            size="small"
          >
            {action.label}
          </Button>
        ))}

        {/* Bulk actions — only when rows selected */}
        {selectionCount > 0 && visibleBulkActions.length > 0 && (
          <Space className="psms-bulk-bar" style={{
            background: '#E6F7FF',
            border: '1px solid #91D5FF',
            borderRadius: 2,
            padding: '2px 8px',
          }}>
            <Text style={{ fontSize: 12, color: '#0066CC' }}>
              {selectionCount} selected
            </Text>
            {visibleBulkActions.map(action =>
              action.confirm ? (
                <Popconfirm
                  key={action.key}
                  title={action.confirm.title}
                  description={action.confirm.description}
                  onConfirm={() => action.onClick(selectedRows)}
                >
                  <Button size="small" danger={action.danger} icon={action.icon}>
                    {action.label}
                  </Button>
                </Popconfirm>
              ) : (
                <Button
                  key={action.key}
                  size="small"
                  danger={action.danger}
                  icon={action.icon}
                  onClick={() => action.onClick(selectedRows)}
                >
                  {action.label}
                </Button>
              ),
            )}
          </Space>
        )}
      </Space>

      {/* Right side */}
      <Space>
        {/* Column visibility */}
        <Dropdown menu={{ items: columnVisibilityItems }} trigger={['click']} placement="bottomRight">
          <Button size="small" icon={<SettingOutlined />}>
            Columns
          </Button>
        </Dropdown>

        {/* Saved views */}
        {personalization?.enabled && viewItems.length > 0 && (
          <Dropdown menu={{ items: viewItems }} trigger={['click']} placement="bottomRight">
            <Button size="small" icon={<AppstoreOutlined />}>
              {currentView?.name ?? 'Views'}
            </Button>
          </Dropdown>
        )}

        {/* Export */}
        {canExport && (
          <Dropdown menu={{ items: exportItems }} trigger={['click']} placement="bottomRight">
            <Button size="small" icon={<DownloadOutlined />} loading={isExporting}>
              Export
            </Button>
          </Dropdown>
        )}

        {/* Refresh */}
        <Button size="small" icon={<ReloadOutlined />} onClick={onRefresh} />

        {/* Last updated */}
        {showLastUpdated && lastFetchedAt && (
          <Text className="psms-last-updated">
            {formatTimeAgo(lastFetchedAt)}
          </Text>
        )}
      </Space>
    </div>
  );
}
