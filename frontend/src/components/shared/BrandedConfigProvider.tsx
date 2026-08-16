"use client";

import React, { useEffect, useMemo } from "react";
import { ConfigProvider } from "antd";
import { useBrandingState } from "@/providers/branding";
import {
  buildBrandingCssVariables,
  componentStyles,
  getPsmsTheme,
} from "@/utils/theme-config";

/**
 * Applies the current tenant's branding (issue #56) to everything below it:
 *
 *  - Ant Design components, via `ConfigProvider` token overrides
 *  - plain CSS, via `--psms-*` custom properties on `:root`
 *  - the browser tab, via a swapped favicon link
 *
 * Sits inside BrandingProvider so a colour change re-themes the app live,
 * with no reload.
 */
export const BrandedConfigProvider = ({
  children,
}: {
  children: React.ReactNode;
}) => {
  const { branding } = useBrandingState();

  const theme = useMemo(
    () =>
      getPsmsTheme({
        primaryColor: branding.primaryColor,
        secondaryColor: branding.secondaryColor,
      }),
    [branding.primaryColor, branding.secondaryColor]
  );

  const styles = useMemo(
    () =>
      buildBrandingCssVariables({
        primaryColor: branding.primaryColor,
        secondaryColor: branding.secondaryColor,
      }) + componentStyles.global,
    [branding.primaryColor, branding.secondaryColor]
  );

  // The favicon lives in <head>, outside this tree, so it has to be swapped
  // imperatively. Reuses the existing <link rel="icon"> when there is one.
  useEffect(() => {
    if (!branding.faviconUrl) return;

    const existing =
      document.querySelector<HTMLLinkElement>("link[rel~='icon']");

    if (existing) {
      // Restore the original href on cleanup. Read the raw attribute, not the
      // .href property — the property resolves to an absolute URL, and for a
      // link with no href it resolves to the page itself.
      const previous = existing.getAttribute("href");
      existing.setAttribute("href", branding.faviconUrl);

      return () => {
        if (previous === null) existing.removeAttribute("href");
        else existing.setAttribute("href", previous);
      };
    }

    // Nothing to restore — we own this element, so remove it on cleanup.
    const created = document.createElement("link");
    created.rel = "icon";
    created.href = branding.faviconUrl;
    document.head.appendChild(created);

    return () => {
      created.remove();
    };
  }, [branding.faviconUrl]);

  return (
    <>
      <style dangerouslySetInnerHTML={{ __html: styles }} />
      <ConfigProvider theme={theme}>{children}</ConfigProvider>
    </>
  );
};
