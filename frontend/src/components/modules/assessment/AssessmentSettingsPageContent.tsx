'use client';

/**
 * Assessment settings for the principal.
 *
 * The weightings form also appears on the admin School Settings page, but a
 * principal cannot reach /admin — and the SBA / examination split is an
 * academic policy decision they own, not system administration. Hence its own
 * permission (Assessment.Weightings.Manage) and its own home here.
 */

import React from 'react';
import { Typography } from 'antd';
import AssessmentWeightingsForm from '@/components/modules/admin/AssessmentWeightingsForm';

const { Title, Paragraph } = Typography;

export default function AssessmentSettingsPageContent() {
  return (
    <div>
      <Title level={4} style={{ marginBottom: 4 }}>
        Assessment settings
      </Title>
      <Paragraph type="secondary" style={{ marginBottom: 24 }}>
        How this school composes a year-end subject mark.
      </Paragraph>

      <AssessmentWeightingsForm />
    </div>
  );
}
