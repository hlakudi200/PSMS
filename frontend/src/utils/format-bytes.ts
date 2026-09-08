/**
 * Human-readable file size ("1.4 MB"). Returns an em dash for missing or
 * non-positive sizes so table cells stay aligned.
 */
export function formatBytes(bytes?: number | null): string {
  if (bytes == null || bytes <= 0) return '—';
  const units = ['B', 'KB', 'MB', 'GB'];
  let value = bytes;
  let unit = 0;
  while (value >= 1024 && unit < units.length - 1) {
    value /= 1024;
    unit += 1;
  }
  return `${value.toFixed(unit === 0 ? 0 : 1)} ${units[unit]}`;
}
