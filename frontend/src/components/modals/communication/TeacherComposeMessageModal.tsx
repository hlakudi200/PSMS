'use client';

import { useEffect, useMemo, useState } from 'react';
import { Alert, Form, Input, Modal, Select, Space, Typography, message } from 'antd';
import { z } from 'zod';
import { useMessageActions } from '@/providers/communication/messages';
import { getAxiosInstance } from '@/utils/axios-instance';

const { Text } = Typography;

const composeSchema = z.object({
  content: z.string().trim().min(1, 'Message body is required.').max(5000),
  subject: z.string().max(200).optional().or(z.literal('')),
  attachmentUrl: z.string().url('Must be a valid URL.').optional().or(z.literal('')),
});

interface ParentRecipient {
  userId: number;
  name: string;
}

interface TeacherComposeMessageModalProps {
  open: boolean;
  onClose: (refresh?: boolean) => void;
  // Classes the teacher is assigned to — recipients are scoped to the
  // parents of students in the selected class (US-TCH-012).
  classOptions: { value: string; label: string }[];
}

export const TeacherComposeMessageModal: React.FC<TeacherComposeMessageModalProps> = ({
  open,
  onClose,
  classOptions,
}) => {
  const [form] = Form.useForm();
  const { sendAsync } = useMessageActions();

  const [classId, setClassId] = useState<string | undefined>(undefined);
  const [parents, setParents] = useState<ParentRecipient[]>([]);
  const [resolving, setResolving] = useState(false);
  const [resolveError, setResolveError] = useState(false);
  const [selectedUserIds, setSelectedUserIds] = useState<number[]>([]);
  const [sending, setSending] = useState(false);

  useEffect(() => {
    if (open) {
      form.resetFields();
      setClassId(undefined);
      setParents([]);
      setSelectedUserIds([]);
    }
  }, [open, form]);

  // Resolve the parents of every student in the selected class. There is no
  // batch endpoint, so we fan out StudentParent/GetByStudent per student and
  // de-duplicate by parent user id. Scoping to a class the teacher teaches is
  // what keeps the picker to "parents of my students" (US-TCH-012).
  useEffect(() => {
    let cancelled = false;
    const resolve = async () => {
      if (!classId) {
        setParents([]);
        return;
      }
      setResolving(true);
      setResolveError(false);
      setSelectedUserIds([]);
      try {
        const instance = getAxiosInstance();
        const clsRes = await instance.get(
          `/api/services/app/StudentClass/GetByClass?classId=${classId}`
        );
        const students: { studentId: string }[] = (clsRes.data.result.items ?? []).filter(
          (s: { isActive?: boolean; isCurrent?: boolean }) => s.isActive !== false
        );

        const byUserId = new Map<number, string>();
        // allSettled, not all: one flaky per-student fetch must not blank the
        // entire recipient list — accumulate from the ones that succeeded and
        // flag if any failed so the list can be shown as possibly partial.
        const settled = await Promise.allSettled(
          students.map((s) =>
            instance.get(`/api/services/app/StudentParent/GetByStudent?studentId=${s.studentId}`)
          )
        );
        let anyFailed = false;
        settled.forEach((r) => {
          if (r.status === 'fulfilled') {
            (r.value.data.result.items ?? []).forEach(
              (link: { parentUserId?: number; parentName?: string }) => {
                if (link.parentUserId && link.parentUserId > 0) {
                  byUserId.set(link.parentUserId, link.parentName ?? 'Parent');
                }
              }
            );
          } else {
            anyFailed = true;
          }
        });

        if (!cancelled) {
          setParents(
            Array.from(byUserId.entries())
              .map(([userId, name]) => ({ userId, name }))
              .sort((a, b) => a.name.localeCompare(b.name))
          );
          setResolveError(anyFailed);
        }
      } catch {
        if (!cancelled) {
          setParents([]);
          setResolveError(true);
        }
      } finally {
        if (!cancelled) setResolving(false);
      }
    };
    resolve();
    return () => {
      cancelled = true;
    };
  }, [classId]);

  const recipientOptions = useMemo(
    () => parents.map((p) => ({ value: p.userId, label: p.name })),
    [parents]
  );

  const handleSubmit = async () => {
    const values = form.getFieldsValue();
    const parsed = composeSchema.safeParse({
      content: values.content ?? '',
      subject: values.subject ?? '',
      attachmentUrl: values.attachmentUrl ?? '',
    });
    if (!parsed.success) {
      form.setFields(
        parsed.error.issues.map((iss) => ({ name: iss.path as string[], errors: [iss.message] }))
      );
      return;
    }
    if (selectedUserIds.length === 0) {
      message.error('Pick at least one parent recipient.');
      return;
    }

    setSending(true);
    try {
      // Single-recipient backend, so a broadcast to several parents is N
      // sends. Each lands as its own thread (audit trail server-side, BR-CM-002).
      const results = await Promise.allSettled(
        selectedUserIds.map((recipientUserId) =>
          sendAsync({
            recipientUserId,
            subject: parsed.data.subject || undefined,
            content: parsed.data.content,
            attachmentUrl: parsed.data.attachmentUrl || undefined,
          })
        )
      );
      // Index-aligned with selectedUserIds (allSettled preserves order), so we
      // can prune to just the failures — a retry then won't double-send to the
      // recipients who already received the message.
      const failedIds = selectedUserIds.filter((_, i) => results[i].status === 'rejected');
      if (failedIds.length === 0) {
        message.success(`Message sent to ${selectedUserIds.length} parent${selectedUserIds.length === 1 ? '' : 's'}`);
        onClose(true);
      } else {
        message.warning(
          `Sent to ${selectedUserIds.length - failedIds.length}; ${failedIds.length} failed — retry the remaining.`
        );
        setSelectedUserIds(failedIds);
      }
    } finally {
      setSending(false);
    }
  };

  return (
    <Modal
      title="Message parents"
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      okText="Send"
      confirmLoading={sending}
      destroyOnHidden
      width={560}
    >
      <Space direction="vertical" style={{ width: '100%' }} size={12}>
        <div>
          <Text type="secondary" style={{ fontSize: 12 }}>
            Class
          </Text>
          <Select
            placeholder="Select one of your classes"
            showSearch
            optionFilterProp="label"
            value={classId}
            onChange={setClassId}
            options={classOptions}
            style={{ width: '100%' }}
          />
        </div>

        <div>
          <Text type="secondary" style={{ fontSize: 12 }}>
            Recipients (parents of students in this class)
          </Text>
          <Select
            mode="multiple"
            placeholder={classId ? 'Select parents' : 'Select a class first'}
            value={selectedUserIds}
            onChange={setSelectedUserIds}
            options={recipientOptions}
            loading={resolving}
            disabled={!classId}
            optionFilterProp="label"
            maxTagCount="responsive"
            style={{ width: '100%' }}
            notFoundContent={resolving ? 'Loading parents…' : 'No parents found for this class'}
          />
        </div>

        {resolveError && !resolving && (
          <Alert
            type="warning"
            showIcon
            message="Some recipients couldn't be loaded — the list may be incomplete. Re-select the class to retry."
          />
        )}

        {classId && !resolving && !resolveError && parents.length === 0 && (
          <Alert
            type="info"
            showIcon
            message="No messageable parents found for this class (parents need a linked login account)."
          />
        )}

        <Form form={form} layout="vertical">
          <Form.Item label="Subject" name="subject" style={{ marginBottom: 12 }}>
            <Input placeholder="Optional subject" maxLength={200} />
          </Form.Item>
          <Form.Item label="Message" name="content" rules={[{ required: true }]} style={{ marginBottom: 12 }}>
            <Input.TextArea rows={5} maxLength={5000} showCount placeholder="Type your message…" />
          </Form.Item>
          <Form.Item label="Attachment URL" name="attachmentUrl" style={{ marginBottom: 0 }}>
            <Input placeholder="https://… (optional)" />
          </Form.Item>
        </Form>
      </Space>
    </Modal>
  );
};
