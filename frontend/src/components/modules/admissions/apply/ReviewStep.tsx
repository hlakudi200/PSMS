'use client';

import React, { useEffect, useState } from 'react';
import { Alert, Button, Card, Descriptions, Result, Space, Tag, Typography, message } from 'antd';
import { CreditCardOutlined, SendOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { useApplicationFeeActions } from '@/providers/admissions/application_fees';
import type { IFeeCheckout } from '@/providers/admissions/application_fees/context';
import type { IApplicantParent, IApplication } from '@/providers/admissions/shared/interfaces';

const { Paragraph, Text } = Typography;

/** Draft is 1; anything beyond it is in the school's hands. */
const DRAFT = 1;
const PAYMENT_PENDING = 3;

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
  onRefresh,
}: {
  application: IApplication;
  /* Passed in rather than read off the application: the DTO carries counts,
     not the people, and the step before this one has already loaded them. */
  parents: IApplicantParent[];
  documentCount: number;
  onSubmit: () => void;
  submitting: boolean;
  onRefresh: () => void;
}) {
  const { getCheckoutAsync, simulatePaymentAsync } = useApplicationFeeActions();
  const [checkout, setCheckout] = useState<IFeeCheckout | undefined>();
  const [paying, setPaying] = useState(false);

  const isDraft = application.status === DRAFT;

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

      <Button type="primary" size="large" icon={<SendOutlined />} loading={submitting} onClick={onSubmit}>
        Submit this application
      </Button>
    </>
  );
}
