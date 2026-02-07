// Pagination Interfaces
export interface IPagedAndSortedResultRequest {
    maxResultCount?: number;
    skipCount?: number;
    sorting?: string;
}

export interface IPagedResult<T> {
    totalCount: number;
    items: T[];
}

export interface IListResult<T> {
    items: T[];
}

// FeeStructure Interfaces
export interface IFeeStructure {
    id: string;
    gradeId: string;
    academicYearId: string;
    feeType: number;
    feeName: string;
    amount: number;
    currency: string;
    billingFrequency: string;
    dueDay: number;
    isActive: boolean;
    gradeName: string;
    academicYearName: string;
    formattedAmount: string;
    studentFeeCount: number;
}

export interface IFeeStructureList {
    id: string;
    gradeId: string;
    academicYearId: string;
    feeType: number;
    feeName: string;
    amount: number;
    currency: string;
    billingFrequency: string;
    dueDay: number;
    isActive: boolean;
    gradeName: string;
    academicYearName: string;
    formattedAmount: string;
    studentFeeCount: number;
}

export interface ICreateFeeStructure {
    gradeId: string;
    academicYearId: string;
    feeType: number;
    feeName: string;
    amount: number;
    currency?: string;
    billingFrequency?: string;
    dueDay: number;
}

export interface IUpdateFeeStructure {
    feeName?: string;
    amount?: number;
    feeType?: number;
    currency?: string;
    billingFrequency?: string;
    dueDay?: number;
}

export interface IGetFeeStructuresInput extends IPagedAndSortedResultRequest {
    gradeId?: string;
    academicYearId?: string;
    feeType?: number;
    isActive?: boolean;
    feeName?: string;
}

// StudentFee Interfaces
export interface IStudentFee {
    id: string;
    studentId: string;
    feeStructureId: string;
    amountDue: number;
    amountPaid: number;
    discountAmount: number;
    dueDate: string;
    status: number;
    notes?: string;
    concurrencyStamp: string;
    outstandingBalance: number;
    formattedAmountDue: string;
    allocationCount: number;
    studentName: string;
    studentAdmissionNumber: string;
    feeStructureName: string;
    feeType: number;
}

export interface IStudentFeeList {
    id: string;
    studentId: string;
    feeStructureId: string;
    amountDue: number;
    amountPaid: number;
    discountAmount: number;
    dueDate: string;
    status: number;
    outstandingBalance: number;
    studentName: string;
    studentAdmissionNumber: string;
    feeStructureName: string;
    feeType: number;
}

export interface ICreateStudentFee {
    studentId: string;
    feeStructureId: string;
    amountDue: number;
    dueDate: string;
    notes?: string;
}

export interface IBulkCreateStudentFees {
    feeStructureId: string;
    studentIds: string[];
    dueDateOverride?: string;
}

export interface IUpdateStudentFee {
    amountDue?: number;
    dueDate?: string;
    notes?: string;
}

export interface IGetStudentFeesInput extends IPagedAndSortedResultRequest {
    studentId?: string;
    feeStructureId?: string;
    gradeId?: string;
    academicYearId?: string;
    status?: number;
    studentName?: string;
}

// Payment Interfaces
export interface IPayment {
    id: string;
    studentId: string;
    parentId: string;
    amount: number;
    currency: string;
    paymentMethod: number;
    paymentDate: string;
    paymentReference?: string;
    receiptNumber: string;
    status: number;
    notes?: string;
    concurrencyStamp: string;
    formattedAmount: string;
    totalAllocated: number;
    unallocatedAmount: number;
    allocationCount: number;
    studentName: string;
    studentAdmissionNumber: string;
    parentName: string;
}

export interface IPaymentList {
    id: string;
    studentId: string;
    parentId: string;
    amount: number;
    currency: string;
    paymentMethod: number;
    paymentDate: string;
    paymentReference?: string;
    receiptNumber: string;
    status: number;
    formattedAmount: string;
    totalAllocated: number;
    unallocatedAmount: number;
    allocationCount: number;
    studentName: string;
    studentAdmissionNumber: string;
    parentName: string;
}

export interface ICreatePayment {
    studentId: string;
    parentId: string;
    amount: number;
    paymentMethod: number;
    paymentDate?: string;
    paymentReference?: string;
    notes?: string;
}

export interface IUpdatePayment {
    paymentReference?: string;
    notes?: string;
    paymentMethod?: number;
}

export interface IGetPaymentsInput extends IPagedAndSortedResultRequest {
    studentId?: string;
    parentId?: string;
    status?: number;
    paymentMethod?: number;
    fromDate?: string;
    toDate?: string;
    receiptNumber?: string;
    studentName?: string;
}

// PaymentAllocation Interfaces
export interface IPaymentAllocation {
    id: string;
    paymentId: string;
    studentFeeId: string;
    amount: number;
    allocatedDate: string;
    receiptNumber: string;
    paymentStatus: number;
    paymentAmount: number;
    feeStructureName: string;
    studentFeeAmountDue: number;
    studentFeeOutstandingBalance: number;
    studentName: string;
}

export interface IPaymentAllocationList {
    id: string;
    paymentId: string;
    studentFeeId: string;
    amount: number;
    allocatedDate: string;
    receiptNumber: string;
    feeStructureName: string;
    studentName: string;
}

export interface ICreatePaymentAllocation {
    paymentId: string;
    studentFeeId: string;
    amount: number;
}

export interface IBulkAllocatePayment {
    paymentId: string;
    allocations: IAllocationEntry[];
}

export interface IAllocationEntry {
    studentFeeId: string;
    amount: number;
}

export interface IUpdatePaymentAllocation {
    amount: number;
}
