import axios from 'axios';

export type FieldErrors = Record<string, string>;

export interface ParsedApiError {
  generalError: string;
  fieldErrors: FieldErrors;
}

/**
 * Handles both error shapes the backend returns:
 *
 * FluentValidation (400):
 *   { errors: { "Username": ["msg"], "Email": ["msg"] }, message: "Validation failed" }
 *
 * Domain exception (401 / 404 / 409 / 500):
 *   { error: "Email already in use", statusCode: 409 }
 */
export function parseApiError(err: unknown, fallback: string): ParsedApiError {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data;

    if (data?.errors && typeof data.errors === 'object') {
      const fieldErrors: FieldErrors = {};
      for (const [key, messages] of Object.entries(data.errors)) {
        fieldErrors[key.toLowerCase()] = (messages as string[])[0];
      }
      return { generalError: '', fieldErrors };
    }

    if (data?.error) {
      return { generalError: data.error, fieldErrors: {} };
    }
  }

  return { generalError: fallback, fieldErrors: {} };
}
