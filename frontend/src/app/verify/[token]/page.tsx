'use client';

import React, { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { Card, Descriptions, Result, Skeleton, Space, Tag, Typography } from 'antd';
import {
  CheckCircleTwoTone,
  CloseCircleTwoTone,
  ExclamationCircleTwoTone,
  SafetyCertificateOutlined,
} from '@ant-design/icons';
import { getAxiosInstance } from '@/utils/axios-instance';

const { Title, Text, Paragraph } = Typography;

interface IVerification {
  found: boolean;
  issued: boolean;
  schoolName?: string;
  learnerInitials?: string;
  className?: string;
  gradeName?: string;
  termName?: string;
  reportType?: string;
  academicYear?: string;
  reportCardNumber?: string;
  issuedOn?: string;
  signedOff?: boolean;
  message?: string;
}

/**
 * The page a QR on a printed report card leads to.
 *
 * Deliberately outside every portal layout: whoever scans this — another
 * school, a bursary office, an employer — has no account here, so it must
 * render with no session and never redirect to a login screen.
 */
export default function VerifyReportCardPage() {
  const params = useParams<{ token: string }>();
  const token = Array.isArray(params?.token) ? params.token[0] : params?.token;
  const [result, setResult] = useState<IVerification | null>(null);
  const [loading, setLoading] = useState(true);
  /* A check that could not be run is not the same answer as a code that does
     not match, and must not be shown as one. */
  const [unreachable, setUnreachable] = useState(false);

  useEffect(() => {
    if (!token) return;
    const instance = getAxiosInstance();
    instance
      /* POST, because that is what ABP exposes this as: a method not named
         Get* is routed as POST, and the GET this used to send came back 405.
         The catch below then rendered that transport failure as "no report
         card matches this code" — so every genuine code on every printed card
         was told it was not genuine. */
      .post(`/api/services/app/ReportVerification/Verify?token=${encodeURIComponent(token)}`, null, {
        // A code that does not resolve is an answer for the reader, not a
        // dialog thrown over an otherwise blank page.
        suppressErrorModal: true,
      })
      .then((res) => {
        setResult(res.data?.result ?? null);
        setUnreachable(false);
      })
      .catch(() => {
        setResult(null);
        setUnreachable(true);
      })
      .finally(() => setLoading(false));
  }, [token]);

  const shell = (children: React.ReactNode) => (
    <div style={{ minHeight: '100vh', background: '#f5f5f5', padding: '48px 16px' }}>
      <div style={{ maxWidth: 620, margin: '0 auto' }}>
        <Space align="center" size={10} style={{ marginBottom: 20 }}>
          <SafetyCertificateOutlined style={{ fontSize: 26, color: '#1677ff' }} />
          <Title level={4} style={{ margin: 0 }}>Report card check</Title>
        </Space>
        {children}
      </div>
    </div>
  );

  if (loading) return shell(<Card><Skeleton active paragraph={{ rows: 5 }} /></Card>);

  /* Say what actually happened. Telling a parent their card is not genuine
     because the server could not be reached is worse than saying nothing. */
  if (unreachable) {
    return shell(
      <Card>
        <Result
          icon={<ExclamationCircleTwoTone twoToneColor="#faad14" />}
          title="This code could not be checked right now"
          subTitle="The school's records could not be reached. Try again in a moment — this does not mean the card is not genuine."
        />
      </Card>,
    );
  }

  if (!result || !result.found) {
    return shell(
      <Card>
        <Result
          icon={<CloseCircleTwoTone twoToneColor="#cf1322" />}
          title="No report card matches this code"
          subTitle={
            result?.message
            ?? 'Check the code printed on the document, or ask the school to confirm it.'
          }
        />
      </Card>,
    );
  }

  if (!result.issued) {
    return shell(
      <Card>
        <Result
          icon={<ExclamationCircleTwoTone twoToneColor="#faad14" />}
          title="This card has not been issued"
          subTitle={result.message}
        />
      </Card>,
    );
  }

  return shell(
    <Card>
      <Result
        style={{ paddingBottom: 8 }}
        icon={<CheckCircleTwoTone twoToneColor="#52c41a" />}
        title="This is a genuine report card"
        subTitle={`Issued by ${result.schoolName}`}
      />

      <Descriptions column={1} bordered size="small">
        <Descriptions.Item label="Learner">
          {result.learnerInitials ?? '—'}
          <Text type="secondary" style={{ marginLeft: 8, fontSize: 12 }}>
            initials only
          </Text>
        </Descriptions.Item>
        <Descriptions.Item label="Class">
          {[result.gradeName, result.className].filter(Boolean).join(' · ') || '—'}
        </Descriptions.Item>
        <Descriptions.Item label="Report">
          {[result.reportType, result.termName, result.academicYear].filter(Boolean).join(' · ') || '—'}
        </Descriptions.Item>
        <Descriptions.Item label="Reference">
          <Text code style={{ fontSize: 11 }}>{result.reportCardNumber ?? '—'}</Text>
        </Descriptions.Item>
        <Descriptions.Item label="Issued on">
          {result.issuedOn ? new Date(result.issuedOn).toLocaleDateString('en-ZA', {
            day: 'numeric', month: 'long', year: 'numeric',
          }) : '—'}
        </Descriptions.Item>
        <Descriptions.Item label="Signed off">
          {result.signedOff
            ? <Tag color="success">Class teacher and principal</Tag>
            : <Tag color="warning">Not fully signed</Tag>}
        </Descriptions.Item>
      </Descriptions>

      <Paragraph type="secondary" style={{ marginTop: 16, marginBottom: 0, fontSize: 12 }}>
        Only the learner&apos;s initials are shown. Compare them, the class and the reference
        against the document you are holding. Marks are never published here — if the figures on
        the document are in question, ask the school directly.
      </Paragraph>
    </Card>,
  );
}
