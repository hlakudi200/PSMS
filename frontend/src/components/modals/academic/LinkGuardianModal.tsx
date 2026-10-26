'use client';

import React, { useEffect, useState } from 'react';
import { Alert, Checkbox, Form, Modal, Select, Space, Typography, message } from 'antd';
import { z } from 'zod';
import { ParentProvider, useParentActions, useParentState } from '@/providers/academic/parents';
import { useStudentParentActions } from '@/providers/academic/student_parents';

const { Text } = Typography;

/** The backend RelationshipType, which starts at 1. */
export const RELATIONSHIP_OPTIONS = [
  { value: 1, label: 'Father' },
  { value: 2, label: 'Mother' },
  { value: 3, label: 'Guardian' },
  { value: 4, label: 'Stepfather' },
  { value: 5, label: 'Stepmother' },
  { value: 6, label: 'Grandparent' },
  { value: 7, label: 'Sibling' },
  { value: 8, label: 'Other' },
];

const schema = z.object({
  parentId: z.string({ error: 'Choose the parent or guardian to link.' }).min(1, 'Choose the parent or guardian to link.'),
  relationshipType: z.number({ error: 'Say how they are related to the learner.' }).int().min(1).max(8),
  isPrimaryContact: z.boolean(),
  isFinanciallyResponsible: z.boolean(),
  canPickupStudent: z.boolean(),
  livesWithStudent: z.boolean(),
});

type FormValues = z.infer<typeof schema>;

/**
 * #437. Linking a parent or guardian to a learner.
 *
 * A learner with nobody linked has no one the school can contact, and no one
 * who can see their report cards. The office already had the parent records
 * and the endpoint; no screen joined the two. The parent has to exist first;
 * new ones are added on the Parents page.
 */
function LinkGuardianForm({
  open, studentId, studentName, onClose, onLinked,
}: {
  open: boolean;
  studentId: string;
  studentName?: string;
  onClose: () => void;
  onLinked: () => void;
}) {
  const [form] = Form.useForm<FormValues>();
  const { parents, isPending: isSearching } = useParentState();
  const { getAllAsync } = useParentActions();
  const { linkAsync } = useStudentParentActions();
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | undefined>();

  useEffect(() => {
    if (!open) return;
    setError(undefined);
    form.resetFields();
    form.setFieldsValue({
      isPrimaryContact: true, isFinanciallyResponsible: false, canPickupStudent: true, livesWithStudent: false,
    });
    Promise.resolve(getAllAsync({ maxResultCount: 20, sorting: 'LastName ASC' })).catch(() => undefined);
  }, [open]);

  const search = (keyword: string) => {
    Promise.resolve(getAllAsync({ keyword: keyword.trim() || undefined, maxResultCount: 20, sorting: 'LastName ASC' }))
      .catch(() => undefined);
  };

  const link = async () => {
    const parsed = schema.safeParse(form.getFieldsValue(true));
    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message);
      return;
    }
    setSaving(true);
    setError(undefined);
    try {
      await linkAsync({ studentId, ...parsed.data });
      message.success('Linked.');
      onLinked();
      onClose();
    } catch (e) {
      const abp = (e as { response?: { data?: { error?: { message?: string; details?: string } } } })
        ?.response?.data?.error;
      setError(abp?.message || abp?.details || 'Could not link this parent.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal
      open={open}
      title={`Link a parent or guardian${studentName ? ` to ${studentName}` : ''}`}
      okText="Link"
      onOk={() => void link()}
      confirmLoading={saving}
      onCancel={onClose}
      destroyOnHidden
    >
      <Form form={form} layout="vertical">
        <Form.Item label="Parent or guardian" name="parentId" required>
          <Select
            showSearch
            placeholder="Search by name"
            filterOption={false}
            onSearch={search}
            loading={isSearching}
            notFoundContent={isSearching ? 'Searching…' : 'No parent found. Add them on the Parents page first.'}
            options={(parents ?? []).map((p) => {
              const parent = p as unknown as { id: string; fullName?: string; firstName?: string; lastName?: string; email?: string; phone?: string };
              const name = parent.fullName || [parent.firstName, parent.lastName].filter(Boolean).join(' ');
              return { value: parent.id, label: [name, parent.email || parent.phone].filter(Boolean).join(' · ') };
            })}
          />
        </Form.Item>
        <Form.Item label="Relationship to the learner" name="relationshipType" required>
          <Select placeholder="Choose" options={RELATIONSHIP_OPTIONS} />
        </Form.Item>
        <Space direction="vertical" size={0}>
          <Form.Item name="isPrimaryContact" valuePropName="checked" noStyle>
            <Checkbox>Primary contact</Checkbox>
          </Form.Item>
          <Form.Item name="isFinanciallyResponsible" valuePropName="checked" noStyle>
            <Checkbox>Financially responsible</Checkbox>
          </Form.Item>
          <Form.Item name="canPickupStudent" valuePropName="checked" noStyle>
            <Checkbox>May collect the learner</Checkbox>
          </Form.Item>
          <Form.Item name="livesWithStudent" valuePropName="checked" noStyle>
            <Checkbox>Lives with the learner</Checkbox>
          </Form.Item>
        </Space>
      </Form>
      <Text type="secondary" style={{ display: 'block', marginTop: 12 }}>
        Once linked, they see this learner in the parent app: report cards, marks, attendance and messages.
      </Text>
      {error && <Alert type="error" showIcon style={{ marginTop: 12 }} message={error} />}
    </Modal>
  );
}

export default function LinkGuardianModal(props: React.ComponentProps<typeof LinkGuardianForm>) {
  return (
    <ParentProvider>
      <LinkGuardianForm {...props} />
    </ParentProvider>
  );
}
