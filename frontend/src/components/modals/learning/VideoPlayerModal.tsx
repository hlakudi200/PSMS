'use client';

import React from 'react';
import { Modal } from 'antd';

interface VideoPlayerModalProps {
  open: boolean;
  onClose: () => void;
  title?: string;
  /** Short-lived signed URL from LearningMaterial/GetVideoUrl. */
  src?: string;
}

/**
 * View-only player for video materials. The file sits in a private bucket and
 * the URL expires, so there is no lasting link to share; the browser's own
 * download button, picture-in-picture and right-click "Save video" are also
 * switched off. A determined user can still capture a stream, but students
 * are never handed a file to download.
 */
export const VideoPlayerModal: React.FC<VideoPlayerModalProps> = ({
  open,
  onClose,
  title,
  src,
}) => (
  <Modal
    open={open}
    title={title ?? 'Video'}
    onCancel={onClose}
    footer={null}
    width={880}
    destroyOnHidden
  >
    {src && (
      <video
        src={src}
        controls
        autoPlay
        controlsList="nodownload noremoteplayback"
        disablePictureInPicture
        onContextMenu={(e) => e.preventDefault()}
        style={{ width: '100%', maxHeight: '70vh', background: '#000', borderRadius: 8 }}
      />
    )}
  </Modal>
);
