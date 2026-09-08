'use client';

import React, { useEffect, useState } from 'react';
import { Modal, Form, Input, InputNumber, Select, Switch, message, Typography } from 'antd';
import { z } from 'zod';
import { useWorkflowStepActions } from '@/providers/workflow/workflow-steps';
import { useWorkflowExtensionActions } from '@/providers/workflow/workflow-extensions';
import type { IWorkflowStep, IWorkflowExtensionCatalog } from '@/providers/workflow/shared/interfaces';
import { WorkflowActionType } from '@/providers/workflow/shared/interfaces';
import { WorkflowUserSelect } from './WorkflowUserSelect';

const { Text } = Typography;

const optionalKey = z.string().max(100).optional();

const stepSchema = z.object({
  workflowDefinitionId: z.string().min(1),
  stepOrder: z.number().min(1, 'Step order must be at least 1'),
  name: z.string().min(1, 'Name is required').max(200),
  description: z.string().max(500).optional(),
  assignedRole: z.string({ error: 'Assigned role is required' }).min(1, 'Assigned role is required').max(100),
  actionType: z.number({ error: 'Action type is required' }).min(1, 'Action type is required'),
  isTerminal: z.boolean(),
  isCommentRequired: z.boolean(),
  isOptional: z.boolean(),
  nextStepOnApprove: z.number().optional(),
  nextStepOnReject: z.number().optional(),
  assignedUserId: z.number().optional(),
  slaHours: z.number().min(1).optional(),
  guardKey: optionalKey,
  entryEffectKey: optionalKey,
  exitEffectKey: optionalKey,
  decisionSchemaKey: optionalKey,
});

interface WorkflowStepFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IWorkflowStep | null;
  definitionId: string;
  /** The definition's entity type — decides which guards / effects / schemas are offered. */
  entityType?: number;
  nextStepOrder: number;
}

const actionTypeOptions = [
  { value: WorkflowActionType.Submit, label: 'Submit' },
  { value: WorkflowActionType.Review, label: 'Review' },
  { value: WorkflowActionType.Approve, label: 'Approve' },
];

const roleOptions = [
  { value: 'Admin', label: 'Admin' },
  { value: 'Principal', label: 'Principal' },
  { value: 'VicePrincipal', label: 'Vice Principal' },
  { value: 'HOD', label: 'HOD' },
  { value: 'Teacher', label: 'Teacher' },
  { value: 'Finance', label: 'Finance' },
  { value: 'AdmissionsOfficer', label: 'Admissions Officer' },
];

const EXTENSION_KEYS = ['guardKey', 'entryEffectKey', 'exitEffectKey', 'decisionSchemaKey'] as const;
const EXTENSION_CLEAR: Record<(typeof EXTENSION_KEYS)[number], string> = {
  guardKey: 'clearGuardKey',
  entryEffectKey: 'clearEntryEffectKey',
  exitEffectKey: 'clearExitEffectKey',
  decisionSchemaKey: 'clearDecisionSchemaKey',
};

export const WorkflowStepFormModal: React.FC<WorkflowStepFormModalProps> = ({
  open,
  onClose,
  editRecord,
  definitionId,
  entityType,
  nextStepOrder,
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useWorkflowStepActions();
  const { getAvailableAsync } = useWorkflowExtensionActions();
  const [loading, setLoading] = React.useState(false);
  const [catalog, setCatalog] = useState<IWorkflowExtensionCatalog | undefined>();
  const isEdit = !!editRecord;
  // The user picker is steered to users holding the currently-selected role.
  const selectedRole = Form.useWatch('assignedRole', form);

  // WF-30/31/32: guards, effects and decision schemas are registered per entity
  // type; the dropdowns only ever offer keys the engine can resolve.
  useEffect(() => {
    if (!open || entityType == null) return;
    let active = true;
    getAvailableAsync(entityType).then((c) => { if (active) setCatalog(c); });
    return () => { active = false; };
  }, [open, entityType, getAvailableAsync]);

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({
          name: editRecord.name,
          description: editRecord.description,
          assignedRole: editRecord.assignedRole,
          actionType: editRecord.actionType,
          isTerminal: editRecord.isTerminal,
          isCommentRequired: editRecord.isCommentRequired,
          isOptional: editRecord.isOptional,
          nextStepOnApprove: editRecord.nextStepOnApprove,
          nextStepOnReject: editRecord.nextStepOnReject,
          slaHours: editRecord.slaHours,
          assignedUserId: editRecord.assignedUserId,
          guardKey: editRecord.guardKey,
          entryEffectKey: editRecord.entryEffectKey,
          exitEffectKey: editRecord.exitEffectKey,
          decisionSchemaKey: editRecord.decisionSchemaKey,
        });
      } else {
        form.resetFields();
        form.setFieldsValue({
          stepOrder: nextStepOrder,
          isTerminal: false,
          isCommentRequired: false,
          isOptional: false,
        });
      }
    }
  }, [open, editRecord, form, nextStepOrder]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();

      if (isEdit) {
        const updateData: Record<string, unknown> = {};
        if (values.name !== undefined) updateData.name = values.name;
        if (values.description !== undefined) updateData.description = values.description;
        if (values.assignedRole !== undefined) updateData.assignedRole = values.assignedRole;
        if (values.actionType !== undefined) updateData.actionType = values.actionType;
        if (values.isTerminal !== undefined) updateData.isTerminal = values.isTerminal;
        if (values.isCommentRequired !== undefined) updateData.isCommentRequired = values.isCommentRequired;
        if (values.isOptional !== undefined) updateData.isOptional = values.isOptional;
        if (values.nextStepOnApprove !== undefined) updateData.nextStepOnApprove = values.nextStepOnApprove;
        else if (editRecord?.nextStepOnApprove) updateData.clearNextStepOnApprove = true;
        if (values.nextStepOnReject !== undefined) updateData.nextStepOnReject = values.nextStepOnReject;
        else if (editRecord?.nextStepOnReject) updateData.clearNextStepOnReject = true;
        if (values.slaHours !== undefined) updateData.slaHours = values.slaHours;
        else if (editRecord?.slaHours) updateData.clearSlaHours = true;
        if (values.assignedUserId !== undefined) updateData.assignedUserId = values.assignedUserId;
        else if (editRecord?.assignedUserId) updateData.clearAssignedUserId = true;
        for (const key of EXTENSION_KEYS) {
          const v = values[key];
          if (v) updateData[key] = v;
          else if (editRecord?.[key]) updateData[EXTENSION_CLEAR[key]] = true;
        }

        setLoading(true);
        await updateAsync(editRecord!.id, updateData);
      } else {
        values.workflowDefinitionId = definitionId;
        const result = stepSchema.safeParse(values);
        if (!result.success) {
          const fieldErrors = result.error.issues.map(err => ({
            name: err.path as string[],
            errors: [err.message],
          }));
          form.setFields(fieldErrors);
          return;
        }
        setLoading(true);
        await createAsync(result.data);
      }
      message.success(`Step ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      // Axios interceptor handles ABP error display
    } finally {
      setLoading(false);
    }
  };

  const toOptions = (items?: { key: string; displayName: string }[]) =>
    (items ?? []).map((i) => ({ value: i.key, label: `${i.displayName} (${i.key})` }));

  const guardOptions = toOptions(catalog?.guards);
  const effectOptions = toOptions(catalog?.effects);
  const schemaOptions = toOptions(catalog?.decisionSchemas);
  const noCatalog = entityType == null || !catalog;

  return (
    <Modal
      title={isEdit ? 'Edit Step' : 'New Step'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
      width={640}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Step Name" name="name" rules={[{ required: true }]}>
          <Input placeholder="e.g. Principal Review" maxLength={200} />
        </Form.Item>
        {!isEdit && (
          <Form.Item label="Step Order" name="stepOrder" rules={[{ required: true }]}>
            <InputNumber min={1} style={{ width: '100%' }} />
          </Form.Item>
        )}
        <Form.Item label="Assigned Role" name="assignedRole" rules={[{ required: true }]}>
          <Select options={roleOptions} placeholder="Select role" />
        </Form.Item>
        <Form.Item
          label="Action Type"
          name="actionType"
          rules={[{ required: true }]}
          extra="Submit, Review and Approve all pass the item forward; the label tells the actor what their action means."
        >
          <Select options={actionTypeOptions} placeholder="Select action" />
        </Form.Item>
        <Form.Item label="Description" name="description">
          <Input.TextArea rows={2} maxLength={500} placeholder="Optional description" />
        </Form.Item>
        <div style={{ display: 'flex', gap: 16 }}>
          <Form.Item label="Terminal Step" name="isTerminal" valuePropName="checked" style={{ flex: 1 }}>
            <Switch />
          </Form.Item>
          <Form.Item label="Comment Required" name="isCommentRequired" valuePropName="checked" style={{ flex: 1 }}>
            <Switch />
          </Form.Item>
          <Form.Item
            label="Optional (can be waived)"
            name="isOptional"
            valuePropName="checked"
            style={{ flex: 1 }}
            tooltip="The actor may skip this step with a mandatory reason, e.g. an interview a grade does not require."
          >
            <Switch />
          </Form.Item>
        </div>
        <div style={{ display: 'flex', gap: 16 }}>
          <Form.Item label="Next Step on Approve" name="nextStepOnApprove" style={{ flex: 1 }}>
            <InputNumber min={1} style={{ width: '100%' }} placeholder="Auto (next)" />
          </Form.Item>
          <Form.Item label="Next Step on Reject" name="nextStepOnReject" style={{ flex: 1 }}>
            <InputNumber min={1} style={{ width: '100%' }} placeholder="Terminal reject" />
          </Form.Item>
        </div>
        <div style={{ display: 'flex', gap: 16 }}>
          <Form.Item label="SLA Hours" name="slaHours" style={{ flex: 1 }}>
            <InputNumber min={1} style={{ width: '100%' }} placeholder="Optional deadline (hours)" />
          </Form.Item>
          <Form.Item label="Assigned User" name="assignedUserId" style={{ flex: 1 }}>
            <WorkflowUserSelect
              roleFilter={selectedRole}
              placeholder="Optional — pin to a specific user"
            />
          </Form.Item>
        </div>

        <Text strong style={{ display: 'block', marginBottom: 8 }}>What this step checks and does</Text>
        {noCatalog && (
          <Text type="secondary" style={{ display: 'block', marginBottom: 8 }}>
            Loading the options available for this entity type…
          </Text>
        )}
        <Form.Item
          label="Exit criteria (guard)"
          name="guardKey"
          tooltip="The step cannot be passed until this is true of the record. Management can override with a recorded reason."
        >
          <Select options={guardOptions} placeholder="None" allowClear disabled={noCatalog} showSearch optionFilterProp="label" />
        </Form.Item>
        <div style={{ display: 'flex', gap: 16 }}>
          <Form.Item label="Effect on entering" name="entryEffectKey" style={{ flex: 1 }}>
            <Select options={effectOptions} placeholder="None" allowClear disabled={noCatalog} showSearch optionFilterProp="label" />
          </Form.Item>
          <Form.Item label="Effect on leaving" name="exitEffectKey" style={{ flex: 1 }}>
            <Select options={effectOptions} placeholder="None" allowClear disabled={noCatalog} showSearch optionFilterProp="label" />
          </Form.Item>
        </div>
        <Form.Item
          label="Decision fields"
          name="decisionSchemaKey"
          tooltip="Fields the actor must fill in when passing this step, e.g. the approved amount on a terminal approval."
        >
          <Select options={schemaOptions} placeholder="None" allowClear disabled={noCatalog} showSearch optionFilterProp="label" />
        </Form.Item>
      </Form>
    </Modal>
  );
};
