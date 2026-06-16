'use client';

import React, { useEffect, useState } from 'react';
import { Select } from 'antd';
import { getAxiosInstance } from '@/utils/axios-instance';

interface AdminUserLite {
  id: number;
  userName: string;
  name: string;
  surname: string;
  roleNames: string[];
  isActive: boolean;
}

interface WorkflowUserSelectProps {
  value?: number;
  onChange?: (value?: number) => void;
  /**
   * When set, the list is narrowed to active users who hold this role — steering
   * the picker toward valid assignees (the server still enforces the role match,
   * WF-05). The currently-selected user is always kept visible even if it no
   * longer matches, so editing an existing assignment never loses the value.
   */
  roleFilter?: string;
  placeholder?: string;
  disabled?: boolean;
}

/**
 * WF-04: searchable user picker for pinning a workflow step (or delegation) to a
 * specific person by name, replacing the old raw numeric "user id" input.
 */
export const WorkflowUserSelect: React.FC<WorkflowUserSelectProps> = ({
  value,
  onChange,
  roleFilter,
  placeholder,
  disabled,
}) => {
  const [users, setUsers] = useState<AdminUserLite[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    getAxiosInstance()
      .get('/api/services/app/User/GetAll?MaxResultCount=1000&IsActive=true')
      .then((res) => {
        if (!cancelled) setUsers(res.data?.result?.items ?? []);
      })
      .catch(() => {
        if (!cancelled) setUsers([]);
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  // Case-insensitive on purpose and load-bearing: User/GetAll returns roleNames
  // as the NORMALIZED (upper-cased) role name, while roleFilter is the display
  // name (e.g. "Teacher"). Do NOT simplify this to a direct === compare.
  const norm = (r?: string) => (r ?? '').toLowerCase();
  const matchesRole = (u: AdminUserLite) =>
    !roleFilter || (u.roleNames ?? []).some((r) => norm(r) === norm(roleFilter));

  const filtered = users.filter(matchesRole);
  // Keep the selected user in the list even if it doesn't match the role filter.
  const selected = users.find((u) => u.id === value);
  const list =
    selected && !filtered.some((u) => u.id === value) ? [selected, ...filtered] : filtered;

  const labelFor = (u: AdminUserLite) => {
    const full = `${u.name ?? ''} ${u.surname ?? ''}`.trim();
    return full ? `${full} (${u.userName})` : u.userName;
  };

  const options = list.map((u) => ({ value: u.id, label: labelFor(u) }));

  return (
    <Select
      showSearch
      allowClear
      loading={loading}
      disabled={disabled}
      placeholder={placeholder ?? 'Optional — pin to a specific user'}
      value={value}
      onChange={(v) => onChange?.(v as number | undefined)}
      options={options}
      optionFilterProp="label"
      notFoundContent={
        roleFilter ? `No active users hold the role "${roleFilter}"` : 'No users found'
      }
      style={{ width: '100%' }}
    />
  );
};
