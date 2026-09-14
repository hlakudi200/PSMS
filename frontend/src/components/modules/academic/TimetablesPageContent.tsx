'use client';

import React, { useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Table, Tag, Drawer, Spin, Empty, Typography, Descriptions, message } from 'antd';
import {
  EyeOutlined,
  CheckCircleOutlined,
  StopOutlined,
  ThunderboltOutlined,
  PlusOutlined,
} from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, BulkAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { TimetableProvider, useTimetableState, useTimetableActions } from '@/providers/academic/timetables';
import { TimetableSlotProvider, useTimetableSlotState, useTimetableSlotActions } from '@/providers/academic/timetable_slots';
import { SubjectProvider } from '@/providers/academic/subjects';
import { TeacherProvider } from '@/providers/academic/teachers';
import { useAuthState } from '@/providers/auth';
import { TimetableSlotEditModal } from '@/components/modals/academic/TimetableSlotEditModal';
import type { ITimetableList, ITimetableSlotList } from '@/providers/academic/shared/interfaces';
import styles from './timetable.module.css';

const { Text } = Typography;

const WEEKDAY_NAMES = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday'];

/** "08:00:00" -> "08:00"; tolerates a missing value. */
const hhmm = (time?: string) => time?.slice(0, 5) ?? '';

const timeRange = (slot?: ITimetableSlotList) =>
  slot ? `${hhmm(slot.startTime)}–${hhmm(slot.endTime)}` : '';

interface ScheduleRow extends Record<string, unknown> {
  key: number;
  period: number;
  /** Time range shared by the slots in this period, when they agree. */
  periodTime: string;
  /** Raw start/end of that shared range, used to prefill a new slot in this period. */
  periodStart?: string;
  periodEnd?: string;
}

function TimetablesContent() {
  const router = useRouter();
  const { timetables, totalCount, isPending, isError } = useTimetableState();
  const { getAllAsync, activateAsync, deactivateAsync } = useTimetableActions();
  const { timetableSlots, isPending: slotsLoading } = useTimetableSlotState();
  const { getByTimetableAsync } = useTimetableSlotActions();
  const { currentRole } = useAuthState();

  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [selectedTimetable, setSelectedTimetable] = useState<ITimetableList | null>(null);
  const [slotModalOpen, setSlotModalOpen] = useState(false);
  const [slotModalDay, setSlotModalDay] = useState(1);
  const [slotModalPeriod, setSlotModalPeriod] = useState(1);
  const [editingSlot, setEditingSlot] = useState<ITimetableSlotList | null>(null);
  // Times the rest of this period already runs at, so a new slot starts prefilled.
  const [slotDefaults, setSlotDefaults] = useState<{ startTime?: string; endTime?: string }>({});

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

  const handleCellClick = (
    day: number,
    period: number,
    slot?: ITimetableSlotList,
    defaults?: { startTime?: string; endTime?: string }
  ) => {
    setSlotModalDay(day);
    setSlotModalPeriod(period);
    setEditingSlot(slot ?? null);
    setSlotDefaults(defaults ?? {});
    setSlotModalOpen(true);
  };

  const handleSlotModalClose = (refresh?: boolean) => {
    setSlotModalOpen(false);
    setEditingSlot(null);
    if (refresh && selectedTimetable) {
      getByTimetableAsync(selectedTimetable.id);
      refreshData();
    }
  };

  const toolbarActions: ToolbarAction[] = [
    {
      key: 'generate',
      label: 'Generate Timetables',
      icon: <ThunderboltOutlined />,
      type: 'primary',
      onClick: () => router.push('/principal/timetables/generate'),
      requiredPermissions: ['Admin', 'Principal', 'VicePrincipal'],
    },
  ];

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
  const slots = timetableSlots ?? [];
  const slotsByDay = slots.reduce<Record<number, ITimetableSlotList[]>>((acc, slot) => {
    if (!acc[slot.dayOfWeek]) acc[slot.dayOfWeek] = [];
    acc[slot.dayOfWeek].push(slot);
    acc[slot.dayOfWeek].sort((a, b) => a.periodNumber - b.periodNumber);
    return acc;
  }, {});

  // Find max periods across all days — default to 8 so an empty timetable
  // still shows a fillable grid rather than nothing to click on.
  const maxPeriods = Math.max(8, ...slots.map(s => s.periodNumber));

  // Build rows first: each carries the time range its slots share, so the grid
  // prints the time once in the period column instead of in all five cells.
  const scheduleData: ScheduleRow[] = Array.from({ length: maxPeriods }, (_, i) => {
    const period = i + 1;
    const row = { key: period, period, periodTime: '' } as ScheduleRow;
    const inPeriod: ITimetableSlotList[] = [];
    for (let d = 1; d <= 5; d++) {
      const slot = slotsByDay[d]?.find(s => s.periodNumber === period);
      row[`day${d}`] = slot;
      if (slot) inPeriod.push(slot);
    }
    const ranges = new Set(inPeriod.map(timeRange));
    if (ranges.size === 1) {
      row.periodTime = [...ranges][0];
      row.periodStart = inPeriod[0].startTime;
      row.periodEnd = inPeriod[0].endTime;
    }
    return row;
  });

  const scheduleColumns = [
    {
      title: 'Period',
      dataIndex: 'period',
      key: 'period',
      width: 92,
      render: (period: number, row: ScheduleRow) => (
        <>
          <span className={styles.periodNumber}>Period {period}</span>
          {row.periodTime && <span className={styles.periodTime}>{row.periodTime}</span>}
        </>
      ),
    },
    ...WEEKDAY_NAMES.map((day, idx) => ({
      title: day,
      dataIndex: `day${idx + 1}`,
      key: `day${idx + 1}`,
      render: (slot: ITimetableSlotList | undefined, row: ScheduleRow) => {
        const dayOfWeek = idx + 1;
        const label = slot
          ? `Edit ${slot.subjectName ?? 'lesson'}, ${day} period ${row.period}`
          : `Add a lesson for ${day} period ${row.period}`;
        // Only repeat the time in the cell when it differs from the row's.
        const ownTime = slot && timeRange(slot) !== row.periodTime ? timeRange(slot) : '';

        return (
          <button
            type="button"
            className={styles.cell}
            aria-label={label}
            onClick={() => handleCellClick(dayOfWeek, row.period, slot, {
              startTime: row.periodStart,
              endTime: row.periodEnd,
            })}
          >
            {slot ? (
              <>
                <span className={styles.subject}>{slot.subjectName ?? 'Unassigned subject'}</span>
                <span className={styles.teacher}>{slot.teacherName ?? 'No teacher'}</span>
                {(slot.roomNumber || ownTime) && (
                  <span className={styles.meta}>
                    {slot.roomNumber && <Tag style={{ margin: 0, fontSize: 11 }}>{slot.roomNumber}</Tag>}
                    {ownTime}
                  </span>
                )}
              </>
            ) : (
              <span className={styles.empty}>
                <PlusOutlined style={{ marginRight: 4 }} />
                Add
              </span>
            )}
          </button>
        );
      },
    })),
  ];

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
        toolbarActions={toolbarActions}
        selectionMode="multi"
        currentUserRole={currentRole}
        exportConfig={{
          enabled: true,
          formats: ['csv', 'xlsx'],
          requiredPermissions: ['Admin', 'Principal', 'VicePrincipal'],
        }}
      />

      <Drawer
        title={selectedTimetable ? `${selectedTimetable.className} — weekly schedule` : 'Weekly schedule'}
        open={drawerOpen}
        onClose={handleDrawerClose}
        width={880}
        styles={{ body: { padding: 16 } }}
      >
        {selectedTimetable && (
          <Descriptions size="small" column={{ xs: 1, sm: 3 }} style={{ marginBottom: 16 }}>
            <Descriptions.Item label="Status">
              <Tag color={selectedTimetable.isActive ? 'green' : 'default'} style={{ margin: 0 }}>
                {selectedTimetable.isActive ? 'Active' : 'Inactive'}
              </Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Effective from">
              {new Date(selectedTimetable.effectiveDate).toLocaleDateString('en-ZA', {
                day: 'numeric', month: 'short', year: 'numeric',
              })}
            </Descriptions.Item>
            <Descriptions.Item label="Periods filled">{slots.length}</Descriptions.Item>
          </Descriptions>
        )}

        {slotsLoading ? (
          <Spin style={{ display: 'block', margin: '60px auto' }} />
        ) : (
          <>
            {slots.length === 0 && (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description="Nothing scheduled yet — pick a period below to add the first lesson."
                style={{ marginBottom: 8 }}
              />
            )}
            <Table<ScheduleRow>
              columns={scheduleColumns}
              dataSource={scheduleData}
              pagination={false}
              size="small"
              bordered
              scroll={{ x: 760 }}
            />
            <Text type="secondary" style={{ display: 'block', marginTop: 12, fontSize: 12 }}>
              Select any period to add, edit or remove a lesson. Clashes with the teacher&apos;s
              other classes are rejected when you save.
            </Text>
          </>
        )}
      </Drawer>

      {selectedTimetable && (
        <TimetableSlotEditModal
          open={slotModalOpen}
          onClose={handleSlotModalClose}
          timetableId={selectedTimetable.id}
          className={selectedTimetable.className}
          dayOfWeek={slotModalDay}
          periodNumber={slotModalPeriod}
          existingSlot={editingSlot}
          defaultStartTime={slotDefaults.startTime}
          defaultEndTime={slotDefaults.endTime}
        />
      )}
    </>
  );
}

export default function TimetablesPageContent() {
  return (
    <TimetableProvider>
      <TimetableSlotProvider>
        <SubjectProvider>
          <TeacherProvider>
            <TimetablesContent />
          </TeacherProvider>
        </SubjectProvider>
      </TimetableSlotProvider>
    </TimetableProvider>
  );
}
