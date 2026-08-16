import type { ThemeConfig } from "antd";

/**
 * PSMS Theme Configuration
 *
 * Inspired by classic enterprise systems (Oracle EBS, SAP GUI)
 * - Professional, utilitarian design
 * - High contrast for readability
 * - Dense information display
 * - Classic color palette
 * - Grid-based layouts
 */

export const psmsTheme: ThemeConfig = {
  token: {
    // === COLOR SYSTEM ===
    // Primary: Classic business blue (Oracle/SAP style)
    colorPrimary: "#0066CC", // Deep professional blue
    colorSuccess: "#52c41a", // Standard green
    colorWarning: "#faad14", // Standard amber
    colorError: "#ff4d4f", // Standard red
    colorInfo: "#1890ff", // Info blue

    // Background colors - Light gray enterprise feel
    colorBgBase: "#F5F5F5", // Page background (light gray like SAP)
    colorBgContainer: "#FFFFFF", // Card/container backgrounds
    colorBgElevated: "#FFFFFF", // Modal/dropdown backgrounds
    colorBgLayout: "#E5E5E5", // Layout background (slightly darker gray)

    // Text colors - High contrast
    colorText: "#262626", // Primary text - very dark gray
    colorTextSecondary: "#595959", // Secondary text
    colorTextTertiary: "#8C8C8C", // Tertiary text
    colorTextQuaternary: "#BFBFBF", // Disabled text

    // Border colors - Defined borders like old systems
    colorBorder: "#D9D9D9", // Default border
    colorBorderSecondary: "#E8E8E8", // Secondary border (lighter)

    // === TYPOGRAPHY ===
    fontFamily: '"Segoe UI", Tahoma, Geneva, Verdana, sans-serif', // Classic Windows system fonts
    fontSize: 13, // Slightly smaller, denser text like enterprise systems
    fontSizeHeading1: 28,
    fontSizeHeading2: 24,
    fontSizeHeading3: 20,
    fontSizeHeading4: 16,
    fontSizeHeading5: 14,

    // === SPACING ===
    // Tighter spacing for information density
    padding: 12, // Default padding
    paddingLG: 16, // Large padding
    paddingSM: 8, // Small padding
    paddingXS: 4, // Extra small padding

    margin: 12,
    marginLG: 16,
    marginSM: 8,
    marginXS: 4,

    // === BORDERS & RADIUS ===
    borderRadius: 2, // Minimal radius - sharp corners like old systems
    borderRadiusLG: 4,
    borderRadiusSM: 2,

    // === COMPONENT SIZING ===
    controlHeight: 32, // Standard control height
    controlHeightLG: 40, // Large controls
    controlHeightSM: 24, // Small controls

    // === SHADOWS ===
    // Minimal shadows - flat design like enterprise systems
    boxShadow: "0 1px 2px 0 rgba(0, 0, 0, 0.03), 0 1px 6px -1px rgba(0, 0, 0, 0.02), 0 2px 4px 0 rgba(0, 0, 0, 0.02)",
    boxShadowSecondary: "0 1px 4px 0 rgba(0, 0, 0, 0.08)",

    // === LAYOUT ===
    lineWidth: 1, // Thin borders
    lineType: "solid",

    // === MOTION ===
    motion: false, // Disable animations - snappy like old systems
  },

  // === COMPONENT-SPECIFIC OVERRIDES ===
  components: {
    // LAYOUT
    Layout: {
      headerBg: "#003D73", // Dark blue header like Oracle
      headerColor: "#FFFFFF",
      headerHeight: 56,
      headerPadding: "0 24px",
      siderBg: "#F0F0F0", // Light gray sidebar
      triggerBg: "#002C54",
      triggerColor: "#FFFFFF",
      bodyBg: "#F5F5F5",
    },

    // MENU
    Menu: {
      itemBg: "transparent",
      itemColor: "#262626",
      itemHoverBg: "#E6F7FF",
      itemHoverColor: "#0066CC",
      itemSelectedBg: "#BAE7FF",
      itemSelectedColor: "#0066CC",
      itemActiveBg: "#BAE7FF",
      itemHeight: 36,
      fontSize: 13,
      iconSize: 16,
      collapsedWidth: 56,
      subMenuItemBg: "#FAFAFA",
    },

    // TABLE - Dense, grid-based like SAP
    Table: {
      headerBg: "#E8E8E8", // Gray header like classic grids
      headerColor: "#262626",
      headerSplitColor: "#D9D9D9",
      rowHoverBg: "#F0F7FF",
      rowSelectedBg: "#E6F7FF",
      rowSelectedHoverBg: "#D4EDFF",
      borderColor: "#D9D9D9",
      headerBorderRadius: 0, // No radius - sharp corners
      cellPaddingBlock: 8, // Compact padding
      cellPaddingInline: 12,
      cellFontSize: 13,
      footerBg: "#FAFAFA",
    },

    // FORM
    Form: {
      labelColor: "#262626",
      labelFontSize: 13,
      labelHeight: 32,
      itemMarginBottom: 16,
      verticalLabelPadding: "0 0 4px",
    },

    // INPUT
    Input: {
      activeBorderColor: "#0066CC",
      hoverBorderColor: "#4096FF",
      activeShadow: "0 0 0 2px rgba(0, 102, 204, 0.1)",
      paddingBlock: 4,
      paddingInline: 11,
      borderRadius: 2,
    },

    // BUTTON
    Button: {
      primaryColor: "#FFFFFF",
      primaryShadow: "none", // Flat buttons
      defaultBorderColor: "#D9D9D9",
      defaultBg: "#FFFFFF",
      defaultColor: "#262626",
      defaultHoverBorderColor: "#0066CC",
      defaultHoverColor: "#0066CC",
      defaultActiveBorderColor: "#0050A3",
      defaultActiveColor: "#0050A3",
      borderRadius: 2,
      contentFontSize: 13,
      paddingBlock: 4,
      paddingInline: 15,
      controlHeight: 32,
      controlHeightLG: 40,
      controlHeightSM: 24,
    },

    // CARD
    Card: {
      headerBg: "#FAFAFA", // Light gray header
      headerHeight: 48,
      headerFontSize: 14,
      headerFontSizeSM: 13,
      boxShadow: "0 1px 2px 0 rgba(0, 0, 0, 0.03), 0 1px 6px -1px rgba(0, 0, 0, 0.02)",
      borderRadius: 2,
      paddingLG: 16,
    },

    // MODAL
    Modal: {
      headerBg: "#FAFAFA",
      contentBg: "#FFFFFF",
      titleFontSize: 16,
      titleColor: "#262626",
      borderRadius: 4,
    },

    // TABS
    Tabs: {
      cardBg: "#FAFAFA",
      itemColor: "#595959",
      itemSelectedColor: "#0066CC",
      itemHoverColor: "#0066CC",
      itemActiveColor: "#0066CC",
      titleFontSize: 13,
      horizontalItemPadding: "12px 0",
      horizontalItemGutter: 32,
      cardHeight: 40,
      cardPadding: "0 16px",
    },

    // BREADCRUMB
    Breadcrumb: {
      itemColor: "#595959",
      lastItemColor: "#262626",
      linkColor: "#0066CC",
      linkHoverColor: "#0050A3",
      separatorColor: "#8C8C8C",
      fontSize: 13,
    },

    // PAGINATION
    Pagination: {
      itemBg: "#FFFFFF",
      itemActiveBg: "#0066CC",
      itemSize: 32,
      itemSizeSM: 24,
      borderRadius: 2,
    },

    // SELECT
    Select: {
      optionSelectedBg: "#E6F7FF",
      optionSelectedColor: "#0066CC",
      optionActiveBg: "#F0F7FF",
      selectorBg: "#FFFFFF",
      borderRadius: 2,
    },

    // DATE PICKER
    DatePicker: {
      cellHoverBg: "#F0F7FF",
      cellActiveWithRangeBg: "#E6F7FF",
      cellBgDisabled: "#F5F5F5",
      cellHoverWithRangeBg: "#E6F7FF",
      borderRadius: 2,
    },

    // NOTIFICATION & MESSAGE
    Notification: {
      width: 384,
      borderRadius: 4,
    },

    Message: {
      contentBg: "#FFFFFF",
      borderRadius: 4,
    },

    // BADGE & TAG
    Badge: {
      dotSize: 6,
      statusSize: 6,
    },

    Tag: {
      defaultBg: "#FAFAFA",
      defaultColor: "#262626",
    },

    // PROGRESS
    Progress: {
      defaultColor: "#0066CC",
      remainingColor: "#F0F0F0",
      circleTextColor: "#262626",
    },

    // STEPS
    Steps: {
      iconSize: 32,
      dotSize: 8,
      titleLineHeight: 32,
    },

    // DRAWER
    Drawer: {
      footerPaddingBlock: 12,
      footerPaddingInline: 16,
    },

    // COLLAPSE
    Collapse: {
      headerBg: "#FAFAFA",
      headerPadding: "12px 16px",
      contentBg: "#FFFFFF",
      contentPadding: "16px",
      borderRadius: 2,
    },

    // DESCRIPTIONS
    Descriptions: {
      labelBg: "#FAFAFA",
      titleColor: "#262626",
      contentColor: "#262626",
      itemPaddingBottom: 12,
      colonMarginLeft: 2,
      colonMarginRight: 8,
    },

    // DIVIDER
    Divider: {
      colorSplit: "#D9D9D9",
      marginLG: 16,
    },

    // STATISTIC
    Statistic: {
      titleFontSize: 13,
      contentFontSize: 24,
    },

    // TIMELINE
    Timeline: {
      dotBg: "#FFFFFF",
      dotBorderWidth: 2,
      itemPaddingBottom: 16,
    },

    // TREE
    Tree: {
      titleHeight: 32,
      nodeHoverBg: "#F0F7FF",
      nodeSelectedBg: "#E6F7FF",
    },

    // TRANSFER
    Transfer: {
      listHeight: 400,
      listWidth: 200,
      itemHeight: 32,
      itemPaddingBlock: 6,
    },

    // TYPOGRAPHY
    Typography: {
      titleMarginTop: "1.2em",
      titleMarginBottom: "0.5em",
    },
  },

  // === ALGORITHM ===
  // Using default light algorithm (no dark mode for enterprise consistency)
};

/**
 * Component-specific style overrides for additional customization
 */
export const componentStyles = {
  // Global styles to inject
  global: `
    /* Remove focus outlines for mouse users, keep for keyboard */
    :focus:not(:focus-visible) {
      outline: none;
    }

    /* Dense table rows */
    .ant-table-small .ant-table-tbody > tr > td {
      padding: 6px 8px;
    }

    /* Toolbar styling - Oracle/SAP style */
    .psms-toolbar {
      background: #F0F0F0;
      border-bottom: 1px solid #D9D9D9;
      padding: 8px 16px;
      display: flex;
      gap: 8px;
      align-items: center;
    }

    /* Form grid layout - classic two-column */
    .psms-form-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 16px 24px;
    }

    /* Full-width form items */
    .psms-form-full {
      grid-column: 1 / -1;
    }

    /* Status badges - system colors */
    .psms-status-active { background: #52c41a; color: white; }
    .psms-status-inactive { background: #8C8C8C; color: white; }
    .psms-status-pending { background: #faad14; color: white; }
    .psms-status-approved { background: var(--psms-primary, #0066CC); color: white; }
    .psms-status-rejected { background: #ff4d4f; color: white; }

    /* Highlight row on hover - more prominent */
    .ant-table-tbody > tr:hover > td {
      background: var(--psms-primary-tint, #E6F7FF) !important;
    }

    /* Selected row - clear indication */
    .ant-table-tbody > tr.ant-table-row-selected > td {
      background: var(--psms-primary-tint-strong, #BAE7FF) !important;
      border-color: var(--psms-primary-tint-strong, #91D5FF);
    }

    /* Sticky headers for long tables */
    .psms-table-sticky .ant-table-thead > tr > th {
      position: sticky;
      top: 0;
      z-index: 10;
    }

    /* Dense info displays */
    .psms-info-dense {
      line-height: 1.4;
      font-size: 13px;
    }

    /* Page header - classic style */
    .psms-page-header {
      background: #FFFFFF;
      border-bottom: 2px solid var(--psms-primary, #0066CC);
      padding: 16px 24px;
      margin-bottom: 16px;
    }

    .psms-page-header h1 {
      margin: 0;
      font-size: 20px;
      font-weight: 600;
      color: #262626;
    }

    /* Button groups - classic toolbar buttons */
    .psms-button-group {
      display: flex;
      gap: 8px;
    }

    .psms-button-group .ant-btn {
      min-width: 80px;
    }

    /* Required field indicator */
    .ant-form-item-required::before {
      color: #ff4d4f !important;
    }

    /* Compact mode adjustments */
    .psms-compact .ant-form-item {
      margin-bottom: 12px;
    }

    .psms-compact .ant-card-body {
      padding: 12px;
    }
  `,
};

/**
 * Per-tenant branding overrides applied on top of the base theme (issue #56).
 */
export interface IThemeBranding {
  /** Action colour — buttons, links, selected states. "#RRGGBB". */
  primaryColor: string;
  /** Chrome colour — app header background and sidebar accent. "#RRGGBB". */
  secondaryColor: string;
}

/**
 * The stock blues baked into the base theme above. Anywhere one of these
 * literals appears in a COMPONENT token it has to be swapped per tenant:
 * Ant Design resolves component tokens ahead of the global `colorPrimary`, so
 * overriding the global alone leaves menus, links, focused inputs, tabs,
 * pagination and badges stubbornly blue for a school that picked red.
 */
const BASE_PRIMARY = "#0066CC";
const BASE_TINT_STRONG = "#BAE7FF"; // selected rows / menu items
const BASE_TINT_SOFT = "#E6F7FF"; // hover states

/**
 * Picks a readable foreground for an arbitrary tenant background.
 *
 * The header, the login image panel and the settings preview all sit on
 * `secondaryColor`, which a school is free to set to anything. Hard-coding
 * white there makes a pale choice (#FFFFFF, #FFF4CC) unreadable across every
 * portal, with nothing rejecting it. Uses WCAG relative luminance.
 */
export const getReadableForeground = (hex: string): string => {
  const value = (hex || "").replace("#", "");
  if (value.length < 6) return "#FFFFFF";

  const channel = (offset: number) => {
    const srgb = parseInt(value.slice(offset, offset + 2), 16) / 255;
    return srgb <= 0.03928
      ? srgb / 12.92
      : Math.pow((srgb + 0.055) / 1.055, 2.4);
  };

  const luminance =
    0.2126 * channel(0) + 0.7152 * channel(2) + 0.0722 * channel(4);

  // 0.179 is the crossover where white and black text give equal contrast.
  return luminance > 0.179 ? "#1F1F1F" : "#FFFFFF";
};

/** Mixes a #RRGGBB colour toward white. `amount` 0 = unchanged, 1 = white. */
const tintTowardWhite = (hex: string, amount: number): string => {
  const value = hex.replace("#", "");
  const channel = (offset: number) => {
    const base = parseInt(value.slice(offset, offset + 2), 16);
    return Math.round(base + (255 - base) * amount)
      .toString(16)
      .padStart(2, "0");
  };
  return `#${channel(0)}${channel(2)}${channel(4)}`.toUpperCase();
};

/**
 * Rewrites the three stock blues wherever they appear in the component token
 * tree. Done by value rather than by naming each of the ~13 sites so that a
 * hard-coded blue added to the base theme later is rebranded automatically
 * instead of silently escaping.
 */
const rebrandComponents = (
  components: ThemeConfig["components"],
  branding: IThemeBranding
): ThemeConfig["components"] => {
  const replacements = new Map<string, string>([
    [BASE_PRIMARY, branding.primaryColor],
    [BASE_TINT_STRONG, tintTowardWhite(branding.primaryColor, 0.73)],
    [BASE_TINT_SOFT, tintTowardWhite(branding.primaryColor, 0.9)],
  ]);

  return Object.fromEntries(
    Object.entries(components ?? {}).map(([component, tokens]) => [
      component,
      Object.fromEntries(
        Object.entries(tokens as Record<string, unknown>).map(([name, value]) => [
          name,
          typeof value === "string"
            ? replacements.get(value.toUpperCase()) ?? value
            : value,
        ])
      ),
    ])
    // Object.fromEntries widens away Ant Design's per-component token types;
    // the values themselves are untouched apart from the colour swap.
  ) as ThemeConfig["components"];
};

/**
 * Utility function to get theme configuration.
 *
 * Called with a tenant's branding it returns the base PSMS theme rebranded;
 * called with nothing it returns the stock theme unchanged, so any caller that
 * predates branding behaves exactly as before.
 *
 * Note this covers only what Ant Design renders from tokens. The app chrome
 * (header, sidebar accent) is not token-driven — it reads the branding
 * directly, and non-Ant CSS reads the variables from
 * {@link buildBrandingCssVariables}.
 */
export const getPsmsTheme = (branding?: IThemeBranding): ThemeConfig => {
  if (!branding) return psmsTheme;

  const components = rebrandComponents(psmsTheme.components, branding);

  return {
    ...psmsTheme,
    token: {
      ...psmsTheme.token,
      colorPrimary: branding.primaryColor,
      colorLink: branding.primaryColor,
    },
    components: {
      ...components,
      Layout: {
        ...(components?.Layout ?? {}),
        // The header sits on secondaryColor, so its default foreground has to
        // be derived rather than left as a hard-coded white.
        headerColor: getReadableForeground(branding.secondaryColor),
      },
    },
  };
};

/**
 * The `:root` custom properties that let plain CSS (see
 * {@link componentStyles.global}) follow the tenant's palette. Every rule that
 * consumes one keeps a literal fallback, so styles still resolve if this block
 * is ever absent.
 */
export const buildBrandingCssVariables = (branding: IThemeBranding): string => `
    :root {
      --psms-primary: ${branding.primaryColor};
      --psms-secondary: ${branding.secondaryColor};
      --psms-primary-tint: ${tintTowardWhite(branding.primaryColor, 0.9)};
      --psms-primary-tint-strong: ${tintTowardWhite(branding.primaryColor, 0.73)};
      --psms-on-secondary: ${getReadableForeground(branding.secondaryColor)};
    }
  `;

/**
 * Role-based color mappings
 */
export const roleColors = {
  Admin: "#722ED1", // Purple
  Principal: "#0066CC", // Deep blue
  VicePrincipal: "#1890FF", // Bright blue
  HOD: "#13C2C2", // Cyan
  AdmissionsOfficer: "#52C41A", // Green
  Finance: "#FAAD14", // Gold
  Teacher: "#2F54EB", // Blue
  Parent: "#EB2F96", // Magenta
  Student: "#FA8C16", // Orange
  Applicant: "#8C8C8C", // Gray
};

/**
 * Module color coding for navigation
 */
export const moduleColors = {
  Academic: "#0066CC",
  Admissions: "#52C41A",
  Assessment: "#722ED1",
  Learning: "#1890FF",
  Financial: "#FAAD14",
  SASpecific: "#13C2C2",
  Communication: "#EB2F96",
};
