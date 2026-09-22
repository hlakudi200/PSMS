# National Protocol for Assessment Grades R–12 — what it mandates for PSMS

**RC-18 (#269).** The report card work so far was checked against the NPPPPR
(Feb 2021) and DBE Circular S8 of 2023. Both defer repeatedly to a third
document — *"as specified in the policy document, National Protocol for
Assessment Grades R–12"* — which we had not read. This is what it says, and
what it means for the code.

**Source.** [National Protocol for Assessment Grades R–12](https://www.education.gov.za/Portals/0/Documents/Policies/NatProtAssess.pdf),
Government Notices No. 722 and 723, *Government Gazette* No. 34600 of
12 September 2011, as amended by Government Notices No. 1115 and 1116,
*Government Gazette* No. 36042 of 28 December 2012. Paragraph numbers below are
the Protocol's own. Read on 22 September 2026.

---

## 1. Reporting frequency — answered, and our model is right

> **§25(2)** "Formal report cards should be sent to parents **once a term**. The
> report cards must provide a clear holistic picture of the learner's
> achievements in different subjects."

> **§25(4)** "Learner performance for a term should be reflected on the report
> card for that term."
>
> **§25(5)** "The **end-of-year report card should indicate cumulative learner
> performance for the year**."

So: **four term report cards a year, plus an end-of-year card that is
cumulative, not a fifth term.** Our `ReportType` of Term1–Term4 plus YearEnd is
exactly this shape, and the assumption flagged in #269 turns out to be correct —
it now has a source. `MidYear` and `Progress` are additional to policy, not
required by it; nothing forbids them.

Schedules (§26(1), §26(6)) are also four times a year, and the end-of-year
schedule is "a compilation of learner performance across all four school
terms" (§26(4)). We do not model schedules at all — see the new issues below.

## 2. How a year mark is composed — answered, and it settles RC-05

> **§17(1)** "Recording of learner performance is against the assessment task
> and **reporting is against the total mark obtained in all tasks completed in a
> term**. The **promotion of a learner is based on the composite marks obtained
> in all four terms**."

This is the missing definition RC-05 (#242) was asking for. A term card reports
the term's tasks; the year-end card is the composite of all four terms. It is
not "a mark query with no term filter" and it is not the fourth term's marks.

Two related rules that the aggregation has to respect:

> **§12(2)** A learner who cannot write the end-of-year examination in Grades
> 4–11 through illness or circumstances beyond their control "**will be
> exempted from the examination. A mark based on the School-Based Assessment
> and Practical Assessment mark obtained by the learner prior to his or her
> illness, will be calculated and awarded**."

> **§8(5)(b)** Where School-Based Assessment is outstanding for a valid reason
> and stays outstanding, "the marks for these components will be omitted and the
> **final mark for the relevant subject will be adjusted for promotion purposes
> in terms of the completed tasks**."

Both say the same thing in code terms: **a missing component is dropped from the
weighting, not counted as zero** — which is what `Report.RecalculateOverall`
already does for an unmarked subject, and what the subject aggregation must do
for a missing examination. The one case that *is* a zero is §8(4): Grades 10–12
SBA missing **without** a valid reason, after a three-week grace period, "will be
awarded a zero ('0') for the School-Based Assessment".

## 3. Marks versus percentages — a real gap, per phase

> **§17(4)** "The following is applicable to recording and reporting per phase:
> (a) **Foundation Phase (Grades R–3): Record and report in national codes and
> their achievement descriptions.** (b) Intermediate Phase (Grades 4–6): Record
> and report in national codes and their achievement descriptions **and
> percentages**. (c) Senior Phase (Grades 7–9): … codes … descriptions and
> percentages. (d) **Grades 10–12: Record in marks and report in percentages.**"

> **§17(3)** "Achievement rating on a report card should be indicated by a
> combination of national codes, percentages and comments."

**Our report card prints a percentage for every grade, including Grades R–3,
where the Protocol asks for the code and its description and does not provide
for a percentage.** The Foundation Phase is also 100% school-based (S8 of 2023),
so there is no examination column to print either. This is a new issue.

The seven-level table is reproduced identically four times in the Protocol
(Tables 1–4, §§18–21) and matches `CapsAchievementScale` exactly — 7 Outstanding
80–100 down to 1 Not Achieved 0–29. The scale is confirmed against the primary
source.

> **§17(6)** "In the case of Languages, each language that the learner offers
> should be **recorded and reported on separately according to the different
> levels** on which they are offered. For example, Home Language – English,
> First Additional Language – IsiXhosa, Second Additional Language – Afrikaans."

We model a `Subject` with no language level, so "English" cannot be distinguished
as Home Language or First Additional Language on the card. The promotion rules in
NPPPPR §21(1) and §29(1) are stated *in terms of those levels*, so RC-16 (#266)
cannot be implemented correctly without them. New issue.

## 4. The required contents of a report card — §25(8)

> "Report cards should include information in the following essential components:
> **(a) Personal details:** Name of the learner, grade and class of the learner,
> **date of birth**, **school attendance profile**.
> **(b) Official school details:** Year and term, name of the school, date,
> **signature and comment of parent or guardian, teacher and principal**,
> **dates of closing and opening of school**, **school stamp**, **explanation of
> the codes of the national coding system**.
> **(c) Performance details:** A national code and/or a percentage indicating
> the level of performance per subject and a description of the strengths and
> developmental needs of the learner.
> **(d) Constructive feedback:** The feedback should contain comments about the
> learner's performance **in relation to his or her previous performance**."

Checked against what `ReportPdfGenerator` prints today:

| §25(8) requirement | PSMS |
|---|---|
| Name of learner | ✅ |
| Grade and class | ⚠️ class only — the grade is not printed |
| **Date of birth** | ❌ not on the card |
| School attendance profile | ✅ present/absent/late/total |
| Year and term | ✅ |
| Name of the school | ✅ |
| Date | ✅ generated date and printed date |
| Signature **and comment** of teacher, principal, parent | ⚠️ comments for teacher and principal; blank signature lines for all three; the parent comment is stored but not printed |
| **Dates of closing and opening of school** | ❌ not modelled |
| **School stamp** | ❌ no image slot (the logo is not a stamp) |
| Explanation of the national codes | ✅ the CAPS legend |
| Code and/or percentage per subject | ✅ |
| **Description of strengths and developmental needs** | ⚠️ a free-text teacher comment per subject, unstructured and, before RC-12, unwritable |
| **Comments relative to previous performance** | ❌ nothing compares against the previous term |

`Term` already carries `StartDate` and `EndDate`, so "dates of closing and
opening of school" is available without new data — it is a rendering gap, not a
modelling one.

Two more rules with product consequences:

> **§25(13)** "Schools may **not withhold report cards from learners for any
> reason whatsoever**."
>
> **§25(12)** "The parents or guardians have the **right of access** to report
> cards of their children."

Whatever gating we build around publishing must never become a "withhold the
card until the fees are paid" feature. Worth writing into the business rules.

> **§25(3)** "Schools should ensure that there are **no errors, erasures or
> corrections that will compromise the legal status of the report cards**."

This is the policy backing for RC-10 (#247): a published report card is a legal
document and must be locked, with corrections reissued rather than edited in
place.

## 5. Retention of evidence and moderation

- **§26(9)** The end-of-year schedule "should be kept at school … for at least
  **5 years**", and once signed by the principal and a departmental official
  "this then constitutes a **legal document**" (§26(8)).
- **§28(11)** A learner profile is kept for **three years** after the learner
  exits, then destroyed.
- **§6(4)** SBA moderation applies "from Grade 4 onwards"; **§6(5)** in Grade 12
  the SBA "must be moderated by the Department of Basic Education, the accredited
  assessment body, and Umalusi".
- **§22** Teacher files must hold the annual teaching plan, assessment plan,
  every formal task and memorandum, and a record sheet of every learner's mark
  per task — available on request at all times. **§8(3)** failure to maintain it
  "constitutes an act of misconduct".

Nothing here contradicts what we store; the retention periods are POPIA-relevant
and should be reflected in whatever retention policy the system grows.

## 6. Where the Protocol is out of date

> **§6(2)** Foundation 100:0, Intermediate **75:25**, Senior **40:60**, FET
> **25:75**.

These are the pre-2023 figures — the same ones in the NPPPPR of February 2021.
**DBE Circular S8 of 2023 supersedes them** (Intermediate 80:20, Senior 60:40,
Grades 10–11 40:60, Grade 12 25:75) and is what `AssessmentWeightingDefaults`
implements. The Protocol has not been reissued since 2012, so an older
weighting table inside it is expected and is not a contradiction of S8. Noted
here so nobody "corrects" the code back to the Protocol's numbers.

The Practical Assessment Task rule is still live, though:

> **§7(2)** "The Practical Assessment Tasks mark must count **25% of the
> end-of-year examination mark**" — for the subjects listed in §7(1), which
> include Life Orientation, the technologies, the arts, Computer Applications
> Technology, Information Technology, Consumer Studies, Hospitality Studies,
> Tourism, and languages' oral marks.

We do not model PATs at all. This sits with RC-15 (#263), which already flags
the Physical Education Task inside Life Orientation.

---

## Issues this research opens

1. **Foundation Phase cards print a percentage the Protocol does not provide
   for** (§17(4)(a)) — a Grade R–3 card should carry the code and its
   description, with no percentage and no examination column.
2. **Language levels are not modelled** (§17(6)) — Home Language / First
   Additional / Second Additional. Blocks correct promotion rules in RC-16.
3. **§25(8) report card fields that are missing** — date of birth, grade,
   school opening and closing dates, school stamp, the parent's comment in
   print, and a comparison against the previous term's performance. Overlaps
   RC-17 (#268), which was derived from our own RE-002; this is the primary
   source behind it.
4. **Schedules are not modelled** (§26) — a quarterly per-grade summary, signed,
   kept five years, and the legal record of promotion decisions (RP/NRP for
   Grades R–8, P/NP for Grades 9–11).
5. **Practical Assessment Tasks are not modelled** (§7) — 25% of the
   examination mark in a named list of subjects.
6. **"Schools may not withhold report cards for any reason whatsoever"**
   (§25(13)) — a constraint to write into the business rules before anyone
   builds a fees gate.

## What this research confirms, and needs no change

- The four-term reporting cycle and a cumulative end-of-year card (§25).
- The seven-level achievement scale and its boundaries, now verified against the
  primary source in four separate tables (§§18–21).
- Reporting a subject as a percentage for Grades 4–12 (§17(4)(b)–(d)).
- Dropping a missing component from the weighting rather than scoring it zero
  (§12(2), §8(5)(b)).
- That a published report card is a legal document (§25(3), §26(8)), which is
  the policy basis for locking it.
