import { useCallback, useEffect, useState } from "react";
import { StyleSheet, Text, View } from "react-native";
import { Badge, Button, Card, Screen, StateView } from "../../components";
import { colors, spacing, typography } from "../../theme";
import { useMarksActions, useMarksState } from "../../providers/marks";
import type { IAssessmentDetail, IMark } from "../../providers/marks/context";
import {
  ACHIEVEMENT_LEVEL_LABELS, ACHIEVEMENT_LEVEL_TONES,
  formatDate, formatPercentage, markDisplayStatusLabel, markDisplayStatusTone,
} from "../../utils/assessment-labels";

const ASSESSMENT_TYPE_LABELS: Record<number, string> = {
  1: "Test", 2: "Exam", 3: "Assignment", 4: "Practical", 5: "Oral", 6: "Project", 7: "Other",
};

export function AssessmentDetailScreen({ assessmentId }: { assessmentId: string }) {
  const { assessmentDetailsById } = useMarksState();
  const { getAssessmentDetailAsync, ensureMarkForAssessmentAsync } = useMarksActions();

  const [isLoading, setIsLoading] = useState(true);
  const [isError, setIsError] = useState(false);
  const [detail, setDetail] = useState<IAssessmentDetail | undefined>(assessmentDetailsById[assessmentId]);
  const [mark, setMark] = useState<IMark | undefined>(undefined);

  const [retryCount, setRetryCount] = useState(0);

  const load = useCallback(async (onCancelled: () => boolean) => {
    setIsLoading(true);
    setIsError(false);
    const [loadedDetail, loadedMark] = await Promise.all([
      getAssessmentDetailAsync(assessmentId),
      ensureMarkForAssessmentAsync(assessmentId),
    ]);
    if (onCancelled()) return;
    if (!loadedDetail) { setIsError(true); setIsLoading(false); return; }
    setDetail(loadedDetail);
    setMark(loadedMark);
    setIsLoading(false);
  }, [assessmentId, getAssessmentDetailAsync, ensureMarkForAssessmentAsync]);

  useEffect(() => {
    let cancelled = false;
    void load(() => cancelled);
    return () => { cancelled = true; };
  }, [load, retryCount]);

  if (isLoading) return <Screen><StateView state="loading" message="Loading assessment…" /></Screen>;
  if (isError || !detail) {
    return (
      <Screen>
        <StateView state="error" message="Couldn't load this assessment." />
        <Button onPress={() => setRetryCount((n) => n + 1)}>Retry</Button>
      </Screen>
    );
  }

  const marksReleased = detail.marksReleased;
  const hasMark = !!mark;
  const showScore = hasMark && marksReleased && !mark?.wasAbsent;

  return (
    <Screen scroll>
      <Text style={styles.subject}>{detail.subjectName} · {detail.className}</Text>
      <Text style={styles.title}>{detail.name}</Text>
      <View style={styles.metaRow}>
        <Badge label={ASSESSMENT_TYPE_LABELS[detail.assessmentType] ?? "Assessment"} tone="muted" />
        <Badge label={detail.termName} tone="muted" />
      </View>

      <View style={styles.stack}>
        <Card>
          <Text style={styles.sectionTitle}>Your result</Text>
          {!marksReleased || !hasMark ? (
            <Badge label={markDisplayStatusLabel(1, false)} tone={markDisplayStatusTone(1, false)} />
          ) : mark?.wasAbsent ? (
            <Badge label={markDisplayStatusLabel(3, true)} tone={markDisplayStatusTone(3, true)} />
          ) : (
            <>
              <Text style={styles.score}>{formatPercentage(mark?.percentage)}</Text>
              {mark?.rawMark != null && (
                <Text style={styles.rawMark}>{mark.rawMark} / {detail.maxMarks} marks</Text>
              )}
              {mark?.achievementLevel != null && (
                <View style={styles.achievementBadge}>
                  <Badge
                    label={`Level ${mark.achievementLevel} · ${ACHIEVEMENT_LEVEL_LABELS[mark.achievementLevel]}`}
                    tone={ACHIEVEMENT_LEVEL_TONES[mark.achievementLevel]}
                  />
                </View>
              )}
            </>
          )}
        </Card>

        {!!mark?.feedback && (
          <Card>
            <Text style={styles.sectionTitle}>Teacher feedback</Text>
            <Text style={styles.body}>{mark.feedback}</Text>
          </Card>
        )}

        <Card>
          <Text style={styles.sectionTitle}>About this assessment</Text>
          <InfoRow label="Max marks" value={String(detail.maxMarks)} />
          <InfoRow label="Weight" value={`${detail.weight}%`} />
          <InfoRow label="Pass mark" value={`${detail.passPercentage}%`} />
          {!!detail.scheduledDate && <InfoRow label="Scheduled" value={formatDate(detail.scheduledDate)} />}
          {!!detail.dueDate && <InfoRow label="Due" value={formatDate(detail.dueDate)} />}
        </Card>

        {!!detail.instructions && (
          <Card>
            <Text style={styles.sectionTitle}>Instructions</Text>
            <Text style={styles.body}>{detail.instructions}</Text>
          </Card>
        )}
      </View>
    </Screen>
  );
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <View style={styles.infoRow}>
      <Text style={styles.infoLabel}>{label}</Text>
      <Text style={styles.infoValue}>{value}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  subject: { color: colors.textMuted, fontSize: typography.overline, fontWeight: "700", textTransform: "uppercase" },
  title: { color: colors.text, fontSize: typography.heading, fontWeight: "800", marginTop: spacing.xs },
  metaRow: { flexDirection: "row", gap: spacing.sm, marginTop: spacing.md, marginBottom: spacing.xl },
  stack: { gap: spacing.lg },
  sectionTitle: { color: colors.text, fontSize: 16, fontWeight: "800", marginBottom: spacing.sm },
  score: { color: colors.text, fontSize: 32, fontWeight: "800" },
  rawMark: { color: colors.textMuted, fontSize: 14, marginTop: spacing.xs },
  achievementBadge: { marginTop: spacing.sm },
  body: { color: colors.text, fontSize: 15, lineHeight: 22 },
  infoRow: { flexDirection: "row", justifyContent: "space-between", paddingVertical: spacing.xs },
  infoLabel: { color: colors.textMuted, fontSize: 14 },
  infoValue: { color: colors.text, fontSize: 14, fontWeight: "700" },
});
