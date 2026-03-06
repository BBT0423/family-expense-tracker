export type Result<T> = {
  isSuccess: boolean;
  value?: T;
  error?: string;
  statusCode: number;
  validationErrors: Record<string, string[]>;
};

export type ProblemDetails = {
  title: string;
  status: number;
  detail: string;
  instance: string;
};

export type BaseFilter = {
  searchTerm?: string;
  page?: number;
  pageSize?: number;
  sortField?: string;
  sortDirection?: 'asc' | 'desc';
};

export interface Column {
  label: string;
  key: string;
  sortable?: boolean;
  sortBy?: string;
  slot?: string;
  style?: string | Record<string, string | number>;
}

export interface Sort {
  field: string;
  direction: 'asc' | 'desc';
}

export interface SelectOption {
  value: string;
  display: string;
  metadata?: any;
}

export interface SelectGroup {
  label: string;
  options: SelectOption[];
}

export interface BaseEmail {
  template?: string;
  origin?: string;
}

export interface Actor {
  id: number;
  code?: string;
  name?: string;
  actionDate: Date | string;
  remarks?: string;
  metadata?: any;
}

export interface StatusObject {
  id: number;
  code?: string;
  name?: string;
  bgColor?: string;
  borderColor?: string;
  fontColor?: string;
  fontSize?: number;
}

export interface CreatedResult {
  id: number;
  no: string;
}
