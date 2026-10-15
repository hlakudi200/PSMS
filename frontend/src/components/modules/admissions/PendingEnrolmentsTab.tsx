'use client';

import React, { useCallback, useEffect, useState } from 'react';
import { Alert, Button, Card, Empty, Input, List, Modal, Space, Spin, Tag, Typography, message } from 'antd';
import { PhoneOutlined, SolutionOutlined } from '@ant-design/icons';
import dayjs from 'dayjs';
import { useEnrollmentActions, useEnrollmentState } from '@/providers/admissions/enrollments';
import type { IEnrollment } from '@/providers/admissions/shared/interfaces';
import EnrolLearnerModal from '@/components/modals/admissions/EnrolLearnerModal';

const { Title, Paragraph, Text } = Typography;

/**
 * Applicants the school has approved and not yet put on the roll.
 *
 * The endpoint for this list existed and nothing called it, so the only way to
 * find somebody waiting to be enrolled was to remember their name and open
 * their application. An approved applicant who is never enrolled is invisible:
 * their place is neither taken nor given away.
 */
export default function PendingEnrolmentsTab({ mayEnrol }: { mayEnrol: boolean }) {
  const { enrollments, totalCount, isPending } = useEnrollmentState();
  const { getPendingEnrollmentsAsync, acceptOfferAsync } = useEnrollmentActions();
  const [enrolling, setEnrolling] = useState<IEnrollment | undefined>();
  const [recording, setRecording] = useState<IEnrollment | undefined>();
  const [saving, setSaving] = useState(false);

  const load = useCallback(() => {
    getPendingEnrollmentsAsync({ maxResultCount: 50, skipCount: 0 });
  }, [getPendingEnrollmentsAsync]);

  useEffect(() => { load(); }, []);

  const waiting = enrollments ?? [];

  /* Families telephone, and some have no email. Staff may take their word for
     it — but as their own act, with their name on it, rather than the mapper
     quietly reporting every approved application as accepted. */
  const recordAcceptance = async () => {
    if (!recording) return;
    setSaving(true);
    try {
      await acceptOfferAsync({ applicationId: recording.applicationId, confirmAcceptance: true });
      message.success(`Recorded that ${recording.applicantName}'s family accepted the place.`);
      setRecording(undefined);
      load();
    } catch {
      // The interceptor shows why.
    } finally {
      setSaving(false);
    }
  };

  return (
    <>
      <Space style={{ width: '100%', justifyContent: 'space-between', marginBottom: 4 }} wrap>
        <Title level={5} style={{ margin: 0 }}>Waiting to be enrolled</Title>
        {totalCount != null && totalCount > 0 && <Tag color="green">{totalCount}</Tag>}
      </Space>
      <Paragraph type="secondary">
        Approved applicants who are not yet on the roll. Enrolling one creates their learner
        record and their sign-in, and takes one of the grade&rsquo;s places.
      </Paragraph>

      {isPending && <Spin />}

      {!isPending && waiting.length === 0 && (
        <Card>
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description="Nobody is waiting to be enrolled"
          />
        </Card>
      )}

      {!isPending && waiting.length > 0 && (
        <List
          dataSource={waiting}
          renderItem={(e) => (
            <Card size="small" style={{ marginBottom: 12 }}>
              <Space style={{ width: '100%', justifyContent: 'space-between' }} align="start" wrap>
                <Space direction="vertical" size={2}>
                  <Space wrap>
                    <Text strong>{e.applicantName}</Text>
                    <Tag>{e.gradeName}</Tag>
                    {e.isClassAssigned && e.assignedClassName && (
                      <Tag color="blue">Class {e.assignedClassName}</Tag>
                    )}
                    {e.isOfferAccepted
                      ? <Tag color="green">Place accepted</Tag>
                      : <Tag color="gold">Waiting for the family to answer</Tag>}
                  </Space>
                  <Text type="secondary" style={{ fontSize: 12 }}>
                    {e.applicationNumber}
                    {e.offerAcceptedDate
                      ? ` · accepted ${dayjs(e.offerAcceptedDate).format('DD MMM YYYY')}`
                      : e.offerExpiryDate
                        ? ` · offer open until ${dayjs(e.offerExpiryDate).format('DD MMM YYYY')}`
                        : ''}
                  </Text>
                </Space>

                {mayEnrol && (
                  e.isOfferAccepted ? (
                    <Button
                      type="primary"
                      size="small"
                      icon={<SolutionOutlined />}
                      onClick={() => setEnrolling(e)}
                    >
                      Enrol
                    </Button>
                  ) : (
                    <Button
                      size="small"
                      icon={<PhoneOutlined />}
                      onClick={() => setRecording(e)}
                    >
                      They have accepted
                    </Button>
                  )
                )}
              </Space>
            </Card>
          )}
        />
      )}

      {!isPending && waiting.some((e) => !e.isOfferAccepted) && (
        <Alert
          type="info"
          showIcon
          style={{ marginTop: 8 }}
          message="Some of these families have not answered yet"
          description="A place is not held for ever and a child is not enrolled on a maybe. The family can accept on their own application screen; if they told you some other way, record it with the button beside their name."
        />
      )}

      <Modal
        open={!!recording}
        title={`Record that ${recording?.applicantName ?? 'this family'} accepted the place`}
        okText="Record it"
        okButtonProps={{ loading: saving }}
        onOk={recordAcceptance}
        onCancel={() => setRecording(undefined)}
      >
        <Paragraph>
          Only do this if the family has actually told you they want the place — in person, by
          telephone or in writing. It is recorded against your name and is what lets the child be
          enrolled.
        </Paragraph>
        <Paragraph type="secondary" style={{ marginBottom: 0 }}>
          If they have not answered yet, leave it: they can accept on their own application screen.
        </Paragraph>
      </Modal>

      <EnrolLearnerModal
        open={!!enrolling}
        applicationId={enrolling?.applicationId ?? ''}
        applicantName={enrolling?.applicantName}
        gradeId={enrolling?.gradeId}
        gradeName={enrolling?.gradeName}
        onClose={() => setEnrolling(undefined)}
        onEnrolled={load}
      />
    </>
  );
}
