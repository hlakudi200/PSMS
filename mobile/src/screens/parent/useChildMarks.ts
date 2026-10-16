import { useCallback, useEffect, useMemo } from "react";
import { useMarksActions, useMarksState } from "../../providers/marks";
import { buildSubjectSections } from "../../utils/marks-sections";
import { useSelectedChild } from "./useSelectedChild";

/**
 * The selected child's marks for one term, grouped by subject. Shared by the parent's progress
 * screens, which sit on one MarksProvider: whatever is in it may belong to another child or term,
 * so nothing is shown until it matches.
 */
export function useChildMarks(termId?: string) {
  const { child, hasChildren, isChildrenError, retryChildren } = useSelectedChild();
  const {
    marks, marksStudentId, marksTermId, isMarksPending, isMarksError,
    classAssessments, classAssessmentsKey, assessmentDetailsById,
  } = useMarksState();
  const { getMarksByStudentAsync, getClassAssessmentsAsync } = useMarksActions();

  const studentId = child?.studentId;
  const classId = child?.classId;
  const isLoaded = !!studentId && !!termId && marksStudentId === studentId && marksTermId === termId;

  const reload = useCallback(() => {
    if (!studentId || !termId) return;
    void getMarksByStudentAsync(studentId, termId);
    if (classId) void getClassAssessmentsAsync(classId, termId);
  }, [studentId, classId, termId, getMarksByStudentAsync, getClassAssessmentsAsync]);

  // Opening a subject from the list must not reload what the list already has. A failed load
  // leaves both deps unchanged, so it waits for a retry instead of looping.
  useEffect(() => {
    if (!isLoaded) reload();
  }, [isLoaded, reload]);

  const sections = useMemo(() => {
    if (!isLoaded) return [];
    const pending = classAssessmentsKey === `${classId}:${termId}` ? classAssessments ?? [] : [];
    return buildSubjectSections(marks ?? [], pending, assessmentDetailsById);
  }, [isLoaded, marks, classAssessments, classAssessmentsKey, classId, termId, assessmentDetailsById]);

  return {
    child,
    hasChildren,
    isChildrenError,
    retryChildren,
    isLoaded,
    isMarksPending,
    isMarksError,
    sections,
    reload,
  };
}
