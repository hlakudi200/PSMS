'use client';

import React, { useState } from 'react';
import { Card, Descriptions, Tag, Select, message, Typography } from 'antd';
import { useAuthState } from '@/providers/auth';
import { getAxiosInstance } from '@/utils/axios-instance';
import BrandingSettingsForm from './BrandingSettingsForm';
import AssessmentWeightingsForm from './AssessmentWeightingsForm';

const { Title } = Typography;

const themeOptions = [
  { value: 'default', label: 'Default' },
  { value: 'red', label: 'Red' },
  { value: 'green', label: 'Green' },
  { value: 'blue', label: 'Blue' },
];

export default function SettingsPageContent() {
  const { currentTenant } = useAuthState();
  const [themeLoading, setThemeLoading] = useState(false);
  const [currentTheme, setCurrentTheme] = useState('default');

  const handleThemeChange = async (theme: string) => {
    setThemeLoading(true);
    try {
      const instance = getAxiosInstance();
      await instance.post('/api/services/app/Configuration/ChangeUiTheme', { theme });
      setCurrentTheme(theme);
      message.success('Theme updated successfully');
    } catch {
      message.error('Failed to update theme');
    } finally {
      setThemeLoading(false);
    }
  };

  return (
    <div>
      <Title level={4} style={{ marginBottom: 24 }}>School Settings</Title>

      <Card title="School Information" style={{ marginBottom: 24 }}>
        <Descriptions column={{ xs: 1, sm: 2 }} bordered>
          <Descriptions.Item label="School Name">
            {currentTenant?.name ?? '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Tenancy Name">
            {currentTenant?.tenancyName ?? '—'}
          </Descriptions.Item>
          <Descriptions.Item label="Status">
            {currentTenant ? (
              <Tag color={currentTenant.isActive ? 'green' : 'default'}>
                {currentTenant.isActive ? 'Active' : 'Inactive'}
              </Tag>
            ) : '—'}
          </Descriptions.Item>
        </Descriptions>
      </Card>

      <BrandingSettingsForm />

      <AssessmentWeightingsForm />

      <Card title="UI Theme">
        <div style={{ maxWidth: 320 }}>
          <p style={{ marginBottom: 12, color: '#595959', fontSize: 13 }}>
            Select the UI theme for your school portal.
          </p>
          <Select
            value={currentTheme}
            onChange={handleThemeChange}
            options={themeOptions}
            loading={themeLoading}
            style={{ width: '100%' }}
          />
        </div>
      </Card>
    </div>
  );
}
