'use client';

import React, { useCallback, useEffect, useRef, useState } from 'react';
import { Alert, Button, Modal, Space, Typography, message } from 'antd';
import { ClearOutlined } from '@ant-design/icons';
import SignaturePad from 'signature_pad';
import {
  useStaffSignatureActions,
  useStaffSignatureState,
} from '@/providers/assessment/staff_signature';

const { Text, Paragraph } = Typography;

interface Props {
  open: boolean;
  onClose: () => void;
  /** Called once a signature is on file, so the caller can carry on and sign. */
  onSaved?: () => void;
}

/** Draw-once signature capture. SVG, so it stays sharp on the printed card. */
const SignatureDrawModalInner: React.FC<Props> = ({ open, onClose, onSaved }) => {
  const [canvas, setCanvas] = useState<HTMLCanvasElement | null>(null);
  const padRef = useRef<SignaturePad | null>(null);
  const [empty, setEmpty] = useState(true);
  const [saving, setSaving] = useState(false);
  const { saveMineAsync, getMineAsync } = useStaffSignatureActions();
  const { signature, isLoaded } = useStaffSignatureState();

  useEffect(() => {
    if (open && !isLoaded) getMineAsync();
  }, [open, isLoaded, getMineAsync]);

  /* The backing store is scaled to the device's pixel ratio, or the stroke is
     blurry on a laptop screen.

     Sizing it is the whole trick. The modal animates open, so on the frame the
     canvas mounts it can still measure 0 x 0 — and a pad built against a canvas
     of no size records strokes quite happily while writing viewBox="0 0 0 0"
     around them, which prints as a blank line above the teacher's name. So the
     size is taken when the element actually has one, and again whenever it
     changes; resizing a canvas wipes it, so the strokes are put back. */
  const resize = useCallback(() => {
    const pad = padRef.current;
    if (!canvas || !pad) return;

    const ratio = Math.max(window.devicePixelRatio || 1, 1);
    const width = Math.round(canvas.offsetWidth * ratio);
    const height = Math.round(canvas.offsetHeight * ratio);
    if (width === 0 || height === 0) return;            // not laid out yet
    if (canvas.width === width && canvas.height === height) return;

    const strokes = pad.toData();
    canvas.width = width;
    canvas.height = height;
    canvas.getContext('2d')?.scale(ratio, ratio);
    pad.clear();
    if (strokes.length) pad.fromData(strokes);
  }, [canvas]);

  useEffect(() => {
    if (!open || !canvas) return undefined;

    const pad = new SignaturePad(canvas, {
      penColor: '#1a1a1a',
      minWidth: 0.7,
      maxWidth: 2.2,
      backgroundColor: 'rgba(255,255,255,0)',
    });
    const onEnd = () => setEmpty(pad.isEmpty());
    pad.addEventListener('endStroke', onEnd);
    padRef.current = pad;
    setEmpty(true);
    resize();

    const observer = new ResizeObserver(resize);
    observer.observe(canvas);

    return () => {
      observer.disconnect();
      pad.removeEventListener('endStroke', onEnd);
      pad.off();
      padRef.current = null;
    };
  }, [open, canvas, resize]);

  const clear = () => { padRef.current?.clear(); setEmpty(true); };

  const save = async () => {
    const pad = padRef.current;
    if (!pad || pad.isEmpty()) {
      message.warning('Draw your signature in the box first.');
      return;
    }
    setSaving(true);
    try {
      /* SVG rather than PNG: it is what the pad draws natively, it stays sharp
         at print resolution, and it is a few KB of text instead of an image. */
      const svg = pad.toSVG();

      /* A drawing with no canvas around it saves without complaint and then
         prints as nothing. Catch it here rather than on the report card. */
      if (!canvas?.width || !canvas?.height || /viewBox="[\d.\s]*?\s0\s+0"/.test(svg)) {
        message.error('That did not come out. Clear the box and draw your signature again.');
        return;
      }

      await saveMineAsync(svg);
      message.success('Signature saved. It will be used whenever you sign a report card.');
      onSaved?.();
      onClose();
    } catch {
      // Surfaced by the axios interceptor
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal
      open={open}
      title="Your signature"
      onCancel={onClose}
      width={560}
      destroyOnHidden
      footer={[
        <Button key="clear" icon={<ClearOutlined />} onClick={clear} disabled={empty || saving}>
          Clear
        </Button>,
        <Button key="cancel" onClick={onClose} disabled={saving}>Cancel</Button>,
        <Button key="save" type="primary" loading={saving} onClick={save} disabled={empty}>
          Save signature
        </Button>,
      ]}
    >
      <Space direction="vertical" size="middle" style={{ width: '100%' }}>
        <Paragraph type="secondary" style={{ marginBottom: 0 }}>
          Draw your signature once. It is printed above your name on every report card you sign,
          and is only stored for you — nobody else can read it or sign on your behalf.
        </Paragraph>

        {signature?.svgContent && (
          <Alert
            type="info"
            showIcon
            message="You already have a signature on file"
            description="Drawing a new one replaces it. Report cards you have already signed keep the signature you used at the time."
          />
        )}

        <div
          style={{
            border: '1px dashed #bfbfbf',
            borderRadius: 6,
            background: '#fafafa',
            position: 'relative',
          }}
        >
          <canvas
            ref={setCanvas}
            style={{ width: '100%', height: 180, display: 'block', touchAction: 'none', cursor: 'crosshair' }}
          />
          <div
            style={{
              position: 'absolute', left: 24, right: 24, bottom: 34,
              borderBottom: '1px solid #d9d9d9', pointerEvents: 'none',
            }}
          />
          <Text
            type="secondary"
            style={{ position: 'absolute', left: 24, bottom: 12, fontSize: 12, pointerEvents: 'none' }}
          >
            Sign above the line
          </Text>
        </div>
      </Space>
    </Modal>
  );
};

/**
 * Deliberately not wrapped in its own StaffSignatureProvider. It used to be,
 * which shadowed the caller's: the save landed in the modal's private copy of
 * the state and the screen behind it went on showing no signature. Every caller
 * already provides one, and sharing it means a save is visible immediately.
 */
export const SignatureDrawModal = SignatureDrawModalInner;

export default SignatureDrawModal;
