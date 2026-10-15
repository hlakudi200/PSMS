import { useEffect, useMemo, useState } from "react";
import { FlatList, Pressable, RefreshControl, ScrollView, StyleSheet, Text, View } from "react-native";
import { router } from "expo-router";
import { Badge, Button, Card, Chip, Screen, StateView } from "../../components";
import { colors, spacing, typography } from "../../theme";
import { useChildrenActions, useChildrenState } from "../../providers/children";
import { useMarksActions, useMarksState } from "../../providers/marks";
import {
  ACHIEVEMENT_LEVEL_LABELS, ACHIEVEMENT_LEVEL_TONES, achievementLevelFor, formatPercentage,
} from "../../utils/assessment-labels";
import type { ISubjectSection } from "../../utils/marks-sections";
import { useChildMarks } from "./useChildMarks";

export function ChildPerformanceScreen() {
  const { myChildren, selectedChildId } = useChildrenState();
  const { selectChild } = useChildrenActions();
  const { currentTerm, terms, isTermPending, isTermError, subjectTrend } = useMarksState();
  const { getCurrentTermAsync, getSubjectTrendAsync } = useMarksActions();
  const [selectedTermId, setSelectedTermId] = useState<string>();

  useEffect(() => {
    if (!currentTerm) void getCurrentTermAsync();
  }, [currentTerm, getCurrentTermAsync]);

  useEffect(() => {
    if (!selectedTermId && currentTerm) setSelectedTermId(currentTerm.id);
  }, [currentTerm, selectedTermId]);

  const {
    child, hasChildren, isChildrenError, retryChildren, isLoaded, isMarksPending, isMarksError, sections, reload,
  } = useChildMarks(selectedTermId);

  const childStudentId = child?.studentId;
  useEffect(() => {
    if (childStudentId && terms) void getSubjectTrendAsync(childStudentId);
  }, [childStudentId, terms, getSubjectTrendAsync]);

  const previousAverages = useMemo(() => {
    if (!child || !terms || !selectedTermId || subjectTrend?.studentId !== child.studentId) return {};
    const index = terms.findIndex((t) => t.id === selectedTermId);
    const previousTerm = index > 0 ? terms[index - 1] : undefined;
    return (previousTerm && subjectTrend.averagesByTermId[previousTerm.id]) ?? {};
  }, [child, terms, selectedTermId, subjectTrend]);

  if (isChildrenError) {
    return (
      <Screen>
        <StateView state="error" message="Couldn't load your children." />
        <Button onPress={() => void retryChildren()}>Retry</Button>
      </Screen>
    );
  }
  if (hasChildren === false) {
    return <Screen><StateView state="empty" message="No children are linked to your account yet. Your school office can link them." /></Screen>;
  }
  if (isTermError) {
    return (
      <Screen>
        <StateView state="error" message="Couldn't load the current term." />
        <Button onPress={() => void getCurrentTermAsync()}>Retry</Button>
      </Screen>
    );
  }
  if (!child || (isTermPending && !currentTerm) || !selectedTermId) {
    return <Screen><StateView state="loading" message="Loading progress…" /></Screen>;
  }

  const header = (
    <View>
      <Badge label="PROGRESS" />
      <Text style={styles.title}>{child.studentName}</Text>
      {!!(child.gradeName || child.className) && (
        <Text style={styles.subtitle}>{[child.gradeName, child.className].filter(Boolean).join(" · ")}</Text>
      )}

      {myChildren && myChildren.length > 1 && (
        <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.chips}>
          {myChildren.map((c) => (
            <Chip
              key={c.studentId}
              label={c.studentName}
              selected={c.studentId === selectedChildId}
              onPress={() => selectChild(c.studentId)}
            />
          ))}
        </ScrollView>
      )}

      <AttendanceCard
        percentage={child.attendancePercentage}
        daysPresent={child.attendanceDaysPresent}
        totalDays={child.attendanceTotalDays}
        onPress={() => router.push("/(parent)/attendance")}
      />

      <Text style={styles.sectionTitle}>Marks</Text>
      {terms && terms.length > 0 && (
        <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.chips}>
          {terms.map((term) => (
            <Chip
              key={term.id}
              label={term.termName}
              selected={term.id === selectedTermId}
              onPress={() => setSelectedTermId(term.id)}
            />
          ))}
        </ScrollView>
      )}

      {isLoaded && isMarksError && (
        <Text style={styles.inlineError}>Couldn&apos;t refresh — showing what loaded earlier.</Text>
      )}
    </View>
  );

  if (!isLoaded) {
    return (
      <Screen>
        {header}
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

  return (
    <Screen>
      <FlatList
        data={sections}
        keyExtractor={(section) => section.title}
        contentContainerStyle={styles.list}
        refreshControl={<RefreshControl refreshing={isMarksPending} onRefresh={reload} />}
        ListHeaderComponent={header}
        ListEmptyComponent={<StateView state="empty" message="No assessments set for this term yet." />}
        renderItem={({ item }) => (
          <SubjectCard
            section={item}
            previousAverage={previousAverages[item.title]}
            onPress={() => router.push({ pathname: "/(parent)/subject", params: { subject: item.title, termId: selectedTermId } })}
          />
        )}
      />
    </Screen>
  );
}

/** This term's figure, already loaded for Home; the attendance screen has the detail. */
function AttendanceCard({ percentage, daysPresent, totalDays, onPress }: {
  percentage?: number;
  daysPresent?: number;
  totalDays?: number;
  onPress: () => void;
}) {
  return (
    <Pressable accessibilityRole="button" accessibilityLabel="Open attendance" onPress={onPress} style={styles.cardWrapper}>
      <Card>
        <View style={styles.cardTop}>
          <Text style={styles.subjectName}>Attendance this term</Text>
          {percentage != null && <Text style={styles.average}>{formatPercentage(percentage)}</Text>}
        </View>
        <Text style={styles.meta}>
          {percentage != null
            ? `Present ${daysPresent ?? 0} of ${totalDays} recorded days · see every day`
            : "No register captured yet · see attendance"}
        </Text>
      </Card>
    </Pressable>
  );
}

function SubjectCard({ section, previousAverage, onPress }: {
  section: ISubjectSection;
  previousAverage?: number;
  onPress: () => void;
}) {
  const level = section.average != null ? achievementLevelFor(section.average) : undefined;
  const delta = section.average != null && previousAverage != null
    ? Math.round((section.average - previousAverage) * 10) / 10
    : undefined;
  const releasedCount = section.data.length - section.pendingCount;

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={`${section.title}, ${section.average != null ? formatPercentage(section.average) : "no marks released yet"}`}
      onPress={onPress}
      style={styles.cardWrapper}
    >
      <Card>
        <View style={styles.cardTop}>
          <Text style={styles.subjectName} numberOfLines={2}>{section.title}</Text>
          {section.average != null ? (
            <Text style={styles.average}>{formatPercentage(section.average)}</Text>
          ) : (
            <Badge label="Pending" tone="muted" />
          )}
        </View>

        {level != null && (
          <View style={styles.badgeRow}>
            <Badge label={`Level ${level} · ${ACHIEVEMENT_LEVEL_LABELS[level]}`} tone={ACHIEVEMENT_LEVEL_TONES[level]} />
          </View>
        )}

        <View style={styles.metaRow}>
          <Text style={styles.meta}>
            {releasedCount} of {section.data.length} assessment{section.data.length === 1 ? "" : "s"} marked
          </Text>
          {!!delta && (
            <Text style={[styles.delta, delta > 0 ? styles.deltaUp : styles.deltaDown]}>
              {delta > 0 ? "▲" : "▼"} {Math.abs(delta)} vs last term
            </Text>
          )}
        </View>
      </Card>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  title: { color: colors.text, fontSize: typography.title, fontWeight: "800", marginTop: spacing.lg },
  subtitle: { color: colors.textMuted, fontSize: 14, marginTop: spacing.xs },
  sectionTitle: { color: colors.text, fontSize: 18, fontWeight: "800", marginTop: spacing.xl },
  chips: { gap: spacing.sm, marginTop: spacing.md },
  inlineError: { color: colors.danger, fontSize: 14, marginTop: spacing.md },
  list: { paddingBottom: spacing.xxl },
  cardWrapper: { marginTop: spacing.md },
  cardTop: { alignItems: "center", flexDirection: "row", gap: spacing.sm, justifyContent: "space-between" },
  subjectName: { color: colors.text, flex: 1, fontSize: 17, fontWeight: "800" },
  average: { color: colors.text, fontSize: 22, fontWeight: "800" },
  badgeRow: { flexDirection: "row", marginTop: spacing.sm },
  metaRow: { alignItems: "center", flexDirection: "row", flexWrap: "wrap", gap: spacing.sm, justifyContent: "space-between", marginTop: spacing.sm },
  meta: { color: colors.textMuted, fontSize: 13 },
  delta: { fontSize: 13, fontWeight: "800" },
  deltaUp: { color: colors.success },
  deltaDown: { color: colors.danger },
});
