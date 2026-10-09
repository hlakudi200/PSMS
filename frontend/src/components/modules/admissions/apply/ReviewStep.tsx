'use client';

import React, { useEffect, useState } from 'react';
import {
  Alert, Button, Card, Descriptions, Input, Modal, Result, Space, Tag, Typography, message,
} from 'antd';
import { CreditCardOutlined, SendOutlined, StopOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { useApplicationFeeActions } from '@/providers/admissions/application_fees';
import type { IFeeCheckout } from '@/providers/admissions/application_fees/context';
import type { IApplicantParent, IApplication } from '@/providers/admissions/shared/interfaces';
import {
  ApplicationStatus, applicationStatusLabel, canWithdraw,
} from '@/providers/admissions/shared/application-status';

const { Paragraph, Text } = Typography;

/** Draft is 1; anything beyond it is in the school's hands. */
const DRAFT = ApplicationStatus.Draft;
const PAYMENT_PENDING = ApplicationStatus.PaymentPending;

/**
 * Step four: what the school is about to receive, then handing it over.
 *
 * Submitting is the point at which this stops being the parent's document and
 * becomes the school's, so it shows them what is on it first.
 */
export default function ReviewStep({
  application,
  parents,
  documentCount,
  onSubmit,
  submitting,
  onWithdraw,
  withdrawing,
  onRefresh,
}: {
  application: IApplication;
  /* Passed in rather than read off the application: the DTO carries counts,
     not the people, and the step before this one has already loaded them. */
  parents: IApplicantParent[];
  documentCount: number;
  onSubmit: () => void;
  submitting: boolean;
  onWithdraw: (reason?: string) => Promise<void>;
  withdrawing: boolean;
  onRefresh: () => void;
}) {
  const { getCheckoutAsync, simulatePaymentAsync } = useApplicationFeeActions();
  const [checkout, setCheckout] = useState<IFeeCheckout | undefined>();
  const [paying, setPaying] = useState(false);
  const [askingToWithdraw, setAskingToWithdraw] = useState(false);
  const [reason, setReason] = useState('');

  const isDraft = application.status === DRAFT;
  const withdrawn = application.status === ApplicationStatus.Withdrawn;
  const learnerName = `${application.firstName ?? ''} ${application.lastName ?? ''}`.trim();
  const grade = application.applyingForGradeName ?? 'this grade';

  const confirmWithdrawal = async () => {
    await onWithdraw(reason.trim() || undefined);
    setAskingToWithdraw(false);
    setReason('');
  };

  /**
   * The way out, wherever the application has got to.
   *
   * Deliberately quiet — a text button, not a red one beside Submit — because
   * withdrawing is rare and cannot be undone, and nobody should land on it
   * while aiming at something else. The confirmation says so plainly.
   */
  const withdrawAffordance = !canWithdraw(application.status) ? null : (
    <>
      <Button
        type="text"
        danger
        icon={<StopOutlined />}
        onClick={() => setAskingToWithdraw(true)}
        style={{ paddingLeft: 0 }}
      >
        {isDraft ? 'I no longer want to apply' : 'Withdraw this application'}
      </Button>

      <Modal
        open={askingToWithdraw}
        title="Withdraw this application?"
        okText="Yes, withdraw it"
        okButtonProps={{ danger: true, loading: withdrawing }}
        cancelText="Keep it"
        onOk={confirmWithdrawal}
        onCancel={() => setAskingToWithdraw(false)}
        destroyOnHidden
      >
        <Paragraph>
          {isDraft
            ? `${learnerName || 'This learner'} will not be put forward for ${grade}, and this application will be closed.`
            : `The school will stop considering ${learnerName || 'this learner'} for ${grade}.`}
        </Paragraph>
        <Paragraph type="secondary">
          This cannot be undone. If you change your mind you would have to apply again, and only
          while applications are still open.
          {checkout?.feeRequired && !checkout.awaitingPayment
            ? ' A fee you have already paid is not refunded automatically — ask the school about it.'
            : ''}
        </Paragraph>
        <Paragraph style={{ marginBottom: 4 }}>Why are you withdrawing? (optional)</Paragraph>
        <Input.TextArea
          id="withdraw-reason"
          rows={3}
          maxLength={500}
          showCount
          value={reason}
          onChange={(e) => setReason(e.target.value)}
          placeholder="The school will see this."
        />
      </Modal>
    </>
  );

  useEffect(() => {
    if (isDraft) return;
    getCheckoutAsync(application.id).then(setCheckout).catch(() => undefined);
  }, [application.id, application.status, isDraft]);

  const pay = async () => {
    setPaying(true);
    try {
      await simulatePaymentAsync(application.id);
      message.success('Payment recorded. Your application is now with the school.');
      const next = await getCheckoutAsync(application.id).catch(() => undefined);
      setCheckout(next);
      onRefresh();
    } catch (e) {
      const abp = (e as { response?: { data?: { error?: { message?: string; details?: string } } } })
        ?.response?.data?.error;
      message.error(abp?.message || abp?.details || 'Could not record that payment.');
    } finally {
      setPaying(false);
    }
  };

  if (withdrawn) {
    return (
      <Result
        status="warning"
        title="You have withdrawn this application"
        subTitle={
          <Space direction="vertical" size={2}>
            <Text>{learnerName} is no longer being considered for {grade}.</Text>
            {application.decisionReason && (
              <Text type="secondary">You told the school: {application.decisionReason}</Text>
            )}
            <Text type="secondary">
              Reference {application.applicationNumber}, should you need to refer to it.
            </Text>
          </Space>
        }
      />
    );
  }

  if (!isDraft) {
    const awaitingPayment = application.status === PAYMENT_PENDING && checkout?.awaitingPayment;

    return (
      <>
        <Result
          status="success"
          title="Your application is with the school"
          subTitle={
            <Space direction="vertical" size={2}>
              <Text>
                Reference <Text strong copyable>{application.applicationNumber}</Text>
              </Text>
              <Text type="secondary">Keep that number — the school will ask for it.</Text>
              <Text type="secondary">
                Where it stands: {applicationStatusLabel(application.status)}.
              </Text>
            </Space>
          }
        />

        {awaitingPayment && (
          <Card title="Application fee" style={{ marginTop: 8 }}>
            <Paragraph>
              R {checkout!.amount.toFixed(2)} is owed before the school begins reviewing the application.
            </Paragraph>

            {checkout!.isSimulated ? (
              <>
                <Alert
                  type="warning"
                  showIcon
                  style={{ marginBottom: 16 }}
                  message="This is a simulated payment — no money will be taken"
                  description="The school has not connected a payment service yet. This button exists so the process can be tested end to end; it records the fee as paid and moves nothing."
                />
                <Button type="primary" icon={<CreditCardOutlined />} loading={paying} onClick={pay} danger>
                  Simulate paying R {checkout!.amount.toFixed(2)}
                </Button>
              </>
            ) : (
              <Alert
                type="info"
                showIcon
                message="Pay the school directly"
                description={`Use your application number ${application.applicationNumber} as the reference, and the school will record it against this application.`}
              />
            )}
          </Card>
        )}

        {checkout && !checkout.awaitingPayment && checkout.feeRequired && (
          <Alert
            type="success"
            showIcon
            style={{ marginTop: 8 }}
            message="The application fee has been paid"
            description={checkout.receiptNumber ? `Receipt ${checkout.receiptNumber}.` : undefined}
          />
        )}

        {checkout && !checkout.feeRequired && (
          <Alert
            type="info"
            showIcon
            style={{ marginTop: 8 }}
            message="There is no application fee for this grade"
            description="The school is reviewing your application."
          />
        )}

        <div style={{ marginTop: 24 }}>
          <Paragraph type="secondary" style={{ marginBottom: 4 }}>
            Changed your mind, or taken a place somewhere else?
          </Paragraph>
          {withdrawAffordance}
        </div>
      </>
    );
  }

  return (
    <>
      <Paragraph type="secondary">
        Check this over. Once you submit it, the school can see it and you will not be able to change
        it yourself.
      </Paragraph>

      <Descriptions bordered column={{ xs: 1, md: 2 }} size="small" style={{ marginBottom: 16 }}>
        <Descriptions.Item label="Learner">
          {`${application.firstName ?? ''} ${application.middleName ?? ''} ${application.lastName ?? ''}`.replace(/\s+/g, ' ').trim()}
        </Descriptions.Item>
        <Descriptions.Item label="Date of birth">
          {application.dateOfBirth ? dayjs(application.dateOfBirth).format('DD MMM YYYY') : '—'}
        </Descriptions.Item>
        <Descriptions.Item label="Applying for">
          {application.applyingForGradeName ?? '—'}
        </Descriptions.Item>
        <Descriptions.Item label="Academic year">
          {application.academicYearName ?? '—'}
        </Descriptions.Item>
        <Descriptions.Item label="Citizenship">
          {application.isSACitizen ? 'South African' : 'Not a South African citizen'}
        </Descriptions.Item>
        <Descriptions.Item label={application.isSACitizen ? 'ID number' : 'Passport'}>
          {application.isSACitizen ? application.idNumber ?? '—' : application.passportNumber ?? '—'}
        </Descriptions.Item>
        <Descriptions.Item label="Previous school" span={2}>
          {application.previousSchool || 'None given'}
        </Descriptions.Item>
        <Descriptions.Item label="Parents and guardians" span={2}>
          {parents.length === 0 ? (
            <Text type="danger">None added — the school cannot accept this without one.</Text>
          ) : (
            <Space direction="vertical" size={2}>
              {parents.map((p) => (
                <Space key={p.id} wrap>
                  <Text>{`${p.firstName} ${p.lastName}`}</Text>
                  {p.isPrimaryContact && <Tag color="blue">Main contact</Tag>}
                  {p.isFinanciallyResponsible && <Tag color="green">Pays the fees</Tag>}
                </Space>
              ))}
            </Space>
          )}
        </Descriptions.Item>
        <Descriptions.Item label="Documents" span={2}>
          {documentCount === 0 ? 'None uploaded yet' : `${documentCount} uploaded`}
        </Descriptions.Item>
      </Descriptions>

      <Space direction="vertical" size={4}>
        <Button type="primary" size="large" icon={<SendOutlined />} loading={submitting} onClick={onSubmit}>
          Submit this application
        </Button>
        {withdrawAffordance}
      </Space>
    </>
  );
}
