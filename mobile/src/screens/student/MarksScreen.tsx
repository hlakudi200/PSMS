import { useCallback, useEffect, useMemo, useState } from "react";
import { Pressable, RefreshControl, SectionList, StyleSheet, Text, View } from "react-native";
import { router } from "expo-router";
import { Badge, Button, Card, Screen, StateView } from "../../components";
import type { BadgeTone } from "../../components/Badge";
import { colors, radius, spacing, typography } from "../../theme";
import { useAuthState } from "../../providers/auth";
import { useMarksActions, useMarksState } from "../../providers/marks";
import type { ITerm } from "../../providers/marks/context";
import {
  ACHIEVEMENT_LEVEL_LABELS, ACHIEVEMENT_LEVEL_TONES,
  formatPercentage, markDisplayStatusLabel, markDisplayStatusTone,
} from "../../utils/assessment-labels";
import { buildSubjectSections, type IMarkRow, type ISubjectSection } from "../../utils/marks-sections";

type IRow = IMarkRow;

interface ISubjectSectionWithTrend extends ISubjectSection {
  previousAverage?: number;
}

export function MarksScreen() {
  const { currentStudentId, currentClassId } = useAuthState();
  const {
    currentTerm, terms, isTermPending, isTermError,
    marks, isMarksPending, isMarksError, classAssessments, assessmentDetailsById, subjectTrend,
  } = useMarksState();
  const { getCurrentTermAsync, getMarksByStudentAsync, getClassAssessmentsAsync, getSubjectTrendAsync } = useMarksActions();

  const [selectedTermId, setSelectedTermId] = useState<string | undefined>(undefined);

  useEffect(() => { void getCurrentTermAsync(); }, [getCurrentTermAsync]);

  useEffect(() => {
    if (!selectedTermId && currentTerm) setSelectedTermId(currentTerm.id);
  }, [currentTerm, selectedTermId]);

  const reloadMarks = useCallback(() => {
    if (currentStudentId && selectedTermId) void getMarksByStudentAsync(currentStudentId, selectedTermId);
    if (currentClassId && selectedTermId) void getClassAssessmentsAsync(currentClassId, selectedTermId);
  }, [currentStudentId, currentClassId, selectedTermId, getMarksByStudentAsync, getClassAssessmentsAsync]);

  useEffect(() => { reloadMarks(); }, [reloadMarks]);

  // Best-effort trend comparison against the previous term — non-blocking.
  useEffect(() => {
    if (currentStudentId && terms) void getSubjectTrendAsync(currentStudentId);
  }, [currentStudentId, terms, getSubjectTrendAsync]);

  const previousAverages = useMemo(() => {
    if (!terms || !selectedTermId || subjectTrend?.studentId !== currentStudentId) return {};
    const index = terms.findIndex((t) => t.id === selectedTermId);
    const previousTerm = index > 0 ? terms[index - 1] : undefined;
    return (previousTerm && subjectTrend?.averagesByTermId[previousTerm.id]) ?? {};
  }, [terms, selectedTermId, subjectTrend, currentStudentId]);

  const sections = useMemo<ISubjectSectionWithTrend[]>(
    () => buildSubjectSections(marks ?? [], classAssessments ?? [], assessmentDetailsById)
      .map((section) => ({ ...section, previousAverage: previousAverages[section.title] })),
    [marks, classAssessments, assessmentDetailsById, previousAverages]
  );

  const openAssessment = useCallback((assessmentId: string) => {
    router.push({ pathname: "/(student)/assessment/[id]", params: { id: assessmentId } });
  }, []);

  if (isTermPending && !currentTerm) return <Screen><StateView state="loading" message="Loading your term…" /></Screen>;
  if (isTermError) {
    return (
      <Screen>
        <StateView state="error" message="Couldn't load the current term." />
        <Button onPress={() => void getCurrentTermAsync()}>Retry</Button>
      </Screen>
    );
  }

  return (
    <Screen>
      <Text style={styles.title}>My Marks</Text>

      {terms && terms.length > 0 && (
        <View style={styles.termRow}>
          {terms.map((term) => (
            <TermChip key={term.id} term={term} selected={term.id === selectedTermId} onPress={() => setSelectedTermId(term.id)} />
          ))}
        </View>
      )}

      {isMarksPending && !marks && <StateView state="loading" message="Loading your marks…" />}
      {!isMarksPending && isMarksError && (
        <>
          <StateView state="error" message="Couldn't load your marks." />
          <Button onPress={reloadMarks}>Retry</Button>
        </>
      )}
      {!isMarksError && !(isMarksPending && !marks) && sections.length === 0 && (
        <StateView state="empty" message="No assessments recorded for this term yet." />
      )}

      {!isMarksError && !(isMarksPending && !marks) && sections.length > 0 && (
        <SectionList
          sections={sections}
          keyExtractor={(item) => item.key}
          contentContainerStyle={styles.list}
          renderSectionHeader={({ section }) => <SubjectHeader section={section} />}
          renderItem={({ item }) => <MarkRow row={item} onPress={() => openAssessment(item.assessmentId)} />}
          stickySectionHeadersEnabled={false}
          refreshControl={<RefreshControl refreshing={isMarksPending} onRefresh={reloadMarks} />}
        />
      )}
    </Screen>
  );
}

function TermChip({ term, selected, onPress }: { term: ITerm; selected: boolean; onPress: () => void }) {
  return (
    <Pressable accessibilityRole="button" onPress={onPress} style={[chipStyles.chip, selected && chipStyles.chipSelected]}>
      <Text style={[chipStyles.label, selected && chipStyles.labelSelected]}>{term.termName}</Text>
    </Pressable>
  );
}

function SubjectHeader({ section }: { section: ISubjectSectionWithTrend }) {
  const delta = section.average != null && section.previousAverage != null
    ? Math.round((section.average - section.previousAverage) * 10) / 10
    : undefined;
  return (
    <View style={styles.sectionHeader}>
      <Text style={styles.sectionTitle}>{section.title}</Text>
      <View style={styles.sectionAverage}>
        {section.average != null && <Text style={styles.averageText}>{formatPercentage(section.average)} avg</Text>}
        {!!delta && (
          <Text style={[styles.deltaText, delta > 0 ? styles.deltaUp : styles.deltaDown]}>
            {delta > 0 ? "▲" : "▼"} {Math.abs(delta)}
          </Text>
        )}
      </View>
    </View>
  );
}

function MarkRow({ row, onPress }: { row: IRow; onPress: () => void }) {
  const showScore = row.hasMark && row.marksReleased && !row.wasAbsent;
  const statusLabel: string = row.hasMark && row.marksReleased
    ? (row.wasAbsent ? markDisplayStatusLabel(3, true) : markDisplayStatusLabel(2, true))
    : markDisplayStatusLabel(1, false);
  const statusTone: BadgeTone = row.hasMark && row.marksReleased
    ? (row.wasAbsent ? markDisplayStatusTone(3, true) : markDisplayStatusTone(2, true))
    : markDisplayStatusTone(1, false);

  return (
    <Pressable accessibilityRole="button" onPress={onPress} style={rowStyles.wrapper}>
      <Card>
        <View style={rowStyles.top}>
          <Text style={rowStyles.name} numberOfLines={2}>{row.assessmentName}</Text>
          <Badge label={statusLabel} tone={statusTone} />
        </View>
        {showScore ? (
          <>
            <View style={rowStyles.scoreRow}>
              <Text style={rowStyles.score}>{formatPercentage(row.percentage)}</Text>
              {row.rawMark != null && <Text style={rowStyles.rawMark}>({row.rawMark} marks)</Text>}
            </View>
            {row.achievementLevel != null && (
              <View style={rowStyles.achievementBadge}>
                <Badge
                  label={`Level ${row.achievementLevel} · ${ACHIEVEMENT_LEVEL_LABELS[row.achievementLevel]}`}
                  tone={ACHIEVEMENT_LEVEL_TONES[row.achievementLevel]}
                />
              </View>
            )}
          </>
        ) : row.hasMark && row.wasAbsent ? (
          <Text style={rowStyles.pending}>Absent for this assessment</Text>
        ) : (
          <Text style={rowStyles.pending}>Marks not released yet</Text>
        )}
        {row.hasFeedback && <Text style={rowStyles.feedbackHint}>Has teacher feedback</Text>}
      </Card>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  title: { color: colors.text, fontSize: typography.title, fontWeight: "800", marginBottom: spacing.lg },
  termRow: { flexDirection: "row", flexWrap: "wrap", gap: spacing.sm, marginBottom: spacing.lg },
  list: { paddingBottom: spacing.xxl },
  sectionHeader: { alignItems: "center", flexDirection: "row", justifyContent: "space-between", marginTop: spacing.lg, marginBottom: spacing.sm },
  sectionTitle: { color: colors.text, fontSize: 18, fontWeight: "800" },
  sectionAverage: { alignItems: "center", flexDirection: "row", gap: spacing.sm },
  averageText: { color: colors.textMuted, fontSize: 14, fontWeight: "700" },
  deltaText: { fontSize: 13, fontWeight: "800" },
  deltaUp: { color: colors.success },
  deltaDown: { color: colors.danger },
});

const chipStyles = StyleSheet.create({
  chip: { backgroundColor: colors.surface, borderColor: colors.border, borderRadius: radius.pill, borderWidth: 1, paddingHorizontal: spacing.md, paddingVertical: spacing.sm },
  chipSelected: { backgroundColor: colors.primary, borderColor: colors.primary },
  label: { color: colors.text, fontSize: 13, fontWeight: "700" },
  labelSelected: { color: colors.surface },
});

const rowStyles = StyleSheet.create({
  wrapper: { marginBottom: spacing.md },
  top: { alignItems: "flex-start", flexDirection: "row", gap: spacing.sm, justifyContent: "space-between" },
  name: { color: colors.text, flex: 1, fontSize: 16, fontWeight: "700" },
  scoreRow: { alignItems: "baseline", flexDirection: "row", gap: spacing.xs, marginTop: spacing.sm },
  score: { color: colors.text, fontSize: 22, fontWeight: "800" },
  rawMark: { color: colors.textMuted, fontSize: 13 },
  pending: { color: colors.textMuted, fontSize: 14, marginTop: spacing.sm },
  achievementBadge: { marginTop: spacing.sm },
  feedbackHint: { color: colors.primary, fontSize: 13, fontWeight: "700", marginTop: spacing.sm },
});
