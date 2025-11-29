/**
 * Match Module - Handles match operations
 */

import { apiClient } from './api.js';

class MatchService {
  async createMatch(fingerprintId, mealRequestId) {
    return apiClient.post(`/matches?fingerprintId=${fingerprintId}&mealRequestId=${mealRequestId}`);
  }

  async getMyMatches() {
    return apiClient.get('/matches/my');
  }

  async updateStatus(matchId, status) {
    return apiClient.put(`/matches/${matchId}/status?status=${status}`);
  }
}

export const matchService = new MatchService();

