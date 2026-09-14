'use client';

import React, { useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Table, Tag, Drawer, Spin, Empty, Typography, Descriptions, Button, message } from 'antd';
import {
  EyeOutlined,
  CheckCircleOutlined,
  StopOutlined,
  ThunderboltOutlined,
  PlusOutlined,
  PrinterOutlined,
  CoffeeOutlined,
} from '@ant-design/icons';
import { EnterpriseTable } from '@/components/shared/enterprise-table';
import type { ColumnConfig, TableQuery, RowAction, BulkAction, ToolbarAction } from '@/components/shared/enterprise-table';
import { TimetableProvider, useTimetableState, useTimetableActions } from '@/providers/academic/timetables';
import { TimetableSlotProvider, useTimetableSlotState, useTimetableSlotActions } from '@/providers/academic/timetable_slots';
import { SubjectProvider } from '@/providers/academic/subjects';
import { TeacherProvider } from '@/providers/academic/teachers';
import { useAuthState } from '@/providers/auth';
import { TimetableSlotEditModal } from '@/components/modals/academic/TimetableSlotEditModal';
import { detectBreaks, buildPeriodSchedule } from '@/utils/timetable-grid';
import type { ITimetableList, ITimetableSlotList } from '@/providers/academic/shared/interfaces';
import styles from './timetable.module.css';

const { Text } = Typography;

const WEEKDAY_NAMES = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday'];

/** "08:00:00" -> "08:00"; tolerates a missing value. */
const hhmm = (time?: string) => time?.slice(0, 5) ?? '';

const timeRange = (slot?: ITimetableSlotList) =>
  slot ? `${hhmm(slot.startTime)}–${hhmm(slot.endTime)}` : '';

/* Print stylesheet — same technique as the Report Card print view
 * (ReportDetailPage.tsx): hide everything except the marked print area,
 * reveal print-only elements, on `@media print`. */
const printStyles = `
@media print {
  @page { size: A4 landscape; margin: 12mm 10mm; }
  body * { visibility: hidden; }
  .timetable-print-area, .timetable-print-area * { visibility: visible; }
  .timetable-print-area { position: absolute; left: 0; top: 0; width: 100%; }
  .no-print { display: none !important; }
}
`;

interface ScheduleRow extends Record<string, unknown> {
  key: number;
  period: number;
  /** Time range this period runs, established from any slot placed at it
   * anywhere in the timetable, or extrapolated from the timetable's period
   * duration when nothing has been placed there yet. */
  periodTime: string;
  /** Raw start/end backing periodTime, used to lock a new/edited slot to it. */
  periodStart?: string;
  periodEnd?: string;
  /** True for a synthetic row rendered as a full-width "Break" bar. */
  isBreak?: boolean;
  breakStart?: string;
  breakEnd?: string;
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
  // This period's established time — a property of its position in the
  // school day, not of whichever lesson sits in it — so the modal locks to
  // it rather than letting a slot drift off the rest of the period column.
  const [slotFixedTime, setSlotFixedTime] = useState<{ startTime?: string; endTime?: string }>({});

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
    fixedTime?: { startTime?: string; endTime?: string }
  ) => {
    setSlotModalDay(day);
    setSlotModalPeriod(period);
    setEditingSlot(slot ?? null);
    setSlotFixedTime(fixedTime ?? {});
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

  const handleDownloadPdf = () => {
    window.print();
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

  // A period's time is a property of its position in the school day, not of
  // whichever lesson (if any) sits in it. This covers every period from 1 to
  // maxPeriods — including ones nothing has been placed in yet, like a
  // trailing last period — by extrapolating from the timetable's established
  // period duration, so a new slot always locks to a consistent time.
  const periodSchedule = buildPeriodSchedule(slots, maxPeriods);

  // Build rows first: each carries the time range its period runs, so the grid
  // prints the time once in the period column instead of in all five cells.
  const periodRows: ScheduleRow[] = Array.from({ length: maxPeriods }, (_, i) => {
    const period = i + 1;
    const scheduled = periodSchedule.get(period);
    const row = {
      key: period,
      period,
      periodTime: scheduled ? `${hhmm(scheduled.startTime)}–${hhmm(scheduled.endTime)}` : '',
      periodStart: scheduled?.startTime,
      periodEnd: scheduled?.endTime,
    } as ScheduleRow;
    for (let d = 1; d <= 5; d++) {
      row[`day${d}`] = slotsByDay[d]?.find(s => s.periodNumber === period);
    }
    return row;
  });

  // Breaks aren't stored data — infer them from the time gap between
  // consecutive periods so the grid shows a "Break" bar instead of an
  // unexplained gap.
  const breakRows: ScheduleRow[] = detectBreaks(slots).map((b) => ({
    key: b.sortOrder,
    period: b.sortOrder,
    periodTime: '',
    isBreak: true,
    breakStart: b.startTime,
    breakEnd: b.endTime,
  } as ScheduleRow));

  const scheduleData: ScheduleRow[] = [...periodRows, ...breakRows].sort((a, b) => a.period - b.period);

  // Break rows span the full width via antd's merge-cell technique (first
  // column returns colSpan: N, the rest return colSpan: 0).
  const scheduleColumns = [
    {
      title: 'Period',
      dataIndex: 'period',
      key: 'period',
      width: 92,
      render: (period: number, row: ScheduleRow) => {
        if (row.isBreak) {
          return {
            children: (
              <span className={styles.breakBar}>
                <CoffeeOutlined /> Break &nbsp;{hhmm(row.breakStart)}–{hhmm(row.breakEnd)}
              </span>
            ),
            props: { colSpan: 6 },
          };
        }
        return (
          <>
            <span className={styles.periodNumber}>Period {period}</span>
            {row.periodTime && <span className={styles.periodTime}>{row.periodTime}</span>}
          </>
        );
      },
    },
    ...WEEKDAY_NAMES.map((day, idx) => ({
      title: day,
      dataIndex: `day${idx + 1}`,
      key: `day${idx + 1}`,
      render: (slot: ITimetableSlotList | undefined, row: ScheduleRow) => {
        if (row.isBreak) {
          return { children: null, props: { colSpan: 0 } };
        }

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

      <style>{printStyles}</style>

      <Drawer
        title={selectedTimetable ? `${selectedTimetable.className} — weekly schedule` : 'Weekly schedule'}
        extra={
          <Button className="no-print" icon={<PrinterOutlined />} onClick={handleDownloadPdf} disabled={slotsLoading}>
            Download PDF
          </Button>
        }
        open={drawerOpen}
        onClose={handleDrawerClose}
        width={880}
        styles={{ body: { padding: 16 } }}
      >
        <div className="timetable-print-area">
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
              <Text type="secondary" className="no-print" style={{ display: 'block', marginTop: 12, fontSize: 12 }}>
                Select any period to add, edit or remove a lesson. Clashes with the teacher&apos;s
                other classes are rejected when you save.
              </Text>
            </>
          )}
        </div>
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
          fixedStartTime={slotFixedTime.startTime}
          fixedEndTime={slotFixedTime.endTime}
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
