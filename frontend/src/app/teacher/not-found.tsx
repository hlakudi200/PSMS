'use client';

// Renders inside frontend/src/app/teacher/layout.tsx, so the sidebar and
// header stay visible — a teacher who clicks a menu item whose route does
// not exist yet (e.g. /teacher/classes before T-T03 lands) sees a friendly
// "coming soon" panel without losing navigation.
//
// Next.js App Router scopes not-found.tsx to its route segment and respects
// the segment's layout, which is what gives us the inside-shell render.

import { Card, Button } from 'antd';
import { ClockCircleOutlined, HomeOutlined } from '@ant-design/icons';
import { useRouter } from 'next/navigation';

export default function TeacherNotFound() {
  const router = useRouter();

  return (
    <Card variant="borderless" style={{ textAlign: 'center' }}>
      <div style={{ padding: '32px 16px' }}>
        <ClockCircleOutlined style={{ fontSize: 48, color: '#2F54EB', marginBottom: 16 }} />
        <h2 style={{ fontSize: 20, margin: '0 0 8px', color: '#262626' }}>
          Coming soon
        </h2>
        <p style={{ color: '#595959', maxWidth: 480, margin: '0 auto 24px' }}>
          This section of the Teacher Portal is being built. Future tickets in the
          teacher-portal milestone will replace this placeholder with the real page.
        </p>
        <Button
          type="primary"
          icon={<HomeOutlined />}
          onClick={() => router.push('/teacher')}
        >
          Back to dashboard
        </Button>
      </div>
    </Card>
  );
}
