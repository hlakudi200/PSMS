'use client';

import React, { useEffect, useState, useCallback } from 'react';
import { Drawer, Table, Tag, Button, Space, Popconfirm, message, Typography, Empty } from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  CheckCircleOutlined,
} from '@ant-design/icons';
import { TermProvider, useTermState, useTermActions } from '@/providers/academic/terms';
import { TermFormModal } from '@/components/modals/academic/TermFormModal';
import type { IAcademicYear, ITerm } from '@/providers/academic/shared/interfaces';
import dayjs from 'dayjs';

const { Text } = Typography;

interface TermManagementDrawerProps {
  open: boolean;
  onClose: () => void;
  academicYear: IAcademicYear | null;
}

function TermManagementContent({ academicYear, onClose, open }: TermManagementDrawerProps) {
  const { terms, isPending } = useTermState();
  const { getByAcademicYearAsync, deleteAsync, setAsCurrentAsync } = useTermActions();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<ITerm | null>(null);

  const refreshTerms = useCallback(() => {
    if (academicYear) {
      getByAcademicYearAsync(academicYear.id);
    }
  }, [academicYear, getByAcademicYearAsync]);

  useEffect(() => {
    if (open && academicYear) {
      refreshTerms();
    }
  }, [open, academicYear, refreshTerms]);

  const handleModalClose = (refresh?: boolean) => {
    setModalOpen(false);
    setEditRecord(null);
    if (refresh) refreshTerms();
  };

  const handleDelete = async (id: string) => {
    try {
      await deleteAsync(id);
      message.success('Term deleted');
      refreshTerms();
    } catch {
      // Server errors are surfaced by the axios response interceptor.
    }
  };

  const handleSetCurrent = async (id: string) => {
    try {
      await setAsCurrentAsync(id);
      message.success('Term set as current');
      refreshTerms();
    } catch {
      // Server errors are surfaced by the axios response interceptor.
    }
  };

  const existingTermNumbers = (terms ?? []).map(t => t.termNumber);

  const columns = [
    {
      title: '#',
      dataIndex: 'termNumber',
      key: 'termNumber',
      width: 60,
      sorter: (a: ITerm, b: ITerm) => a.termNumber - b.termNumber,
    },
    {
      title: 'Name',
      dataIndex: 'termName',
      key: 'termName',
      render: (text: string, record: ITerm) => (
        <Space>
          <Text strong>{text}</Text>
          {record.isCurrent && <Tag color="green">Current</Tag>}
        </Space>
      ),
    },
    {
      title: 'Start Date',
      dataIndex: 'startDate',
      key: 'startDate',
      render: (date: string) => dayjs(date).format('DD MMM YYYY'),
    },
    {
      title: 'End Date',
      dataIndex: 'endDate',
      key: 'endDate',
      render: (date: string) => dayjs(date).format('DD MMM YYYY'),
    },
    {
      title: 'Duration',
      key: 'totalDays',
      render: (_: unknown, record: ITerm) => `${dayjs(record.endDate).diff(dayjs(record.startDate), 'day')} days`,
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 200,
      render: (_: unknown, record: ITerm) => (
        <Space size="small">
          <Button
            type="text"
            size="small"
            icon={<EditOutlined />}
            onClick={() => { setEditRecord(record); setModalOpen(true); }}
          />
          {!record.isCurrent && academicYear?.isCurrent && (
            <Button
              type="text"
              size="small"
              icon={<CheckCircleOutlined />}
              style={{ color: '#52C41A' }}
              onClick={() => handleSetCurrent(record.id)}
              title="Set as current term"
            />
          )}
          {!record.isCurrent && (
            <Popconfirm
              title="Delete this term?"
              description="This action cannot be undone."
              onConfirm={() => handleDelete(record.id)}
            >
              <Button type="text" size="small" danger icon={<DeleteOutlined />} />
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <Drawer
      title={
        <div>
          <div>Manage Terms</div>
          <Text type="secondary" style={{ fontSize: 13 }}>
            {academicYear?.yearName ?? ''}
          </Text>
        </div>
      }
      open={open}
      onClose={onClose}
      width={720}
      extra={
        existingTermNumbers.length < 4 ? (
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => { setEditRecord(null); setModalOpen(true); }}
          >
            Add Term
          </Button>
        ) : null
      }
    >
      {academicYear && (
        <div style={{ marginBottom: 16, padding: '12px 16px', background: '#F6F8FA', borderRadius: 6 }}>
          <Space size="large">
            <div>
              <Text type="secondary" style={{ fontSize: 12 }}>Year Period</Text>
              <div>
                <Text strong>
                  {dayjs(academicYear.startDate).format('DD MMM YYYY')} — {dayjs(academicYear.endDate).format('DD MMM YYYY')}
                </Text>
              </div>
            </div>
            <div>
              <Text type="secondary" style={{ fontSize: 12 }}>Terms</Text>
              <div><Text strong>{existingTermNumbers.length} / 4</Text></div>
            </div>
            <div>
              <Text type="secondary" style={{ fontSize: 12 }}>Status</Text>
              <div>
                <Tag color={academicYear.isCurrent ? 'green' : 'default'}>
                  {academicYear.isCurrent ? 'Current Year' : 'Inactive'}
                </Tag>
              </div>
            </div>
          </Space>
        </div>
      )}

      <Table<ITerm>
        dataSource={(terms ?? []).sort((a, b) => a.termNumber - b.termNumber)}
        columns={columns}
        rowKey="id"
        loading={isPending}
        pagination={false}
        size="middle"
        locale={{
          emptyText: (
            <Empty
              description="No terms configured for this academic year"
              image={Empty.PRESENTED_IMAGE_SIMPLE}
            />
          ),
        }}
      />

      {academicYear && (
        <TermFormModal
          open={modalOpen}
          onClose={handleModalClose}
          editRecord={editRecord}
          academicYearId={academicYear.id}
          academicYearStartDate={academicYear.startDate}
          academicYearEndDate={academicYear.endDate}
          existingTermNumbers={editRecord
            ? existingTermNumbers.filter(n => n !== editRecord.termNumber)
            : existingTermNumbers
          }
        />
      )}
    </Drawer>
  );
}

const TermManagementDrawer: React.FC<TermManagementDrawerProps> = (props) => {
  return (
    <TermProvider>
      <TermManagementContent {...props} />
    </TermProvider>
  );
};

export default TermManagementDrawer;
