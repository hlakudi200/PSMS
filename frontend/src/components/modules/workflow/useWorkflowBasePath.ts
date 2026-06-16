'use client';

import { usePathname } from 'next/navigation';

const DEFAULT_BASE = '/admin/workflow';

/**
 * The workflow module is mounted under different role portals (e.g.
 * `/admin/workflow`, `/principal/workflow`). Shared module components must
 * navigate relative to wherever they are mounted rather than hard-coding
 * `/admin/workflow`, or a principal would be bounced into the admin-only tree.
 *
 * Derives the base path from the current route — everything up to and including
 * the `workflow` path SEGMENT (segment-accurate, so a sibling like
 * `/principal/workflows-archive` would never be mistaken for the base). Falls
 * back to `/admin/workflow` if called outside a workflow route (shouldn't
 * happen; these components only render under one).
 */
export function useWorkflowBasePath(): string {
  const segments = (usePathname() ?? '').split('/');
  const idx = segments.indexOf('workflow');
  return idx > 0 ? segments.slice(0, idx + 1).join('/') : DEFAULT_BASE;
}
