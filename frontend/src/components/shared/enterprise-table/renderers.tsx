import React from 'react';
import { Tag } from 'antd';
import { CheckOutlined, CloseOutlined } from '@ant-design/icons';
import type { StatusRenderConfig, MoneyRenderConfig, MaskedRenderConfig } from './types';

export function StatusRenderer(
  value: any,
  _record: any,
  config: StatusRenderConfig,
): React.ReactNode {
  const key = String(value);
  const entry = config.statusMap[key] ?? config.statusMap[value];
  if (!entry) {
    return <Tag>{config.fallbackLabel ?? String(value)}</Tag>;
  }
  return <Tag color={entry.color}>{entry.label}</Tag>;
}

export function DateRenderer(
  value: any,
  _record: any,
  type: 'date' | 'datetime' = 'date',
): React.ReactNode {
  if (!value) return '—';
  try {
    const date = new Date(value);
    if (isNaN(date.getTime())) return String(value);
    const options: Intl.DateTimeFormatOptions =
      type === 'datetime'
        ? { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' }
        : { day: '2-digit', month: 'short', year: 'numeric' };
    return date.toLocaleDateString('en-ZA', options);
  } catch {
    return String(value);
  }
}

export function MoneyRenderer(
  value: any,
  _record: any,
  config?: MoneyRenderConfig,
): React.ReactNode {
  if (value === null || value === undefined) return '—';
  const currency = config?.currency ?? 'ZAR';
  const locale = config?.locale ?? 'en-ZA';
  try {
    return new Intl.NumberFormat(locale, {
      style: 'currency',
      currency,
    }).format(Number(value));
  } catch {
    return String(value);
  }
}

export function BooleanRenderer(value: any): React.ReactNode {
  return value ? (
    <CheckOutlined style={{ color: '#52c41a' }} />
  ) : (
    <CloseOutlined style={{ color: '#ff4d4f' }} />
  );
}

export function MaskedRenderer(
  value: any,
  _record: any,
  config?: MaskedRenderConfig,
  currentRole?: string,
): React.ReactNode {
  if (value === null || value === undefined) return '—';

  // If user has unmask permission, show full value
  if (
    config?.unmaskPermissions &&
    currentRole &&
    config.unmaskPermissions.includes(currentRole)
  ) {
    return String(value);
  }

  const str = String(value);
  const maskChar = config?.maskChar ?? '*';
  const visibleChars = config?.visibleChars ?? 4;

  if (str.length <= visibleChars) {
    return <span className="psms-masked">{maskChar.repeat(4)}</span>;
  }

  const masked = maskChar.repeat(str.length - visibleChars) + str.slice(-visibleChars);
  return <span className="psms-masked">{masked}</span>;
}

export function applyRenderer<T>(
  renderType: string | undefined,
  renderConfig: any,
  value: any,
  record: T,
  currentRole?: string,
): React.ReactNode {
  switch (renderType) {
    case 'status':
      return StatusRenderer(value, record, renderConfig as StatusRenderConfig);
    case 'date':
      return DateRenderer(value, record, 'date');
    case 'datetime':
      return DateRenderer(value, record, 'datetime');
    case 'money':
      return MoneyRenderer(value, record, renderConfig as MoneyRenderConfig);
    case 'boolean':
      return BooleanRenderer(value);
    case 'masked':
      return MaskedRenderer(value, record, renderConfig as MaskedRenderConfig, currentRole);
    default:
      return undefined;
  }
}
