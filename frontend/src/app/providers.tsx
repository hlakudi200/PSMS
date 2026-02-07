"use client";

import { ConfigProvider } from "antd";
import { AuthProvider } from "@/providers/auth";
import { getPsmsTheme, componentStyles } from "@/utils/theme-config";

export const Providers = ({ children }: { children: React.ReactNode }) => {
  return (
    <>
      <style dangerouslySetInnerHTML={{ __html: componentStyles.global }} />
      <ConfigProvider theme={getPsmsTheme()}>
        <AuthProvider>{children}</AuthProvider>
      </ConfigProvider>
    </>
  );
};
