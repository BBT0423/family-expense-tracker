import dayjs from 'dayjs';
import buddhistEra from 'dayjs/plugin/buddhistEra';
import 'dayjs/locale/th';

dayjs.extend(buddhistEra);

/**
 * Utility functions for formatting currency and dates
 * Supports both Thai and English locales
 */

/**
 * Format date with locale support
 * @param dateString - ISO date string or date-like string
 * @param options - Intl.DateTimeFormat options
 * @param locale - Locale override (optional)
 * @returns Formatted date string
 */
export const formatDate = (dateString: string, options?: Intl.DateTimeFormatOptions): string => {
  if (!dateString || dateString.startsWith('0001-01-01')) {
    return '-';
  }

  const date = new Date(dateString);

  // Check if date is valid
  if (Number.isNaN(date.getTime())) {
    return '-';
  }

  // If options are provided, use Intl.DateTimeFormat
  if (options) {
    try {
      return date.toLocaleDateString('en-GB', options); // en-GB gives dd/MM/yyyy order
    } catch (error) {
      console.warn('Date formatting failed, using fallback', error);
      return date.toLocaleDateString('en-GB', options);
    }
  }

  // Default format: dd/MM/yyyy
  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const year = date.getFullYear();

  return `${day}/${month}/${year}`;
};

/**
 * Format date with full format (includes time)
 * @param dateString - ISO date string
 * @param locale - Locale override (optional)
 * @returns Formatted date and time string
 */
export const formatDateTime = (dateString: string): string => {
  const d = dayjs(dateString);
  return d.isValid() ? d.locale('th').format('DD/MMM/BBBB HH:mm') : '-';
};

/**
 * Format date in short format (DD/MM/YYYY or MM/DD/YYYY based on locale)
 * @param dateString - ISO date string
 * @param locale - Locale override (optional)
 * @returns Formatted date string in short format
 */
export const formatDateShort = (dateString: any): string => {
  return formatDate(dateString, {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  });
};

/**
 * Format relative date (e.g., "2 days ago", "in 3 hours")
 * @param dateString - ISO date string
 * @param locale - Locale override (optional)
 * @returns Relative time string
 */
export const formatRelativeDate = (dateString: string): string => {
  if (!dateString || dateString.startsWith('0001-01-01')) {
    return '-';
  }

  const date = new Date(dateString);
  const now = new Date();

  if (Number.isNaN(date.getTime())) {
    return '-';
  }

  try {
    const rtf = new Intl.RelativeTimeFormat('en-US', { numeric: 'auto' });
    const diffInSeconds = (date.getTime() - now.getTime()) / 1000;

    // Determine the appropriate unit
    if (Math.abs(diffInSeconds) < 60) {
      return rtf.format(Math.round(diffInSeconds), 'second');
    } else if (Math.abs(diffInSeconds) < 3600) {
      return rtf.format(Math.round(diffInSeconds / 60), 'minute');
    } else if (Math.abs(diffInSeconds) < 86400) {
      return rtf.format(Math.round(diffInSeconds / 3600), 'hour');
    } else {
      return rtf.format(Math.round(diffInSeconds / 86400), 'day');
    }
  } catch (error) {
    // Fallback to regular date format
    console.warn('Relative date formatting failed, using fallback', error);
    return formatDate(dateString);
  }
};

/**
 * Format number with locale support and decimal place option
 * @param value - Number to format
 * @param decimals - Number of decimal places (optional)
 * @param locale - Locale override (optional)
 * @returns Formatted number string
 */
export const formatNumber = (
  value: number,
  decimals?: number,
  locale: string = 'en-US'
): string => {
  return new Intl.NumberFormat(locale, {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(value);
};

/**
 * Thai Buddhist Era date formatter
 * @param dateString - ISO date string
 * @returns Date in Buddhist Era format (for Thai locale)
 */
export const formatDateThaiBuddhistEra = (dateString: string): string => {
  if (!dateString || dateString.startsWith('0001-01-01')) {
    return '-';
  }

  const date = new Date(dateString);
  if (Number.isNaN(date.getTime())) {
    return '-';
  }

  // Convert to Buddhist Era (add 543 years)
  const buddhistYear = date.getFullYear() + 543;

  return date
    .toLocaleDateString('th-TH', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    })
    .replace(date.getFullYear().toString(), buddhistYear.toString());
};

// All date formats
export const dateFormats = {
  formatOne: 'dd/MM/yyyy',
  formatTwo: 'MM/dd/yyyy',
  formatThree: 'yyyy-MM-dd',
  formatFour: 'dd MMM yyyy',
  formatFive: 'dd MMMM yyyy',
  formatSix: 'dd MMM yyyy HH:mm',
  formatSeven: 'dd MMMM yyyy HH:mm',
  formatEight: 'yyyy-MM-dd HH:mm:ss',
  formatNine: 'dd/MM/yyyy HH:mm',
};

// Get default date format key from environment variable, fallback to 'formatOne'
const DEFAULT_DATE_FORMAT_KEY =
  (import.meta.env.VITE_DEFAULT_DATE_FORMAT_KEY as keyof typeof dateFormats) || 'formatOne';

/**
 * Format date string or Date object to a specific format defined in dateFormats
 * @param dateInput - ISO date string or Date object
 * @param format - Format key from dateFormats
 * @returns Formatted date string
 */
export const formatDateToFormat = (
  dateInput: string | Date,
  format?: keyof typeof dateFormats
): string => {
  const formatKey = format || DEFAULT_DATE_FORMAT_KEY;
  if (!dateInput || (typeof dateInput === 'string' && dateInput.startsWith('0001-01-01'))) {
    return '-';
  }

  const date = typeof dateInput === 'string' ? new Date(dateInput) : dateInput;
  if (Number.isNaN(date.getTime())) {
    return '-';
  }

  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const year = date.getFullYear();
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');
  const seconds = String(date.getSeconds()).padStart(2, '0');

  const monthShort = date.toLocaleString('en-US', { month: 'short' });
  const monthLong = date.toLocaleString('en-US', { month: 'long' });

  switch (formatKey) {
    case 'formatOne':
      return `${day}/${month}/${year}`;
    case 'formatTwo':
      return `${month}/${day}/${year}`;
    case 'formatThree':
      return `${year}-${month}-${day}`;
    case 'formatFour':
      return `${day} ${monthShort} ${year}`;
    case 'formatFive':
      return `${day} ${monthLong} ${year}`;
    case 'formatSix':
      return `${day} ${monthShort} ${year} ${hours}:${minutes}`;
    case 'formatSeven':
      return `${day} ${monthLong} ${year} ${hours}:${minutes}`;
    case 'formatEight':
      return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
    case 'formatNine':
      return `${day}/${month}/${year} ${hours}:${minutes}`;
    default:
      return `${day}/${month}/${year}`;
  }
};

/**
 * Format date to API format (YYYY-MM-DD) without timezone conversion
 * This ensures the local date is preserved when sending to API
 * @param date - Date object or date string
 * @returns Date string in YYYY-MM-DD format
 */
export const formatDateForApi = (date: Date | string | null | undefined): string | null => {
  if (!date) return null;

  const dateObj = typeof date === 'string' ? new Date(date) : date;

  // Check if date is valid
  if (Number.isNaN(dateObj.getTime())) {
    return null;
  }

  // Use local date components to avoid timezone conversion
  const year = dateObj.getFullYear();
  const month = String(dateObj.getMonth() + 1).padStart(2, '0');
  const day = String(dateObj.getDate()).padStart(2, '0');

  return `${year}-${month}-${day}`;
};

/**
 * Thai short date in Buddhist Era — DD/MMM/BBBB (e.g. 16/ก.ย./2569)
 */
export const formatThaiDate = (value?: string | Date | null): string => {
  if (!value || (typeof value === 'string' && value.startsWith('0001-01-01'))) return '-';
  const d = dayjs(value);
  return d.isValid() ? d.locale('th').format('DD/MMM/BBBB') : '-';
};

/** Thai month + Buddhist year — MMM BBBB (e.g. ก.ย. 2569) */
export const formatThaiMonth = (value?: string | Date | null): string => {
  if (!value) return '-';
  const d = dayjs(value);
  return d.isValid() ? d.locale('th').format('MMM BBBB') : '-';
};
