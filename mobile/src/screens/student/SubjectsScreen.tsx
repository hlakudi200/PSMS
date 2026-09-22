import { useCallback, useEffect } from "react";
import { FlatList, RefreshControl, StyleSheet, Text } from "react-native";
import { router } from "expo-router";
import { Button, ListRow, Screen, StateView } from "../../components";
import { colors, spacing, typography } from "../../theme";
import { useAuthActions, useAuthState } from "../../providers/auth";
import { useSubjectsActions, useSubjectsState } from "../../providers/subjects";
import type { IMySubject } from "../../providers/subjects/context";

export function SubjectsScreen() {
  const { currentStudentId, currentClassId, currentStudentIdError } = useAuthState();
  const { signOut } = useAuthActions();
  const { subjects, isPending, isError } = useSubjectsState();
  const { getMySubjectsAsync } = useSubjectsActions();

  const reload = useCallback(() => {
    if (currentStudentId && currentClassId) void getMySubjectsAsync(currentStudentId, currentClassId);
  }, [currentStudentId, currentClassId, getMySubjectsAsync]);

  useEffect(() => { reload(); }, [reload]);

  // Materials (MOB-S03) and lessons (MOB-S04/S05) screens don't exist yet —
  // marks is the only built destination to tap through to today.
  const openMarks = useCallback(() => router.push("/(student)/(tabs)/marks"), []);

  const handleSignOut = useCallback(async () => {
    await signOut();
    router.dismissAll();
    router.replace("/(auth)/login");
  }, [signOut]);

  // currentStudentIdError means auth already tried and failed to resolve the
  // student profile behind currentStudentId/currentClassId — showing a
  // loading spinner here would spin forever, since reload() can't do
  // anything without those ids. Only fall back to the ordinary loading state
  // while that lookup is still in flight.
  if (currentStudentIdError) {
    return (
      <Screen>
        <StateView state="error" message="We couldn't load your student profile. Try signing in again." />
        <Button onPress={() => void handleSignOut()}>Sign out</Button>
      </Screen>
    );
  }
  if ((isPending && !subjects) || !currentStudentId || !currentClassId) {
    return <Screen><StateView state="loading" message="Loading your subjects…" /></Screen>;
  }
  if (isError) {
    return (
      <Screen>
        <StateView state="error" message="Couldn't load your subjects." />
        <Button onPress={reload}>Retry</Button>
      </Screen>
    );
  }

  return (
    <Screen>
      <Text style={styles.title}>My Subjects</Text>

      {subjects && subjects.length === 0 ? (
        <StateView state="empty" message="You're not enrolled in any subjects for this academic year yet." />
      ) : (
        <FlatList
          data={subjects}
          keyExtractor={(item) => item.subjectId}
          contentContainerStyle={styles.list}
          refreshControl={<RefreshControl refreshing={isPending} onRefresh={reload} />}
          renderItem={({ item }) => <SubjectRow subject={item} onPress={openMarks} />}
        />
      )}
    </Screen>
  );
}

function SubjectRow({ subject, onPress }: { subject: IMySubject; onPress: () => void }) {
  return (
    <ListRow
      title={subject.subjectName}
      detail={[subject.teacherName ?? "No teacher assigned", subject.className].filter(Boolean).join(" · ")}
      onPress={onPress}
    />
  );
}

const styles = StyleSheet.create({
  title: { color: colors.text, fontSize: typography.title, fontWeight: "800", marginBottom: spacing.lg },
  list: { paddingBottom: spacing.xxl },
});
