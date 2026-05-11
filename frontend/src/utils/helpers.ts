/**
 * Formats a DateOnly string from the backend (YYYY-MM-DD) to a localized date string.
 * Avoids timezone conversion issues by parsing as local date.
 */
export const formatDateOnly = (dateString: string): string => {
  // Split and parse as local date to avoid timezone conversion
  const [year, month, day] = dateString.split('T')[0].split('-');
  return new Date(
    parseInt(year),
    parseInt(month) - 1,
    parseInt(day)
  ).toLocaleDateString();
};

/**
 * Formats a number as CAD currency.
 */
export const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('en-CA', {
    style: 'currency',
    currency: 'CAD',
  }).format(amount);
};

/**
 * Converts a DateOnly string from backend to YYYY-MM-DD format for HTML date inputs.
 * Handles both "YYYY-MM-DD" and "YYYY-MM-DDTHH:mm:ss" formats.
 */
export const toDateInputValue = (dateString?: string): string => {
  if (!dateString) {
    return new Date().toISOString().split('T')[0];
  }
  return dateString.split('T')[0];
};

