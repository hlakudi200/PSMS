'use client';

import React, { useEffect, useState } from 'react';
import {
  Button,
  Card,
  ColorPicker,
  Form,
  Input,
  Space,
  Typography,
  Upload,
  message,
} from 'antd';
import { DeleteOutlined, UploadOutlined } from '@ant-design/icons';
import { z } from 'zod';
import { useBrandingActions, useBrandingState } from '@/providers/branding';
import { getReadableForeground } from '@/utils/theme-config';
import type { BrandingAssetType } from '@/providers/branding/context';

const { Text } = Typography;

const HEX_PATTERN = /^#[0-9A-Fa-f]{6}$/;

// Mirrors the backend's SchoolBranding guards and UpdateSchoolBrandingDto
// annotations, so the same input is rejected on either side.
const brandingSchema = z.object({
  primaryColor: z
    .string()
    .regex(HEX_PATTERN, 'Primary colour must be a #RRGGBB hex value'),
  secondaryColor: z
    .string()
    .regex(HEX_PATTERN, 'Secondary colour must be a #RRGGBB hex value'),
  schoolName: z.string().max(200, 'School name must be 200 characters or fewer'),
});

/** AC 2: logo is PNG/JPG, at most 2MB. */
const LOGO_MAX_BYTES = 2 * 1024 * 1024;
const LOGO_ACCEPT = ['image/png', 'image/jpeg'];
const LOGO_EXTENSIONS = ['.png', '.jpg', '.jpeg'];

const FAVICON_MAX_BYTES = 512 * 1024;
const FAVICON_ACCEPT = ['image/png', 'image/x-icon', 'image/vnd.microsoft.icon'];
const FAVICON_EXTENSIONS = ['.png', '.ico'];

/**
 * Normalises an Ant Design ColorPicker value to the "#RRGGBB" the API accepts.
 *
 * Read from `color.toHexString()`, NOT ColorPicker's second onChange argument —
 * that one is `color.toCssString()`, which returns `rgb(0, 102, 204)`, not hex.
 * toHexString still yields lower case, and 8 digits when alpha < 1, so trim to
 * 7 and upper-case. (`disabledAlpha` is set on both pickers, so the alpha case
 * should not arise; the slice is a belt-and-braces guard.)
 */
const normaliseHex = (color: { toHexString: () => string }): string =>
  (color.toHexString() || '').slice(0, 7).toUpperCase();

interface AssetRowProps {
  label: string;
  description: string;
  url?: string | null;
  accept: string;
  previewHeight: number;
  busy: boolean;
  onSelect: (file: File) => void;
  onClear: () => void;
}

const AssetRow: React.FC<AssetRowProps> = ({
  label,
  description,
  url,
  accept,
  previewHeight,
  busy,
  onSelect,
  onClear,
}) => (
  <div style={{ marginBottom: 20 }}>
    <Text strong style={{ display: 'block', marginBottom: 4 }}>
      {label}
    </Text>
    <Text style={{ display: 'block', marginBottom: 10, fontSize: 12, color: '#595959' }}>
      {description}
    </Text>

    <Space align="center" size={16} wrap>
      <div
        style={{
          width: 140,
          height: previewHeight + 16,
          border: '1px dashed #D9D9D9',
          background: '#FFFFFF',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          padding: 8,
        }}
      >
        {url ? (
          /* eslint-disable-next-line @next/next/no-img-element */
          <img
            src={url}
            alt={`${label} preview`}
            style={{ maxHeight: previewHeight, maxWidth: '100%', objectFit: 'contain' }}
          />
        ) : (
          <Text style={{ fontSize: 12, color: '#8C8C8C' }}>No {label.toLowerCase()}</Text>
        )}
      </div>

      <Space>
        <Upload
          accept={accept}
          showUploadList={false}
          beforeUpload={(file) => {
            onSelect(file);
            // Returning false stops Ant Design uploading it for us — the
            // provider drives the three-step signed-URL flow instead.
            return false;
          }}
        >
          <Button icon={<UploadOutlined />} disabled={busy}>
            {url ? 'Replace' : 'Upload'}
          </Button>
        </Upload>

        {url && (
          <Button icon={<DeleteOutlined />} danger disabled={busy} onClick={onClear}>
            Remove
          </Button>
        )}
      </Space>
    </Space>
  </div>
);

export default function BrandingSettingsForm() {
  const [form] = Form.useForm();
  const { branding, isPending, isConfigured } = useBrandingState();
  const { updateBranding, uploadAsset, removeAsset } = useBrandingActions();
  const [saving, setSaving] = useState(false);

  // Unsaved colour edits. Null means "not touched since the last load", so the
  // pickers derive straight from the saved branding — no effect mirroring
  // state into state, and a successful save re-syncs by clearing the draft.
  const [colorDraft, setColorDraft] = useState<{
    primaryColor: string;
    secondaryColor: string;
  } | null>(null);

  const primaryColor = colorDraft?.primaryColor ?? branding.primaryColor;
  const secondaryColor = colorDraft?.secondaryColor ?? branding.secondaryColor;

  const setPrimaryColor = (hex: string) =>
    setColorDraft({ primaryColor: hex, secondaryColor });
  const setSecondaryColor = (hex: string) =>
    setColorDraft({ primaryColor, secondaryColor: hex });

  // Ant Design's Form holds its own state, so the name field does need to be
  // pushed in when the loaded branding changes. This is not a setState call,
  // so it does not cause the cascading re-render that mirroring colours would.
  //
  // Keyed on configuredSchoolName (the raw stored value), NOT the resolved
  // schoolName. Two reasons:
  //   - uploading a logo returns fresh branding whose resolved name is the
  //     default; keying on that would re-run this effect and wipe a name the
  //     admin had typed but not yet saved.
  //   - echoing the resolved fallback into the field would persist the default
  //     as an explicit choice on the next save, so "leave blank for the
  //     default" would only work once.
  useEffect(() => {
    form.setFieldsValue({ schoolName: branding.configuredSchoolName ?? '' });
  }, [branding.configuredSchoolName, form]);

  // Watched rather than read via getFieldValue so the live preview below
  // updates as the name is typed.
  const watchedSchoolName = Form.useWatch('schoolName', form);

  const handleSave = async () => {
    // primaryColor/secondaryColor are already normalised by the pickers above;
    // the schema is the last guard before the request goes out.
    const result = brandingSchema.safeParse({
      primaryColor,
      secondaryColor,
      schoolName: (form.getFieldValue('schoolName') ?? '').trim(),
    });

    if (!result.success) {
      form.setFields(
        result.error.issues.map((issue) => ({
          name: issue.path as string[],
          errors: [issue.message],
        }))
      );
      message.error(result.error.issues[0].message);
      return;
    }

    setSaving(true);
    const ok = await updateBranding(result.data);
    setSaving(false);

    if (ok) {
      // Drop the draft so the pickers track the saved branding again.
      setColorDraft(null);
      message.success('Branding saved');
    }
  };

  const handleAssetSelect = async (
    assetType: BrandingAssetType,
    file: File,
    maxBytes: number,
    acceptedTypes: string[],
    acceptedExtensions: string[]
  ) => {
    // Browsers routinely report an empty file.type for .ico (no registry/OS
    // mapping). Rejecting on that alone would refuse a favicon the server
    // happily accepts, so fall back to the extension when the type is blank.
    const extension = file.name.slice(file.name.lastIndexOf('.')).toLowerCase();
    const typeOk = file.type
      ? acceptedTypes.includes(file.type)
      : acceptedExtensions.includes(extension);

    if (!typeOk) {
      message.error(
        `${assetType} must be one of: ${acceptedExtensions.join(', ')}`
      );
      return;
    }
    if (file.size > maxBytes) {
      message.error(
        `${assetType} is ${Math.round(file.size / 1024)}KB. The maximum is ${maxBytes / 1024}KB.`
      );
      return;
    }

    const ok = await uploadAsset(assetType, file);
    if (ok) message.success(`${assetType} updated`);
  };

  const handleAssetClear = async (assetType: BrandingAssetType) => {
    const ok = await removeAsset(assetType);
    if (ok) message.success(`${assetType} removed`);
  };

  const busy = saving || isPending;

  return (
    <Card
      title="School Branding"
      style={{ marginBottom: 24 }}
      extra={
        !isConfigured && (
          <Text style={{ fontSize: 12, color: '#8C8C8C' }}>
            Using default PSMS branding
          </Text>
        )
      }
    >
      <Form form={form} layout="vertical">
        <Form.Item
          label="Display Name"
          name="schoolName"
          extra="Shown in the sidebar, the login page and the browser tab. Leave blank to use the default."
        >
          <Input placeholder="e.g. St. John's Preparatory" maxLength={200} />
        </Form.Item>

        <Space size={32} wrap style={{ marginBottom: 8 }}>
          <Form.Item
            label="Primary Colour"
            extra="Buttons, links and selected states."
            style={{ marginBottom: 0 }}
          >
            <ColorPicker
              value={primaryColor}
              disabledAlpha
              showText
              onChange={(color) => setPrimaryColor(normaliseHex(color))}
            />
          </Form.Item>

          <Form.Item
            label="Secondary Colour"
            extra="App header background."
            style={{ marginBottom: 0 }}
          >
            <ColorPicker
              value={secondaryColor}
              disabledAlpha
              showText
              onChange={(color) => setSecondaryColor(normaliseHex(color))}
            />
          </Form.Item>
        </Space>

        {/* Live preview — the real header uses the secondary colour as its
            background and the primary colour for its actions. */}
        <div style={{ margin: '20px 0' }}>
          <Text strong style={{ display: 'block', marginBottom: 8 }}>
            Preview
          </Text>
          <div
            style={{
              background: secondaryColor,
              height: 44,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
              padding: '0 16px',
            }}
          >
            <span
              style={{
                color: getReadableForeground(secondaryColor),
                fontSize: 14,
                fontWeight: 600,
              }}
            >
              {watchedSchoolName || branding.schoolName}
            </span>
            <span
              style={{
                background: primaryColor,
                color: getReadableForeground(primaryColor),
                fontSize: 12,
                padding: '4px 12px',
                borderRadius: 2,
              }}
            >
              Action
            </span>
          </div>
        </div>

        <AssetRow
          label="Logo"
          description="PNG or JPG, up to 2MB. Appears in the sidebar and on the login page."
          url={branding.logoUrl}
          accept=".png,.jpg,.jpeg"
          previewHeight={40}
          busy={busy}
          onSelect={(file) =>
            handleAssetSelect(
              'Logo', file, LOGO_MAX_BYTES, LOGO_ACCEPT, LOGO_EXTENSIONS
            )
          }
          onClear={() => handleAssetClear('Logo')}
        />

        <AssetRow
          label="Favicon"
          description="PNG or ICO, up to 512KB. Shown in the browser tab."
          url={branding.faviconUrl}
          accept=".png,.ico"
          previewHeight={24}
          busy={busy}
          onSelect={(file) =>
            handleAssetSelect(
              'Favicon', file, FAVICON_MAX_BYTES, FAVICON_ACCEPT, FAVICON_EXTENSIONS
            )
          }
          onClear={() => handleAssetClear('Favicon')}
        />

        <Button type="primary" onClick={handleSave} loading={saving} disabled={isPending}>
          Save Branding
        </Button>
      </Form>
    </Card>
  );
}
