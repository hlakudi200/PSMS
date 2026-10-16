/**
 * The one vocabulary for admission documents.
 *
 * The school used to type its requirements as free text ("Parents ID \n
 * Learners birth certificate \n Previous school report ") while uploads
 * carried a category from this enum. Nothing could match one against the
 * other, so neither the required-documents status nor the documents-verified
 * guard could answer the only question they exist for — is this application's
 * paperwork in?
 *
 * Names must match the server's `DocumentCategory` exactly: the school's
 * choice is stored by name, and the server parses it back.
 */
export enum DocumentCategory {
  BirthCertificate = 1,
  IdDocument = 2,
  PreviousSchoolReport = 3,
  TransferLetter = 4,
  ImmunizationRecord = 5,
  MedicalReport = 6,
  ProofOfResidence = 7,
  ParentIdDocument = 8,
  Photo = 9,
  Other = 10,
  PassportPhoto = 11,
  StudyPermit = 12,
  Passport = 13,
}

/** What each one is called where a parent will read it. */
export const documentCategoryOptions: { value: DocumentCategory; label: string }[] = [
  { value: DocumentCategory.BirthCertificate, label: "Learner's birth certificate" },
  { value: DocumentCategory.IdDocument, label: "Learner's ID document" },
  { value: DocumentCategory.Passport, label: "Learner's passport" },
  { value: DocumentCategory.StudyPermit, label: 'Study permit' },
  { value: DocumentCategory.PreviousSchoolReport, label: 'Report from the previous school' },
  { value: DocumentCategory.TransferLetter, label: 'Transfer letter' },
  { value: DocumentCategory.ImmunizationRecord, label: 'Immunisation record' },
  { value: DocumentCategory.MedicalReport, label: 'Medical report' },
  { value: DocumentCategory.ProofOfResidence, label: 'Proof of residence' },
  { value: DocumentCategory.ParentIdDocument, label: "Parent's ID document" },
  { value: DocumentCategory.PassportPhoto, label: 'Passport photo' },
  { value: DocumentCategory.Photo, label: 'Photo' },
  { value: DocumentCategory.Other, label: 'Something else' },
];

export const documentCategoryLabel = (value?: number): string =>
  documentCategoryOptions.find((o) => o.value === value)?.label ?? 'Document';

/**
 * The school's list, as the server stores it: category names, comma-separated.
 */
export const serialiseRequiredDocuments = (categories?: DocumentCategory[]): string | undefined =>
  categories?.length ? categories.map((c) => DocumentCategory[c]).join(',') : undefined;

/**
 * Reads the stored list back.
 *
 * Anything unrecognised is dropped rather than shown as a broken chip — a
 * school's leftover free-text note means "they have not chosen yet", not "they
 * require this thing called 'Parents ID'".
 */
export const parseRequiredDocuments = (stored?: string | null): DocumentCategory[] => {
  if (!stored) return [];

  return stored
    .split(/[,\n\r[\]"']+/)
    .map((word) => word.trim())
    .map((word) =>
      documentCategoryOptions.find(
        (o) => DocumentCategory[o.value].toLowerCase() === word.toLowerCase(),
      )?.value,
    )
    .filter((v, i, all): v is DocumentCategory => v !== undefined && all.indexOf(v) === i);
};
