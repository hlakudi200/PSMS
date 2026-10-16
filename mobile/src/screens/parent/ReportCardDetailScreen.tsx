import { useCallback, useEffect, useState } from "react";
import { KeyboardAvoidingView, Linking, Platform, ScrollView, StyleSheet, Text, TextInput, View } from "react-native";
import { Badge, Button, Card, Screen, StateView } from "../../components";
import { colors, radius, spacing, typography } from "../../theme";
import { useReportCardsActions, useReportCardsState } from "../../providers/report-cards";
import type { IReportCard, IReportCardSubject } from "../../providers/report-cards/context";
import {
  ACHIEVEMENT_LEVEL_LABELS, ACHIEVEMENT_LEVEL_TONES, formatDate, formatPercentage,
} from "../../utils/assessment-labels";
import {
  CONDUCT_RATING_LABELS, MAX_PARENT_COMMENT_LENGTH, PROMOTION_DECISION_LABELS, acknowledgementSchema, reportTitle,
} from "../../utils/report-labels";

export function ReportCardDetailScreen({ reportId }: { reportId: string }) {
  const { reportsById, isDetailPending, isDetailError, isPdfPending, pdfErrorMessage } = useReportCardsState();
  const { getReportAsync, getPdfUrlAsync } = useReportCardsActions();
  const [openError, setOpenError] = useState<string>();
  const report = reportsById[reportId];

  // Always fetch on open: the list row has no acknowledgement or subject detail.
  useEffect(() => { void getReportAsync(reportId); }, [reportId, getReportAsync]);

  const handleOpenPdf = useCallback(async () => {
    setOpenError(undefined);
    const url = await getPdfUrlAsync(reportId);
    if (!url) return;
    try {
      // Opens in the device's PDF viewer, which offers save and share from there.
      await Linking.openURL(url);
    } catch {
      setOpenError("The report card couldn't be opened on this device.");
    }
  }, [reportId, getPdfUrlAsync]);

  if (!report) {
    return (
      <Screen>
        {isDetailError && !isDetailPending ? (
          <>
            <StateView state="error" message="Couldn't load this report card." />
            <Button onPress={() => void getReportAsync(reportId)}>Retry</Button>
          </>
        ) : (
          <StateView state="loading" message="Loading report card…" />
        )}
      </Screen>
    );
  }

  return (
    <KeyboardAvoidingView style={styles.flex} behavior={Platform.OS === "ios" ? "padding" : undefined}>
      <Screen scroll>
        <Text style={styles.overline}>{[report.academicYearName, report.gradeName, report.className].filter(Boolean).join(" · ")}</Text>
        <Text style={styles.title}>{reportTitle(report)}</Text>
        <Text style={styles.muted}>
          {[report.publishedDate ? `Issued ${formatDate(report.publishedDate)}` : undefined, report.reportCardNumber]
            .filter(Boolean).join(" · ")}
        </Text>
        {isDetailError && <Text style={styles.inlineError}>Couldn&apos;t refresh — showing what loaded earlier.</Text>}

        <View style={styles.stack}>
          <OverallCard report={report} />

          <View style={styles.download}>
            {report.hasPdf ? (
              <Button disabled={isPdfPending} onPress={() => void handleOpenPdf()}>
                {isPdfPending ? "Opening…" : "Open the report card PDF"}
              </Button>
            ) : (
              <Text style={styles.muted}>The school hasn&apos;t produced a PDF of this card yet.</Text>
            )}
            {!!(pdfErrorMessage ?? openError) && <Text style={styles.inlineError}>{pdfErrorMessage ?? openError}</Text>}
          </View>

          <AcknowledgementCard report={report} />

          <Text style={styles.sectionTitle}>Subjects</Text>
          {report.subjects.length === 0 ? (
            <Card><Text style={styles.muted}>No subjects on this card.</Text></Card>
          ) : (
            report.subjects.map((subject) => (
              <SubjectRow key={subject.id} subject={subject} reportsPercentages={report.reportsPercentages} />
            ))
          )}

          <AttendanceAndConduct report={report} />

          {!!report.teacherComment && <CommentCard title="Class teacher's comment" text={report.teacherComment} />}
          {!!report.principalComment && <CommentCard title="Principal's comment" text={report.principalComment} />}

          {report.promotionDecision != null && (
            <Card>
              <Text style={styles.cardTitle}>Promotion</Text>
              <Text style={styles.body}>
                {PROMOTION_DECISION_LABELS[report.promotionDecision]}
                {report.promotedToGradeName ? ` to ${report.promotedToGradeName}` : ""}
              </Text>
            </Card>
          )}
        </View>
      </Screen>
    </KeyboardAvoidingView>
  );
}

function OverallCard({ report }: { report: IReportCard }) {
  const level = report.overallAchievementLevel;
  // Foundation Phase cards report national codes and descriptions only — no percentages,
  // so no position either (§17(4)(a)). The PDF follows the same rule.
  const showPercentage = report.reportsPercentages && report.overallPercentage != null;
  const showPosition = report.reportsPercentages && report.classPosition != null;

  return (
    <Card>
      <Text style={styles.label}>Overall</Text>
      {showPercentage && <Text style={styles.big}>{formatPercentage(report.overallPercentage)}</Text>}
      {level != null && (
        <View style={styles.badgeRow}>
          <Badge label={`Level ${level} · ${ACHIEVEMENT_LEVEL_LABELS[level]}`} tone={ACHIEVEMENT_LEVEL_TONES[level]} />
        </View>
      )}
      {!showPercentage && level == null && <Text style={styles.muted}>No overall result on this card.</Text>}
      {showPosition && (
        <Text style={styles.muted}>
          Position {report.classPosition}{report.totalStudentsInClass ? ` of ${report.totalStudentsInClass}` : ""} in class
        </Text>
      )}
    </Card>
  );
}

function SubjectRow({ subject, reportsPercentages }: { subject: IReportCardSubject; reportsPercentages: boolean }) {
  const level = subject.achievementLevel;
  return (
    <Card>
      <View style={styles.rowTop}>
        <Text style={styles.subjectName}>{subject.subjectName}</Text>
        {reportsPercentages && subject.finalMark != null && (
          <Text style={styles.subjectMark}>{formatPercentage(subject.finalMark)}</Text>
        )}
      </View>
      {level != null && (
        <View style={styles.badgeRow}>
          <Badge label={`Level ${level} · ${ACHIEVEMENT_LEVEL_LABELS[level]}`} tone={ACHIEVEMENT_LEVEL_TONES[level]} />
        </View>
      )}
      {subject.awaitsExternalExamination && (
        <Text style={styles.muted}>The final mark follows the external examination.</Text>
      )}
      {!!subject.teacherComment && <Text style={styles.body}>{subject.teacherComment}</Text>}
      {!!subject.teacherName && <Text style={styles.muted}>{subject.teacherName}</Text>}
    </Card>
  );
}

function AttendanceAndConduct({ report }: { report: IReportCard }) {
  const hasConduct = report.conductRating != null || report.diligenceRating != null || !!report.behaviourComments;
  return (
    <Card>
      <Text style={styles.cardTitle}>Attendance{hasConduct ? " and conduct" : ""}</Text>
      <InfoRow label="Days present" value={`${report.daysPresent}${report.daysInTerm ? ` of ${report.daysInTerm}` : ""}`} />
      <InfoRow label="Days absent" value={String(report.daysAbsent)} />
      <InfoRow label="Days late" value={String(report.daysLate)} />
      {report.conductRating != null && <InfoRow label="Conduct" value={CONDUCT_RATING_LABELS[report.conductRating]} />}
      {report.diligenceRating != null && <InfoRow label="Diligence" value={CONDUCT_RATING_LABELS[report.diligenceRating]} />}
      {!!report.behaviourComments && <Text style={styles.body}>{report.behaviourComments}</Text>}
    </Card>
  );
}

function CommentCard({ title, text }: { title: string; text: string }) {
  return (
    <Card>
      <Text style={styles.cardTitle}>{title}</Text>
      <Text style={styles.body}>{text}</Text>
    </Card>
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

/**
 * One action, an optional comment. The backend overwrites the date and comment if called
 * again, so once given it is shown, not offered a second time.
 */
function AcknowledgementCard({ report }: { report: IReportCard }) {
  const { isAcknowledgePending, acknowledgeErrorMessage } = useReportCardsState();
  const { acknowledgeAsync } = useReportCardsActions();
  const [comment, setComment] = useState("");
  const [validationMessage, setValidationMessage] = useState<string>();

  if (report.parentAcknowledgedDate) {
    return (
      <Card style={styles.acknowledged}>
        <View style={styles.rowTop}>
          <Text style={styles.cardTitle}>Received</Text>
          <Badge label="Acknowledged" tone="success" />
        </View>
        <Text style={styles.body}>You acknowledged this report card on {formatDate(report.parentAcknowledgedDate)}.</Text>
        {!!report.parentComment && (
          <View style={styles.quote}>
            <Text style={styles.label}>Your comment</Text>
            <Text style={styles.body}>{report.parentComment}</Text>
          </View>
        )}
      </Card>
    );
  }

  const submit = async () => {
    const parsed = acknowledgementSchema.safeParse({ comment });
    if (!parsed.success) {
      setValidationMessage(parsed.error.issues[0]?.message);
      return;
    }
    setValidationMessage(undefined);
    await acknowledgeAsync(report.id, parsed.data.comment);
  };

  return (
    <Card>
      <Text style={styles.cardTitle}>Let the school know you&apos;ve received it</Text>
      <Text style={styles.muted}>Add a comment for the teacher if you&apos;d like. This is recorded once.</Text>
      <TextInput
        accessibilityLabel="Comment for the school (optional)"
        value={comment}
        onChangeText={setComment}
        placeholder="Comment (optional)"
        multiline
        maxLength={MAX_PARENT_COMMENT_LENGTH}
        editable={!isAcknowledgePending}
        style={styles.input}
      />
      <Text style={styles.counter}>{comment.length}/{MAX_PARENT_COMMENT_LENGTH}</Text>
      {!!(validationMessage ?? acknowledgeErrorMessage) && (
        <Text accessibilityRole="alert" style={styles.inlineError}>{validationMessage ?? acknowledgeErrorMessage}</Text>
      )}
      <View style={styles.formButton}>
        <Button disabled={isAcknowledgePending} onPress={() => void submit()}>
          {isAcknowledgePending ? "Saving…" : "Acknowledge receipt"}
        </Button>
      </View>
    </Card>
  );
}

const styles = StyleSheet.create({
  flex: { flex: 1 },
  overline: { color: colors.textMuted, fontSize: typography.overline, fontWeight: "700", textTransform: "uppercase" },
  title: { color: colors.text, fontSize: typography.heading, fontWeight: "800", marginTop: spacing.xs },
  muted: { color: colors.textMuted, fontSize: 13, marginTop: spacing.xs },
  inlineError: { color: colors.danger, fontSize: 14, marginTop: spacing.sm },
  stack: { gap: spacing.md, marginTop: spacing.lg },
  label: { color: colors.textMuted, fontSize: 13, fontWeight: "700" },
  big: { color: colors.text, fontSize: 32, fontWeight: "800", marginTop: spacing.xs },
  badgeRow: { flexDirection: "row", marginTop: spacing.sm },
  download: { marginTop: spacing.xs },
  sectionTitle: { color: colors.text, fontSize: 18, fontWeight: "800", marginTop: spacing.md },
  cardTitle: { color: colors.text, fontSize: 16, fontWeight: "800" },
  body: { color: colors.text, fontSize: 15, lineHeight: 22, marginTop: spacing.sm },
  rowTop: { alignItems: "center", flexDirection: "row", gap: spacing.sm, justifyContent: "space-between" },
  subjectName: { color: colors.text, flex: 1, fontSize: 16, fontWeight: "700" },
  subjectMark: { color: colors.text, fontSize: 18, fontWeight: "800" },
  infoRow: { flexDirection: "row", justifyContent: "space-between", paddingVertical: spacing.xs },
  infoLabel: { color: colors.textMuted, fontSize: 14 },
  infoValue: { color: colors.text, fontSize: 14, fontWeight: "700" },
  acknowledged: { borderColor: colors.success },
  quote: { backgroundColor: colors.mutedBackground, borderRadius: radius.sm, marginTop: spacing.md, padding: spacing.md },
  input: {
    borderColor: colors.border, borderRadius: radius.sm, borderWidth: 1, color: colors.text, fontSize: 15,
    marginTop: spacing.md, minHeight: 88, padding: spacing.md, textAlignVertical: "top",
  },
  counter: { alignSelf: "flex-end", color: colors.textMuted, fontSize: 12, marginTop: spacing.xs },
  formButton: { marginTop: spacing.md },
});
