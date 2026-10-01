'use client';

import React, { useEffect, useState } from 'react';
import { Alert, Button, Card, Empty, Popconfirm, Skeleton, Space, Typography, message } from 'antd';
import { DeleteOutlined, EditOutlined, HighlightOutlined } from '@ant-design/icons';
import {
  StaffSignatureProvider,
  useStaffSignatureActions,
  useStaffSignatureState,
} from '@/providers/assessment/staff_signature';
import SignatureDrawModal from './SignatureDrawModal';

const { Title, Paragraph, Text } = Typography;

/**
 * RC-17. Where a teacher or principal sets up the signature that is printed on
 * every report card they sign.
 *
 * It existed only as a prompt inside a report card's sign-off panel, which meant
 * the only way to change a signature was to open somebody's report card and the
 * only way to discover it was to try to sign one.
 */
const MySignaturePageContentInner: React.FC = () => {
  const { signature, isLoaded, isPending } = useStaffSignatureState();
  const { getMineAsync, deleteMineAsync } = useStaffSignatureActions();
  const [drawOpen, setDrawOpen] = useState(false);
  const [removing, setRemoving] = useState(false);

  useEffect(() => {
    if (!isLoaded) getMineAsync();
  }, [isLoaded, getMineAsync]);

  const svg = signature?.svgContent;

  const remove = async () => {
    setRemoving(true);
    try {
      await deleteMineAsync();
      message.success('Signature removed. Report cards you have already signed are unchanged.');
    } catch {
      // Surfaced by the axios interceptor
    } finally {
      setRemoving(false);
    }
  };

  return (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <div>
        <Title level={3} style={{ marginBottom: 4 }}>My signature</Title>
        <Paragraph type="secondary" style={{ marginBottom: 0, maxWidth: 680 }}>
          Draw your signature once. It is printed above your name on every report card you sign,
          and it is stored only for you — nobody else can see it or sign on your behalf.
        </Paragraph>
      </div>

      <Card>
        {!isLoaded && isPending ? (
          <Skeleton active paragraph={{ rows: 3 }} />
        ) : svg ? (
          <Space direction="vertical" size="middle" style={{ width: '100%' }}>
            <div
              style={{
                border: '1px solid #f0f0f0',
                borderRadius: 6,
                background: '#ffffff',
                padding: 16,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                minHeight: 160,
              }}
            >
              {/* The stored drawing. It is sanitised on the way in — no script,
                  no remote reference — and rendered in an img rather than inline
                  so it cannot reach the rest of the page either way. */}
              <img
                src={`data:image/svg+xml;utf8,${encodeURIComponent(svg)}`}
                alt="Your signature"
                style={{ maxWidth: '100%', maxHeight: 180 }}
              />
            </div>

            {signature?.updatedAt && (
              <Text type="secondary" style={{ fontSize: 12 }}>
                Last updated {new Date(signature.updatedAt).toLocaleDateString('en-ZA', {
                  day: '2-digit', month: 'short', year: 'numeric',
                })}
              </Text>
            )}

            <Space wrap>
              <Button type="primary" icon={<EditOutlined />} onClick={() => setDrawOpen(true)}>
                Draw a new signature
              </Button>
              <Popconfirm
                title="Remove your signature?"
                description="You will not be able to sign a report card until you draw a new one. Cards you have already signed keep the signature you used at the time."
                okText="Remove"
                okButtonProps={{ danger: true }}
                cancelText="Keep it"
                onConfirm={remove}
              >
                <Button danger icon={<DeleteOutlined />} loading={removing}>Remove</Button>
              </Popconfirm>
            </Space>

            <Alert
              type="info"
              showIcon
              message="Replacing it does not change cards you have already signed"
              description="Each report card keeps a copy of the signature as it was when you signed, so an issued card stays exactly as it was issued."
            />
          </Space>
        ) : (
          <Empty
            image={<HighlightOutlined style={{ fontSize: 48, color: '#bfbfbf' }} />}
            description={
              <Space direction="vertical" size={4}>
                <Text strong>You have not set up a signature yet</Text>
                <Text type="secondary">
                  You need one before you can sign a report card.
                </Text>
              </Space>
            }
          >
            <Button type="primary" icon={<HighlightOutlined />} onClick={() => setDrawOpen(true)}>
              Draw your signature
            </Button>
          </Empty>
        )}
      </Card>

      <SignatureDrawModal
        open={drawOpen}
        onClose={() => setDrawOpen(false)}
        onSaved={() => getMineAsync()}
      />
    </Space>
  );
};

export const MySignaturePageContent: React.FC = () => (
  <StaffSignatureProvider>
    <MySignaturePageContentInner />
  </StaffSignatureProvider>
);

export default MySignaturePageContent;
