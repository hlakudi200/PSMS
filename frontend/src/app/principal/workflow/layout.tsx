'use client';

import React from 'react';
import WorkflowTabsLayout from '@/components/modules/workflow/WorkflowTabsLayout';

export default function PrincipalWorkflowLayout({ children }: { children: React.ReactNode }) {
  return <WorkflowTabsLayout>{children}</WorkflowTabsLayout>;
}
