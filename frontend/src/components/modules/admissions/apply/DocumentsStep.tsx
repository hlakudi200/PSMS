'use client';

import React, { useEffect, useState } from 'react';
import { documentCategoryOptions } from '@/providers/admissions/shared/document-categories';
import { Alert, Button, List, Select, Space, Tag, Typography, Upload, message } from 'antd';
import { CheckCircleTwoTone, DeleteOutlined, UploadOutlined } from '@ant-design/icons';
import { useApplicationDocumentActions, useApplicationDocumentState } from '@/providers/admissions/application_documents';
import { documentsAsList } from './schema';

const { Text, Paragraph } = Typography;

/* One list, shared with the settings screen that decides which of these the
   school asks for. It used to live here alone, while the school typed its
   requirements as prose into a textarea — so the two could never be compared. */
const categories = documentCategoryOptions;

/**
 * Step three: the documents the school asks for.
 *
 * Uploads go straight to storage through a short-lived ticket the server mints,
 * so a certified ID never travels through the application server, and the
 * record is only written once the file is actually there.
 */
export default function DocumentsStep({
  applicationId,
  requiredDocuments,
  locked,
  onChanged,
}: {
  applicationId: string;
  requiredDocuments?: string;
  locked: boolean;
  onChanged: () => void;
}) {
  const { documents } = useApplicationDocumentState();
  const {
    getAllByApplicationAsync,
    requestUploadUrlAsync,
    uploadFileToStorageAsync,
    uploadAsync,
    deleteAsync,
  } = useApplicationDocumentActions();

  const [category, setCategory] = useState<number | undefined>();
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    if (applicationId) getAllByApplicationAsync(applicationId);
  }, [applicationId]);

  const uploaded = documents ?? [];
  const asked = documentsAsList(requiredDocuments);

  const handleUpload = async (file: File) => {
    if (!category) {
      message.warning('Choose what this document is first.');
      return Upload.LIST_IGNORE;
    }

    setBusy(true);
    try {
      const ticket = await requestUploadUrlAsync({
        applicationId,
        category,
        fileName: file.name,
      });

      if (!ticket?.uploadUrl) throw new Error('no ticket');

      await uploadFileToStorageAsync(ticket.uploadUrl, file);

      // Only now is there a file to point at.
      await uploadAsync({
        applicationId,
        category,
        objectKey: ticket.objectKey,
        fileName: file.name,
      });

      message.success(`${file.name} uploaded`);
      setCategory(undefined);
      getAllByApplicationAsync(applicationId);
      onChanged();
    } catch (e) {
      const abp = (e as { response?: { data?: { error?: { message?: string; details?: string } } } })
        ?.response?.data?.error;
      message.error(abp?.message || abp?.details || `Could not upload ${file.name}. Try again.`);
    } finally {
      setBusy(false);
    }

    return Upload.LIST_IGNORE;
  };

  return (
    <>
      <Paragraph type="secondary">
        Photographs of the documents are fine, as long as everything on them can be read.
      </Paragraph>

      {asked.length > 0 && (
        <Alert
          type="info"
          showIcon
          style={{ marginBottom: 16 }}
          message="What this school asks for"
          description={
            <ul style={{ margin: '4px 0 0 18px' }}>
              {asked.map((d) => (
                <li key={d}>{d}</li>
              ))}
            </ul>
          }
        />
      )}

      {!locked && (
        <Space.Compact style={{ width: '100%', marginBottom: 16 }}>
          <Select
            style={{ width: '60%' }}
            placeholder="What is this document?"
            value={category}
            onChange={setCategory}
            options={categories}
          />
          <Upload beforeUpload={handleUpload} showUploadList={false} accept="image/*,.pdf" disabled={busy}>
            <Button icon={<UploadOutlined />} loading={busy} type="primary">
              Choose a file
            </Button>
          </Upload>
        </Space.Compact>
      )}

      <List
        bordered
        dataSource={uploaded}
        locale={{ emptyText: 'Nothing uploaded yet' }}
        renderItem={(d) => (
          <List.Item
            actions={
              locked
                ? []
                : [
                    <Button
                      key="d"
                      size="small"
                      danger
                      icon={<DeleteOutlined />}
                      onClick={async () => {
                        await deleteAsync(d.id);
                        getAllByApplicationAsync(applicationId);
                        onChanged();
                      }}
                    >
                      Remove
                    </Button>,
                  ]
            }
          >
            <List.Item.Meta
              avatar={d.isVerified ? <CheckCircleTwoTone twoToneColor="#52c41a" /> : undefined}
              title={
                <Space wrap>
                  <Text>{d.fileName}</Text>
                  <Tag>{categories.find((c) => c.value === d.category)?.label ?? 'Document'}</Tag>
                  {d.isVerified && <Tag color="green">Checked by the school</Tag>}
                </Space>
              }
            />
          </List.Item>
        )}
      />

      <Paragraph type="secondary" style={{ marginTop: 12, marginBottom: 0 }}>
        You can submit the application before every document is in, and add the rest afterwards —
        the school will not finish reviewing it until they are all there.
      </Paragraph>
    </>
  );
}
