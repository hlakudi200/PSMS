// Canonical pagination types shared across all modules and enterprise table
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
