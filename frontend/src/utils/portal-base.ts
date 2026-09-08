'use client';

import { usePathname } from 'next/navigation';

/**
 * Role portals the shared module components can be mounted under. Anything
 * else (e.g. a workflow deep link opened outside a portal) falls back to the
 * principal portal, which is the only one that mounts every module page.
 */
const KNOWN_PORTALS = [
  '/admin',
  '/principal',
  '/academic',
  '/admissions',
  '/finance',
  '/teacher',
  '/parent',
  '/student',
] as const;

export const DEFAULT_PORTAL_BASE = '/principal';

/**
 * Derives the portal root (`/principal`, `/academic`, `/teacher`, …) from a
 * pathname so shared list/detail components can build links relative to the
 * portal they are mounted in rather than hard-coding `/principal/...`, which
 * bounced Vice Principals, HODs and teachers into a role-gated tree.
 */
export function portalBaseFrom(pathname: string | null | undefined): string {
  const first = (pathname ?? '').split('/')[1];
  if (!first) return DEFAULT_PORTAL_BASE;
  const candidate = `/${first}`;
  return (KNOWN_PORTALS as readonly string[]).includes(candidate)
    ? candidate
    : DEFAULT_PORTAL_BASE;
}

export function usePortalBase(): string {
  return portalBaseFrom(usePathname());
}
