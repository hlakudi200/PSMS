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
      // Restore the original attributes on cleanup. Read the raw attribute,
      // not the .href property — the property resolves to an absolute URL, and
      // for a link with no href it resolves to the page itself.
      //
      // `type` and `sizes` matter as much as href: Next emits its own icon as
      // type="image/x-icon" sizes="16x16", and leaving those in place while
      // swapping in a tenant PNG advertises the wrong MIME type and a bogus
      // size hint. Drop them and let the browser sniff.
      const previous = {
        href: existing.getAttribute("href"),
        type: existing.getAttribute("type"),
        sizes: existing.getAttribute("sizes"),
      };

      existing.setAttribute("href", branding.faviconUrl);
      existing.removeAttribute("type");
      existing.removeAttribute("sizes");

      return () => {
        const restore = (name: string, value: string | null) => {
          if (value === null) existing.removeAttribute(name);
          else existing.setAttribute(name, value);
        };
        restore("href", previous.href);
        restore("type", previous.type);
        restore("sizes", previous.sizes);
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
