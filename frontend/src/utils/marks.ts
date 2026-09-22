/**
 * How a mark is written down.
 *
 * RC-07. A mark is stored as `decimal(5,2)` and printed to one decimal in three
 * places — the report card PDF, the browser print view and the detail table —
 * which have to agree to the last digit. The PDF rounds with .NET's decimal
 * arithmetic, half away from zero. `Number.toFixed` rounds a binary double,
 * so 72.45 came out "72.5" on paper and "72.4" on screen.
 *
 * This recovers the exact stored hundredths first, then rounds them the same
 * way the PDF does.
 */
export function formatMark(
  value: number | null | undefined,
  decimals = 1,
): string {
  if (value == null || Number.isNaN(value)) return '-';

  // The stored value has at most two decimals, so this is exact.
  const hundredths = Math.round(value * 100);
  const scaled = hundredths / 10 ** (2 - decimals);

  // Math.round is half-up; away-from-zero only differs below zero, which a
  // percentage never is — but a mark is not the only thing that reaches here.
  const rounded = scaled < 0 ? -Math.round(-scaled) : Math.round(scaled);

  return (rounded / 10 ** decimals).toFixed(decimals);
}

/** As {@link formatMark}, with the per-cent sign, or a dash when there is no mark. */
export function formatPercentage(
  value: number | null | undefined,
  decimals = 1,
): string {
  const mark = formatMark(value, decimals);

  return mark === '-' ? mark : `${mark}%`;
}
