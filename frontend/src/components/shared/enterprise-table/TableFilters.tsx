'use client';

import React, { useState } from 'react';
import { Button, Input, Select, DatePicker, InputNumber, Space, Badge } from 'antd';
import { FilterOutlined, CloseOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import type { ColumnConfig } from './types';

const { RangePicker } = DatePicker;

interface TableFiltersProps<T> {
  columns: ColumnConfig<T>[];
  filters: Record<string, unknown>;
  onFilterChange: (key: string, value: unknown) => void;
  onFiltersReset: () => void;
}

export function TableFilters<T extends Record<string, any>>(
  props: TableFiltersProps<T>,
) {
  const { columns, filters, onFilterChange, onFiltersReset } = props;
  const [expanded, setExpanded] = useState(false);

  const filterableColumns = columns.filter(col => col.filterable);
  if (filterableColumns.length === 0) return null;

  const activeCount = Object.keys(filters).filter(
    k => filters[k] !== undefined && filters[k] !== null && filters[k] !== '',
  ).length;

  return (
    <div style={{ borderBottom: '1px solid #f0f0f0' }}>
      <div style={{ padding: '6px 16px', display: 'flex', alignItems: 'center', gap: 8 }}>
        <Badge count={activeCount} size="small" offset={[2, 0]}>
          <Button
            size="small"
            icon={<FilterOutlined />}
            type={expanded ? 'primary' : 'default'}
            ghost={expanded}
            onClick={() => setExpanded(!expanded)}
          >
            Filters
          </Button>
        </Badge>
        {activeCount > 0 && (
          <Button
            size="small"
            type="link"
            icon={<CloseOutlined />}
            onClick={() => {
              onFiltersReset();
              setExpanded(false);
            }}
            style={{ fontSize: 12, padding: 0 }}
          >
            Clear all
          </Button>
        )}
      </div>

      {expanded && (
        <div style={{
          padding: '8px 16px 12px',
          background: '#fafafa',
          display: 'flex',
          flexWrap: 'wrap',
          gap: 12,
          alignItems: 'flex-end',
        }}>
          {filterableColumns.map(col => {
            const filterKey = col.filterKey ?? (Array.isArray(col.dataIndex) ? col.dataIndex.join('.') : col.dataIndex);
            const filterType = col.filterType ?? 'text';
            const currentValue = filters[filterKey];

            return (
              <div key={col.key} style={{ minWidth: 160 }}>
                <div style={{ fontSize: 12, color: '#595959', marginBottom: 4, fontWeight: 500 }}>
                  {col.title}
                </div>
                {filterType === 'text' && (
                  <Input
                    size="small"
                    placeholder={`Filter ${col.title.toLowerCase()}...`}
                    allowClear
                    value={(currentValue as string) ?? ''}
                    onChange={e => onFilterChange(filterKey, e.target.value || undefined)}
                    style={{ width: '100%' }}
                  />
                )}
                {filterType === 'number' && (
                  <InputNumber
                    size="small"
                    placeholder={`Filter...`}
                    value={currentValue as number | undefined}
                    onChange={val => onFilterChange(filterKey, val ?? undefined)}
                    style={{ width: '100%' }}
                  />
                )}
                {filterType === 'enum' && (
                  <Select
                    size="small"
                    placeholder={`All`}
                    allowClear
                    value={currentValue as string | number | undefined}
                    onChange={val => onFilterChange(filterKey, val ?? undefined)}
                    options={col.filterOptions ?? []}
                    style={{ width: '100%', minWidth: 140 }}
                  />
                )}
                {filterType === 'date' && (
                  <DatePicker
                    size="small"
                    value={currentValue ? dayjs(currentValue as string) : null}
                    onChange={date => onFilterChange(filterKey, date?.format('YYYY-MM-DD') ?? undefined)}
                    style={{ width: '100%' }}
                  />
                )}
                {filterType === 'dateRange' && (
                  <RangePicker
                    size="small"
                    value={
                      Array.isArray(currentValue) && currentValue.length === 2
                        ? [dayjs(currentValue[0] as string), dayjs(currentValue[1] as string)]
                        : null
                    }
                    onChange={(dates) => {
                      if (dates && dates[0] && dates[1]) {
                        onFilterChange(filterKey, [
                          dates[0].format('YYYY-MM-DD'),
                          dates[1].format('YYYY-MM-DD'),
                        ]);
                      } else {
                        onFilterChange(filterKey, undefined);
                      }
                    }}
                    style={{ width: '100%' }}
                  />
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
