export class PaginatedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;

  constructor(items: T[] = [], count: number = 0, pageNumber: number = 1, pageSize: number = 1) {
    this.items = items;
    this.pageNumber = pageNumber;
    this.totalPages = Math.ceil(count / pageSize);
    this.totalCount = count;
  }

  get hasPreviousPage(): boolean {
    return this.pageNumber > 1;
  }

  get hasNextPage(): boolean {
    return this.pageNumber < this.totalPages;
  }
}
