import { z } from 'zod';
import dayjs, { Dayjs } from 'dayjs';

/**
 * What the school asks of a prospective parent, and the rules the server will
 * hold them to.
 *
 * These mirror ApplicationAppService.CreateAsync deliberately: the server is
 * the authority and refuses the same things, but a parent should find out at
 * the field rather than after pressing Submit. Where the two could drift the
 * server wins, and the screen shows whatever sentence it sends back.
 */

/** Luhn, the same check the server applies to an SA ID number. */
export function isValidSaIdNumber(value: string): boolean {
  const digits = value.replace(/\s/g, '');
  if (!/^\d{13}$/.test(digits)) return false;

  let sum = 0;
  let double = false;
  for (let i = digits.length - 1; i >= 0; i -= 1) {
    let digit = Number(digits[i]);
    if (double) {
      digit *= 2;
      if (digit > 9) digit -= 9;
    }
    sum += digit;
    double = !double;
  }
  return sum % 10 === 0;
}

export const learnerSchema = z
  .object({
    intake: z.string().min(1, 'Choose what you are applying for.'),
    firstName: z.string().trim().min(2, "Enter the learner's first name."),
    middleName: z.string().trim().max(100).optional().or(z.literal('')),
    lastName: z.string().trim().min(2, "Enter the learner's surname."),
    dateOfBirth: z.custom<Dayjs>((v) => dayjs.isDayjs(v), 'Enter the date of birth.'),
    gender: z.number({ error: 'Choose the gender as it appears on the birth certificate.' }),
    isSACitizen: z.boolean(),
    idNumber: z.string().trim().optional().or(z.literal('')),
    passportNumber: z.string().trim().optional().or(z.literal('')),
    previousSchool: z.string().trim().max(200).optional().or(z.literal('')),
  })
  .refine((v) => !v.dateOfBirth || v.dateOfBirth.isBefore(dayjs(), 'day'), {
    path: ['dateOfBirth'],
    message: 'A date of birth has to be in the past.',
  })
  /* ADM-002: the school needs one or the other, and which one depends on
     citizenship. The server refuses the same way. */
  .refine((v) => !v.isSACitizen || !!v.idNumber, {
    path: ['idNumber'],
    message: 'An ID number is required for a South African citizen.',
  })
  .refine((v) => !v.isSACitizen || !v.idNumber || isValidSaIdNumber(v.idNumber), {
    path: ['idNumber'],
    message: 'That is not a valid South African ID number. Check the thirteen digits.',
  })
  .refine((v) => v.isSACitizen || !!v.passportNumber, {
    path: ['passportNumber'],
    message: 'A passport number is required for a learner who is not a South African citizen.',
  });

export type LearnerFormValues = z.infer<typeof learnerSchema>;

export const parentSchema = z.object({
  relationship: z.number({ error: 'Choose how you are related to the learner.' }),
  firstName: z.string().trim().min(2, 'Enter a first name.'),
  lastName: z.string().trim().min(2, 'Enter a surname.'),
  idNumber: z
    .string()
    .trim()
    .optional()
    .or(z.literal(''))
    .refine((v) => !v || isValidSaIdNumber(v), 'That is not a valid South African ID number.'),
  email: z.string().trim().email('Enter a valid email address.'),
  phoneNumber: z.string().trim().min(10, 'Enter a contactable phone number.'),
  alternatePhone: z.string().trim().optional().or(z.literal('')),
  streetAddress: z.string().trim().max(200).optional().or(z.literal('')),
  suburb: z.string().trim().max(100).optional().or(z.literal('')),
  city: z.string().trim().max(100).optional().or(z.literal('')),
  province: z.string().trim().max(50).optional().or(z.literal('')),
  postalCode: z.string().trim().max(10).optional().or(z.literal('')),
  occupation: z.string().trim().max(100).optional().or(z.literal('')),
  employer: z.string().trim().max(200).optional().or(z.literal('')),
  isPrimaryContact: z.boolean(),
  isFinanciallyResponsible: z.boolean(),
});

export type ParentFormValues = z.infer<typeof parentSchema>;

/**
 * Whether the learner's age fits what the school said it would accept.
 * A warning on the screen rather than a refusal here — the server decides,
 * and it is the one that knows the intake's exact bounds.
 */
export function ageAgainstIntake(
  dateOfBirth: Dayjs | undefined,
  minimumAge?: number,
  maximumAge?: number
): string | undefined {
  if (!dateOfBirth) return undefined;

  const age = dayjs().diff(dateOfBirth, 'year');
  if (minimumAge != null && age < minimumAge)
    return `The school accepts learners from ${minimumAge} for this grade, and this one would be ${age}.`;
  if (maximumAge != null && age > maximumAge)
    return `The school accepts learners up to ${maximumAge} for this grade, and this one would be ${age}.`;

  return undefined;
}

/** "Certified ID parent\nCertified ID student" as a list a person reads. */
export function documentsAsList(requiredDocuments?: string): string[] {
  if (!requiredDocuments) return [];
  return requiredDocuments
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean);
}
