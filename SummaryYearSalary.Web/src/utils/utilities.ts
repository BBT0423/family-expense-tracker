import type { ProblemDetails } from '@/types';

export const toNumber = (num: unknown): number => {
  if (!num) return 0;
  return Number(num);
};

export function skip<T>(array: T[], count: number): T[] {
  return array.slice(count);
}

export function take<T>(array: T[], count: number): T[] {
  return array.slice(0, count);
}

export function skipAndTake<T>(array: T[], skipCount: number, takeCount: number): T[] {
  return array.slice(skipCount, skipCount + takeCount);
}

export function isEmpty<T>(array: T[] | null | undefined): boolean {
  return !array || array.length === 0;
}

export function concatenatedString(...parts: (string | null | undefined)[]): string {
  return parts.filter((part) => part && part.trim() !== '').join(' ');
}

export function toProblemDetails(error: any): ProblemDetails {
  const { title, status, detail, instance } = error.response?.data || error;
  console.log({ title, status, detail, instance });
  return {
    title: title ?? 'Unknown Error',
    status: status ?? 500,
    detail: detail ?? 'An unexpected error occurred',
    instance: instance ?? '',
  };
}

export function getGreeting(): string {
  const hour = new Date().getHours();
  if (hour >= 5 && hour < 12) {
    return 'Good morning';
  } else if (hour >= 12 && hour < 18) {
    return 'Good afternoon';
  } else if (hour >= 18 && hour < 22) {
    return 'Good evening';
  } else {
    return 'Hello';
  }
}
