'use client';

import React, { useEffect, useState } from 'react';
import { Modal, Form, Select, message, Typography } from 'antd';
import { z } from 'zod';
import { useWorkflowInstanceActions } from '@/providers/workflow/workflow-instances';
import { WorkflowEntityTypeLabels } from '@/providers/workflow/shared/interfaces';
import type { IWorkflowDefinitionList } from '@/providers/workflow/shared/interfaces';
import { getAxiosInstance } from '@/utils/axios-instance';

const { Text } = Typography;

const startSchema = z.object({
  entityType: z.number({ required_error: 'Entity type is required' }).min(1, 'Entity type is required'),
  entityId: z.string({ required_error: 'Please select an entity' }).min(1, 'Please select an entity'),
  workflowDefinitionId: z.string().nullish(),
});

interface StartWorkflowModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  entityType?: number;
  entityId?: string;
}

const entityTypeOptions = Object.entries(WorkflowEntityTypeLabels).map(([value, label]) => ({
  value: Number(value),
  label,
}));

interface EntityOption {
  value: string;
  label: string;
}

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
        setDefinitions(response.data.result.items ?? []);
      })
      .catch(() => setDefinitions([]))
      .finally(() => setLoadingDefs(false));
  }, [selectedEntityType, open]);

  useEffect(() => {
    if (!selectedEntityType || !open) return;
    setLoadingEntities(true);
    const instance = getAxiosInstance();

    // Map entity type to the correct API endpoint and label field
    const entityApiMap: Record<number, { endpoint: string; labelFn: (item: any) => string }> = {
      1: {
        endpoint: '/api/services/app/Application/GetAll?MaxResultCount=100',
        labelFn: (item) => `${item.applicantFirstName ?? ''} ${item.applicantLastName ?? ''} (${item.applicationNumber ?? item.id})`.trim(),
      },
      2: {
        endpoint: '/api/services/app/Report/GetAll?MaxResultCount=100',
        labelFn: (item) => item.reportName ?? item.title ?? `Report ${item.id?.substring(0, 8)}`,
      },
      3: {
        endpoint: '/api/services/app/FeeStructure/GetAll?MaxResultCount=100',
        labelFn: (item) => item.name ?? `Fee Waiver ${item.id?.substring(0, 8)}`,
      },
      4: {
        endpoint: '/api/services/app/Attendance/GetAll?MaxResultCount=100',
        labelFn: (item) => `${item.studentName ?? 'Student'} — ${item.attendanceDate ?? item.id?.substring(0, 8)}`,
      },
      5: {
        endpoint: '/api/services/app/LearningMaterial/GetAll?MaxResultCount=100',
        labelFn: (item) => item.title ?? item.name ?? `Material ${item.id?.substring(0, 8)}`,
      },
      6: {
        endpoint: '/api/services/app/Student/GetAll?MaxResultCount=100',
        labelFn: (item) => `${item.fullName ?? item.firstName ?? ''} ${item.lastName ?? ''} (Transfer)`.trim(),
      },
      7: {
        endpoint: '/api/services/app/Student/GetAll?MaxResultCount=100',
        labelFn: (item) => `${item.fullName ?? item.firstName ?? ''} ${item.lastName ?? ''} (Disciplinary)`.trim(),
      },
    };

    const config = entityApiMap[selectedEntityType];
    if (!config) {
      setEntities([]);
      setLoadingEntities(false);
      return;
    }

    instance
      .get(config.endpoint)
      .then((response) => {
        const items = response.data.result.items ?? response.data.result ?? [];
        setEntities(
          items.map((item: any) => ({
            value: item.id,
            label: config.labelFn(item),
          }))
        );
      })
      .catch(() => setEntities([]))
      .finally(() => setLoadingEntities(false));
  }, [selectedEntityType, open]);

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

  const definitionOptions = definitions.map((d) => ({
    value: d.id,
    label: `${d.name} (v${d.version})${d.isActive ? ' — Active' : ''}`,
  }));

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
            placeholder={loadingEntities ? 'Loading...' : 'Select an entity'}
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
                  : <Text type="secondary">No entities found</Text>
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
                : <Text type="secondary">No definitions found</Text>
            }
          />
        </Form.Item>
      </Form>
    </Modal>
  );
};
