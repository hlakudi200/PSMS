'use client';

import React, { useEffect, useState } from 'react';
import { Modal, Form, Select, message, Typography } from 'antd';
import { z } from 'zod';
import { useWorkflowInstanceActions } from '@/providers/workflow/workflow-instances';
import {
  WorkflowEntityType,
  WorkflowEntityTypeLabels,
  WORKFLOW_SUPPORTED_ENTITY_TYPES,
} from '@/providers/workflow/shared/interfaces';
import type { IWorkflowDefinitionList } from '@/providers/workflow/shared/interfaces';
import { getAxiosInstance } from '@/utils/axios-instance';

const { Text } = Typography;

const startSchema = z.object({
  entityType: z.number({ error: 'Entity type is required' }).min(1, 'Entity type is required'),
  entityId: z.string({ error: 'Please select an entity' }).min(1, 'Please select an entity'),
  workflowDefinitionId: z.string().optional(),
});

interface StartWorkflowModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  entityType?: number;
  entityId?: string;
}

// WF-35: only types with a definition, summary and write-back handler are offered.
const entityTypeOptions = Object.entries(WorkflowEntityTypeLabels)
  .filter(([value]) => WORKFLOW_SUPPORTED_ENTITY_TYPES.includes(Number(value)))
  .map(([value, label]) => ({ value: Number(value), label }));

interface EntityOption {
  value: string;
  label: string;
}

type ListItem = Record<string, unknown>;

/**
 * WF-35: how to list and label each entity type, and which records are in a
 * state a workflow can legitimately start from. Submit normally starts the
 * workflow automatically (WF-36); this modal is the admin fallback, so it only
 * offers submitted / in-review records — never drafts or finished ones.
 * Statuses mirror the backend enums (serialised as numbers).
 */
const entityApiMap: Record<number, { endpoint: string; labelFn: (item: ListItem) => string; startable: (item: ListItem) => boolean }> = {
  [WorkflowEntityType.Application]: {
    endpoint: '/api/services/app/Application/GetAll?MaxResultCount=200',
    labelFn: (item) => `${item.fullName ?? `${item.firstName ?? ''} ${item.lastName ?? ''}`.trim()} (${item.applicationNumber ?? item.id})`,
    // Submitted, UnderReview, DocumentsRequired, InterviewScheduled, AssessmentScheduled, UnderConsideration
    startable: (item) => [2, 4, 5, 6, 7, 8].includes(Number(item.status)),
  },
  [WorkflowEntityType.Report]: {
    endpoint: '/api/services/app/Report/GetAll?MaxResultCount=200',
    labelFn: (item) => `${item.studentName ?? 'Student'} — ${item.className ?? ''} ${item.termName ?? ''}`.trim(),
    startable: (item) => Number(item.status) === 3, // PendingApproval
  },
  [WorkflowEntityType.FeeWaiver]: {
    endpoint: '/api/services/app/FeeWaiver/GetAll?MaxResultCount=200',
    labelFn: (item) => `${item.studentName ?? 'Student'} — R${item.requestedAmount}`,
    startable: (item) => [2, 3].includes(Number(item.status)),
  },
  [WorkflowEntityType.StudentTransfer]: {
    endpoint: '/api/services/app/StudentTransfer/GetAll?MaxResultCount=200',
    labelFn: (item) => `${item.transferNumber ?? ''} — ${item.studentName ?? 'Student'} (${item.transferType === 1 ? 'In' : 'Out'})`,
    startable: (item) => [2, 3].includes(Number(item.status)),
  },
  [WorkflowEntityType.Disciplinary]: {
    endpoint: '/api/services/app/DisciplinaryCase/GetAll?MaxResultCount=200',
    labelFn: (item) => `${item.caseNumber ?? ''} — ${item.studentName ?? 'Student'}`,
    // Reported, UnderInvestigation, HearingScheduled, HearingCompleted
    startable: (item) => [2, 3, 4, 5].includes(Number(item.status)),
  },
  [WorkflowEntityType.StaffLeave]: {
    endpoint: '/api/services/app/StaffLeaveRequest/GetAll?MaxResultCount=200',
    labelFn: (item) => `${item.leaveNumber ?? ''} — ${item.userName ?? 'Staff'} (${String(item.startDate ?? '').substring(0, 10)})`,
    startable: (item) => [2, 3].includes(Number(item.status)), // Submitted, HODApproved
  },
  [WorkflowEntityType.FieldTrip]: {
    endpoint: '/api/services/app/FieldTrip/GetAll?MaxResultCount=200',
    labelFn: (item) => `${item.tripName ?? 'Trip'} — ${item.destination ?? ''}`,
    startable: (item) => [2, 3].includes(Number(item.status)),
  },
  [WorkflowEntityType.ExpenseRequest]: {
    endpoint: '/api/services/app/ExpenseRequest/GetAll?MaxResultCount=200',
    labelFn: (item) => `${item.requestNumber ?? ''} — R${item.amount} (${String(item.description ?? '').substring(0, 30)})`,
    startable: (item) => [2, 3].includes(Number(item.status)),
  },
};

export const StartWorkflowModal: React.FC<StartWorkflowModalProps> = ({
  open,
  onClose,
  entityType,
  entityId,
}) => {
  const [form] = Form.useForm();
  const { startAsync } = useWorkflowInstanceActions();
  const [loading, setLoading] = React.useState(false);
  const [definitions, setDefinitions] = useState<IWorkflowDefinitionList[]>([]);
  const [entities, setEntities] = useState<EntityOption[]>([]);
  const [loadingEntities, setLoadingEntities] = useState(false);
  const [loadingDefs, setLoadingDefs] = useState(false);

  const selectedEntityType = Form.useWatch('entityType', form);

  useEffect(() => {
    if (open) {
      form.resetFields();
      setDefinitions([]);
      setEntities([]);
      if (entityType) form.setFieldsValue({ entityType });
      if (entityId) form.setFieldsValue({ entityId });
    }
  }, [open, entityType, entityId, form]);

  // Raw axios for cross-module lookups — these are read-only selects, not managed state
  useEffect(() => {
    if (!selectedEntityType || !open) return;
    setLoadingDefs(true);
    const instance = getAxiosInstance();
    instance
      .get(`/api/services/app/WorkflowDefinition/GetAll?EntityType=${selectedEntityType}&MaxResultCount=50`)
      .then((response) => {
        // Only an active definition can be started (the server enforces this too).
        const items = (response.data.result.items ?? []) as IWorkflowDefinitionList[];
        setDefinitions(items.filter((d) => d.isActive));
      })
      .catch(() => setDefinitions([]))
      .finally(() => setLoadingDefs(false));
  }, [selectedEntityType, open]);

  useEffect(() => {
    if (!selectedEntityType || !open) return;
    // A pre-selected entity (opened from a record) needs no listing.
    if (entityId) { setEntities([{ value: entityId, label: entityId }]); return; }
    setLoadingEntities(true);
    const instance = getAxiosInstance();

    const config = entityApiMap[selectedEntityType];
    if (!config) {
      setEntities([]);
      setLoadingEntities(false);
      return;
    }

    instance
      .get(config.endpoint)
      .then((response) => {
        const items: ListItem[] = response.data.result.items ?? response.data.result ?? [];
        setEntities(
          items
            .filter(config.startable)
            .map((item) => ({ value: item.id as string, label: config.labelFn(item) }))
        );
      })
      .catch(() => setEntities([]))
      .finally(() => setLoadingEntities(false));
  }, [selectedEntityType, open, entityId]);

  const definitionOptions = definitions.map((d) => ({
    value: d.id,
    label: `${d.name} (v${d.version})${d.isActive ? ' — Active' : ''}`,
  }));

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      const result = startSchema.safeParse(values);
      if (!result.success) {
        const fieldErrors = result.error.issues.map(err => ({
          name: err.path as string[],
          errors: [err.message],
        }));
        form.setFields(fieldErrors);
        return;
      }
      setLoading(true);
      await startAsync(result.data);
      message.success('Workflow started successfully');
      onClose(true);
    } catch {
      // Axios interceptor handles ABP error display
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title="Start Workflow"
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
      width={520}
    >
      <Text type="secondary" style={{ display: 'block', marginTop: 12 }}>
        Submitting a record normally starts its workflow automatically. Use this only for a
        submitted record that has none, for example after a workflow was cancelled.
      </Text>
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Entity Type" name="entityType" rules={[{ required: true }]}>
          <Select
            options={entityTypeOptions}
            placeholder="Select entity type"
            disabled={!!entityType}
            onChange={() => {
              form.setFieldsValue({ entityId: undefined, workflowDefinitionId: undefined });
            }}
          />
        </Form.Item>

        <Form.Item label="Entity" name="entityId" rules={[{ required: true, message: 'Please select an entity' }]}>
          <Select
            options={entities}
            placeholder={loadingEntities ? 'Loading...' : 'Select a submitted record'}
            loading={loadingEntities}
            disabled={!!entityId || !selectedEntityType}
            showSearch
            filterOption={(input, option) =>
              (option?.label as string)?.toLowerCase().includes(input.toLowerCase()) ?? false
            }
            notFoundContent={
              !selectedEntityType
                ? <Text type="secondary">Select an entity type first</Text>
                : loadingEntities
                  ? <Text type="secondary">Loading...</Text>
                  : <Text type="secondary">No submitted records without a workflow</Text>
            }
          />
        </Form.Item>

        <Form.Item label="Workflow Definition" name="workflowDefinitionId">
          <Select
            options={definitionOptions}
            placeholder={loadingDefs ? 'Loading...' : 'Auto — uses active definition'}
            loading={loadingDefs}
            disabled={!selectedEntityType}
            allowClear
            notFoundContent={
              !selectedEntityType
                ? <Text type="secondary">Select an entity type first</Text>
                : <Text type="secondary">No active definition — seed or activate one first</Text>
            }
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};
