'use client';

import React, { useEffect, useState } from 'react';
import {
  Alert, Button, Col, Empty, Form, Input, List, Modal, Row, Select, Space, Switch, Tag, Typography, message,
} from 'antd';
import { DeleteOutlined, EditOutlined, PlusOutlined, UserAddOutlined } from '@ant-design/icons';
import { useApplicantParentActions, useApplicantParentState } from '@/providers/admissions/applicant_parents';
import { parentSchema, type ParentFormValues } from './schema';

const { Text, Paragraph } = Typography;

const relationships = [
  { value: 1, label: 'Father' },
  { value: 2, label: 'Mother' },
  { value: 3, label: 'Guardian' },
  { value: 4, label: 'Stepfather' },
  { value: 5, label: 'Stepmother' },
  { value: 6, label: 'Grandparent' },
  { value: 7, label: 'Sibling' },
  { value: 8, label: 'Other' },
];

/**
 * Step two: the parents and guardians.
 *
 * Not optional, and not an afterthought. The server refuses to accept a
 * submission without at least one of these, one marked as the primary contact
 * and one financially responsible — so the screen says which of those three
 * are still outstanding rather than letting a parent discover it at the end.
 */
export default function ParentsStep({
  applicationId,
  locked,
  onChanged,
}: {
  applicationId: string;
  locked: boolean;
  onChanged: () => void;
}) {
  const { applicantParents } = useApplicantParentState();
  const { getAllByApplicationAsync, createAsync, updateAsync, deleteAsync } = useApplicantParentActions();

  const [form] = Form.useForm<ParentFormValues>();
  const [editing, setEditing] = useState<string | null>(null);
  const [open, setOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | undefined>();

  useEffect(() => {
    if (applicationId) getAllByApplicationAsync(applicationId);
  }, [applicationId]);

  const parents = applicantParents ?? [];
  const missing = [
    parents.length === 0 && 'at least one parent or guardian',
    parents.length > 0 && !parents.some((p) => p.isPrimaryContact) && 'one of them marked as the main contact',
    parents.length > 0 && !parents.some((p) => p.isFinanciallyResponsible) && 'one of them marked as paying the fees',
  ].filter(Boolean) as string[];

  const openFor = (id?: string) => {
    const existing = id ? parents.find((p) => p.id === id) : undefined;
    setEditing(id ?? null);
    setError(undefined);
    form.setFieldsValue(
      existing
        ? {
            relationship: existing.relationship,
            firstName: existing.firstName,
            lastName: existing.lastName,
            idNumber: existing.idNumber ?? '',
            email: existing.email,
            phoneNumber: existing.phoneNumber,
            alternatePhone: existing.alternatePhone ?? '',
            streetAddress: existing.streetAddress ?? '',
            suburb: existing.suburb ?? '',
            city: existing.city ?? '',
            province: existing.province ?? '',
            postalCode: existing.postalCode ?? '',
            occupation: existing.occupation ?? '',
            employer: existing.employer ?? '',
            isPrimaryContact: existing.isPrimaryContact,
            isFinanciallyResponsible: existing.isFinanciallyResponsible,
          }
        : {
            relationship: undefined as unknown as number,
            firstName: '',
            lastName: '',
            idNumber: '',
            email: '',
            phoneNumber: '',
            alternatePhone: '',
            streetAddress: '',
            suburb: '',
            city: '',
            province: '',
            postalCode: '',
            occupation: '',
            employer: '',
            // The first person added is almost always both, so start there.
            isPrimaryContact: parents.length === 0,
            isFinanciallyResponsible: parents.length === 0,
          }
    );
    setOpen(true);
  };

  const save = async () => {
    const parsed = parentSchema.safeParse(form.getFieldsValue());
    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message);
      return;
    }

    setSaving(true);
    setError(undefined);
    try {
      if (editing) await updateAsync(editing, parsed.data);
      else await createAsync({ applicationId, ...parsed.data });

      message.success(editing ? 'Details updated' : 'Added');
      setOpen(false);
      getAllByApplicationAsync(applicationId);
      onChanged();
    } catch (e) {
      const abp = (e as { response?: { data?: { error?: { message?: string; details?: string } } } })
        ?.response?.data?.error;
      setError(abp?.message || abp?.details || 'Could not save these details.');
    } finally {
      setSaving(false);
    }
  };

  const remove = async (id: string) => {
    await deleteAsync(id);
    getAllByApplicationAsync(applicationId);
    onChanged();
  };

  return (
    <>
      <Paragraph type="secondary">
        The school needs to know who to contact about this application, and who is responsible for the
        fees. Add both parents, or a guardian, or whoever that is.
      </Paragraph>

      {missing.length > 0 && (
        <Alert
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          message="Still needed before you can submit"
          description={`You need ${missing.join(', and ')}.`}
        />
      )}

      {parents.length === 0 ? (
        <Empty description="Nobody added yet" image={Empty.PRESENTED_IMAGE_SIMPLE}>
          <Button type="primary" icon={<UserAddOutlined />} onClick={() => openFor()} disabled={locked}>
            Add a parent or guardian
          </Button>
        </Empty>
      ) : (
        <>
          <List
            bordered
            dataSource={parents}
            renderItem={(p) => (
              <List.Item
                actions={
                  locked
                    ? []
                    : [
                        <Button key="e" size="small" icon={<EditOutlined />} onClick={() => openFor(p.id)}>
                          Edit
                        </Button>,
                        <Button key="d" size="small" danger icon={<DeleteOutlined />} onClick={() => remove(p.id)}>
                          Remove
                        </Button>,
                      ]
                }
              >
                <List.Item.Meta
                  title={
                    <Space wrap>
                      <Text strong>{`${p.firstName} ${p.lastName}`}</Text>
                      <Tag>{relationships.find((r) => r.value === p.relationship)?.label ?? 'Guardian'}</Tag>
                      {p.isPrimaryContact && <Tag color="blue">Main contact</Tag>}
                      {p.isFinanciallyResponsible && <Tag color="green">Pays the fees</Tag>}
                    </Space>
                  }
                  description={`${p.email} · ${p.phoneNumber}`}
                />
              </List.Item>
            )}
          />
          <Button
            style={{ marginTop: 12 }}
            icon={<PlusOutlined />}
            onClick={() => openFor()}
            disabled={locked}
          >
            Add another
          </Button>
        </>
      )}

      <Modal
        open={open}
        title={editing ? 'Edit parent or guardian' : 'Add a parent or guardian'}
        okText="Save"
        okButtonProps={{ loading: saving }}
        onOk={save}
        onCancel={() => setOpen(false)}
        width={720}
        destroyOnHidden
      >
        {error && <Alert type="error" showIcon message={error} style={{ marginBottom: 16 }} />}

        <Form form={form} layout="vertical">
          <Row gutter={16}>
            <Col xs={24} md={8}>
              <Form.Item label="Relationship to the learner" name="relationship" rules={[{ required: true }]}>
                <Select placeholder="Choose" options={relationships} />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item label="First name" name="firstName" rules={[{ required: true }]}>
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item label="Surname" name="lastName" rules={[{ required: true }]}>
                <Input />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col xs={24} md={8}>
              <Form.Item label="ID number" name="idNumber" extra="Optional, thirteen digits.">
                <Input maxLength={13} inputMode="numeric" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item label="Email" name="email" rules={[{ required: true, type: 'email' }]}>
                <Input placeholder="you@example.com" />
              </Form.Item>
            </Col>
            <Col xs={24} md={8}>
              <Form.Item label="Phone" name="phoneNumber" rules={[{ required: true }]}>
                <Input placeholder="082 000 0000" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col xs={24} md={12}>
              <Form.Item label="Street address" name="streetAddress">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={12} md={6}>
              <Form.Item label="Suburb" name="suburb">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={12} md={6}>
              <Form.Item label="City" name="city">
                <Input />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col xs={12} md={6}>
              <Form.Item label="Province" name="province">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={12} md={6}>
              <Form.Item label="Postal code" name="postalCode">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={6}>
              <Form.Item label="Occupation" name="occupation">
                <Input />
              </Form.Item>
            </Col>
            <Col xs={24} md={6}>
              <Form.Item label="Employer" name="employer">
                <Input />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col xs={24} md={12}>
              <Form.Item
                label="Main contact for this application"
                name="isPrimaryContact"
                valuePropName="checked"
                extra="The school will phone this person first."
              >
                <Switch />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item
                label="Responsible for the fees"
                name="isFinanciallyResponsible"
                valuePropName="checked"
              >
                <Switch />
              </Form.Item>
            </Col>
          </Row>
        </Form>
      </Modal>
    </>
  );
}
