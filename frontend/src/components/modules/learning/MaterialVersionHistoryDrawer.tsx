'use client';

import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Button,
  Drawer,
  Empty,
  Input,
  List,
  Popconfirm,
  Skeleton,
  Space,
  Tag,
  Typography,
  Upload,
  message,
} from 'antd';
import type { UploadFile } from 'antd';
import {
  CloudUploadOutlined,
  DownloadOutlined,
  HistoryOutlined,
  RollbackOutlined,
} from '@ant-design/icons';
import {
  useLearningMaterialActions,
  useLearningMaterialState,
} from '@/providers/learning/learning_materials';
import type {
  ILearningMaterialList,
  ILearningMaterialVersion,
} from '@/providers/learning/shared/interfaces';

const { Text } = Typography;

const MAX_VERSIONS_RETAINED = 10; // mirrors backend LM-003

function formatBytes(value: number | undefined | null): string {
  if (!value) return '—';
  const units = ['B', 'KB', 'MB', 'GB'];
  let v = value;
  let i = 0;
  while (v >= 1024 && i < units.length - 1) {
    v /= 1024;
    i++;
  }
  return `${v.toFixed(v < 10 ? 1 : 0)} ${units[i]}`;
}

interface MaterialVersionHistoryDrawerProps {
  open: boolean;
  onClose: () => void;
  /**
   * The material whose history is being viewed. When null the drawer
   * renders nothing meaningful — caller is expected to set open=false in
   * that case.
   */
  material: ILearningMaterialList | null;
}

export const MaterialVersionHistoryDrawer: React.FC<
  MaterialVersionHistoryDrawerProps
> = ({ open, onClose, material }) => {
  const {
    getVersionsAsync,
    requestVersionUploadUrlAsync,
    uploadFileToStorageAsync,
    uploadNewVersionAsync,
    restoreVersionAsync,
  } = useLearningMaterialActions();
  const {
    versions,
    versionsLoading,
    versionsError,
  } = useLearningMaterialState();

  const [changeDescription, setChangeDescription] = useState('');
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [uploading, setUploading] = useState(false);
  const [restoringId, setRestoringId] = useState<string | null>(null);

  // Reload version history whenever the drawer opens with a fresh
  // material id. Closing leaves `versions` in state — that's fine; the
  // next open re-fetches before the user sees anything stale.
  useEffect(() => {
    if (open && material) {
      getVersionsAsync(material.id);
      setChangeDescription('');
      setFileList([]);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, material?.id]);

  const handleUpload = async () => {
    if (!material) return;
    if (!changeDescription.trim() || changeDescription.trim().length < 5) {
      message.error('Change description must be at least 5 characters.');
      return;
    }
    const file = fileList[0]?.originFileObj as File | undefined;
    if (!file) {
      message.error('Please select a file for the new version.');
      return;
    }
    setUploading(true);
    try {
      // Direct upload: signed URL → PUT to storage → record the version.
      const ticket = await requestVersionUploadUrlAsync({
        learningMaterialId: material.id,
        fileName: file.name,
      });
      await uploadFileToStorageAsync(ticket.uploadUrl, file);
      await uploadNewVersionAsync({
        learningMaterialId: material.id,
        changeDescription: changeDescription.trim(),
        fileUrl: ticket.publicUrl,
        fileName: file.name,
        fileSizeBytes: file.size,
        contentType: file.type || 'application/octet-stream',
      });
      message.success('New version uploaded');
      setChangeDescription('');
      setFileList([]);
      // Refresh history so the new row shows immediately.
      getVersionsAsync(material.id);
    } catch {
      // Server errors surfaced by the axios response interceptor.
    } finally {
      setUploading(false);
    }
  };

  const handleRestore = async (version: ILearningMaterialVersion) => {
    if (!material) return;
    setRestoringId(version.id);
    try {
      await restoreVersionAsync(material.id, version.id);
      message.success(
        `Restored to v${version.versionNumber} (saved as a new version)`
      );
      getVersionsAsync(material.id);
    } catch {
      // Surfaced by axios interceptor
    } finally {
      setRestoringId(null);
    }
  };

  const handleDownload = (version: ILearningMaterialVersion) => {
    if (version.fileUrl) {
      window.open(version.fileUrl, '_blank', 'noopener,noreferrer');
    } else {
      message.info('This version has no downloadable file.');
    }
  };

  // Memoised so we don't reallocate the array on every keystroke in the
  // change-description textarea.
  const sortedVersions = useMemo(
    () =>
      (versions ?? [])
        .slice()
        .sort((a, b) => b.versionNumber - a.versionNumber),
    [versions]
  );
  const currentVersionNumber = sortedVersions[0]?.versionNumber ?? null;
  // The cap warning fires once we're AT the cap (next upload triggers the
  // backend prune). The "over the cap" branch handles the transient case
  // where a prior prune failed and the row count is briefly above 10 — we
  // escalate to a warning style so the row gets a `role="alert"` from AntD.
  const atRetentionCap = sortedVersions.length >= MAX_VERSIONS_RETAINED;
  const overRetentionCap = sortedVersions.length > MAX_VERSIONS_RETAINED;

  return (
    <Drawer
      title={
        <Space>
          <HistoryOutlined />
          <span>Version history — {material?.title ?? 'Material'}</span>
        </Space>
      }
      open={open}
      onClose={onClose}
      width={560}
      // AntD v6 prefers destroyOnHidden over the deprecated destroyOnClose.
      destroyOnHidden
    >
      {/* Upload-new-version form. Stays at the top so the most-common
          action is one click away when the drawer opens. */}
      <div style={{ marginBottom: 16 }}>
        <Text strong style={{ display: 'block', marginBottom: 8 }}>
          Upload a new version
        </Text>
        <Input.TextArea
          rows={2}
          placeholder="What changed? (5–500 characters, required)"
          maxLength={500}
          value={changeDescription}
          onChange={(e) => setChangeDescription(e.target.value)}
          style={{ marginBottom: 8 }}
        />
        <Upload.Dragger
          multiple={false}
          maxCount={1}
          fileList={fileList}
          // beforeUpload returning false keeps AntD from auto-uploading;
          // we hand the file to uploadNewVersionAsync ourselves.
          beforeUpload={() => false}
          onChange={(info) => setFileList(info.fileList)}
          onRemove={() => setFileList([])}
          style={{ marginBottom: 8 }}
        >
          <p className="ant-upload-drag-icon">
            <CloudUploadOutlined />
          </p>
          <p className="ant-upload-text" style={{ fontSize: 13 }}>
            Click or drag the new file here
          </p>
          <p className="ant-upload-hint" style={{ fontSize: 11 }}>
            Server validates type and size against the material's type
            (LM-001). At most {MAX_VERSIONS_RETAINED} versions are retained
            (LM-003) — older versions are pruned automatically.
          </p>
        </Upload.Dragger>
        <Button
          type="primary"
          icon={<CloudUploadOutlined />}
          loading={uploading}
          disabled={!changeDescription.trim() || fileList.length === 0}
          onClick={handleUpload}
        >
          Upload new version
        </Button>
      </div>

      {atRetentionCap && (
        <Alert
          // type="warning" gets us role="alert" in AntD by default, so the
          // copy is announced to screen-reader users — this message warns
          // about destructive behaviour (next upload prunes the oldest row).
          type="warning"
          showIcon
          message={
            overRetentionCap
              ? `Over the ${MAX_VERSIONS_RETAINED}-version retention cap`
              : `You're at the ${MAX_VERSIONS_RETAINED}-version retention cap`
          }
          description="Uploading another version will permanently delete the oldest one (LM-003)."
          style={{ marginBottom: 12 }}
        />
      )}

      {versionsError && (
        <Alert
          type="warning"
          showIcon
          message="Could not load version history"
          style={{ marginBottom: 12 }}
        />
      )}

      <Text strong style={{ display: 'block', marginBottom: 8 }}>
        History
      </Text>

      {versionsLoading && !versions ? (
        <Skeleton active paragraph={{ rows: 4 }} />
      ) : sortedVersions.length === 0 ? (
        <Empty
          description="No versions yet."
          image={Empty.PRESENTED_IMAGE_SIMPLE}
        />
      ) : (
        <List<ILearningMaterialVersion>
          dataSource={sortedVersions}
          rowKey="id"
          renderItem={(v) => {
            const isCurrent = v.versionNumber === currentVersionNumber;
            return (
              <List.Item
                actions={[
                  <Button
                    key="download"
                    size="small"
                    icon={<DownloadOutlined />}
                    onClick={() => handleDownload(v)}
                    disabled={!v.fileUrl}
                    aria-label={`Download v${v.versionNumber}`}
                  >
                    Download
                  </Button>,
                  isCurrent ? null : (
                    <Popconfirm
                      key="restore"
                      title={`Restore v${v.versionNumber}?`}
                      description="A new version is created — the older row remains in history."
                      onConfirm={() => handleRestore(v)}
                      okText="Restore"
                    >
                      <Button
                        size="small"
                        icon={<RollbackOutlined />}
                        loading={restoringId === v.id}
                        aria-label={`Restore v${v.versionNumber}`}
                      >
                        Restore
                      </Button>
                    </Popconfirm>
                  ),
                ].filter(Boolean) as React.ReactNode[]}
              >
                <List.Item.Meta
                  title={
                    <Space>
                      <Text strong>v{v.versionNumber}</Text>
                      {isCurrent && <Tag color="green">Current</Tag>}
                      {v.fileName && (
                        <Text type="secondary" style={{ fontSize: 12 }}>
                          {v.fileName} · {formatBytes(v.fileSizeBytes)}
                        </Text>
                      )}
                    </Space>
                  }
                  description={
                    <div>
                      <Text style={{ fontSize: 13 }}>{v.changeDescription}</Text>
                      <div>
                        <Text type="secondary" style={{ fontSize: 11 }}>
                          {new Date(v.creationTime).toLocaleString('en-ZA')}
                        </Text>
                      </div>
                    </div>
                  }
                />
              </List.Item>
            );
          }}
        />
      )}
    </Drawer>
  );
};
