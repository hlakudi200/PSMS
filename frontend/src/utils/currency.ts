// Shared ZAR currency formatter. Used in any UI that displays monetary amounts.
const formatter = new Intl.NumberFormat('en-ZA', {
  style: 'currency',
  currency: 'ZAR',
  maximumFractionDigits: 2,
});

export const formatZAR = (amount: number | null | undefined): string => {
  if (amount == null || Number.isNaN(amount)) return '—';
  return formatter.format(amount);
};
