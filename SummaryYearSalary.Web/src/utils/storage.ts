import { AES, enc } from 'crypto-ts';

// Encryption and Decryption functions
export const encrypt = (value: string, secretKey: string) => {
  return AES.encrypt(value, secretKey).toString();
};

export const decrypt = (encryptedValue: string, secretKey: string) => {
  const decryptedBytes = AES.decrypt(encryptedValue, secretKey);
  return decryptedBytes.toString(enc.Utf8);
};

// Utility functions for secure local storage
export const SecureStorage = {
  setItem: (key: string, value: string) => {
    try {
      const encryptedValue = encrypt(value, import.meta.env.VITE_APP_SECRET_KEY);
      globalThis.localStorage.setItem(key, encryptedValue);
    } catch (error) {
      console.error('Error encrypting and storing data:', error);
    }
  },

  getItem: (key: string) => {
    try {
      const encryptedValue = globalThis.localStorage.getItem(key);
      if (encryptedValue) {
        return decrypt(encryptedValue, import.meta.env.VITE_APP_SECRET_KEY);
      }
      return null;
    } catch (error) {
      console.error('Error decrypting and retrieving data:', error);
      return null;
    }
  },

  removeItem: (key: string) => {
    globalThis.localStorage.removeItem(key);
  },
};
