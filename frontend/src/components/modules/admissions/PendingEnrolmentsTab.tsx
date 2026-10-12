'use client';

import React, { useCallback, useEffect, useState } from 'react';
import { Alert, Button, Card, Empty, List, Space, Spin, Tag, Typography } from 'antd';
import { SolutionOutlined } from '@ant-design/icons';
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
  const { getPendingEnrollmentsAsync } = useEnrollmentActions();
  const [enrolling, setEnrolling] = useState<IEnrollment | undefined>();

  const load = useCallback(() => {
    getPendingEnrollmentsAsync({ maxResultCount: 50, skipCount: 0 });
  }, [getPendingEnrollmentsAsync]);

  useEffect(() => { load(); }, []);

  const waiting = enrollments ?? [];

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
                    {!e.isOfferAccepted && <Tag color="gold">Offer not yet accepted</Tag>}
                  </Space>
                  <Text type="secondary" style={{ fontSize: 12 }}>
                    {e.applicationNumber}
                    {e.offerAcceptedDate
                      ? ` · accepted ${dayjs(e.offerAcceptedDate).format('DD MMM YYYY')}`
                      : ''}
                  </Text>
                </Space>

                {mayEnrol && (
                  <Button
                    type="primary"
                    size="small"
                    icon={<SolutionOutlined />}
                    onClick={() => setEnrolling(e)}
                  >
                    Enrol
                  </Button>
                )}
              </Space>
            </Card>
          )}
        />
      )}

      {/* Nothing records a family's acceptance yet — the endpoint saves no
          field — so an approved applicant shows here whether or not they have
          said yes. Worth knowing when reading the list. */}
      {!isPending && waiting.some((e) => !e.isOfferAccepted) && (
        <Alert
          type="info"
          showIcon
          style={{ marginTop: 8 }}
          message="Acceptance is not recorded yet"
          description="Nothing in the system yet captures a family saying yes to their offer, so that tag reflects a field nobody can set. Check with the family before enrolling."
        />
      )}

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
