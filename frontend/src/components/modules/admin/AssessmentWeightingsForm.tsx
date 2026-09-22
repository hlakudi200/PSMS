'use client';

/**
 * A school's School-Based Assessment / examination split per grade band — what
 * a year-end subject mark is made of.
 *
 * Defaults are the national ones from DBE Circular S8 of 2023. A school may
 * change them, because an independent school can run an approved variation, but
 * a departure is called out rather than saved silently.
 */

import React, { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Card,
  InputNumber,
  Popconfirm,
  Space,
  Table,
  Tag,
  Tooltip,
  Typography,
  message,
} from 'antd';
import { ReloadOutlined, SaveOutlined } from '@ant-design/icons';
import { z } from 'zod';
import {
  AssessmentWeightingProvider,
  useAssessmentWeightingActions,
  useAssessmentWeightingState,
} from '@/providers/assessment/weightings';
import type {
  AssessmentWeightingBand,
  IAssessmentWeighting,
} from '@/providers/assessment/weightings';

const { Text, Paragraph } = Typography;

/**
 * Mirrors the backend guard. The two halves have to account for the whole mark:
 * anything else silently under- or over-states every learner's final result.
 */
const bandSchema = z
  .object({
    sbaPercentage: z.number().int().min(0).max(100),
    examPercentage: z.number().int().min(0).max(100),
  })
  .refine((d) => d.sbaPercentage + d.examPercentage === 100, {
    message: 'School-based assessment and examination must total 100%',
  });

type Draft = Record<number, { sbaPercentage: number; examPercentage: number }>;

function AssessmentWeightingsContent() {
  const { weightings, isPending } = useAssessmentWeightingState();
  const { getAllAsync, updateAsync, resetToDefaultsAsync } = useAssessmentWeightingActions();

  const [draft, setDraft] = useState<Draft>({});
  const [saving, setSaving] = useState(false);
  const [resetting, setResetting] = useState(false);

  useEffect(() => {
    getAllAsync();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Rebuild the draft whenever the server's copy changes, so Save and Reset
  // both leave the form showing what was actually stored.
  useEffect(() => {
    if (!weightings) return;
    setDraft(
      Object.fromEntries(
        weightings.map((w) => [
          w.band,
          { sbaPercentage: w.sbaPercentage, examPercentage: w.examPercentage },
        ])
      )
    );
  }, [weightings]);

  const rowOf = (band: AssessmentWeightingBand) => draft[band];

  /** Bands whose two halves do not total 100. Save stays disabled while any exist. */
  const invalidBands = useMemo(
    () =>
      (weightings ?? []).filter((w) => {
        const d = draft[w.band];
        return d ? !bandSchema.safeParse(d).success : false;
      }),
    [weightings, draft]
  );

  /** Bands the school has moved away from the national default. */
  const departures = useMemo(
    () =>
      (weightings ?? []).filter((w) => {
        const d = draft[w.band];
        return d
          ? d.sbaPercentage !== w.policySbaPercentage ||
              d.examPercentage !== w.policyExamPercentage
          : false;
      }),
    [weightings, draft]
  );

  const dirty = useMemo(
    () =>
      (weightings ?? []).some((w) => {
        const d = draft[w.band];
        return d
          ? d.sbaPercentage !== w.sbaPercentage || d.examPercentage !== w.examPercentage
          : false;
      }),
    [weightings, draft]
  );

  const setValue = (
    band: AssessmentWeightingBand,
    field: 'sbaPercentage' | 'examPercentage',
    value: number | null
  ) => {
    const next = value ?? 0;
    setDraft((prev) => {
      const current = prev[band] ?? { sbaPercentage: 0, examPercentage: 0 };
      // Moving one half moves the other, since they always total 100. The actor
      // can still correct either afterwards.
      return {
        ...prev,
        [band]:
          field === 'sbaPercentage'
            ? { sbaPercentage: next, examPercentage: 100 - next }
            : { sbaPercentage: 100 - next, examPercentage: next },
      };
    });
  };

  const handleSave = async () => {
    if (invalidBands.length > 0) return;
    setSaving(true);
    try {
      await updateAsync(
        (weightings ?? []).map((w) => ({
          band: w.band,
          sbaPercentage: draft[w.band]?.sbaPercentage ?? w.sbaPercentage,
          examPercentage: draft[w.band]?.examPercentage ?? w.examPercentage,
        }))
      );
      message.success('Assessment weightings saved');
    } catch {
      // Surfaced by the axios error interceptor.
    } finally {
      setSaving(false);
    }
  };

  const handleReset = async () => {
    setResetting(true);
    try {
      await resetToDefaultsAsync();
      message.success('Weightings returned to the national defaults');
    } catch {
      // Surfaced by the axios error interceptor.
    } finally {
      setResetting(false);
    }
  };

  const columns = [
    {
      title: 'Grade band',
      dataIndex: 'bandName',
      key: 'bandName',
      render: (name: string, record: IAssessmentWeighting) => {
        const d = rowOf(record.band);
        const departed =
          d &&
          (d.sbaPercentage !== record.policySbaPercentage ||
            d.examPercentage !== record.policyExamPercentage);
        return (
          <Space direction="vertical" size={0}>
            <Text strong>{name}</Text>
            {record.examinationIsExternal && (
              <Tooltip title="The examination half is the National Senior Certificate paper, set and marked nationally. The school holds only the 25% school-based assessment, so a Grade 12 final mark cannot be computed here.">
                <Tag color="blue" style={{ marginTop: 4 }}>
                  External examination
                </Tag>
              </Tooltip>
            )}
            {departed && (
              <Tooltip
                title={`The national default is ${record.policySbaPercentage}% school-based assessment and ${record.policyExamPercentage}% examination.`}
              >
                <Tag color="orange" style={{ marginTop: 4 }}>
                  Differs from policy
                </Tag>
              </Tooltip>
            )}
          </Space>
        );
      },
    },
    {
      title: 'School-based assessment %',
      key: 'sba',
      width: 220,
      render: (_: unknown, record: IAssessmentWeighting) => (
        <InputNumber
          min={0}
          max={100}
          value={rowOf(record.band)?.sbaPercentage}
          onChange={(v) => setValue(record.band, 'sbaPercentage', v)}
          style={{ width: '100%' }}
          addonAfter="%"
        />
      ),
    },
    {
      title: 'Examination %',
      key: 'exam',
      width: 220,
      render: (_: unknown, record: IAssessmentWeighting) => (
        <InputNumber
          min={0}
          max={100}
          value={rowOf(record.band)?.examPercentage}
          onChange={(v) => setValue(record.band, 'examPercentage', v)}
          style={{ width: '100%' }}
          addonAfter="%"
        />
      ),
    },
    {
      title: 'Total',
      key: 'total',
      width: 110,
      render: (_: unknown, record: IAssessmentWeighting) => {
        const d = rowOf(record.band);
        const total = (d?.sbaPercentage ?? 0) + (d?.examPercentage ?? 0);
        return (
          <Text type={total === 100 ? 'secondary' : 'danger'} strong={total !== 100}>
            {total}%
          </Text>
        );
      },
    },
    {
      title: 'National default',
      key: 'policy',
      width: 160,
      render: (_: unknown, record: IAssessmentWeighting) => (
        <Text type="secondary">
          {record.policySbaPercentage} : {record.policyExamPercentage}
        </Text>
      ),
    },
  ];

  return (
    <Card
      title="Assessment weightings"
      style={{ marginBottom: 24 }}
      extra={
        <Space>
          <Popconfirm
            title="Return every band to the national default?"
            description="Any weighting this school has changed will be overwritten."
            onConfirm={handleReset}
          >
            <Button icon={<ReloadOutlined />} loading={resetting} disabled={isPending}>
              Reset to policy defaults
            </Button>
          </Popconfirm>
          <Button
            type="primary"
            icon={<SaveOutlined />}
            onClick={handleSave}
            loading={saving}
            disabled={isPending || !dirty || invalidBands.length > 0}
          >
            Save
          </Button>
        </Space>
      }
    >
      <Paragraph type="secondary" style={{ marginTop: 0 }}>
        How much of a year-end subject mark comes from the year&rsquo;s school-based assessment,
        and how much from the end-of-year examination. Defaults are the national weightings in{' '}
        <Text strong>DBE Circular S8 of 2023</Text>. Change them only if this school has an
        approved variation.
      </Paragraph>

      {invalidBands.length > 0 && (
        <Alert
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
          message="Each band must total 100%"
          description={invalidBands.map((b) => b.bandName).join(', ')}
        />
      )}

      {invalidBands.length === 0 && departures.length > 0 && (
        <Alert
          type="warning"
          showIcon
          style={{ marginBottom: 16 }}
          message="Some bands differ from the national policy"
          description={`${departures
            .map((b) => b.bandName)
            .join(', ')}. This is allowed, but marks for these grades will not match the DBE weighting.`}
        />
      )}

      <Table<IAssessmentWeighting>
        rowKey="band"
        dataSource={weightings ?? []}
        columns={columns}
        loading={isPending && !weightings}
        pagination={false}
        size="small"
      />

      <Space direction="vertical" size={4} style={{ marginTop: 12 }}>
        <Text type="secondary" style={{ fontSize: 12 }}>
          Foundation Phase has no examination component — Grades R&ndash;3 are assessed entirely
          through school-based assessment.
        </Text>
        <Text type="secondary" style={{ fontSize: 12 }}>
          Grade 12&rsquo;s examination is the external National Senior Certificate paper, set and
          marked nationally. The school holds only the 25% school-based assessment, so a final
          Grade 12 mark comes from the DBE, not from here.
        </Text>
        <Text type="secondary" style={{ fontSize: 12 }}>
          Life Orientation is an exception at 100% school-based assessment in Grades 10&ndash;12
          (NPPPPR §31(2)). These weightings are per band, so that subject is not yet handled.
        </Text>
      </Space>
    </Card>
  );
}

export default function AssessmentWeightingsForm() {
  return (
    <AssessmentWeightingProvider>
      <AssessmentWeightingsContent />
    </AssessmentWeightingProvider>
  );
}
