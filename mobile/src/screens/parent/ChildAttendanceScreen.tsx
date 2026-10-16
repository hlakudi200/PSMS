import { useCallback, useEffect, useMemo, useState } from "react";
import { RefreshControl, ScrollView, SectionList, StyleSheet, Text, View } from "react-native";
import { Badge, Button, Card, Chip, Screen, StateView } from "../../components";
import { colors, spacing, typography } from "../../theme";
import { useAttendanceActions, useAttendanceState } from "../../providers/attendance";
import type { IAttendanceRange, IAttendanceRecord, IAttendanceSummary } from "../../providers/attendance/context";
import { useMarksActions, useMarksState } from "../../providers/marks";
import type { ITerm } from "../../providers/marks/context";
import {
  ATTENDANCE_STATUS_LABELS, ATTENDANCE_STATUS_TONES, findAbsenceStreaks, formatSchoolDay, groupByMonth,
  type IAbsenceStreak,
} from "../../utils/attendance";
import { formatPercentage } from "../../utils/assessment-labels";
import { useSelectedChild } from "./useSelectedChild";

const WHOLE_YEAR = "year";

/** A term, or the whole year the terms belong to. */
function rangeFor(key: string, terms: ITerm[]): IAttendanceRange | undefined {
  if (key === WHOLE_YEAR) {
    if (terms.length === 0) return undefined;
    const starts = terms.map((t) => t.startDate).sort();
    const ends = terms.map((t) => t.endDate).sort();
    return { startDate: starts[0], endDate: ends[ends.length - 1] };
  }
  const term = terms.find((t) => t.id === key);
  return term ? { startDate: term.startDate, endDate: term.endDate } : undefined;
}

export function ChildAttendanceScreen() {
  const { child, myChildren, selectedChildId, selectChild, hasChildren, isChildrenError, retryChildren } = useSelectedChild();
  const { currentTerm, terms, isTermPending, isTermError } = useMarksState();
  const { getCurrentTermAsync } = useMarksActions();
  const { summary, records, studentId, range, isPending, isError } = useAttendanceState();
  const { getAttendanceAsync } = useAttendanceActions();
  const [rangeKey, setRangeKey] = useState<string>();

  useEffect(() => {
    if (!currentTerm) void getCurrentTermAsync();
  }, [currentTerm, getCurrentTermAsync]);

  // Default to the current term, as the Home card does.
  useEffect(() => {
    if (!rangeKey && currentTerm) setRangeKey(currentTerm.id);
  }, [currentTerm, rangeKey]);

  const selectedRange = useMemo(
    () => (rangeKey && terms ? rangeFor(rangeKey, terms) : undefined),
    [rangeKey, terms]
  );

  const childStudentId = child?.studentId;
  const isLoaded = !!childStudentId && !!selectedRange && studentId === childStudentId
    && range?.startDate === selectedRange.startDate && range?.endDate === selectedRange.endDate;

  const reload = useCallback(() => {
    if (childStudentId && selectedRange) void getAttendanceAsync(childStudentId, selectedRange);
  }, [childStudentId, selectedRange, getAttendanceAsync]);

  // A failed load leaves both deps unchanged, so it waits for a retry instead of looping.
  useEffect(() => {
    if (!isLoaded) reload();
  }, [isLoaded, reload]);

  const streaks = useMemo(() => (isLoaded ? findAbsenceStreaks(records ?? []) : []), [isLoaded, records]);
  const streakRecordIds = useMemo(() => new Set(streaks.flatMap((s) => s.recordIds)), [streaks]);
  const months = useMemo(() => (isLoaded ? groupByMonth(records ?? []) : []), [isLoaded, records]);

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
  if (!child || (isTermPending && !currentTerm) || !selectedRange) {
    return <Screen><StateView state="loading" message="Loading attendance…" /></Screen>;
  }

  const header = (
    <View>
      <Badge label="ATTENDANCE" />
      <Text style={styles.title}>{child.studentName}</Text>
      {!!(child.gradeName || child.className) && (
        <Text style={styles.subtitle}>{[child.gradeName, child.className].filter(Boolean).join(" · ")}</Text>
      )}

      {myChildren && myChildren.length > 1 && (
        <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.chips}>
          {myChildren.map((c) => (
            <Chip key={c.studentId} label={c.studentName} selected={c.studentId === selectedChildId} onPress={() => selectChild(c.studentId)} />
          ))}
        </ScrollView>
      )}

      {terms && terms.length > 0 && (
        <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.chips}>
          {terms.map((term) => (
            <Chip key={term.id} label={term.termName} selected={term.id === rangeKey} onPress={() => setRangeKey(term.id)} />
          ))}
          <Chip label="Whole year" selected={rangeKey === WHOLE_YEAR} onPress={() => setRangeKey(WHOLE_YEAR)} />
        </ScrollView>
      )}

      {isLoaded && isError && <Text style={styles.inlineError}>Couldn&apos;t refresh — showing what loaded earlier.</Text>}
      {isLoaded && summary && <SummaryCard summary={summary} />}
      {streaks.map((streak) => <StreakCard key={streak.startDate} streak={streak} />)}
    </View>
  );

  if (!isLoaded) {
    return (
      <Screen>
        {header}
        {isError && !isPending ? (
          <>
            <StateView state="error" message="Couldn't load attendance." />
            <Button onPress={reload}>Retry</Button>
          </>
        ) : (
          <StateView state="loading" message="Loading attendance…" />
        )}
      </Screen>
    );
  }

  return (
    <Screen>
      <SectionList
        sections={months}
        keyExtractor={(record) => record.id}
        contentContainerStyle={styles.list}
        stickySectionHeadersEnabled={false}
        refreshControl={<RefreshControl refreshing={isPending} onRefresh={reload} />}
        ListHeaderComponent={header}
        ListEmptyComponent={<StateView state="empty" message="No register has been captured for this period yet." />}
        renderSectionHeader={({ section }) => <Text style={styles.monthTitle}>{section.title}</Text>}
        renderItem={({ item }) => <DayRow record={item} isInStreak={streakRecordIds.has(item.id)} />}
      />
    </Screen>
  );
}

function SummaryCard({ summary }: { summary: IAttendanceSummary }) {
  if (summary.totalDays === 0) {
    return <Card style={styles.card}><Text style={styles.muted}>No register has been captured for this period yet.</Text></Card>;
  }
  const atSchool = summary.presentCount + summary.lateCount;
  const counts: { label: string; value: number; tone: "success" | "warning" | "danger" | "muted" }[] = [
    { label: "Present", value: summary.presentCount, tone: "success" },
    { label: "Late", value: summary.lateCount, tone: "warning" },
    { label: "Absent", value: summary.absentCount, tone: "danger" },
    { label: "Excused", value: summary.excusedCount, tone: "muted" },
    { label: "Sick leave", value: summary.sickLeaveCount, tone: "muted" },
  ];

  return (
    <Card style={styles.card}>
      <Text style={styles.label}>Attendance</Text>
      <Text style={styles.percentage}>{formatPercentage(summary.attendancePercentage)}</Text>
      <Text style={styles.muted}>At school {atSchool} of {summary.totalDays} recorded day{summary.totalDays === 1 ? "" : "s"}</Text>
      <View style={styles.countRow}>
        {counts.map((count) => (
          <View key={count.label} style={styles.count}>
            <Text style={styles.countValue}>{count.value}</Text>
            <Badge label={count.label} tone={count.value > 0 ? count.tone : "muted"} />
          </View>
        ))}
      </View>
    </Card>
  );
}

function StreakCard({ streak }: { streak: IAbsenceStreak }) {
  const span = streak.startDate === streak.endDate
    ? formatSchoolDay(streak.startDate)
    : `${formatSchoolDay(streak.startDate)} – ${formatSchoolDay(streak.endDate)}`;
  return (
    <Card style={[styles.card, styles.streakCard]}>
      <Text style={styles.streakTitle}>Away {streak.days} school days in a row</Text>
      <Text style={styles.streakDetail}>{span}</Text>
      {streak.unexplainedDays > 0 && (
        <Text style={styles.streakDetail}>
          {streak.unexplainedDays === streak.days ? "No reason given for any of them." : `${streak.unexplainedDays} without a reason given.`}
        </Text>
      )}
    </Card>
  );
}

function DayRow({ record, isInStreak }: { record: IAttendanceRecord; isInStreak: boolean }) {
  return (
    <View style={[styles.dayRow, isInStreak && styles.dayRowStreak]}>
      <View style={styles.dayTop}>
        <Text style={styles.dayDate}>{formatSchoolDay(record.attendanceDate)}</Text>
        <Badge label={ATTENDANCE_STATUS_LABELS[record.status] ?? "Recorded"} tone={ATTENDANCE_STATUS_TONES[record.status] ?? "muted"} />
      </View>
      {isInStreak && <Text style={styles.inRow}>Part of a run of absences</Text>}
      {!!record.notes && <Text style={styles.note}>{record.notes}</Text>}
    </View>
  );
}

const styles = StyleSheet.create({
  list: { paddingBottom: spacing.xxl },
  title: { color: colors.text, fontSize: typography.title, fontWeight: "800", marginTop: spacing.lg },
  subtitle: { color: colors.textMuted, fontSize: 14, marginTop: spacing.xs },
  chips: { gap: spacing.sm, marginTop: spacing.md },
  inlineError: { color: colors.danger, fontSize: 14, marginTop: spacing.md },
  card: { marginTop: spacing.md },
  label: { color: colors.textMuted, fontSize: 13, fontWeight: "700" },
  percentage: { color: colors.text, fontSize: 32, fontWeight: "800", marginTop: spacing.xs },
  muted: { color: colors.textMuted, fontSize: 13, marginTop: spacing.xs },
  countRow: { flexDirection: "row", flexWrap: "wrap", gap: spacing.lg, marginTop: spacing.lg },
  count: { alignItems: "flex-start", gap: spacing.xs },
  countValue: { color: colors.text, fontSize: 18, fontWeight: "800" },
  streakCard: { backgroundColor: colors.dangerBackground, borderColor: colors.danger },
  streakTitle: { color: colors.danger, fontSize: 16, fontWeight: "800" },
  streakDetail: { color: colors.text, fontSize: 14, marginTop: spacing.xs },
  monthTitle: { color: colors.text, fontSize: 18, fontWeight: "800", marginTop: spacing.xl, marginBottom: spacing.xs },
  dayRow: {
    borderBottomColor: colors.border, borderBottomWidth: 1, borderLeftColor: "transparent", borderLeftWidth: 3,
    minHeight: 56, paddingLeft: spacing.sm, paddingVertical: spacing.md,
  },
  dayRowStreak: { borderLeftColor: colors.danger },
  dayTop: { alignItems: "center", flexDirection: "row", justifyContent: "space-between" },
  dayDate: { color: colors.text, fontSize: 15, fontWeight: "700" },
  inRow: { color: colors.danger, fontSize: 13, fontWeight: "700", marginTop: spacing.xs },
  note: { color: colors.text, fontSize: 14, marginTop: spacing.xs },
});
