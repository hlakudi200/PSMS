import { Pressable, StyleSheet, Text } from "react-native";
import { colors, radius, spacing } from "../theme";

export function Chip({ label, selected = false, onPress }: { label: string; selected?: boolean; onPress: () => void }) {
  return (
    <Pressable
      accessibilityRole="button"
      accessibilityState={{ selected }}
      onPress={onPress}
      style={[styles.chip, selected && styles.selected]}
    >
      <Text style={[styles.label, selected && styles.selectedLabel]}>{label}</Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  chip: {
    backgroundColor: colors.surface, borderColor: colors.border, borderRadius: radius.pill, borderWidth: 1,
    justifyContent: "center", minHeight: 40, paddingHorizontal: spacing.lg,
  },
  selected: { backgroundColor: colors.primary, borderColor: colors.primary },
  label: { color: colors.text, fontSize: 14, fontWeight: "700" },
  selectedLabel: { color: colors.surface },
});
