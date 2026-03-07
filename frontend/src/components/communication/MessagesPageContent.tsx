'use client';

import React, { useState, useCallback, useEffect } from 'react';
import { Card, Tabs, List, Tag, Typography, Button, Drawer, Divider, Space, Empty, Spin, message } from 'antd';
import {
  MailOutlined,
  SendOutlined,
  CheckOutlined,
  ArrowLeftOutlined,
} from '@ant-design/icons';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';
import { MessageProvider, useMessageState, useMessageActions } from '@/providers/communication/messages';
import type { IMessageList } from '@/providers/communication/shared/interfaces';

// Thread messages include creationTime from API (CreationAuditedEntity)
interface IThreadMessage {
  id: string;
  senderUserId: number;
  recipientUserId: number;
  subject?: string;
  content: string;
  attachmentUrl?: string;
  isRead: boolean;
  creationTime?: string;
}

dayjs.extend(relativeTime);

const { Text, Paragraph } = Typography;

function MessagesContent() {
  const { messages, threadMessages, totalCount, isPending } = useMessageState();
  const { getInboxAsync, getSentAsync, getThreadAsync, markAsReadAsync } = useMessageActions();

  const [activeTab, setActiveTab] = useState<'inbox' | 'sent'>('inbox');
  const [currentPage, setCurrentPage] = useState(1);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [selectedMessage, setSelectedMessage] = useState<IMessageList | null>(null);
  const pageSize = 15;

  const fetchMessages = useCallback((tab: string, page: number) => {
    const input = {
      maxResultCount: pageSize,
      skipCount: (page - 1) * pageSize,
      sorting: 'CreationTime DESC',
    };
    if (tab === 'inbox') {
      getInboxAsync(input);
    } else {
      getSentAsync(input);
    }
  }, [getInboxAsync, getSentAsync]);

  useEffect(() => {
    fetchMessages(activeTab, currentPage);
  }, [activeTab, currentPage]);

  const handleTabChange = (tab: string) => {
    setActiveTab(tab as 'inbox' | 'sent');
    setCurrentPage(1);
  };

  const handleMessageClick = async (msg: IMessageList) => {
    setSelectedMessage(msg);
    setDrawerOpen(true);

    // Load thread
    if (msg.threadId) {
      getThreadAsync(msg.threadId);
    }

    // Mark as read if inbox and unread
    if (activeTab === 'inbox' && !msg.isRead) {
      await markAsReadAsync(msg.id);
      fetchMessages(activeTab, currentPage);
    }
  };

  const handleDrawerClose = () => {
    setDrawerOpen(false);
    setSelectedMessage(null);
  };

  const handleMarkAsRead = async (msg: IMessageList) => {
    await markAsReadAsync(msg.id);
    message.success('Marked as read');
    fetchMessages(activeTab, currentPage);
  };

  const renderMessageItem = (msg: IMessageList) => {
    const isInbox = activeTab === 'inbox';
    const isUnread = isInbox && !msg.isRead;

    return (
      <List.Item
        key={msg.id}
        style={{
          cursor: 'pointer',
          background: isUnread ? '#f0f5ff' : undefined,
          padding: '12px 16px',
        }}
        onClick={() => handleMessageClick(msg)}
        actions={
          isInbox && !msg.isRead
            ? [
                <Button
                  key="read"
                  type="text"
                  size="small"
                  icon={<CheckOutlined />}
                  onClick={(e) => {
                    e.stopPropagation();
                    handleMarkAsRead(msg);
                  }}
                >
                  Mark Read
                </Button>,
              ]
            : undefined
        }
      >
        <List.Item.Meta
          title={
            <Space size="small">
              <Text strong={isUnread}>
                {msg.subject || '(No Subject)'}
              </Text>
              {isUnread && <Tag color="blue">New</Tag>}
            </Space>
          }
          description={
            <Space size="middle">
              <Text type="secondary" style={{ fontSize: 12 }}>
                {isInbox ? `From: User #${msg.senderUserId}` : `To: User #${msg.recipientUserId}`}
              </Text>
              <Text type="secondary" style={{ fontSize: 12 }}>
                {dayjs(msg.creationTime).fromNow()}
              </Text>
            </Space>
          }
        />
      </List.Item>
    );
  };

  const renderThread = () => {
    if (!threadMessages || threadMessages.length === 0) {
      if (isPending) return <Spin style={{ display: 'block', margin: '40px auto' }} />;
      return <Empty description="No messages in this thread" />;
    }

    return (
      <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
        {threadMessages.map((msg: IThreadMessage, index: number) => (
          <Card
            key={msg.id}
            size="small"
            style={{
              background: index === 0 ? '#fafafa' : '#fff',
            }}
          >
            <Space style={{ marginBottom: 8, width: '100%', justifyContent: 'space-between' }}>
              <Text strong style={{ fontSize: 13 }}>
                User #{msg.senderUserId}
              </Text>
              <Text type="secondary" style={{ fontSize: 12 }}>
                {msg.creationTime ? dayjs(msg.creationTime).format('DD MMM YYYY, HH:mm') : ''}
              </Text>
            </Space>
            {msg.subject && index === 0 && (
              <Text strong style={{ display: 'block', marginBottom: 8 }}>
                {msg.subject}
              </Text>
            )}
            <Paragraph style={{ marginBottom: 0, whiteSpace: 'pre-wrap' }}>
              {msg.content}
            </Paragraph>
            {msg.attachmentUrl && (
              <a href={msg.attachmentUrl} target="_blank" rel="noopener noreferrer" style={{ fontSize: 12, marginTop: 4, display: 'inline-block' }}>
                View Attachment
              </a>
            )}
          </Card>
        ))}
      </div>
    );
  };

  const tabItems = [
    {
      key: 'inbox',
      label: (
        <span>
          <MailOutlined style={{ marginRight: 6 }} />
          Inbox
        </span>
      ),
      children: (
        <List
          loading={isPending}
          dataSource={messages ?? []}
          renderItem={renderMessageItem}
          pagination={{
            current: currentPage,
            pageSize,
            total: totalCount ?? 0,
            onChange: setCurrentPage,
            showSizeChanger: false,
            size: 'small',
          }}
          locale={{ emptyText: <Empty description="No messages" /> }}
        />
      ),
    },
    {
      key: 'sent',
      label: (
        <span>
          <SendOutlined style={{ marginRight: 6 }} />
          Sent
        </span>
      ),
      children: (
        <List
          loading={isPending}
          dataSource={messages ?? []}
          renderItem={renderMessageItem}
          pagination={{
            current: currentPage,
            pageSize,
            total: totalCount ?? 0,
            onChange: setCurrentPage,
            showSizeChanger: false,
            size: 'small',
          }}
          locale={{ emptyText: <Empty description="No sent messages" /> }}
        />
      ),
    },
  ];

  return (
    <>
      <Card
        size="small"
        title="Messages"
        styles={{ body: { padding: 0 } }}
      >
        <Tabs
          activeKey={activeTab}
          onChange={handleTabChange}
          items={tabItems}
          style={{ padding: '0 16px' }}
        />
      </Card>

      <Drawer
        title={
          <Space>
            <Button type="text" icon={<ArrowLeftOutlined />} onClick={handleDrawerClose} />
            <span>{selectedMessage?.subject || '(No Subject)'}</span>
          </Space>
        }
        open={drawerOpen}
        onClose={handleDrawerClose}
        width={520}
        styles={{ body: { padding: '16px' } }}
      >
        {selectedMessage && (
          <>
            <Space style={{ marginBottom: 8 }}>
              <Text type="secondary">
                {activeTab === 'inbox'
                  ? `From: User #${selectedMessage.senderUserId}`
                  : `To: User #${selectedMessage.recipientUserId}`}
              </Text>
              <Text type="secondary">
                {dayjs(selectedMessage.creationTime).format('DD MMM YYYY, HH:mm')}
              </Text>
              {selectedMessage.isRead && <Tag color="green">Read</Tag>}
              {!selectedMessage.isRead && <Tag color="blue">Unread</Tag>}
            </Space>
            <Divider style={{ margin: '12px 0' }} />
            {selectedMessage.threadId ? (
              renderThread()
            ) : (
              <Empty description="No thread data available" />
            )}
          </>
        )}
      </Drawer>
    </>
  );
}

export default function MessagesPageContent() {
  return (
    <MessageProvider>
      <MessagesContent />
    </MessageProvider>
  );
}
