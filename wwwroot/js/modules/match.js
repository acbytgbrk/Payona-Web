/**
 * Match Module - Handles match operations
 */

import { apiClient } from './api.js';

class MatchService {
  async createMatch(fingerprintId, mealRequestId) {
    return apiClient.post(`/matches?fingerprintId=${fingerprintId}&mealRequestId=${mealRequestId}`);
  }

  async createAutoMatch(otherUserId, mealType = 'lunch') {
    return apiClient.post(`/matches/auto-match?otherUserId=${otherUserId}&mealType=${mealType}`);
  }

  async getMyMatches() {
    return apiClient.get('/matches/my');
  }

  async updateStatus(matchId, status) {
    return apiClient.put(`/matches/${matchId}/status?status=${status}`);
  }

  async getActivityStats(period = 'week') {
    return apiClient.get(`/matches/activity-stats?period=${period}`);
  }
}

export const matchService = new MatchService();

