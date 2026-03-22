namespace psms.Financial.Shared;

/// <summary>
/// Exception codes for the Financial module.
/// </summary>
public static class FinancialExceptionCodes
{
    // FeeStructure
    public const string FeeStructureNotFound = "FIN_FEE_STRUCTURE_NOT_FOUND";
    public const string DuplicateFeeStructure = "FIN_DUPLICATE_FEE_STRUCTURE";
    public const string GradeNotFound = "FIN_GRADE_NOT_FOUND";
    public const string AcademicYearNotFound = "FIN_ACADEMIC_YEAR_NOT_FOUND";
    public const string InvalidAmount = "FIN_INVALID_AMOUNT";
    public const string CannotDeleteFeeStructureWithStudentFees = "FIN_CANNOT_DELETE_FEE_STRUCTURE_WITH_STUDENT_FEES";
    public const string FeeStructureInactive = "FIN_FEE_STRUCTURE_INACTIVE";

    // StudentFee
    public const string StudentFeeNotFound = "FIN_STUDENT_FEE_NOT_FOUND";
    public const string DuplicateStudentFee = "FIN_DUPLICATE_STUDENT_FEE";
    public const string StudentNotFound = "FIN_STUDENT_NOT_FOUND";
    public const string InvalidDiscountAmount = "FIN_INVALID_DISCOUNT_AMOUNT";
    public const string StudentFeeAlreadyWaived = "FIN_STUDENT_FEE_ALREADY_WAIVED";
    public const string StudentFeeAlreadyPaid = "FIN_STUDENT_FEE_ALREADY_PAID";
    public const string CannotDeleteStudentFeeWithPayments = "FIN_CANNOT_DELETE_STUDENT_FEE_WITH_PAYMENTS";
    public const string DuplicateStudentInBatch = "FIN_DUPLICATE_STUDENT_IN_BATCH";
    public const string InvalidCancelStatus = "FIN_INVALID_CANCEL_STATUS";
    public const string InvalidWaiveStatus = "FIN_INVALID_WAIVE_STATUS";
    public const string CannotModifySettledFee = "FIN_CANNOT_MODIFY_SETTLED_FEE";
    public const string AmountDueBelowAmountPaid = "FIN_AMOUNT_DUE_BELOW_AMOUNT_PAID";

    // Payment
    public const string PaymentNotFound = "FIN_PAYMENT_NOT_FOUND";
    public const string ParentNotFound = "FIN_PARENT_NOT_FOUND";
    public const string InvalidPaymentStatusTransition = "FIN_INVALID_PAYMENT_STATUS_TRANSITION";
    public const string CannotVoidCompletedPayment = "FIN_CANNOT_VOID_COMPLETED_PAYMENT";
    public const string CannotUpdatePayment = "FIN_CANNOT_UPDATE_PAYMENT";
    public const string PaymentDateInFuture = "FIN_PAYMENT_DATE_IN_FUTURE";
    public const string ReceiptNumberRequired = "FIN_RECEIPT_NUMBER_REQUIRED";

    // PaymentAllocation
    public const string PaymentAllocationNotFound = "FIN_PAYMENT_ALLOCATION_NOT_FOUND";
    public const string AllocationExceedsUnallocated = "FIN_ALLOCATION_EXCEEDS_UNALLOCATED";
    public const string AllocationExceedsOutstanding = "FIN_ALLOCATION_EXCEEDS_OUTSTANDING";
    public const string PaymentNotCompleted = "FIN_PAYMENT_NOT_COMPLETED";
    public const string DuplicateAllocation = "FIN_DUPLICATE_ALLOCATION";
    public const string InvalidAllocationAmount = "FIN_INVALID_ALLOCATION_AMOUNT";
    public const string CannotAllocateToSettledFee = "FIN_CANNOT_ALLOCATE_TO_SETTLED_FEE";
    public const string DuplicateStudentFeeInBatch = "FIN_DUPLICATE_STUDENT_FEE_IN_BATCH";

    // FeeWaiver
    public const string FeeWaiverNotFound = "FIN_FEE_WAIVER_NOT_FOUND";
    public const string InvalidFeeWaiverStatusTransition = "FIN_INVALID_FEE_WAIVER_STATUS";

    // ExpenseRequest
    public const string ExpenseNotFound = "EXP_NOT_FOUND";
    public const string ExpenseNumberDuplicate = "EXP_NUMBER_DUPLICATE";
    public const string ExpenseInvalidStatus = "EXP_INVALID_STATUS";
}
