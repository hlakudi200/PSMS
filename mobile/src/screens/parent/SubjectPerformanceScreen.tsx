import { FlatList, RefreshControl, StyleSheet, Text, View } from "react-native";
import { Badge, Button, Card, Screen, StateView } from "../../components";
import { colors, spacing, typography } from "../../theme";
import { useMarksState } from "../../providers/marks";
import {
  ACHIEVEMENT_LEVEL_LABELS, ACHIEVEMENT_LEVEL_TONES, ASSESSMENT_TYPE_LABELS,
  achievementLevelFor, formatDate, formatPercentage, markDisplayStatusLabel, markDisplayStatusTone,
} from "../../utils/assessment-labels";
import { isGradedRow, type IMarkRow } from "../../utils/marks-sections";
import { useChildMarks } from "./useChildMarks";

export function SubjectPerformanceScreen({ subjectName, termId }: { subjectName: string; termId: string }) {
  const { terms, subjectTrend } = useMarksState();
  const { child, isChildrenError, retryChildren, isLoaded, isMarksPending, isMarksError, sections, reload } =
    useChildMarks(termId);

  if (isChildrenError) {
    return (
      <Screen>
        <StateView state="error" message="Couldn't load your children." />
        <Button onPress={() => void retryChildren()}>Retry</Button>
      </Screen>
    );
  }
  if (!isLoaded) {
    return (
      <Screen>
        {isMarksError && !isMarksPending ? (
          <>
            <StateView state="error" message="Couldn't load marks." />
            <Button onPress={reload}>Retry</Button>
          </>
        ) : (
          <StateView state="loading" message="Loading marks…" />
        )}
      </Screen>
    );
  }

  const section = sections.find((s) => s.title === subjectName);
  const termName = terms?.find((t) => t.id === termId)?.termName;

  // Only terms with a released mark in this subject; a trend needs at least two.
  const trend = subjectTrend?.studentId === child?.studentId
    ? (terms ?? [])
      .map((term) => ({ term, average: subjectTrend?.averagesByTermId[term.id]?.[subjectName] }))
      .filter((point): point is { term: typeof point.term; average: number } => point.average != null)
    : [];

  const level = section?.average != null ? achievementLevelFor(section.average) : undefined;

  return (
    <Screen>
      <FlatList
        data={section?.data ?? []}
        keyExtractor={(row) => row.key}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={isMarksPending} onRefresh={reload} />}
        ListHeaderComponent={
          <View>
            <Text style={styles.overline}>{[child?.studentName, termName].filter(Boolean).join(" · ")}</Text>
            <Text style={styles.title}>{subjectName}</Text>

            <Card style={styles.summary}>
              <Text style={styles.label}>Term average</Text>
              {section?.average != null ? (
                <>
                  <Text style={styles.average}>{formatPercentage(section.average)}</Text>
                  {level != null && (
                    <View style={styles.badgeRow}>
                      <Badge label={`Level ${level} · ${ACHIEVEMENT_LEVEL_LABELS[level]}`} tone={ACHIEVEMENT_LEVEL_TONES[level]} />
                    </View>
                  )}
                </>
              ) : (
                <Text style={styles.muted}>No marks released yet this term.</Text>
              )}

              {trend.length > 1 && (
                <View style={styles.trend}>
                  <Text style={styles.label}>Across the year</Text>
                  <View style={styles.trendRow}>
                    {trend.map(({ term, average }) => (
                      <View key={term.id} style={styles.trendPoint}>
                        <Text style={styles.trendValue}>{formatPercentage(average)}</Text>
                        <Text style={styles.muted}>{term.termName}</Text>
                      </View>
                    ))}
                  </View>
                </View>
              )}
            </Card>

            <Text style={styles.sectionTitle}>Assessments</Text>
          </View>
        }
        ListEmptyComponent={<StateView state="empty" message={`No ${subjectName} assessments this term.`} />}
        renderItem={({ item }) => <AssessmentRow row={item} />}
      />
    </Screen>
  );
}

function AssessmentRow({ row }: { row: IMarkRow }) {
  const isReleased = row.hasMark && row.marksReleased;
  const status = isReleased
    ? { label: markDisplayStatusLabel(row.wasAbsent ? 3 : 2, true), tone: markDisplayStatusTone(row.wasAbsent ? 3 : 2, true) }
    : { label: markDisplayStatusLabel(1, false), tone: markDisplayStatusTone(1, false) };
  const meta = [
    row.assessmentType != null ? ASSESSMENT_TYPE_LABELS[row.assessmentType] : undefined,
    row.date ? formatDate(row.date) : undefined,
    row.maxMarks != null ? `out of ${row.maxMarks}` : undefined,
  ].filter(Boolean).join(" · ");

  return (
    <Card style={styles.row}>
      <View style={styles.rowTop}>
        <Text style={styles.rowName} numberOfLines={2}>{row.assessmentName}</Text>
        <Badge label={status.label} tone={status.tone} />
      </View>
      {!!meta && <Text style={styles.muted}>{meta}</Text>}

      {isGradedRow(row) ? (
        <>
          <View style={styles.scoreRow}>
            <Text style={styles.score}>{formatPercentage(row.percentage)}</Text>
            {row.rawMark != null && row.maxMarks != null && (
              <Text style={styles.muted}>{row.rawMark} / {row.maxMarks}</Text>
            )}
          </View>
          {row.achievementLevel != null && (
            <View style={styles.badgeRow}>
              <Badge
                label={`Level ${row.achievementLevel} · ${ACHIEVEMENT_LEVEL_LABELS[row.achievementLevel]}`}
                tone={ACHIEVEMENT_LEVEL_TONES[row.achievementLevel]}
              />
            </View>
          )}
        </>
      ) : isReleased && row.wasAbsent ? (
        <Text style={styles.pending}>Absent for this assessment</Text>
      ) : (
        <Text style={styles.pending}>The teacher hasn&apos;t released marks yet</Text>
      )}

      {/* Feedback belongs to the mark, so it's only shown once the mark itself is released. */}
      {isReleased && !!row.feedback && (
        <View style={styles.feedback}>
          <Text style={styles.label}>Teacher feedback</Text>
          <Text style={styles.feedbackText}>{row.feedback}</Text>
        </View>
      )}
    </Card>
  );
}

const styles = StyleSheet.create({
  list: { paddingBottom: spacing.xxl },
  overline: { color: colors.textMuted, fontSize: typography.overline, fontWeight: "700", textTransform: "uppercase" },
  title: { color: colors.text, fontSize: typography.heading, fontWeight: "800", marginTop: spacing.xs },
  summary: { marginTop: spacing.lg },
  label: { color: colors.textMuted, fontSize: 13, fontWeight: "700" },
  average: { color: colors.text, fontSize: 32, fontWeight: "800", marginTop: spacing.xs },
  badgeRow: { flexDirection: "row", marginTop: spacing.sm },
  muted: { color: colors.textMuted, fontSize: 13, marginTop: spacing.xs },
  trend: { borderTopColor: colors.border, borderTopWidth: 1, marginTop: spacing.lg, paddingTop: spacing.md },
  trendRow: { flexDirection: "row", flexWrap: "wrap", gap: spacing.xl, marginTop: spacing.sm },
  trendPoint: { minWidth: 64 },
  trendValue: { color: colors.text, fontSize: 18, fontWeight: "800" },
  sectionTitle: { color: colors.text, fontSize: 18, fontWeight: "800", marginTop: spacing.xl },
  row: { marginTop: spacing.md },
  rowTop: { alignItems: "flex-start", flexDirection: "row", gap: spacing.sm, justifyContent: "space-between" },
  rowName: { color: colors.text, flex: 1, fontSize: 16, fontWeight: "700" },
  scoreRow: { alignItems: "baseline", flexDirection: "row", gap: spacing.sm, marginTop: spacing.sm },
  score: { color: colors.text, fontSize: 22, fontWeight: "800" },
  pending: { color: colors.textMuted, fontSize: 14, marginTop: spacing.sm },
  feedback: { backgroundColor: colors.primaryLight, borderRadius: 8, marginTop: spacing.md, padding: spacing.md },
  feedbackText: { color: colors.text, fontSize: 15, lineHeight: 22, marginTop: spacing.xs },
});
