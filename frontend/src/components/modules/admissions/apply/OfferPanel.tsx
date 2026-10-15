'use client';

import React, { useState } from 'react';
import { Alert, Button, Card, Input, Modal, Space, Typography, message } from 'antd';
import { CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { useEnrollmentActions } from '@/providers/admissions/enrollments';
import type { IApplication } from '@/providers/admissions/shared/interfaces';

const { Paragraph, Text } = Typography;

/**
 * The school has offered a place. This is where the family answers.
 *
 * Before this existed the screen said "Offered a place" and gave them one
 * button: withdraw. Nothing asked whether they wanted it, nothing told them the
 * offer ran out, and the school was shown an acceptance it had inferred from
 * its own decision — so a child could be enrolled on a yes nobody had given.
 */
export default function OfferPanel({
  application,
  onAnswered,
}: {
  application: IApplication;
  onAnswered: () => void;
}) {
  const { acceptOfferAsync, declineOfferAsync } = useEnrollmentActions();
  const [accepting, setAccepting] = useState(false);
  const [declining, setDeclining] = useState(false);
  const [askingToDecline, setAskingToDecline] = useState(false);
  const [reason, setReason] = useState('');

  const learner = `${application.firstName ?? ''} ${application.lastName ?? ''}`.trim() || 'your child';
  const grade = application.applyingForGradeName ?? 'this grade';
  const expiry = application.offerExpiryDate ? dayjs(application.offerExpiryDate) : undefined;
  const daysLeft = expiry ? expiry.startOf('day').diff(dayjs().startOf('day'), 'day') : undefined;
  const expired = !!application.isOfferExpired;

  /* The school writes conditions into the same field as a rejection reason, so
     it is only worth showing while there is an offer to attach them to. */
  const conditions = application.decisionReason;

  const accept = async () => {
    setAccepting(true);
    try {
      await acceptOfferAsync({ applicationId: application.id, confirmAcceptance: true });
      message.success(`${learner} has a place in ${grade}.`);
      onAnswered();
    } catch {
      // The interceptor shows what the school said.
    } finally {
      setAccepting(false);
    }
  };

  const decline = async () => {
    setDeclining(true);
    try {
      await declineOfferAsync({
        applicationId: application.id,
        confirmDecline: true,
        reason: reason.trim() || undefined,
      });
      message.success('The school has been told, and the place has been released.');
      setAskingToDecline(false);
      onAnswered();
    } catch {
      // The interceptor shows what the school said.
    } finally {
      setDeclining(false);
    }
  };

  if (application.offerDeclinedDate) {
    return (
      <Alert
        type="info"
        showIcon
        style={{ marginTop: 8 }}
        message="You turned this place down"
        description={`Told to the school on ${dayjs(application.offerDeclinedDate).format('DD MMM YYYY')}. If that was a mistake, speak to the school — they would have to make a fresh offer.`}
      />
    );
  }

  if (application.offerAcceptedDate) {
    return (
      <Alert
        type="success"
        showIcon
        style={{ marginTop: 8 }}
        message={`You accepted the place in ${grade}`}
        description={`Accepted on ${dayjs(application.offerAcceptedDate).format('DD MMM YYYY')}. The school will be in touch about enrolling ${learner} and what to bring.`}
      />
    );
  }

  return (
    <>
      <Card
        title={`${learner} has been offered a place in ${grade}`}
        style={{ marginTop: 8 }}
      >
        <Space direction="vertical" size={12} style={{ width: '100%' }}>
          {conditions && (
            <Alert type="info" showIcon message="The school added" description={conditions} />
          )}

          {expired ? (
            <Alert
              type="error"
              showIcon
              message="This offer has run out"
              description={`It was open until ${expiry?.format('DD MMM YYYY')}. Speak to the school if you still want the place — they can put the offer back.`}
            />
          ) : (
            expiry && (
              <Alert
                type={daysLeft !== undefined && daysLeft <= 3 ? 'warning' : 'info'}
                showIcon
                message={
                  daysLeft !== undefined && daysLeft <= 0
                    ? 'Today is the last day to answer'
                    : `You have until ${expiry.format('DD MMM YYYY')} to answer`
                }
                description={
                  daysLeft !== undefined && daysLeft > 0
                    ? `${daysLeft} day${daysLeft === 1 ? '' : 's'} left. After that the school may give the place to somebody else.`
                    : 'After today the school may give the place to somebody else.'
                }
              />
            )
          )}

          <Paragraph type="secondary" style={{ marginBottom: 0 }}>
            Telling the school either way helps them: a place you turn down can go to a family on
            the waiting list.
          </Paragraph>

          <Space wrap>
            <Button
              type="primary"
              icon={<CheckCircleOutlined />}
              loading={accepting}
              disabled={expired || declining}
              onClick={accept}
            >
              Accept the place
            </Button>
            <Button
              danger
              icon={<CloseCircleOutlined />}
              disabled={accepting}
              onClick={() => setAskingToDecline(true)}
            >
              Turn it down
            </Button>
          </Space>
        </Space>
      </Card>

      <Modal
        open={askingToDecline}
        title={`Turn down the place for ${learner}?`}
        okText="Yes, turn it down"
        okButtonProps={{ danger: true, loading: declining }}
        cancelText="Keep thinking"
        onOk={decline}
        onCancel={() => setAskingToDecline(false)}
      >
        <Paragraph>
          This ends the application. The place goes back to {grade}, and you would have to apply
          again if you changed your mind.
        </Paragraph>
        <Paragraph type="secondary" style={{ marginBottom: 8 }}>
          If you would like to say why, the school would find it useful — but you do not have to.
        </Paragraph>
        <Input.TextArea
          rows={3}
          maxLength={1000}
          value={reason}
          onChange={(e) => setReason(e.target.value)}
          placeholder="Optional — for example, they have taken a place at another school"
        />
      </Modal>
    </>
  );
}
