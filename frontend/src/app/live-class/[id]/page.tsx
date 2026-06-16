'use client';

import React from 'react';
import { useParams, useRouter } from 'next/navigation';
import { App } from 'antd';
import { OnlineLessonProvider } from '@/providers/learning/online_lessons';
import { LiveClassroom } from '@/components/modules/learning/LiveClassroom';

/**
 * In-app live classroom route (LC-02). Reached at /live-class/{lessonId} — the
 * same path stored as the InApp lesson's MeetingLink (see LC-01 CreateAsync).
 * Both teachers and students land here; the backend join token decides whether
 * they publish (host) or only watch (student).
 */
export default function LiveClassPage() {
  const params = useParams();
  const router = useRouter();
  const id = typeof params?.id === 'string' ? params.id : Array.isArray(params?.id) ? params.id[0] : '';

  return (
    <App>
      <OnlineLessonProvider>
        <div style={{ height: '100vh', background: '#0b0d12' }}>
          <LiveClassroom lessonId={id} onLeave={() => router.back()} />
        </div>
      </OnlineLessonProvider>
    </App>
  );
}
