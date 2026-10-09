'use client';

import React, { useCallback, useEffect, useState } from 'react';
import {
  Alert, Button, Card, Empty, List, Segmented, Space, Spin, Tag, Tooltip, Typography,
} from 'antd';
import {
  CalendarOutlined, ClockCircleOutlined, EnvironmentOutlined, FormOutlined, VideoCameraOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import {
  AdmissionInterviewProvider, useAdmissionInterviewActions,
} from '@/providers/admissions/admission_interviews';
import {
  InterviewStatus, interviewStatusColour, interviewStatusLabel,
  type IAdmissionInterview,
} from '@/providers/admissions/shared/interfaces';
import RecordInterviewOutcomeModal from '@/components/modals/admissions/RecordInterviewOutcomeModal';

const { Title, Paragraph, Text } = Typography;

/** A time that arrives as "14:30:00". */
const prettyTime = (time?: string) => (time ? time.slice(0, 5) : '—');

const whenItIs = (interview: IAdmissionInterview) => {
  const date = dayjs(interview.scheduledDate);
  if (date.isSame(dayjs(), 'day')) return 'Today';
  if (date.isSame(dayjs().add(1, 'day'), 'day')) return 'Tomorrow';
  return date.format('ddd DD MMM YYYY');
};

/**
 * The interviews a teacher is down to conduct.
 *
 * They had no way of finding out. An interview could only be read through the
 * application it belongs to, and a teacher cannot read applications — so the
 * person expected to sit in the room was the one person who could not see that
 * they were expected.
 */
function MyInterviews() {
  const { getMineAsync } = useAdmissionInterviewActions();

  const [interviews, setInterviews] = useState<IAdmissionInterview[]>([]);
  const [showing, setShowing] = useState<'upcoming' | 'all'>('upcoming');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | undefined>();
  const [recording, setRecording] = useState<IAdmissionInterview | undefined>();

  const load = useCallback(
    (includePast: boolean) => {
      setLoading(true);
      setError(undefined);
      getMineAsync(includePast)
        .then(setInterviews)
        .catch(() => setError('Could not load your interviews.'))
        .finally(() => setLoading(false));
    },
    [getMineAsync]
  );

  useEffect(() => {
    load(showing === 'all');
  }, [showing]);

  /* An interview whose day has passed with nothing recorded is the one thing
     on this page that needs doing, so it is called out rather than left to be
     noticed. */
  const overdue = interviews.filter(
    (i) =>
      (i.status === InterviewStatus.Scheduled || i.status === InterviewStatus.Rescheduled)
      && dayjs(i.scheduledDate).isBefore(dayjs(), 'day')
  );

  return (
    <div style={{ padding: 24, maxWidth: 900 }}>
      <Space style={{ width: '100%', justifyContent: 'space-between', marginBottom: 4 }} wrap>
        <Title level={3} style={{ margin: 0 }}>My interviews</Title>
        <Segmented
          value={showing}
          onChange={(v) => setShowing(v as 'upcoming' | 'all')}
          options={[
            { label: 'Still to do', value: 'upcoming' },
            { label: 'Everything', value: 'all' },
          ]}
        />
      </Space>
      <Paragraph type="secondary">
        Admission interviews you have been asked to conduct.
      </Paragraph>

      {error && <Alert type="error" showIcon message={error} style={{ marginBottom: 16 }} />}

      {overdue.length > 0 && (
        <Alert
          type="warning"
          showIcon
          style={{ marginBottom: 16 }}
          message={
            overdue.length === 1
              ? 'One interview has passed without an outcome'
              : `${overdue.length} interviews have passed without an outcome`
          }
          description="The principal cannot decide on these applications until you record what happened."
        />
      )}

      {loading && <Spin />}

      {!loading && interviews.length === 0 && (
        <Card>
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description={
              showing === 'upcoming'
                ? 'Nothing booked for you at the moment'
                : 'You have not been asked to conduct any interviews'
            }
          />
        </Card>
      )}

      {!loading && interviews.length > 0 && (
        <List
          itemLayout="vertical"
          dataSource={interviews}
          renderItem={(interview) => {
            const past = dayjs(interview.scheduledDate).isBefore(dayjs(), 'day');
            const stillToRecord =
              interview.status === InterviewStatus.Scheduled
              || interview.status === InterviewStatus.Rescheduled;

            return (
              <Card
                size="small"
                style={{ marginBottom: 12 }}
                title={
                  <Space wrap>
                    <Text strong>{interview.applicantName}</Text>
                    <Tag>{interview.gradeName}</Tag>
                    <Tag color={interviewStatusColour(interview.status)}>
                      {interviewStatusLabel(interview.status)}
                    </Tag>
                    {past && stillToRecord && <Tag color="red">Needs an outcome</Tag>}
                  </Space>
                }
                extra={
                  stillToRecord ? (
                    <Button
                      type="primary"
                      size="small"
                      icon={<FormOutlined />}
                      onClick={() => setRecording(interview)}
                    >
                      Record how it went
                    </Button>
                  ) : undefined
                }
              >
                <Space direction="vertical" size={4} style={{ width: '100%' }}>
                  <Space wrap size={16}>
                    <Text>
                      <CalendarOutlined /> {whenItIs(interview)}
                    </Text>
                    <Text>
                      <ClockCircleOutlined /> {prettyTime(interview.scheduledTime)}
                    </Text>
                    {interview.location && (
                      <Text type="secondary">
                        <EnvironmentOutlined /> {interview.location}
                      </Text>
                    )}
                    {interview.meetingLink && (
                      <a href={interview.meetingLink} target="_blank" rel="noreferrer">
                        <VideoCameraOutlined /> Join online
                      </a>
                    )}
                  </Space>

                  <Text type="secondary" style={{ fontSize: 12 }}>
                    Application {interview.applicationNumber}
                  </Text>

                  {interview.status === InterviewStatus.Completed && (
                    <Space direction="vertical" size={0} style={{ marginTop: 8 }}>
                      <Text>
                        You rated this {interview.rating} out of 5 and{' '}
                        {interview.recommended ? 'recommended a place' : 'did not recommend a place'}.
                      </Text>
                      {interview.notes && (
                        <Tooltip title="What you wrote at the time">
                          <Text type="secondary">{interview.notes}</Text>
                        </Tooltip>
                      )}
                    </Space>
                  )}
                </Space>
              </Card>
            );
          }}
        />
      )}

      <RecordInterviewOutcomeModal
        open={!!recording}
        interview={recording}
        onClose={() => setRecording(undefined)}
        onRecorded={() => load(showing === 'all')}
      />
    </div>
  );
}

export default function MyInterviewsContent() {
  return (
    <AdmissionInterviewProvider>
      <MyInterviews />
    </AdmissionInterviewProvider>
  );
}
