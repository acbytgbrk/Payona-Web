/**
 * Fingerprint Module - Handles fingerprint (meal giving) operations
 */

import { apiClient } from './api.js';

class FingerprintService {
  async create(mealType, availableDate, startTime, endTime, description) {
    return apiClient.post('/fingerprints', {
      mealType,
      availableDate,
      startTime,
      endTime,
      description,
    });
  }

  async getAll(mealType = null) {
    const endpoint = mealType ? `/fingerprints?mealType=${mealType}` : '/fingerprints';
    return apiClient.get(endpoint);
  }

  async getMy() {
    return apiClient.get('/fingerprints/my');
  }

  async delete(id) {
    return apiClient.delete(`/fingerprints/${id}`);
  }
}

export const fingerprintService = new FingerprintService();

