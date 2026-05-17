'use client';

import React, { useState, useCallback, useEffect } from 'react';
import { Card, Table, Tag, Button, Drawer, Empty, Spin, Space, message } from 'antd';
import {
  EyeOutlined,
  CheckCircleOutlined,
  StopOutlined,
} from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, BulkAction } from '@/components/shared/enterprise-table';
import { TimetableProvider, useTimetableState, useTimetableActions } from '@/providers/academic/timetables';
import { TimetableSlotProvider, useTimetableSlotState, useTimetableSlotActions } from '@/providers/academic/timetable_slots';
import { useAuthState } from '@/providers/auth';
import type { ITimetableList, ITimetableSlotList } from '@/providers/academic/shared/interfaces';

const dayNames = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

function TimetablesContent() {
  const { timetables, totalCount, isPending, isError } = useTimetableState();
  const { getAllAsync, activateAsync, deactivateAsync } = useTimetableActions();
  const { timetableSlots, isPending: slotsLoading } = useTimetableSlotState();
  const { getByTimetableAsync } = useTimetableSlotActions();
  const { currentRole } = useAuthState();

  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [selectedTimetable, setSelectedTimetable] = useState<ITimetableList | null>(null);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
      ...query.filters,
    });
  }, [getAllAsync]);

  const refreshData = useCallback(() => {
    if (lastQuery) handleQueryChange(lastQuery);
  }, [lastQuery, handleQueryChange]);

  const handleViewSchedule = (record: ITimetableList) => {
    setSelectedTimetable(record);
    setDrawerOpen(true);
    getByTimetableAsync(record.id);
  };

  const handleDrawerClose = () => {
    setDrawerOpen(false);
    setSelectedTimetable(null);
  };

  const columns: ColumnConfig<ITimetableList>[] = [
    { key: 'className', title: 'Class', dataIndex: 'className', sortable: true },
    { key: 'effectiveDate', title: 'Effective From', dataIndex: 'effectiveDate', sortable: true, renderType: 'date', width: 130 },
    { key: 'endDate', title: 'End Date', dataIndex: 'endDate', sortable: true, renderType: 'date', hideOnMobile: true, width: 130 },
    { key: 'slotCount', title: 'Slots', dataIndex: 'slotCount', sortable: true, width: 80 },
    {
      key: 'isActive', title: 'Status', dataIndex: 'isActive',
      renderType: 'status',
      renderConfig: {
        statusMap: {
          true: { label: 'Active', color: 'green' },
          false: { label: 'Inactive', color: 'default' },
        },
      },
    },
  ];

  const rowActions: RowAction<ITimetableList>[] = [
    {
      key: 'view',
      label: 'View Schedule',
      icon: <EyeOutlined />,
      onClick: handleViewSchedule,
    },
    {
      key: 'activate',
      label: 'Activate',
      icon: <CheckCircleOutlined />,
      visible: (record) => !record.isActive,
      confirm: { title: 'Activate this timetable?', description: 'This will make it the active timetable for its class.' },
      onClick: async (record) => {
        try {
          await activateAsync(record.id);
          message.success('Timetable activated');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
    {
      key: 'deactivate',
      label: 'Deactivate',
      icon: <StopOutlined />,
      visible: (record) => record.isActive,
      danger: true,
      confirm: { title: 'Deactivate this timetable?' },
      onClick: async (record) => {
        try {
          await deactivateAsync(record.id);
          message.success('Timetable deactivated');
          refreshData();
        } catch {
          // Surfaced by axios interceptor
        }
      },
    },
  ];

  const runBulkTimetable = async (
    rows: ITimetableList[],
    fn: (id: string) => void | Promise<unknown>,
    okLabel: string,
    failLabel: string
  ) => {
    const results = await Promise.allSettled(
      rows.map((r) => Promise.resolve(fn(r.id) as unknown))
    );
    const ok = results.filter((r) => r.status === 'fulfilled').length;
    const fail = results.length - ok;
    if (fail === 0) {
      message.success(`${ok} timetable(s) ${okLabel}`);
    } else if (ok === 0) {
      message.error(`No timetables ${okLabel}. ${fail} ${failLabel}.`);
    } else {
      message.warning(`${ok} ${okLabel}; ${fail} ${failLabel}.`);
    }
    refreshData();
  };

  const bulkActions: BulkAction<ITimetableList>[] = [
    {
      key: 'bulkActivate',
      label: 'Activate Selected',
      confirm: { title: 'Activate all selected timetables?' },
      onClick: async (rows) => {
        const inactive = rows.filter(r => !r.isActive);
        if (inactive.length === 0) {
          message.warning('No inactive timetables selected');
          return;
        }
        await runBulkTimetable(inactive, activateAsync, 'activated', 'failed');
      },
    },
    {
      key: 'bulkDeactivate',
      label: 'Deactivate Selected',
      danger: true,
      confirm: { title: 'Deactivate all selected timetables?' },
      onClick: async (rows) => {
        const active = rows.filter(r => r.isActive);
        if (active.length === 0) {
          message.warning('No active timetables selected');
          return;
        }
        await runBulkTimetable(active, deactivateAsync, 'deactivated', 'failed');
      },
    },
  ];

  // Group slots by day for the weekly grid
  const slotsByDay = (timetableSlots ?? []).reduce<Record<number, ITimetableSlotList[]>>((acc, slot) => {
    if (!acc[slot.dayOfWeek]) acc[slot.dayOfWeek] = [];
    acc[slot.dayOfWeek].push(slot);
    acc[slot.dayOfWeek].sort((a, b) => a.periodNumber - b.periodNumber);
    return acc;
  }, {});

  // Find max periods across all days
  const maxPeriods = Math.max(
    1,
    ...(timetableSlots ?? []).map(s => s.periodNumber),
  );

  // Build table columns for weekly grid
  const scheduleColumns = [
    {
      title: 'Period',
      dataIndex: 'period',
      key: 'period',
      width: 70,
      render: (val: number) => <strong>P{val}</strong>,
    },
    ...dayNames.slice(0, 5).map((day, idx) => ({
      title: day,
      dataIndex: `day${idx + 1}`,
      key: `day${idx + 1}`,
      render: (slot: ITimetableSlotList | undefined) => {
        if (!slot) return <span style={{ color: '#bfbfbf' }}>-</span>;
        return (
          <div style={{ fontSize: 12, lineHeight: 1.4 }}>
            <strong>{slot.subjectName ?? 'N/A'}</strong>
            <br />
            <span style={{ color: '#595959' }}>{slot.teacherName ?? ''}</span>
            {slot.roomNumber && (
              <>
                <br />
                <Tag style={{ fontSize: 11, marginTop: 2 }}>{slot.roomNumber}</Tag>
              </>
            )}
            <br />
            <span style={{ color: '#8c8c8c', fontSize: 11 }}>
              {slot.startTime?.slice(0, 5)} - {slot.endTime?.slice(0, 5)}
            </span>
          </div>
        );
      },
    })),
  ];

  // Build rows for the weekly grid
  const scheduleData = Array.from({ length: maxPeriods }, (_, i) => {
    const period = i + 1;
    const row: Record<string, unknown> = { key: period, period };
    for (let d = 1; d <= 5; d++) {
      row[`day${d}`] = slotsByDay[d]?.find(s => s.periodNumber === period);
    }
    return row;
  });

  return (
    <>
      <EnterpriseTable<ITimetableList>
        title="Timetables"
        columns={columns}
        data={timetables ?? []}
        totalCount={totalCount}
        loading={isPending}
        error={isError}
        onQueryChange={handleQueryChange}
        rowKey="id"
        rowActions={rowActions}
        bulkActions={bulkActions}
        selectionMode="multi"
        currentUserRole={currentRole}
        exportConfig={{
          enabled: true,
          formats: ['csv', 'xlsx'],
          requiredPermissions: ['Admin', 'Principal', 'VicePrincipal'],
        }}
      />

      <Drawer
        title={
          <Space>
            <span>Weekly Schedule</span>
            {selectedTimetable && (
              <Tag color={selectedTimetable.isActive ? 'green' : 'default'}>
                {selectedTimetable.className} — {selectedTimetable.isActive ? 'Active' : 'Inactive'}
              </Tag>
            )}
          </Space>
        }
        open={drawerOpen}
        onClose={handleDrawerClose}
        width={820}
        styles={{ body: { padding: 16 } }}
      >
        {slotsLoading ? (
          <Spin style={{ display: 'block', margin: '60px auto' }} />
        ) : timetableSlots && timetableSlots.length > 0 ? (
          <Table
            columns={scheduleColumns}
            dataSource={scheduleData}
            pagination={false}
            size="small"
            bordered
            scroll={{ x: 700 }}
          />
        ) : (
          <Empty description="No schedule slots found for this timetable" />
        )}
      </Drawer>
    </>
  );
}

export default function TimetablesPageContent() {
  return (
    <TimetableProvider>
      <TimetableSlotProvider>
        <TimetablesContent />
      </TimetableSlotProvider>
    </TimetableProvider>
  );
}
