/**
 * Role checks that survive the case the role arrives in.
 *
 * `getRole()` lower-cases whatever the JWT carries, so `currentRole` is
 * "principal" while role names are written "Principal" everywhere else. A
 * direct `['Principal', ...].includes(currentRole)` is therefore false for
 * every user alive, and silently — the button it guards simply never renders.
 * That is how the Enrol and Schedule buttons went missing from admissions.
 *
 * The server is the authority on what a role may do; these helpers only decide
 * whether to offer the action.
 */
export function isOneOfRoles(currentRole: string | undefined, roles: string[]): boolean {
  const mine = (currentRole ?? '').trim().toLowerCase();
  if (!mine) return false;
  return roles.some((r) => r.trim().toLowerCase() === mine);
}
