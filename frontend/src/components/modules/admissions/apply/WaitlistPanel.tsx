'use client';

import React, { useEffect, useState } from 'react';
import { Alert, Button, Card, Modal, Space, Typography, message } from 'antd';
import { CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { useWaitlistActions, useWaitlistState } from '@/providers/admissions/waitlists';
import type { IApplication } from '@/providers/admissions/shared/interfaces';

const { Paragraph, Text } = Typography;

/**
 * Where a family stands on the waiting list, and their answer when a place
 * comes free.
 *
 * Before this, a waitlisted family's screen said "On the waiting list" and
 * then, underneath, "The school is reviewing your application" — which was no
 * longer true. It did not say where in the queue they were, and when the
 * school offered them a place there was nothing to accept it with: the
 * endpoints existed and no screen called them.
 */
export default function WaitlistPanel({
  application,
  onAnswered,
}: {
  application: IApplication;
  onAnswered: () => void;
}) {
  const { getByApplicationAsync, acceptOfferAsync, declineOfferAsync } = useWaitlistActions();
  // Read from the provider's state rather than a return value, as the rest of
  // the admissions screens do.
  const { waitlistEntry: entry } = useWaitlistState();
  const [loading, setLoading] = useState(true);
  const [working, setWorking] = useState(false);
  const [askingToDecline, setAskingToDecline] = useState(false);

  const learner = `${application.firstName ?? ''} ${application.lastName ?? ''}`.trim() || 'your child';
  const grade = application.applyingForGradeName ?? 'this grade';

  useEffect(() => {
    let alive = true;
    setLoading(true);
    Promise.resolve(getByApplicationAsync(application.id))
      .catch(() => { /* Not on the list, or not ours to see. */ })
      .finally(() => { if (alive) setLoading(false); });
    return () => { alive = false; };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [application.id]);

  const answer = async (accepting: boolean) => {
    if (!entry) return;
    setWorking(true);
    try {
      if (accepting) {
        await acceptOfferAsync(entry.id);
        message.success(`${learner} has a place in ${grade}.`);
      } else {
        await declineOfferAsync(entry.id);
        message.success('The school has been told, and the place has been released.');
      }
      setAskingToDecline(false);
      onAnswered();
    } catch {
      // The interceptor shows what the school said.
    } finally {
      setWorking(false);
    }
  };

  if (loading) return <Card style={{ marginTop: 8 }} loading />;

  if (!entry) {
    return (
      <Alert
        type="info"
        showIcon
        style={{ marginTop: 8 }}
        message={`${learner} is on the waiting list for ${grade}`}
        description="The school will be in touch if a place comes free."
      />
    );
  }

  const expiry = entry.offerExpiryDate ? dayjs(entry.offerExpiryDate) : undefined;
  const offered = entry.canAcceptOffer || entry.canDeclineOffer;

  /* An offer that ran out is its own state. "On the waiting list" would be a
     lie and an open offer would be a crueller one. */
  if (entry.isOfferExpired && !offered) {
    return (
      <Alert
        type="warning"
        showIcon
        style={{ marginTop: 8 }}
        message="The offer ran out"
        description={`A place was offered and not taken up in time. Speak to the school if you still want it — they would have to offer it again.`}
      />
    );
  }

  if (!offered) {
    return (
      <Alert
        type="info"
        showIcon
        style={{ marginTop: 8 }}
        message={`${learner} is number ${entry.position} on the waiting list for ${grade}`}
        description="The school will be in touch if a place comes free. Moving up the list depends on other families answering, so it is not a queue with a date on it."
      />
    );
  }

  return (
    <>
      <Card title={`A place has come free in ${grade}`} style={{ marginTop: 8 }}>
        <Space direction="vertical" size={12} style={{ width: '100%' }}>
          <Paragraph style={{ marginBottom: 0 }}>
            {learner} was number {entry.position} on the waiting list, and the school is
            offering them the place.
          </Paragraph>

          {expiry && (
            <Alert
              type={(entry.daysUntilOfferExpiry ?? 99) <= 2 ? 'warning' : 'info'}
              showIcon
              message={
                (entry.daysUntilOfferExpiry ?? 99) <= 0
                  ? 'Today is the last day to answer'
                  : `You have until ${expiry.format('DD MMM YYYY')} to answer`
              }
              description="A place on the waiting list is held for a short time only — after that it goes to the next family."
            />
          )}

          <Space wrap>
            <Button
              type="primary"
              icon={<CheckCircleOutlined />}
              loading={working}
              onClick={() => answer(true)}
            >
              Take the place
            </Button>
            <Button
              danger
              icon={<CloseCircleOutlined />}
              disabled={working}
              onClick={() => setAskingToDecline(true)}
            >
              Turn it down
            </Button>
          </Space>

          <Text type="secondary" style={{ fontSize: 12 }}>
            Taking it accepts the place outright — the school will not ask again.
          </Text>
        </Space>
      </Card>

      <Modal
        open={askingToDecline}
        title={`Turn down the place for ${learner}?`}
        okText="Yes, turn it down"
        okButtonProps={{ danger: true, loading: working }}
        cancelText="Keep thinking"
        onOk={() => answer(false)}
        onCancel={() => setAskingToDecline(false)}
      >
        <Paragraph>
          The place goes to the next family on the list, and {learner} comes off it. You
          would have to apply again if you changed your mind.
        </Paragraph>
      </Modal>
    </>
  );
}
