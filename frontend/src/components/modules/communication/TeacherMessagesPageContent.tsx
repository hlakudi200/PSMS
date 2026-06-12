'use client';

import { useEffect, useMemo } from 'react';
import { useAuthState } from '@/providers/auth';
import {
  TeacherProvider,
  useTeacherActions,
  useTeacherState,
} from '@/providers/academic/teachers';
import {
  ClassSubjectProvider,
  useClassSubjectActions,
  useClassSubjectState,
} from '@/providers/academic/class_subjects';
import {
  TeacherClassProvider,
  useTeacherClassActions,
  useTeacherClassState,
} from '@/providers/academic/teacher_classes';
import MessagesPageContent from '@/components/modules/communication/MessagesPageContent';
import { TeacherComposeMessageModal } from '@/components/modals/communication/TeacherComposeMessageModal';

function TeacherMessagesInner() {
  const { currentUser } = useAuthState();

  const { getByCurrentUserAsync } = useTeacherActions();
  const { teacher } = useTeacherState();

  const { getByTeacherAsync: getMyClassSubjects } = useClassSubjectActions();
  const { classSubjects } = useClassSubjectState();

  const { getByTeacherAsync: getMyTeacherClasses } = useTeacherClassActions();
  const { teacherClasses } = useTeacherClassState();

  useEffect(() => {
    if (currentUser?.id != null) getByCurrentUserAsync();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [currentUser?.id]);

  const teacherId = teacher && teacher.userId === currentUser?.id ? teacher.id : null;

  useEffect(() => {
    if (teacherId) {
      getMyClassSubjects(teacherId);
      getMyTeacherClasses(teacherId);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teacherId]);

  // Union of classes the teacher teaches a subject in and classes they are
  // the register/class teacher of, de-duplicated by classId.
  const classOptions = useMemo(() => {
    const map = new Map<string, string>();
    (classSubjects ?? []).forEach((cs) => {
      if (!map.has(cs.classId)) map.set(cs.classId, cs.className ?? 'Class');
    });
    (teacherClasses ?? []).forEach((tc) => {
      if (!map.has(tc.classId)) map.set(tc.classId, tc.className ?? 'Class');
    });
    return Array.from(map.entries()).map(([value, label]) => ({ value, label }));
  }, [classSubjects, teacherClasses]);

  return (
    <MessagesPageContent
      renderCompose={({ open, onClose }) => (
        <TeacherComposeMessageModal open={open} onClose={onClose} classOptions={classOptions} />
      )}
    />
  );
}

export default function TeacherMessagesPageContent() {
  return (
    <TeacherProvider>
      <ClassSubjectProvider>
        <TeacherClassProvider>
          <TeacherMessagesInner />
        </TeacherClassProvider>
      </ClassSubjectProvider>
    </TeacherProvider>
  );
}
