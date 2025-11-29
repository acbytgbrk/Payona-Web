/**
 * Meal Request Module - Handles meal request operations
 */

import { apiClient } from './api.js';

class MealRequestService {
  async create(mealType, preferredDate, preferredStartTime, preferredEndTime, notes) {
    return apiClient.post('/mealrequests', {
      mealType,
      preferredDate,
      preferredStartTime,
      preferredEndTime,
      notes,
    });
  }

  async getAll(mealType = null) {
    const endpoint = mealType ? `/mealrequests?mealType=${mealType}` : '/mealrequests';
    return apiClient.get(endpoint);
  }

  async getMy() {
    return apiClient.get('/mealrequests/my');
  }

  async delete(id) {
    return apiClient.delete(`/mealrequests/${id}`);
  }
}

export const mealRequestService = new MealRequestService();

