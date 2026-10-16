import { StyleSheet, Text, View } from "react-native";
import { router } from "expo-router";
import { Badge, ListRow, Screen } from "../../components";
import { colors, spacing, typography } from "../../theme";

export function ParentMoreScreen() {
  return (
    <Screen>
      <Badge label="MORE" />
      <Text style={styles.title}>More</Text>
      <View style={styles.list}>
        <ListRow
          title="Notifications and contact details"
          detail="How the school reaches you, and about what"
          onPress={() => router.push("/(parent)/notifications")}
          accessory={<Text style={styles.chevron}>›</Text>}
        />
      </View>
    </Screen>
  );
}

const styles = StyleSheet.create({
  title: { color: colors.text, fontSize: typography.title, fontWeight: "800", marginTop: spacing.lg },
  list: { marginTop: spacing.lg },
  chevron: { color: colors.textMuted, fontSize: 24, paddingLeft: spacing.sm },
});
