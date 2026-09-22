import { useCallback, useEffect, useMemo, useState } from "react";
import { FlatList, Pressable, RefreshControl, StyleSheet, Text, View } from "react-native";
import { Badge, Button, Card, Screen, StateView } from "../../components";
import { colors, radius, spacing, typography } from "../../theme";
import { useAuthState } from "../../providers/auth";
import { useTimetableActions, useTimetableState } from "../../providers/timetable";
import {
  DAY_LABELS_FULL, WEEKDAY_KEYS, buildWeekGrid, findCurrentRowIndex, formatTimeShort,
  type IScheduleRow,
} from "../../utils/timetable";

type ViewMode = "today" | "week";

export function TimetableScreen() {
  const { currentClassId } = useAuthState();
  const { slots, isPending, isError } = useTimetableState();
  const { getForClassAsync } = useTimetableActions();

  const [mode, setMode] = useState<ViewMode>("today");
  const todayDayOfWeek = useMemo(() => new Date().getDay(), []);
  const isWeekendToday = todayDayOfWeek === 0 || todayDayOfWeek === 6;
  const [selectedDay, setSelectedDay] = useState(isWeekendToday ? 1 : todayDayOfWeek);

  const reload = useCallback(() => {
    if (currentClassId) void getForClassAsync(currentClassId);
  }, [currentClassId, getForClassAsync]);

  useEffect(() => { reload(); }, [reload]);

  const grid = useMemo(() => (slots && slots.length > 0 ? buildWeekGrid(slots) : undefined), [slots]);
  const days = grid?.days ?? [...WEEKDAY_KEYS];

  const rows = useMemo<IScheduleRow[]>(() => {
    if (!grid) return [];
    if (mode === "today") return isWeekendToday ? [] : grid.rowsByDay.get(todayDayOfWeek) ?? [];
    return grid.rowsByDay.get(selectedDay) ?? [];
  }, [grid, mode, isWeekendToday, todayDayOfWeek, selectedDay]);

  const currentIndex = mode === "today" ? findCurrentRowIndex(rows, new Date()) : -1;

  if ((isPending && !slots) || !currentClassId) {
    return <Screen><StateView state="loading" message="Loading your timetable…" /></Screen>;
  }
  if (isError) {
    return (
      <Screen>
        <StateView state="error" message="Couldn't load your timetable." />
        <Button onPress={reload}>Retry</Button>
      </Screen>
    );
  }

  return (
    <Screen>
      <Text style={styles.title}>My Timetable</Text>

      <View style={styles.modeRow}>
        <Chip label="Today" selected={mode === "today"} onPress={() => setMode("today")} />
        <Chip label="Week" selected={mode === "week"} onPress={() => setMode("week")} />
      </View>

      {mode === "week" && (
        <View style={styles.dayRow}>
          {days.map((day) => (
            <Chip
              key={day}
              label={DAY_LABELS_FULL[day].slice(0, 3)}
              selected={day === selectedDay}
              onPress={() => setSelectedDay(day)}
            />
          ))}
        </View>
      )}

      {!slots || slots.length === 0 ? (
        <StateView state="empty" message="Your school hasn't published a timetable for your class yet." />
      ) : mode === "today" && isWeekendToday ? (
        <StateView state="empty" message={`Enjoy your ${DAY_LABELS_FULL[todayDayOfWeek]} — no school today.`} />
      ) : (
        <FlatList
          data={rows}
          keyExtractor={(row) => row.key}
          contentContainerStyle={styles.list}
          refreshControl={<RefreshControl refreshing={isPending} onRefresh={reload} />}
          renderItem={({ item, index }) => <PeriodRow row={item} isCurrent={index === currentIndex} />}
        />
      )}
    </Screen>
  );
}

function Chip({ label, selected, onPress }: { label: string; selected: boolean; onPress: () => void }) {
  return (
    <Pressable accessibilityRole="button" onPress={onPress} style={[chipStyles.chip, selected && chipStyles.chipSelected]}>
      <Text style={[chipStyles.label, selected && chipStyles.labelSelected]}>{label}</Text>
    </Pressable>
  );
}

function PeriodRow({ row, isCurrent }: { row: IScheduleRow; isCurrent: boolean }) {
  if (row.isBreak) {
    return (
      <View style={[rowStyles.breakWrapper, isCurrent && rowStyles.breakCurrent]}>
        <Text style={rowStyles.breakLabel}>Break</Text>
        <Text style={rowStyles.breakTime}>{formatTimeShort(row.startTime)}–{formatTimeShort(row.endTime)}</Text>
      </View>
    );
  }

  return (
    <View style={rowStyles.wrapper}>
      <Card style={isCurrent && rowStyles.currentCard}>
        <View style={rowStyles.top}>
          <Text style={rowStyles.period}>Period {row.period}</Text>
          {isCurrent && <Badge label="Now" tone="success" />}
        </View>
        {(row.startTime || row.endTime) && (
          <Text style={rowStyles.time}>
            {formatTimeShort(row.startTime)}{row.endTime ? `–${formatTimeShort(row.endTime)}` : ""}
          </Text>
        )}
        {row.slot ? (
          <>
            <Text style={rowStyles.subject}>{row.slot.subjectName ?? "Lesson"}</Text>
            <Text style={rowStyles.detail}>{row.slot.teacherName ?? "No teacher assigned"}</Text>
            {row.slot.roomNumber && (
              <View style={rowStyles.roomBadge}><Badge label={`Room ${row.slot.roomNumber}`} /></View>
            )}
          </>
        ) : (
          <Text style={rowStyles.free}>Free period</Text>
        )}
      </Card>
    </View>
  );
}

const styles = StyleSheet.create({
  title: { color: colors.text, fontSize: typography.title, fontWeight: "800", marginBottom: spacing.lg },
  modeRow: { flexDirection: "row", gap: spacing.sm, marginBottom: spacing.md },
  dayRow: { flexDirection: "row", flexWrap: "wrap", gap: spacing.sm, marginBottom: spacing.lg },
  list: { paddingBottom: spacing.xxl },
});

const chipStyles = StyleSheet.create({
  chip: { backgroundColor: colors.surface, borderColor: colors.border, borderRadius: radius.pill, borderWidth: 1, paddingHorizontal: spacing.md, paddingVertical: spacing.sm },
  chipSelected: { backgroundColor: colors.primary, borderColor: colors.primary },
  label: { color: colors.text, fontSize: 13, fontWeight: "700" },
  labelSelected: { color: colors.surface },
});

const rowStyles = StyleSheet.create({
  wrapper: { marginBottom: spacing.md },
  currentCard: { borderColor: colors.primary, borderWidth: 2 },
  top: { alignItems: "center", flexDirection: "row", gap: spacing.sm, justifyContent: "space-between" },
  period: { color: colors.text, fontSize: 16, fontWeight: "700" },
  time: { color: colors.textMuted, fontSize: 13, marginTop: spacing.xs },
  subject: { color: colors.text, fontSize: 18, fontWeight: "800", marginTop: spacing.sm },
  detail: { color: colors.textMuted, fontSize: 14, marginTop: spacing.xs },
  roomBadge: { alignSelf: "flex-start", marginTop: spacing.sm },
  free: { color: colors.textMuted, fontSize: 14, fontStyle: "italic", marginTop: spacing.sm },
  breakWrapper: { alignItems: "center", backgroundColor: colors.warningBackground, borderRadius: radius.sm, flexDirection: "row", justifyContent: "space-between", marginBottom: spacing.md, paddingHorizontal: spacing.lg, paddingVertical: spacing.sm },
  breakCurrent: { borderColor: colors.warning, borderWidth: 1 },
  breakLabel: { color: colors.warning, fontSize: 14, fontWeight: "800" },
  breakTime: { color: colors.warning, fontSize: 13, fontWeight: "600" },
});
