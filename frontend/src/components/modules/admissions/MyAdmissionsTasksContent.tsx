'use client';

import React, { useCallback, useEffect, useState } from 'react';
import {
  Alert, Badge, Button, Card, Empty, List, Segmented, Space, Spin, Tabs, Tag, Typography,
} from 'antd';
import {
  CalendarOutlined, ClockCircleOutlined, EnvironmentOutlined, FileDoneOutlined, FormOutlined,
  VideoCameraOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import {
  AdmissionInterviewProvider, useAdmissionInterviewActions,
} from '@/providers/admissions/admission_interviews';
import {
  AdmissionAssessmentProvider, useAdmissionAssessmentActions,
} from '@/providers/admissions/admission_assessments';
import {
  InterviewStatus, interviewStatusColour, interviewStatusLabel,
  admissionAssessmentTypeLabel,
  type IAdmissionAssessment, type IAdmissionInterview,
} from '@/providers/admissions/shared/interfaces';
import RecordInterviewOutcomeModal from '@/components/modals/admissions/RecordInterviewOutcomeModal';
import RecordAssessmentResultModal from '@/components/modals/admissions/RecordAssessmentResultModal';

const { Title, Paragraph, Text } = Typography;

/** A time that arrives as "14:30:00". */
const prettyTime = (time?: string) => (time ? time.slice(0, 5) : '—');

const whenItIs = (iso: string) => {
  const date = dayjs(iso);
  if (date.isSame(dayjs(), 'day')) return 'Today';
  if (date.isSame(dayjs().add(1, 'day'), 'day')) return 'Tomorrow';
  return date.format('ddd DD MMM YYYY');
};

const isOverdue = (iso: string, done: boolean) => !done && dayjs(iso).isBefore(dayjs(), 'day');

function Interviews({ showing }: { showing: 'todo' | 'all' }) {
  const { getMineAsync } = useAdmissionInterviewActions();
  const [interviews, setInterviews] = useState<IAdmissionInterview[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | undefined>();
  const [recording, setRecording] = useState<IAdmissionInterview | undefined>();

  const load = useCallback(() => {
    setLoading(true);
    setError(undefined);
    getMineAsync(showing === 'all')
      .then(setInterviews)
      .catch(() => setError('Could not load your interviews.'))
      .finally(() => setLoading(false));
  }, [getMineAsync, showing]);

  useEffect(() => { load(); }, [showing]);

  const stillToRecord = (i: IAdmissionInterview) =>
    i.status === InterviewStatus.Scheduled || i.status === InterviewStatus.Rescheduled;

  const overdue = interviews.filter((i) => isOverdue(i.scheduledDate, !stillToRecord(i)));

  if (loading) return <Spin />;
  if (error) return <Alert type="error" showIcon message={error} />;

  if (interviews.length === 0) {
    return (
      <Card>
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description={
            showing === 'todo'
              ? 'Nothing booked for you at the moment'
              : 'You have not been asked to conduct any interviews'
          }
        />
      </Card>
    );
  }

  return (
    <>
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

      <List
        dataSource={interviews}
        renderItem={(interview) => (
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
                {isOverdue(interview.scheduledDate, !stillToRecord(interview)) && (
                  <Tag color="red">Needs an outcome</Tag>
                )}
              </Space>
            }
            extra={
              stillToRecord(interview) ? (
                <Button type="primary" size="small" icon={<FormOutlined />} onClick={() => setRecording(interview)}>
                  Record how it went
                </Button>
              ) : undefined
            }
          >
            <Space direction="vertical" size={4} style={{ width: '100%' }}>
              <Space wrap size={16}>
                <Text><CalendarOutlined /> {whenItIs(interview.scheduledDate)}</Text>
                <Text><ClockCircleOutlined /> {prettyTime(interview.scheduledTime)}</Text>
                {interview.location && (
                  <Text type="secondary"><EnvironmentOutlined /> {interview.location}</Text>
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
                  {interview.notes && <Text type="secondary">{interview.notes}</Text>}
                </Space>
              )}
            </Space>
          </Card>
        )}
      />

      <RecordInterviewOutcomeModal
        open={!!recording}
        interview={recording}
        onClose={() => setRecording(undefined)}
        onRecorded={load}
      />
    </>
  );
}

function Assessments({ showing }: { showing: 'todo' | 'all' }) {
  const { getMineAsync } = useAdmissionAssessmentActions();
  const [assessments, setAssessments] = useState<IAdmissionAssessment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | undefined>();
  const [recording, setRecording] = useState<IAdmissionAssessment | undefined>();

  const load = useCallback(() => {
    setLoading(true);
    setError(undefined);
    getMineAsync(showing === 'all')
      .then(setAssessments)
      .catch(() => setError('Could not load your assessments.'))
      .finally(() => setLoading(false));
  }, [getMineAsync, showing]);

  useEffect(() => { load(); }, [showing]);

  const overdue = assessments.filter((a) => isOverdue(a.scheduledDate, a.isCompleted));

  if (loading) return <Spin />;
  if (error) return <Alert type="error" showIcon message={error} />;

  if (assessments.length === 0) {
    return (
      <Card>
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description={
            showing === 'todo'
              ? 'Nothing to set or mark at the moment'
              : 'You have not been asked to set any assessments'
          }
        />
      </Card>
    );
  }

  return (
    <>
      {overdue.length > 0 && (
        <Alert
          type="warning"
          showIcon
          style={{ marginBottom: 16 }}
          message={
            overdue.length === 1
              ? 'One assessment has passed without a result'
              : `${overdue.length} assessments have passed without a result`
          }
          description="The principal cannot decide on these applications until the marks are in."
        />
      )}

      <List
        dataSource={assessments}
        renderItem={(assessment) => (
          <Card
            size="small"
            style={{ marginBottom: 12 }}
            title={
              <Space wrap>
                <Text strong>{assessment.applicantName}</Text>
                <Tag>{assessment.gradeName}</Tag>
                <Tag color="blue">{admissionAssessmentTypeLabel(assessment.type)}</Tag>
                {assessment.isCompleted && (
                  <Tag color={assessment.passed ? 'green' : 'red'}>
                    {assessment.passed ? 'Passed' : 'Did not pass'}
                  </Tag>
                )}
                {isOverdue(assessment.scheduledDate, assessment.isCompleted) && (
                  <Tag color="red">Needs a result</Tag>
                )}
              </Space>
            }
            extra={
              !assessment.isCompleted ? (
                <Button type="primary" size="small" icon={<FileDoneOutlined />} onClick={() => setRecording(assessment)}>
                  Record the result
                </Button>
              ) : undefined
            }
          >
            <Space direction="vertical" size={4} style={{ width: '100%' }}>
              <Space wrap size={16}>
                <Text><CalendarOutlined /> {whenItIs(assessment.scheduledDate)}</Text>
                <Text type="secondary">Out of {assessment.maxScore}</Text>
              </Space>
              {assessment.subjects && (
                <Text type="secondary">Covers: {assessment.subjects.split(/\r?\n/).filter(Boolean).join(', ')}</Text>
              )}
              <Text type="secondary" style={{ fontSize: 12 }}>
                Application {assessment.applicationNumber}
              </Text>
              {assessment.isCompleted && (
                <Space direction="vertical" size={0} style={{ marginTop: 8 }}>
                  <Text>
                    {assessment.totalScore} out of {assessment.maxScore} — {assessment.percentageDisplay}
                  </Text>
                  {assessment.feedback && <Text type="secondary">{assessment.feedback}</Text>}
                </Space>
              )}
            </Space>
          </Card>
        )}
      />

      <RecordAssessmentResultModal
        open={!!recording}
        assessment={recording}
        onClose={() => setRecording(undefined)}
        onRecorded={load}
      />
    </>
  );
}

/**
 * What a teacher has been asked to do for admissions.
 *
 * Two steps of the admissions workflow belong to the teacher — the interview
 * and the placement assessment — and both were invisible to them: each record
 * could only be read through the application it belongs to, and a teacher
 * cannot read applications. So the person expected to sit in the room, or mark
 * the paper, was the one person who could not see that they were expected.
 */
function MyAdmissionsTasks() {
  const [showing, setShowing] = useState<'todo' | 'all'>('todo');

  return (
    <div style={{ padding: 24, maxWidth: 900 }}>
      <Space style={{ width: '100%', justifyContent: 'space-between', marginBottom: 4 }} wrap>
        <Title level={3} style={{ margin: 0 }}>Admissions</Title>
        <Segmented
          value={showing}
          onChange={(v) => setShowing(v as 'todo' | 'all')}
          options={[
            { label: 'Still to do', value: 'todo' },
            { label: 'Everything', value: 'all' },
          ]}
        />
      </Space>
      <Paragraph type="secondary">
        Interviews you have been asked to conduct, and assessments you have been asked to set.
      </Paragraph>

      <Tabs
        items={[
          {
            key: 'interviews',
            label: 'Interviews',
            children: <Interviews showing={showing} />,
          },
          {
            key: 'assessments',
            label: 'Assessments',
            children: <Assessments showing={showing} />,
          },
        ]}
      />
    </div>
  );
}

export default function MyAdmissionsTasksContent() {
  return (
    <AdmissionInterviewProvider>
      <AdmissionAssessmentProvider>
        <MyAdmissionsTasks />
      </AdmissionAssessmentProvider>
    </AdmissionInterviewProvider>
  );
}
