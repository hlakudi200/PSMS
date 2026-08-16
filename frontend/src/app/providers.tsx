"use client";

import { AuthProvider } from "@/providers/auth";
import { BrandingProvider } from "@/providers/branding";
import { BrandedConfigProvider } from "@/components/shared/BrandedConfigProvider";

/**
 * Provider order matters:
 *
 *  AuthProvider          owns the session; BrandingProvider watches its token
 *                        so branding reloads on sign-in and resets on sign-out
 *  BrandingProvider      fetches the tenant's palette
 *  BrandedConfigProvider turns that palette into Ant Design tokens, CSS
 *                        variables and the favicon
 *
 * ConfigProvider no longer needs to be outermost — neither auth nor branding
 * renders Ant Design UI of its own.
 */
export const Providers = ({ children }: { children: React.ReactNode }) => {
  return (
    <AuthProvider>
      <BrandingProvider>
        <BrandedConfigProvider>{children}</BrandedConfigProvider>
      </BrandingProvider>
    </AuthProvider>
  );
};
