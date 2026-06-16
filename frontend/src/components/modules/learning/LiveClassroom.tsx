'use client';

import '@livekit/components-styles';
import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  LiveKitRoom,
  VideoConference,
  GridLayout,
  ParticipantTile,
  RoomAudioRenderer,
  Chat,
  useTracks,
  useDataChannel,
  useLocalParticipant,
  useRoomContext,
} from '@livekit/components-react';
import { Track, RoomEvent, type RemoteParticipant } from 'livekit-client';
import { Button, Result, Spin, Badge, Space, Tooltip, App } from 'antd';
import {
  ArrowLeftOutlined,
  MessageOutlined,
  NotificationOutlined,
} from '@ant-design/icons';
import { useOnlineLessonActions } from '@/providers/learning/online_lessons';
import type { ILiveClassJoin } from '@/providers/learning/shared/interfaces';

// Data-channel topic for classroom signalling (raise/lower hand). Kept tiny
// and ephemeral in v1 — no persistence (LC-04 may promote to attendance).
const SIGNAL_TOPIC = 'lc-signal';

interface LiveClassroomProps {
  lessonId: string;
  /** Called when the user leaves / is disconnected, or hits Back on an error. */
  onLeave: () => void;
}

/**
 * In-app live classroom (LC-02). Fetches a LiveKit join token from the backend
 * (which decides publish vs view-only by role), connects to the room, and
 * renders the teacher console or the student viewer accordingly.
 */
export const LiveClassroom: React.FC<LiveClassroomProps> = ({ lessonId, onLeave }) => {
  const { getJoinTokenAsync } = useOnlineLessonActions();
  const [join, setJoin] = useState<ILiveClassJoin | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setError(null);
    setJoin(null);
    getJoinTokenAsync(lessonId)
      .then((j) => {
        if (!cancelled) setJoin(j);
      })
      .catch((e: unknown) => {
        if (cancelled) return;
        const msg =
          (e as { response?: { data?: { error?: { message?: string } } } })?.response?.data?.error
            ?.message ||
          (e as Error)?.message ||
          'Could not join the live class.';
        setError(msg);
      });
    return () => {
      cancelled = true;
    };
  }, [lessonId, getJoinTokenAsync]);

  if (error) {
    return (
      <Result
        status="error"
        title="Can't join the live class"
        subTitle={error}
        extra={
          <Button icon={<ArrowLeftOutlined />} onClick={onLeave}>
            Back
          </Button>
        }
      />
    );
  }

  if (!join) {
    return (
      <div style={{ display: 'grid', placeItems: 'center', height: '70vh' }}>
        <Spin size="large" tip="Joining class…">
          <div style={{ padding: 48 }} />
        </Spin>
      </div>
    );
  }

  return (
    <LiveKitRoom
      serverUrl={join.serverUrl}
      token={join.token}
      connect
      // Only the host publishes audio/video; students join muted/cameraless.
      video={join.canPublish}
      audio={join.canPublish}
      onDisconnected={onLeave}
      data-lk-theme="default"
      style={{ height: '100%', minHeight: '70vh' }}
    >
      {join.canPublish ? <TeacherConsole /> : <StudentViewer />}
      {/* Plays remote audio for everyone (the teacher hears nothing extra). */}
      <RoomAudioRenderer />
    </LiveKitRoom>
  );
};

/**
 * Teacher console — LiveKit's batteries-included conference UI (camera / mic /
 * screen-share controls, participant grid, built-in chat) plus a live
 * raised-hands indicator fed by the data channel.
 */
function TeacherConsole() {
  return (
    <div style={{ position: 'relative', height: '100%', minHeight: '70vh' }}>
      <div style={{ position: 'absolute', top: 8, right: 12, zIndex: 10 }}>
        <RaisedHandsIndicator />
      </div>
      <VideoConference />
    </div>
  );
}

/**
 * Student viewer — subscribes to the teacher's camera + screen-share, shows a
 * toggleable chat panel, and a raise-hand button. No publish controls (the
 * token grant is view-only anyway, but we also hide them for a clean UX).
 */
function StudentViewer() {
  const [chatOpen, setChatOpen] = useState(false);

  // The teacher's published tracks. withPlaceholder keeps a tile before the
  // teacher's camera arrives so the room never looks empty.
  const tracks = useTracks(
    [
      { source: Track.Source.Camera, withPlaceholder: true },
      { source: Track.Source.ScreenShare, withPlaceholder: false },
    ],
    { onlySubscribed: false }
  );

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%', minHeight: '70vh' }}>
      <div style={{ flex: 1, minHeight: 0, display: 'flex' }}>
        <div style={{ flex: 1, minHeight: 0 }}>
          <GridLayout tracks={tracks} style={{ height: '100%' }}>
            <ParticipantTile />
          </GridLayout>
        </div>
        {chatOpen && (
          <div
            style={{
              width: 320,
              borderLeft: '1px solid rgba(255,255,255,0.12)',
              display: 'flex',
              minHeight: 0,
            }}
          >
            <Chat style={{ flex: 1 }} />
          </div>
        )}
      </div>

      <div
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: 8,
          padding: '8px 12px',
          background: 'rgba(0,0,0,0.45)',
        }}
      >
        <RaiseHandButton />
        <Button
          icon={<MessageOutlined />}
          type={chatOpen ? 'primary' : 'default'}
          onClick={() => setChatOpen((o) => !o)}
        >
          Chat
        </Button>
      </div>
    </div>
  );
}

/** Student control: toggles a raised hand, broadcasting it on the data channel. */
function RaiseHandButton() {
  const { localParticipant } = useLocalParticipant();
  const { send } = useDataChannel(SIGNAL_TOPIC);
  const [raised, setRaised] = useState(false);
  const { message: antdMessage } = App.useApp();

  const toggle = useCallback(() => {
    const next = !raised;
    setRaised(next);
    // identity is stable + unique (LC-01 uses "user-{id}"); name is the display
    // label. The teacher keys by identity so same-named students don't collide.
    const identity = localParticipant?.identity || 'unknown';
    const name = localParticipant?.name || identity;
    try {
      // Pass the topic explicitly (and reliable delivery) so the signal is
      // routed to RaisedHandsIndicator's topic-filtered listener regardless of
      // the SDK's auto-topic behaviour.
      send(
        new TextEncoder().encode(JSON.stringify({ type: next ? 'raise' : 'lower', identity, name })),
        { topic: SIGNAL_TOPIC, reliable: true }
      );
    } catch {
      antdMessage.error('Could not signal — connection issue.');
      setRaised(raised); // revert
    }
  }, [raised, send, localParticipant, antdMessage]);

  return (
    <Tooltip title={raised ? 'Lower your hand' : 'Raise your hand'}>
      <Button type={raised ? 'primary' : 'default'} onClick={toggle}>
        {raised ? '✋ Hand raised' : '✋ Raise hand'}
      </Button>
    </Tooltip>
  );
}

/** Teacher indicator: live count + names of students with a raised hand. */
function RaisedHandsIndicator() {
  const { message } = useDataChannel(SIGNAL_TOPIC);
  const room = useRoomContext();
  // Keyed by stable participant identity -> display name.
  const [hands, setHands] = useState<Record<string, string>>({});

  useEffect(() => {
    if (!message) return;
    try {
      const payload = JSON.parse(new TextDecoder().decode(message.payload)) as {
        type: 'raise' | 'lower';
        identity: string;
        name: string;
      };
      if (!payload.identity) return;
      setHands((prev) => {
        const next = { ...prev };
        if (payload.type === 'raise') next[payload.identity] = payload.name || payload.identity;
        else delete next[payload.identity];
        return next;
      });
    } catch {
      // Ignore malformed signals.
    }
  }, [message]);

  // Clear a student's raised hand if they leave without lowering it.
  useEffect(() => {
    if (!room) return;
    const onLeft = (p: RemoteParticipant) =>
      setHands((prev) => {
        if (!(p.identity in prev)) return prev;
        const next = { ...prev };
        delete next[p.identity];
        return next;
      });
    room.on(RoomEvent.ParticipantDisconnected, onLeft);
    return () => {
      room.off(RoomEvent.ParticipantDisconnected, onLeft);
    };
  }, [room]);

  const names = useMemo(() => Object.values(hands), [hands]);
  if (names.length === 0) return null;

  return (
    <Tooltip title={names.join(', ')}>
      <Badge count={names.length} offset={[-4, 4]}>
        <Button icon={<NotificationOutlined />} shape="round">
          Raised hands
        </Button>
      </Badge>
    </Tooltip>
  );
}
