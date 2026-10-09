'use client';

import { useCallback } from 'react';
import { usePathname, useRouter, useSearchParams } from 'next/navigation';

/**
 * Writes page filter state back into the query string, so a filtered page is
 * shareable and survives a refresh. Uses `replace` (no history entry per
 * selector change) and keeps any params the patch doesn't mention. An
 * undefined or empty value removes the key.
 *
 * Callers must sit inside a Suspense boundary (useSearchParams).
 */
export function useReplaceQuery() {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();

  return useCallback(
    (patch: Record<string, string | undefined>) => {
      const params = new URLSearchParams(searchParams.toString());
      Object.entries(patch).forEach(([key, value]) => {
        if (value) params.set(key, value);
        else params.delete(key);
      });
      const query = params.toString();
      router.replace(query ? `${pathname}?${query}` : pathname, { scroll: false });
    },
    [router, pathname, searchParams]
  );
}
