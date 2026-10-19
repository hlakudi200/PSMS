import { useCallback, useEffect, useMemo } from "react";
import { Pressable, RefreshControl, ScrollView, SectionList, StyleSheet, Text, View } from "react-native";
import { router } from "expo-router";
import { Badge, Button, Card, Chip, Screen, StateView } from "../../components";
import { colors, spacing, typography } from "../../theme";
import { useReportCardsActions, useReportCardsState } from "../../providers/report-cards";
import type { IReportCardSummary } from "../../providers/report-cards/context";
import {
  ACHIEVEMENT_LEVEL_LABELS, ACHIEVEMENT_LEVEL_TONES, formatDate, formatPercentage,
} from "../../utils/assessment-labels";
import { reportTitle } from "../../utils/report-labels";
import { useSelectedChild } from "./useSelectedChild";

export function ChildReportCardsScreen() {
  const { child, myChildren, selectedChildId, selectChild, hasChildren, isChildrenError, retryChildren } = useSelectedChild();
  const { reports, reportsStudentId, isPending, isError } = useReportCardsState();
  const { getReportsAsync } = useReportCardsActions();

  const studentId = child?.studentId;
  const isLoaded = !!studentId && reportsStudentId === studentId;

  const reload = useCallback(() => {
    if (studentId) void getReportsAsync(studentId);
  }, [studentId, getReportsAsync]);

  // A failed load leaves both deps unchanged, so it waits for a retry instead of looping.
  useEffect(() => {
    if (!isLoaded) reload();
  }, [isLoaded, reload]);

  // Newest year first; the list arrives newest first, so the order holds within each year.
  const sections = useMemo(() => {
    if (!isLoaded) return [];
    const byYear: { title: string; data: IReportCardSummary[] }[] = [];
    for (const report of reports ?? []) {
      const title = report.academicYearName ?? "Other";
      const year = byYear.find((y) => y.title === title);
      if (year) year.data.push(report);
      else byYear.push({ title, data: [report] });
    }
    return byYear;
  }, [isLoaded, reports]);

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
  if (!child) return <Screen><StateView state="loading" message="Loading report cards…" /></Screen>;

  const header = (
    <View>
      <Badge label="REPORT CARDS" />
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
      {isLoaded && isError && <Text style={styles.inlineError}>Couldn&apos;t refresh — showing what loaded earlier.</Text>}
    </View>
  );

  if (!isLoaded) {
    return (
      <Screen>
        {header}
        {isError && !isPending ? (
          <>
            <StateView state="error" message="Couldn't load report cards." />
            <Button onPress={reload}>Retry</Button>
          </>
        ) : (
          <StateView state="loading" message="Loading report cards…" />
        )}
      </Screen>
    );
  }

  return (
    <Screen>
      <SectionList
        sections={sections}
        keyExtractor={(report) => report.id}
        contentContainerStyle={styles.list}
        stickySectionHeadersEnabled={false}
        refreshControl={<RefreshControl refreshing={isPending} onRefresh={reload} />}
        ListHeaderComponent={header}
        ListEmptyComponent={<StateView state="empty" message="No report cards have been issued yet. They appear here once the school publishes them." />}
        renderSectionHeader={({ section }) => <Text style={styles.yearTitle}>{section.title}</Text>}
        renderItem={({ item }) => (
          <ReportRow
            report={item}
            onPress={() => router.push({ pathname: "/(parent)/report/[id]", params: { id: item.id } })}
          />
        )}
      />
    </Screen>
  );
}

function ReportRow({ report, onPress }: { report: IReportCardSummary; onPress: () => void }) {
  const level = report.overallAchievementLevel;
  return (
    <Pressable accessibilityRole="button" accessibilityLabel={`Open ${reportTitle(report)} report card`} onPress={onPress} style={styles.rowWrapper}>
      <Card>
        <View style={styles.rowTop}>
          <Text style={styles.rowTitle}>{reportTitle(report)}</Text>
          {/* Foundation Phase cards report levels only (§17(4)(a)), so no percentage there. */}
          {report.reportsPercentages && report.overallPercentage != null && (
            <Text style={styles.percentage}>{formatPercentage(report.overallPercentage)}</Text>
          )}
        </View>
        {level != null && (
          <View style={styles.badgeRow}>
            <Badge label={`Level ${level} · ${ACHIEVEMENT_LEVEL_LABELS[level]}`} tone={ACHIEVEMENT_LEVEL_TONES[level]} />
          </View>
        )}
        {!!report.publishedDate && <Text style={styles.muted}>Issued {formatDate(report.publishedDate)}</Text>}
      </Card>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  list: { paddingBottom: spacing.xxl },
  title: { color: colors.text, fontSize: typography.title, fontWeight: "800", marginTop: spacing.lg },
  subtitle: { color: colors.textMuted, fontSize: 14, marginTop: spacing.xs },
  chips: { gap: spacing.sm, marginTop: spacing.md },
  inlineError: { color: colors.danger, fontSize: 14, marginTop: spacing.md },
  yearTitle: { color: colors.text, fontSize: 18, fontWeight: "800", marginTop: spacing.xl },
  rowWrapper: { marginTop: spacing.md },
  rowTop: { alignItems: "center", flexDirection: "row", justifyContent: "space-between" },
  rowTitle: { color: colors.text, flex: 1, fontSize: 17, fontWeight: "800" },
  percentage: { color: colors.text, fontSize: 22, fontWeight: "800" },
  badgeRow: { flexDirection: "row", marginTop: spacing.sm },
  muted: { color: colors.textMuted, fontSize: 13, marginTop: spacing.sm },
});
