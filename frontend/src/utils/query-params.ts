/**
 * Converts a flat input object into URLSearchParams for ABP API calls.
 * - Converts camelCase keys to PascalCase (ABP convention)
 * - Skips undefined, null, and empty string values
 * - Converts all values to strings
 */
export function buildQueryParams(input?: Record<string, unknown>): URLSearchParams {
  const params = new URLSearchParams();
  if (!input) return params;

  for (const [key, value] of Object.entries(input)) {
    if (value === undefined || value === null || value === '') continue;
    const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
    params.append(pascalKey, String(value));
  }

  return params;
}
