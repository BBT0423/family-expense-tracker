/**
 * Password validation utilities
 */

export interface PasswordValidationResult {
  isValid: boolean;
  error: string;
}

/**
 * Validates password according to security requirements:
 * 1. At least 8 characters long
 * 2. Must not contain the user's Employee ID
 * 3. Must meet at least 3 of 4 criteria:
 *    - At least 1 lowercase letter
 *    - At least 1 uppercase letter
 *    - At least 1 number
 *    - At least 1 special character
 */
export const validatePasswordStrength = (
  password: string,
  employeeId?: string
): PasswordValidationResult => {
  // 1. Check minimum length of 8 characters
  if (password.length < 8) {
    return { isValid: false, error: 'Password must be at least 8 characters long.' };
  }

  // 2. Check that password doesn't contain Employee ID
  if (employeeId && password.toLowerCase().includes(employeeId.toLowerCase())) {
    return { isValid: false, error: 'Password must not contain your Employee ID.' };
  }

  // 3. Check that password meets at least 3 of 4 criteria
  const criteria = {
    hasLowercase: /[a-z]/.test(password),
    hasUppercase: /[A-Z]/.test(password),
    hasNumber: /[0-9]/.test(password),
    hasSpecialChar: /[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?~`]/.test(password),
  };

  const metCriteria = Object.values(criteria).filter(Boolean).length;
  if (metCriteria < 3) {
    return {
      isValid: false,
      error:
        'Password must meet at least 3 of the following: lowercase letter, uppercase letter, number, special character.',
    };
  }

  return { isValid: true, error: '' };
};

/**
 * Get password requirements text for display in UI
 */
export const getPasswordRequirements = (): string[] => {
  return [
    'At least 8 characters long',
    'Must not contain your Employee ID',
    'Must meet at least 3 of the following:',
    '  • At least 1 lowercase letter (a-z)',
    '  • At least 1 uppercase letter (A-Z)',
    '  • At least 1 number (0-9)',
    '  • At least 1 special character (!@#$%^&*)',
  ];
};
