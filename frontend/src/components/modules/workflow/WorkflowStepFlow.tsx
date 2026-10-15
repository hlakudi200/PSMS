'use client';

import React, { useMemo, useState } from 'react';
import { Button, Empty, Popconfirm, Space, Spin, Tag, Tooltip, Typography } from 'antd';
import {
  DeleteOutlined, EditOutlined, HolderOutlined, PlayCircleOutlined,
  CheckCircleOutlined, RollbackOutlined, ForwardOutlined,
} from '@ant-design/icons';
import type { IWorkflowStep } from '@/providers/workflow/shared/interfaces';
import { WorkflowActionTypeLabels } from '@/providers/workflow/shared/interfaces';

const { Text } = Typography;

/**
 * A workflow definition drawn as the flow it is, and reordered by dragging.
 *
 * It was a table with up and down arrows, which told you the order of the rows
 * and nothing about the shape: which steps skip themselves, which block until
 * their work is done, where a rejection goes. A workflow is a diagram, and a
 * person configuring one is picturing a diagram — so this draws it.
 *
 * Dragging uses the browser's own drag events rather than a library: a list of
 * four to six cards does not justify a dependency.
 */
export default function WorkflowStepFlow({
  steps,
  loading,
  onEdit,
  onDelete,
  onReorder,
}: {
  steps: IWorkflowStep[];
  loading?: boolean;
  onEdit: (step: IWorkflowStep) => void;
  onDelete: (stepId: string) => void;
  onReorder: (orderedIds: string[]) => Promise<void> | void;
}) {
  const ordered = useMemo(
    () => [...steps].sort((a, b) => a.stepOrder - b.stepOrder),
    [steps]
  );

  const [dragFrom, setDragFrom] = useState<number | null>(null);
  const [dragOver, setDragOver] = useState<number | null>(null);
  const [saving, setSaving] = useState(false);

  const byOrder = (n?: number) => ordered.find((s) => s.stepOrder === n);

  const drop = async (to: number) => {
    const from = dragFrom;
    setDragFrom(null);
    setDragOver(null);
    if (from == null || from === to) return;

    const next = [...ordered];
    const [moved] = next.splice(from, 1);
    next.splice(to, 0, moved);

    setSaving(true);
    try {
      await onReorder(next.map((s) => s.id));
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div style={{ padding: 40, textAlign: 'center' }}><Spin /></div>;

  if (ordered.length === 0) {
    return <Empty description="No steps yet — add the first one" image={Empty.PRESENTED_IMAGE_SIMPLE} />;
  }

  return (
    <div style={{ position: 'relative', paddingBlock: 8 }}>
      {saving && (
        <div style={{
          position: 'absolute', inset: 0, zIndex: 5,
          background: 'rgba(255,255,255,0.55)', display: 'flex',
          alignItems: 'center', justifyContent: 'center',
        }}>
          <Spin tip="Reordering…" />
        </div>
      )}

      <Marker icon={<PlayCircleOutlined />} label="An application arrives" />
      <Connector />

      {ordered.map((step, i) => {
        const isDragging = dragFrom === i;
        const isTarget = dragOver === i && dragFrom !== null && dragFrom !== i;
        const rejectsTo = byOrder(step.nextStepOnReject);
        const approvesTo = byOrder(step.nextStepOnApprove);

        return (
          <div key={step.id}>
            {i > 0 && <Connector />}

            <div
              draggable
              onDragStart={() => setDragFrom(i)}
              onDragEnd={() => { setDragFrom(null); setDragOver(null); }}
              onDragOver={(e) => { e.preventDefault(); setDragOver(i); }}
              onDrop={(e) => { e.preventDefault(); drop(i); }}
              style={{
                position: 'relative',
                margin: '0 auto',
                maxWidth: 560,
                border: `1.5px solid ${isTarget ? '#1677ff' : step.isTerminal ? '#cf1322' : '#d9d9d9'}`,
                borderRadius: 8,
                background: '#fff',
                padding: '12px 14px',
                opacity: isDragging ? 0.4 : 1,
                boxShadow: isTarget ? '0 0 0 3px rgba(22,119,255,0.12)' : '0 1px 2px rgba(0,0,0,0.04)',
                cursor: 'grab',
                transition: 'border-color .15s, box-shadow .15s',
              }}
            >
              <Space style={{ width: '100%', justifyContent: 'space-between' }} align="start">
                <Space align="start">
                  <Tooltip title="Drag to reorder">
                    <HolderOutlined style={{ color: '#bfbfbf', marginTop: 4, cursor: 'grab' }} />
                  </Tooltip>
                  <div>
                    <Space size={8} wrap>
                      <span style={{
                        display: 'inline-flex', alignItems: 'center', justifyContent: 'center',
                        width: 22, height: 22, borderRadius: '50%',
                        background: step.isTerminal ? '#cf1322' : '#1677ff',
                        color: '#fff', fontSize: 12, fontWeight: 600,
                      }}>{step.stepOrder}</span>
                      <Text strong>{step.name}</Text>
                      <Tag>{step.assignedRole}</Tag>
                      <Text type="secondary" style={{ fontSize: 12 }}>
                        {WorkflowActionTypeLabels[step.actionType] ?? step.actionType}
                      </Text>
                    </Space>

                    {step.description && (
                      <div style={{ marginTop: 4 }}>
                        <Text type="secondary" style={{ fontSize: 12 }}>{step.description}</Text>
                      </div>
                    )}

                    {/* What the engine will actually do here, rather than a row
                        of em-dashes in columns nobody can interpret. */}
                    <Space size={[6, 4]} wrap style={{ marginTop: 8 }}>
                      {step.isTerminal && <Tag color="volcano">Ends the workflow</Tag>}
                      {step.isOptional && <Tag color="gold">Can be waived</Tag>}
                      {step.skipWhenKey && (
                        <Tooltip title={`Skips itself when: ${step.skipWhenKey}`}>
                          <Tag color="gold">Skips itself</Tag>
                        </Tooltip>
                      )}
                      {step.guardKey && (
                        <Tooltip title={`Cannot be passed until: ${step.guardKey}`}>
                          <Tag color="blue">Has exit criteria</Tag>
                        </Tooltip>
                      )}
                      {step.decisionSchemaKey && (
                        <Tooltip title={`Asks for: ${step.decisionSchemaKey}`}>
                          <Tag color="purple">Asks for a decision</Tag>
                        </Tooltip>
                      )}
                      {step.isCommentRequired && <Tag>Comment required</Tag>}
                      {step.slaHours != null && <Tag>{step.slaHours}h SLA</Tag>}
                      {step.assignedUserId != null && <Tag color="cyan">One named person</Tag>}
                    </Space>

                    {(rejectsTo || approvesTo) && (
                      <Space size={14} wrap style={{ marginTop: 8 }}>
                        {approvesTo && (
                          <Text type="secondary" style={{ fontSize: 12 }}>
                            <ForwardOutlined /> passes to <b>{approvesTo.name}</b>
                          </Text>
                        )}
                        {rejectsTo && (
                          <Text type="secondary" style={{ fontSize: 12 }}>
                            <RollbackOutlined /> sends back to <b>{rejectsTo.name}</b>
                          </Text>
                        )}
                      </Space>
                    )}
                  </div>
                </Space>

                <Space>
                  <Button size="small" icon={<EditOutlined />} onClick={() => onEdit(step)} />
                  <Popconfirm
                    title="Delete this step?"
                    description="Applications sitting on it will need moving by hand."
                    onConfirm={() => onDelete(step.id)}
                  >
                    <Button size="small" icon={<DeleteOutlined />} danger />
                  </Popconfirm>
                </Space>
              </Space>
            </div>
          </div>
        );
      })}

      <Connector />
      <Marker icon={<CheckCircleOutlined />} label="Decided" />
    </div>
  );
}

function Connector() {
  return (
    <div style={{ display: 'flex', justifyContent: 'center', height: 26 }}>
      <div style={{ width: 2, background: '#d9d9d9', position: 'relative' }}>
        <div style={{
          position: 'absolute', bottom: -1, left: -4,
          width: 0, height: 0,
          borderLeft: '5px solid transparent',
          borderRight: '5px solid transparent',
          borderTop: '6px solid #d9d9d9',
        }} />
      </div>
    </div>
  );
}

function Marker({ icon, label }: { icon: React.ReactNode; label: string }) {
  return (
    <>
      <div style={{ textAlign: "center" }}>
        <Tag style={{ borderRadius: 999, padding: "2px 12px", color: "#8c8c8c" }}>
          {icon} {label}
        </Tag>
      </div>
    </>
  );
}
