'use client';

/**
 * RC-17. The RE-002 fields that had no property to write to, and the RE-003
 * sign-off that gates publication.
 *
 * `PSMS-Business-Rules.md` RE-002 lists conduct and diligence ratings among the
 * fields a South African report card carries, and RE-003 requires a teacher and
 * a principal signature before one may be published. Neither existed: the PDF
 * drew three blank signature lines, so "signed" meant only that somebody had
 * printed it.
 *
 * Nothing in the National Protocol or the NPPPPR prescribes a conduct scale —
 * the seven achievement levels are for subjects — so this is a school
 * convention, worded as behaviour rather than as marks.
 */

import React, { useEffect, useState } from 'react';
import {
  Alert,
  Button,
  Card,
  Col,
  Descriptions,
  Form,
  Input,
  Popconfirm,
  Row,
  Select,
  Space,
  Tag,
  Typography,
  message,
} from 'antd';
import { CheckCircleTwoTone, EditOutlined, SignatureOutlined } from '@ant-design/icons';
import { z } from 'zod';
import dayjs from 'dayjs';
import { useReportActions } from '@/providers/assessment/reports';
import type { IReport } from '@/providers/assessment/shared/interfaces';

const { Text } = Typography;

/** Matches the backend ConductDiligenceRating enum. */
export const conductRatingLabels: Record<number, string> = {
  7: 'Excellent',
  6: 'Very good',
  5: 'Good',
  4: 'Satisfactory',
  3: 'Needs improvement',
  2: 'Poor',
  1: 'Unsatisfactory',
};

const conductOptions = Object.entries(conductRatingLabels)
  .map(([value, label]) => ({ value: Number(value), label }))
  .sort((a, b) => b.value - a.value);

const schema = z.object({
  conductRating: z.number().int().min(1).max(7).optional(),
  diligenceRating: z.number().int().min(1).max(7).optional(),
  behaviourComments: z
    .string()
    .max(1000, 'Keep the behaviour comment under 1000 characters')
    .optional(),
});

type ConductFormValues = z.infer<typeof schema>;

interface Props {
  report: IReport;
  /** ReportCards.Comment — the class teacher and up. */
  canRecordConduct: boolean;
  /**
   * Whether this user may sign the Class Teacher line on THIS card. Separate
   * from canRecordConduct: every teacher in the school holds the comment
   * permission, but only the class's own teacher signs for it, so tying the
   * button to the permission offered the whole staff a signature the server
   * refuses with "Only this class's teacher can sign".
   */
  canSignAsClassTeacher: boolean;
  /** ReportCards.Publish — the principal. */
  canSignAsPrincipal: boolean;
  onChanged: () => void;
}

export const ConductAndSignOffCard: React.FC<Props> = ({
  report,
  canRecordConduct,
  canSignAsClassTeacher,
  canSignAsPrincipal,
  onChanged,
}) => {
  const [form] = Form.useForm<ConductFormValues>();
  const { recordConductAsync, signAsTeacherAsync, signAsPrincipalAsync } = useReportActions();

  const [editing, setEditing] = useState(false);
  const [saving, setSaving] = useState(false);

  /* Published cards are closed to all of this — the National Protocol §25(3)
     asks that an issued card carry no corrections. */
  const closed = report.status === 5;

  useEffect(() => {
    form.setFieldsValue({
      conductRating: report.conductRating,
      diligenceRating: report.diligenceRating,
      behaviourComments: report.behaviourComments,
    });
  }, [form, report]);

  const handleSave = async () => {
    let values: ConductFormValues;

    try {
      values = await form.validateFields();
    } catch {
      return;
    }

    const parsed = schema.safeParse(values);
    if (!parsed.success) {
      message.error(parsed.error.issues[0]?.message ?? 'Check the form');
      return;
    }

    setSaving(true);
    try {
      await recordConductAsync({ reportId: report.id, ...parsed.data });
      message.success('Conduct and diligence saved');
      setEditing(false);
      onChanged();
    } catch {
      message.error('Could not save the conduct and diligence');
    } finally {
      setSaving(false);
    }
  };

  const sign = async (who: 'teacher' | 'principal') => {
    try {
      await (who === 'teacher' ? signAsTeacherAsync(report.id) : signAsPrincipalAsync(report.id));
      message.success('Signed');
      onChanged();
    } catch {
      message.error('Could not sign this report card');
    }
  };

  const signedOn = (date?: string) => (date ? dayjs(date).format('DD MMM YYYY') : undefined);

  return (
    <Card
      className="no-print"
      title="Conduct, diligence and sign-off"
      style={{ marginBottom: 16 }}
      extra={
        canRecordConduct && !closed && !editing ? (
          <Button size="small" icon={<EditOutlined />} onClick={() => setEditing(true)}>
            Edit
          </Button>
        ) : undefined
      }
    >
      <Space direction="vertical" size="middle" style={{ width: '100%' }}>
        {editing ? (
          <Form form={form} layout="vertical">
            <Row gutter={16}>
              <Col xs={24} md={8}>
                <Form.Item label="Conduct" name="conductRating">
                  <Select allowClear placeholder="Not rated" options={conductOptions} />
                </Form.Item>
              </Col>
              <Col xs={24} md={8}>
                <Form.Item label="Diligence" name="diligenceRating">
                  <Select allowClear placeholder="Not rated" options={conductOptions} />
                </Form.Item>
              </Col>
            </Row>
            <Form.Item
              label="What the ratings are based on"
              name="behaviourComments"
              rules={[{ max: 1000, message: 'Keep the behaviour comment under 1000 characters' }]}
            >
              <Input.TextArea rows={3} maxLength={1000} showCount />
            </Form.Item>
            <Space>
              <Button type="primary" loading={saving} onClick={handleSave}>
                Save
              </Button>
              <Button onClick={() => setEditing(false)}>Cancel</Button>
            </Space>
          </Form>
        ) : (
          <Descriptions size="small" column={{ xs: 1, md: 2 }}>
            <Descriptions.Item label="Conduct">
              {report.conductRating
                ? conductRatingLabels[report.conductRating]
                : <Text type="secondary">Not rated</Text>}
            </Descriptions.Item>
            <Descriptions.Item label="Diligence">
              {report.diligenceRating
                ? conductRatingLabels[report.diligenceRating]
                : <Text type="secondary">Not rated</Text>}
            </Descriptions.Item>
            <Descriptions.Item label="Behaviour" span={2}>
              {report.behaviourComments || <Text type="secondary">Nothing recorded</Text>}
            </Descriptions.Item>
          </Descriptions>
        )}

        {/* RE-003: both signatures before the card may be published. */}
        <Row gutter={16}>
          <Col xs={24} md={12}>
            {report.teacherSignedByUserId ? (
              <Tag icon={<CheckCircleTwoTone twoToneColor="#52c41a" />} color="success">
                Class teacher signed {signedOn(report.teacherSignedDate)}
              </Tag>
            ) : canSignAsClassTeacher && !closed ? (
              <Popconfirm
                title="Sign this report card as the class teacher?"
                description="A signature says the card is correct as it stands."
                onConfirm={() => sign('teacher')}
              >
                <Button size="small" icon={<SignatureOutlined />}>
                  Sign as class teacher
                </Button>
              </Popconfirm>
            ) : (
              <Text type="secondary">
                Not signed by the class teacher
                {!canSignAsClassTeacher && !closed && ' — only this class’s teacher can'}
              </Text>
            )}
          </Col>
          <Col xs={24} md={12}>
            {report.principalSignedByUserId ? (
              <Tag icon={<CheckCircleTwoTone twoToneColor="#52c41a" />} color="success">
                Principal signed {signedOn(report.principalSignedDate)}
              </Tag>
            ) : canSignAsPrincipal && !closed ? (
              <Popconfirm
                title="Sign this report card as principal?"
                description="A signature says the card is correct as it stands."
                onConfirm={() => sign('principal')}
              >
                <Button size="small" icon={<SignatureOutlined />}>
                  Sign as principal
                </Button>
              </Popconfirm>
            ) : (
              <Text type="secondary">Not signed by the principal</Text>
            )}
          </Col>
        </Row>

        {!closed && !(report.teacherSignedByUserId && report.principalSignedByUserId) && (
          <Alert
            type="info"
            showIcon
            message="A report card needs both signatures before it can be published."
          />
        )}
      </Space>
    </Card>
  );
};

export default ConductAndSignOffCard;
